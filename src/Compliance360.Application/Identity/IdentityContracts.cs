using Compliance360.Domain.Audit;
using Compliance360.Domain.Identity;
using Compliance360.Shared;

namespace Compliance360.Application.Identity;

public interface IIdentityService
{
    Task<Result<AuthIdentifyResult>> IdentifyAsync(AuthIdentifyCommand command, CancellationToken cancellationToken = default);

    Task<Result<AuthenticationResult>> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default);

    Task<Result<AuthenticationResult>> LoginResolvedAsync(LoginResolvedCommand command, CancellationToken cancellationToken = default);

    Task<Result<AuthenticationResult>> CompleteMfaChallengeAsync(CompleteMfaChallengeCommand command, CancellationToken cancellationToken = default);

    Task<Result<AuthenticationResult>> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken = default);

    Task<Result> LogoutAsync(LogoutCommand command, CancellationToken cancellationToken = default);

    Task<Result> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default);

    Task<Result> AssignRoleAsync(AssignRoleCommand command, CancellationToken cancellationToken = default);

    Task<Result> GrantPermissionAsync(GrantPermissionCommand command, CancellationToken cancellationToken = default);

    Task<Result> ConfigureMfaAsync(ConfigureMfaCommand command, CancellationToken cancellationToken = default);

    Task<Result> UnlockAccountAsync(UnlockAccountCommand command, CancellationToken cancellationToken = default);
}

public interface IIdentityRepository
{
    Task<User?> GetUserByEmailAsync(Guid tenantId, string normalizedEmail, CancellationToken cancellationToken = default);

    Task<User?> GetUserByIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<Role?> GetRoleByIdAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default);

    Task<Permission?> GetPermissionByIdAsync(Guid permissionId, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<UserSession?> GetSessionByIdAsync(Guid tenantId, Guid sessionId, CancellationToken cancellationToken = default) =>
        Task.FromResult<UserSession?>(null);

    Task<bool> IsTenantActiveAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        Task.FromResult(true);

    Task<bool> IsTenantMfaRequiredAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<MfaConfiguration?> GetEnabledMfaConfigurationAsync(Guid tenantId, Guid userId, MfaMethod method, CancellationToken cancellationToken = default);

    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task AddSessionAsync(UserSession session, CancellationToken cancellationToken = default);

    Task AddMfaConfigurationAsync(MfaConfiguration configuration, CancellationToken cancellationToken = default);

    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetRoleNamesAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetPermissionCodesAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserTenantMembership>> GetUserTenantMembershipsByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<Guid?> ResolveTenantByHostAsync(string hostName, CancellationToken cancellationToken = default);
}

public interface IPasswordPolicyValidator
{
    Result Validate(string password);
}

public interface IMfaChallengeTokenService
{
    string Create(MfaChallengePrincipal principal);

    Result<MfaChallengePrincipal> Validate(string challengeToken);
}

public sealed record LoginCommand(
    Guid TenantId,
    string Email,
    string Password,
    string? IpAddress,
    string? UserAgent);

public sealed record LoginResolvedCommand(
    string Email,
    string Password,
    string ResolverToken,
    Guid? OrganizationId,
    bool RememberMe,
    string? IpAddress,
    string? UserAgent);

public sealed record AuthIdentifyCommand(
    string Email,
    string? HostName,
    string? IpAddress,
    string? UserAgent);

public sealed record CompleteMfaChallengeCommand(
    string ChallengeToken,
    MfaMethod Method,
    string VerificationCode,
    string? IpAddress,
    string? UserAgent);

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress,
    string? UserAgent);

public sealed record LogoutCommand(
    Guid TenantId,
    Guid UserId,
    string RefreshTokenHash,
    string? IpAddress,
    string? UserAgent);

public sealed record ChangePasswordCommand(
    Guid TenantId,
    Guid UserId,
    string CurrentPassword,
    string NewPassword,
    string? IpAddress,
    string? UserAgent);

public sealed record AssignRoleCommand(
    Guid TenantId,
    Guid UserId,
    Guid RoleId,
    Guid RequestedByUserId);

public sealed record GrantPermissionCommand(
    Guid TenantId,
    Guid RoleId,
    Guid PermissionId,
    Guid RequestedByUserId);

public sealed record ConfigureMfaCommand(
    Guid TenantId,
    Guid UserId,
    MfaMethod Method,
    string SecretEncrypted,
    Guid RequestedByUserId);

public sealed record UnlockAccountCommand(
    Guid TenantId,
    Guid UserId,
    Guid RequestedByUserId);

public sealed record AuthenticationResult(
    Guid UserId,
    Guid TenantId,
    string Email,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    string RefreshTokenHash,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    Guid SessionId,
    bool MfaRequired,
    string? MfaChallengeToken = null,
    MfaMethod? MfaMethod = null)
{
    public static AuthenticationResult MfaChallenge(Guid userId, Guid tenantId, string email, string challengeToken, MfaMethod method)
    {
        return new AuthenticationResult(
            userId,
            tenantId,
            email,
            string.Empty,
            DateTimeOffset.MinValue,
            string.Empty,
            string.Empty,
            DateTimeOffset.MinValue,
            Guid.Empty,
            true,
            challengeToken,
            method);
    }
}

public sealed record MfaChallengePrincipal(
    Guid TenantId,
    Guid UserId,
    MfaMethod Method,
    DateTimeOffset ExpiresAtUtc);

public sealed record UserTenantMembership(
    Guid TenantId,
    Guid UserId,
    string TenantName,
    string? LogoUri,
    string? PrimaryColor,
    string? Description);

public sealed record AuthIdentifyResult(
    string ResolverToken,
    bool RequiresOrganizationSelection,
    Guid? PreselectedOrganizationId,
    IReadOnlyCollection<UserTenantMembership> Organizations);

public sealed class MfaChallengeOptions
{
    public const string SectionName = "MfaChallenge";

    public int LifetimeMinutes { get; set; } = 5;
}

public interface IAuthResolverTokenService
{
    string Create(AuthResolverPrincipal principal);

    Result<AuthResolverPrincipal> Validate(string token);

    /// <summary>
    /// Marks a validated resolver token as used after a successful login so it cannot be replayed.
    /// Failed password attempts must not consume the token.
    /// </summary>
    void MarkUsed(AuthResolverPrincipal principal);
}

public sealed record AuthResolverPrincipal(
    string NormalizedEmail,
    IReadOnlyCollection<Guid> AllowedTenantIds,
    Guid? PreselectedTenantId,
    DateTimeOffset ExpiresAtUtc,
    string Nonce);

public sealed class AuthResolverOptions
{
    public const string SectionName = "AuthResolver";

    public int LifetimeMinutes { get; set; } = 5;
}

public sealed class PasswordPolicyOptions
{
    public const string SectionName = "PasswordPolicy";

    public int MinimumLength { get; set; } = 12;

    public bool RequireUppercase { get; set; } = true;

    public bool RequireLowercase { get; set; } = true;

    public bool RequireDigit { get; set; } = true;

    public bool RequireSymbol { get; set; } = true;

    public int PasswordHistoryLimit { get; set; } = 5;
}

public sealed class LockoutOptions
{
    public const string SectionName = "Lockout";

    public int MaxFailedAttempts { get; set; } = 5;

    public int LockoutMinutes { get; set; } = 15;
}
