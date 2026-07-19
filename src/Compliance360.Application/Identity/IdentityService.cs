using System.Security.Cryptography;
using System.Text;
using Compliance360.Application.Mfa;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Identity;
using Compliance360.Shared;
using Microsoft.Extensions.Options;

namespace Compliance360.Application.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly IIdentityRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordPolicyValidator _passwordPolicyValidator;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IMfaChallengeTokenService _mfaChallengeTokenService;
    private readonly IAuthResolverTokenService _authResolverTokenService;
    private readonly IMfaSecretProtector _mfaSecretProtector;
    private readonly ITotpService _totpService;
    private readonly IClock _clock;
    private readonly LockoutOptions _lockoutOptions;
    private readonly PasswordPolicyOptions _passwordPolicyOptions;

    public IdentityService(
        IIdentityRepository repository,
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IPasswordPolicyValidator passwordPolicyValidator,
        IJwtTokenService jwtTokenService,
        IRefreshTokenGenerator refreshTokenGenerator,
        IMfaChallengeTokenService mfaChallengeTokenService,
        IAuthResolverTokenService authResolverTokenService,
        IMfaSecretProtector mfaSecretProtector,
        ITotpService totpService,
        IClock clock,
        IOptions<LockoutOptions> lockoutOptions,
        IOptions<PasswordPolicyOptions> passwordPolicyOptions)
    {
        _repository = repository;
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _passwordPolicyValidator = passwordPolicyValidator;
        _jwtTokenService = jwtTokenService;
        _refreshTokenGenerator = refreshTokenGenerator;
        _mfaChallengeTokenService = mfaChallengeTokenService;
        _authResolverTokenService = authResolverTokenService;
        _mfaSecretProtector = mfaSecretProtector;
        _totpService = totpService;
        _clock = clock;
        _lockoutOptions = lockoutOptions.Value;
        _passwordPolicyOptions = passwordPolicyOptions.Value;
    }

    public async Task<Result<AuthIdentifyResult>> IdentifyAsync(AuthIdentifyCommand command, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(command.Email);
        var memberships = await _repository.GetUserTenantMembershipsByEmailAsync(normalizedEmail, cancellationToken);
        var orderedMemberships = memberships.OrderBy(membership => membership.TenantName).ToArray();
        Guid? preselectedTenantId = null;

        if (!string.IsNullOrWhiteSpace(command.HostName))
        {
            var resolvedByHost = await _repository.ResolveTenantByHostAsync(command.HostName, cancellationToken);
            if (resolvedByHost.HasValue && orderedMemberships.Any(membership => membership.TenantId == resolvedByHost.Value))
            {
                preselectedTenantId = resolvedByHost.Value;
            }
        }

        if (!preselectedTenantId.HasValue && orderedMemberships.Length == 1)
        {
            preselectedTenantId = orderedMemberships[0].TenantId;
        }

        var resolverPrincipal = new AuthResolverPrincipal(
            normalizedEmail,
            orderedMemberships.Select(membership => membership.TenantId).Distinct().ToArray(),
            preselectedTenantId,
            _clock.UtcNow.AddMinutes(5),
            Guid.NewGuid().ToString("N"));
        var resolverToken = _authResolverTokenService.Create(resolverPrincipal);

        await AppendAuditAsync(
            orderedMemberships.FirstOrDefault()?.TenantId,
            null,
            nameof(User),
            null,
            AuditAction.Viewed,
            command.IpAddress,
            command.UserAgent,
            cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<AuthIdentifyResult>.Success(new AuthIdentifyResult(
            resolverToken,
            orderedMemberships.Length > 1 && !preselectedTenantId.HasValue,
            preselectedTenantId,
            orderedMemberships));
    }

    public async Task<Result<AuthenticationResult>> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(command.Email);
        var user = await _repository.GetUserByEmailAsync(command.TenantId, normalizedEmail, cancellationToken);
        if (user is null)
        {
            await AppendAuditAsync(command.TenantId, null, nameof(User), null, AuditAction.LoginFailed, command.IpAddress, command.UserAgent, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuthenticationResult>.Failure("Correo o contrasena incorrectos. Verifica e intenta de nuevo.");
        }

        if (!await _repository.IsTenantActiveAsync(user.TenantId, cancellationToken)
            || user.IsLocked(_clock.UtcNow)
            || user.Status == UserStatus.Disabled)
        {
            await AppendAuditAsync(user.TenantId, user.Id, nameof(User), user.Id, AuditAction.PermissionDenied, command.IpAddress, command.UserAgent, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuthenticationResult>.Failure("Tu cuenta no esta disponible. Contacta al administrador del tenant.");
        }

        if (_passwordHasher.Verify(command.Password, user.PasswordHash) != PasswordVerificationResult.Success)
        {
            var locked = user.RegisterFailedLogin(_lockoutOptions.MaxFailedAttempts, _clock.UtcNow.AddMinutes(_lockoutOptions.LockoutMinutes));
            await AppendAuditAsync(user.TenantId, user.Id, nameof(User), user.Id, locked ? AuditAction.AccountLocked : AuditAction.LoginFailed, command.IpAddress, command.UserAgent, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuthenticationResult>.Failure(locked
                ? "Cuenta bloqueada por intentos fallidos. Contacta al administrador."
                : "Correo o contrasena incorrectos. Verifica e intenta de nuevo.");
        }

        var tenantRequiresMfa = await _repository.IsTenantMfaRequiredAsync(user.TenantId, cancellationToken);
        if (tenantRequiresMfa || user.MfaEnabled)
        {
            var mfaConfiguration = await _repository.GetEnabledMfaConfigurationAsync(user.TenantId, user.Id, MfaMethod.Totp, cancellationToken);
            if (mfaConfiguration is null)
            {
                await AppendAuditAsync(user.TenantId, user.Id, nameof(MfaConfiguration), user.Id, AuditAction.MfaChallengeFailed, command.IpAddress, command.UserAgent, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result<AuthenticationResult>.Failure("Tu organizacion exige MFA, pero esta cuenta aun no tiene un metodo TOTP configurado. Contacta al administrador.");
            }

            var challenge = _mfaChallengeTokenService.Create(new MfaChallengePrincipal(user.TenantId, user.Id, mfaConfiguration.Method, _clock.UtcNow.AddMinutes(5)));
            await AppendAuditAsync(user.TenantId, user.Id, nameof(MfaConfiguration), user.Id, AuditAction.MfaChallengeRequired, command.IpAddress, command.UserAgent, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuthenticationResult>.Success(AuthenticationResult.MfaChallenge(user.Id, user.TenantId, user.Email, challenge, mfaConfiguration.Method));
        }

        user.RegisterLogin(_clock.UtcNow);
        var result = await CreateAuthenticationResultAsync(user, command.IpAddress, command.UserAgent, cancellationToken);
        await AppendAuditAsync(user.TenantId, user.Id, nameof(User), user.Id, AuditAction.LoginSucceeded, command.IpAddress, command.UserAgent, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<AuthenticationResult>.Success(result);
    }

    public async Task<Result<AuthenticationResult>> LoginResolvedAsync(LoginResolvedCommand command, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(command.Email);
        var validation = _authResolverTokenService.Validate(command.ResolverToken);
        if (validation.IsFailure || validation.Value is null)
        {
            return Result<AuthenticationResult>.Failure(validation.Error ?? "Tu identificacion expiro. Vuelve a ingresar tu correo.");
        }

        var principal = validation.Value;
        if (!string.Equals(principal.NormalizedEmail, normalizedEmail, StringComparison.Ordinal))
        {
            return Result<AuthenticationResult>.Failure("El correo no coincide con la identificacion. Vuelve a empezar el login.");
        }

        if (principal.AllowedTenantIds.Count == 0)
        {
            return Result<AuthenticationResult>.Failure("No encontramos una organizacion para este correo. Verifica que el administrador te haya creado en el tenant correcto.");
        }

        var selectedTenantId = command.OrganizationId
            ?? principal.PreselectedTenantId
            ?? (principal.AllowedTenantIds.Count == 1 ? principal.AllowedTenantIds.Single() : null);
        if (!selectedTenantId.HasValue || !principal.AllowedTenantIds.Contains(selectedTenantId.Value))
        {
            return Result<AuthenticationResult>.Failure("Selecciona la organizacion para continuar.");
        }

        var loginResult = await LoginAsync(
            new LoginCommand(selectedTenantId.Value, command.Email, command.Password, command.IpAddress, command.UserAgent),
            cancellationToken);
        if (loginResult.IsSuccess)
        {
            _authResolverTokenService.MarkUsed(principal);
        }

        return loginResult;
    }

    public async Task<Result<AuthenticationResult>> CompleteMfaChallengeAsync(CompleteMfaChallengeCommand command, CancellationToken cancellationToken = default)
    {
        var challenge = _mfaChallengeTokenService.Validate(command.ChallengeToken);
        if (challenge.IsFailure || challenge.Value is null || challenge.Value.Method != command.Method)
        {
            return Result<AuthenticationResult>.Failure("Invalid MFA challenge.");
        }

        var principal = challenge.Value;
        var user = await _repository.GetUserByIdAsync(principal.TenantId, principal.UserId, cancellationToken);
        var configuration = await _repository.GetEnabledMfaConfigurationAsync(principal.TenantId, principal.UserId, command.Method, cancellationToken);
        if (user is null
            || configuration is null
            || !await _repository.IsTenantActiveAsync(principal.TenantId, cancellationToken)
            || user.IsLocked(_clock.UtcNow)
            || user.Status == UserStatus.Disabled)
        {
            await AppendAuditAsync(principal.TenantId, principal.UserId, nameof(MfaConfiguration), principal.UserId, AuditAction.MfaChallengeFailed, command.IpAddress, command.UserAgent, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuthenticationResult>.Failure("Invalid MFA challenge.");
        }

        var secret = _mfaSecretProtector.Unprotect(configuration.SecretEncrypted);
        if (!_totpService.VerifyCode(secret, command.VerificationCode, _clock.UtcNow))
        {
            configuration.RegisterFailedVerification();
            await AppendAuditAsync(principal.TenantId, principal.UserId, nameof(MfaConfiguration), principal.UserId, AuditAction.MfaChallengeFailed, command.IpAddress, command.UserAgent, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuthenticationResult>.Failure("Invalid MFA challenge.");
        }

        configuration.RegisterSuccessfulVerification(_clock.UtcNow);
        user.RegisterLogin(_clock.UtcNow);
        var result = await CreateAuthenticationResultAsync(user, command.IpAddress, command.UserAgent, cancellationToken);
        await AppendAuditAsync(user.TenantId, user.Id, nameof(MfaConfiguration), user.Id, AuditAction.MfaChallengeSucceeded, command.IpAddress, command.UserAgent, cancellationToken);
        await AppendAuditAsync(user.TenantId, user.Id, nameof(User), user.Id, AuditAction.LoginSucceeded, command.IpAddress, command.UserAgent, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<AuthenticationResult>.Success(result);
    }

    public async Task<Result<AuthenticationResult>> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        var currentHash = HashRefreshToken(command.RefreshToken);
        var currentToken = await _repository.GetRefreshTokenByHashAsync(currentHash, cancellationToken);
        if (currentToken is null || !currentToken.IsActive(_clock.UtcNow))
        {
            return Result<AuthenticationResult>.Failure("Invalid refresh token.");
        }

        var user = await _repository.GetUserByIdAsync(currentToken.TenantId, currentToken.UserId, cancellationToken);
        if (user is null
            || !await _repository.IsTenantActiveAsync(currentToken.TenantId, cancellationToken)
            || user.IsLocked(_clock.UtcNow)
            || user.Status == UserStatus.Disabled)
        {
            return Result<AuthenticationResult>.Failure("Account is not available.");
        }

        var result = await CreateAuthenticationResultAsync(user, command.IpAddress, command.UserAgent, cancellationToken);
        currentToken.Revoke(_clock.UtcNow, result.RefreshTokenHash);
        if (currentToken.SessionId.HasValue)
        {
            var previousSession = await _repository.GetSessionByIdAsync(user.TenantId, currentToken.SessionId.Value, cancellationToken);
            previousSession?.Revoke(_clock.UtcNow);
        }
        await AppendAuditAsync(user.TenantId, user.Id, nameof(RefreshToken), currentToken.Id, AuditAction.TokenRefreshed, command.IpAddress, command.UserAgent, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<AuthenticationResult>.Success(result);
    }

    public async Task<Result> LogoutAsync(LogoutCommand command, CancellationToken cancellationToken = default)
    {
        var token = await _repository.GetRefreshTokenByHashAsync(command.RefreshTokenHash, cancellationToken);
        if (token is null || token.TenantId != command.TenantId || token.UserId != command.UserId)
        {
            return Result.Failure("Session not found.");
        }

        token.Revoke(_clock.UtcNow);
        if (token.SessionId.HasValue)
        {
            var session = await _repository.GetSessionByIdAsync(command.TenantId, token.SessionId.Value, cancellationToken);
            session?.Revoke(_clock.UtcNow);
        }
        await AppendAuditAsync(command.TenantId, command.UserId, nameof(UserSession), token.SessionId, AuditAction.Logout, command.IpAddress, command.UserAgent, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetUserByIdAsync(command.TenantId, command.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure("User not found.");
        }

        if (_passwordHasher.Verify(command.CurrentPassword, user.PasswordHash) != PasswordVerificationResult.Success)
        {
            await AppendAuditAsync(command.TenantId, command.UserId, nameof(User), command.UserId, AuditAction.LoginFailed, command.IpAddress, command.UserAgent, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Failure("Current password is invalid.");
        }

        var policyResult = _passwordPolicyValidator.Validate(command.NewPassword);
        if (policyResult.IsFailure)
        {
            return policyResult;
        }

        if (user.PasswordHistory.Take(_passwordPolicyOptions.PasswordHistoryLimit).Any(history => _passwordHasher.Verify(command.NewPassword, history.PasswordHash) == PasswordVerificationResult.Success))
        {
            return Result.Failure("Password was used recently.");
        }

        user.ChangePassword(_passwordHasher.HashPassword(command.NewPassword), _clock.UtcNow);
        foreach (var session in user.Sessions.Where(session => session.IsActive(_clock.UtcNow)))
        {
            session.Revoke(_clock.UtcNow);
        }
        foreach (var refreshToken in user.RefreshTokens.Where(token => token.IsActive(_clock.UtcNow)))
        {
            refreshToken.Revoke(_clock.UtcNow);
        }
        await AppendAuditAsync(command.TenantId, command.UserId, nameof(User), command.UserId, AuditAction.PasswordChanged, command.IpAddress, command.UserAgent, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AssignRoleAsync(AssignRoleCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetUserByIdAsync(command.TenantId, command.UserId, cancellationToken);
        var role = await _repository.GetRoleByIdAsync(command.TenantId, command.RoleId, cancellationToken);
        if (user is null || role is null)
        {
            return Result.Failure("User or role not found.");
        }

        user.AssignRole(role.Id);
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(UserRole), user.Id, AuditAction.RoleAssigned, null, null, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> GrantPermissionAsync(GrantPermissionCommand command, CancellationToken cancellationToken = default)
    {
        var role = await _repository.GetRoleByIdAsync(command.TenantId, command.RoleId, cancellationToken);
        var permission = await _repository.GetPermissionByIdAsync(command.PermissionId, cancellationToken);
        if (role is null || permission is null)
        {
            return Result.Failure("Role or permission not found.");
        }

        role.GrantPermission(permission.Id);
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(Permission), permission.Id, AuditAction.PermissionChanged, null, null, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ConfigureMfaAsync(ConfigureMfaCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetUserByIdAsync(command.TenantId, command.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure("User not found.");
        }

        user.EnableMfa(command.SecretEncrypted);
        await _repository.AddMfaConfigurationAsync(new MfaConfiguration(command.TenantId, command.UserId, command.Method, command.SecretEncrypted, _clock.UtcNow), cancellationToken);
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(MfaConfiguration), user.Id, AuditAction.MfaConfigured, null, null, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> UnlockAccountAsync(UnlockAccountCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetUserByIdAsync(command.TenantId, command.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure("User not found.");
        }

        user.Unlock();
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(User), user.Id, AuditAction.AccountUnlocked, null, null, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<AuthenticationResult> CreateAuthenticationResultAsync(User user, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        var roles = await _repository.GetRoleNamesAsync(user.TenantId, user.Id, cancellationToken);
        var permissions = await _repository.GetPermissionCodesAsync(user.TenantId, user.Id, cancellationToken);
        var generatedRefreshToken = _refreshTokenGenerator.Generate();
        var session = new UserSession(user.TenantId, user.Id, _clock.UtcNow, generatedRefreshToken.ExpiresAtUtc);
        var accessToken = _jwtTokenService.CreateAccessToken(
            new AuthenticatedUser(user.Id, user.TenantId, user.Email, user.FullName, roles, permissions, session.Id));
        var refreshToken = new RefreshToken(user.TenantId, user.Id, generatedRefreshToken.TokenHash, generatedRefreshToken.ExpiresAtUtc);
        refreshToken.LinkSession(session.Id);

        user.AddRefreshToken(refreshToken);
        user.AddSession(session);
        await _repository.AddSessionAsync(session, cancellationToken);
        await _repository.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await AppendAuditAsync(user.TenantId, user.Id, nameof(UserSession), session.Id, AuditAction.Created, ipAddress, userAgent, cancellationToken);

        return new AuthenticationResult(
            user.Id,
            user.TenantId,
            user.Email,
            accessToken.AccessToken,
            accessToken.ExpiresAtUtc,
            generatedRefreshToken.PlainTextToken,
            generatedRefreshToken.TokenHash,
            generatedRefreshToken.ExpiresAtUtc,
            session.Id,
            false);
    }

    private async Task AppendAuditAsync(
        Guid? tenantId,
        Guid? userId,
        string entityName,
        Guid? entityId,
        AuditAction action,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var auditLog = AuditLog
            .Create(tenantId, userId, entityName, entityId, action, _clock.UtcNow)
            .WithRequestContext(ipAddress, userAgent, null);
        await _repository.AddAuditLogAsync(auditLog, cancellationToken);
    }

    private static string NormalizeEmail(string email)
    {
        return Guard.AgainstNullOrWhiteSpace(email, nameof(email), 320).ToUpperInvariant();
    }

    private static string HashRefreshToken(string refreshToken)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }
}
