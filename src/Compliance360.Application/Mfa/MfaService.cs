using Compliance360.Domain.Audit;
using Compliance360.Domain.Identity;
using Compliance360.Shared;

namespace Compliance360.Application.Mfa;

public sealed class MfaService : IMfaService
{
    private readonly IMfaRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMfaSecretProtector _secretProtector;
    private readonly ITotpService _totpService;
    private readonly IClock _clock;

    public MfaService(
        IMfaRepository repository,
        IApplicationDbContext dbContext,
        IMfaSecretProtector secretProtector,
        ITotpService totpService,
        IClock clock)
    {
        _repository = repository;
        _dbContext = dbContext;
        _secretProtector = secretProtector;
        _totpService = totpService;
        _clock = clock;
    }

    public async Task<Result<MfaSetupResult>> BeginSetupAsync(BeginMfaSetupCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetUserAsync(command.TenantId, command.UserId, cancellationToken);
        if (user is null)
        {
            return Result<MfaSetupResult>.Failure("User not found in tenant.");
        }

        if (await _repository.GetConfigurationAsync(command.TenantId, command.UserId, command.Method, cancellationToken) is not null)
        {
            return Result<MfaSetupResult>.Failure("MFA method is already configured.");
        }

        var secret = _totpService.GenerateSecret();
        var configuration = new MfaConfiguration(command.TenantId, command.UserId, command.Method, _secretProtector.Protect(secret), _clock.UtcNow);
        configuration.Disable();
        await _repository.AddConfigurationAsync(configuration, cancellationToken);
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, command.UserId, AuditAction.MfaConfigured, true, null, null, null, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<MfaSetupResult>.Success(new MfaSetupResult(command.UserId, command.TenantId, command.Method, secret, secret));
    }

    public async Task<Result> EnableAsync(EnableMfaCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetUserAsync(command.TenantId, command.UserId, cancellationToken);
        var configuration = await _repository.GetConfigurationAsync(command.TenantId, command.UserId, command.Method, cancellationToken);
        if (user is null || configuration is null)
        {
            return Result.Failure("MFA configuration not found.");
        }

        var secret = _secretProtector.Unprotect(configuration.SecretEncrypted);
        if (!_totpService.VerifyCode(secret, command.VerificationCode, _clock.UtcNow))
        {
            configuration.RegisterFailedVerification();
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, command.UserId, AuditAction.SecurityEvent, false, "Invalid MFA verification code.", null, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Failure("Invalid MFA verification code.");
        }

        user.EnableMfa(configuration.SecretEncrypted);
        configuration.Enable();
        configuration.RegisterSuccessfulVerification(_clock.UtcNow);
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, command.UserId, AuditAction.MfaConfigured, true, null, null, null, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> VerifyAsync(VerifyMfaCommand command, CancellationToken cancellationToken = default)
    {
        var configuration = await _repository.GetConfigurationAsync(command.TenantId, command.UserId, command.Method, cancellationToken);
        if (configuration is null || !configuration.IsEnabled)
        {
            return Result.Failure("MFA is not enabled.");
        }

        var secret = _secretProtector.Unprotect(configuration.SecretEncrypted);
        if (!_totpService.VerifyCode(secret, command.VerificationCode, _clock.UtcNow))
        {
            configuration.RegisterFailedVerification();
            await AppendAuditAsync(command.TenantId, command.UserId, command.UserId, AuditAction.SecurityEvent, false, "Invalid MFA challenge.", command.IpAddress, command.UserAgent, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Failure("Invalid MFA challenge.");
        }

        configuration.RegisterSuccessfulVerification(_clock.UtcNow);
        await AppendAuditAsync(command.TenantId, command.UserId, command.UserId, AuditAction.MfaConfigured, true, null, command.IpAddress, command.UserAgent, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DisableAsync(DisableMfaCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetUserAsync(command.TenantId, command.UserId, cancellationToken);
        var configuration = await _repository.GetConfigurationAsync(command.TenantId, command.UserId, command.Method, cancellationToken);
        if (user is null || configuration is null)
        {
            return Result.Failure("MFA configuration not found.");
        }

        configuration.Disable();
        user.DisableMfa();
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, command.UserId, AuditAction.SecurityEvent, true, null, null, null, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task AppendAuditAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid targetUserId,
        AuditAction action,
        bool success,
        string? errorMessage,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var auditLog = AuditLog.FromEvent(
            new AuditEvent(
                nameof(MfaConfiguration),
                targetUserId,
                action,
                AuditCategory.Security,
                new AuditContext(tenantId, actorUserId, null, null, ipAddress, userAgent, null, null, null),
                new AuditSnapshot(null, null),
                new AuditMetadata("{\"source\":\"mfa\"}"),
                success,
                errorMessage),
            _clock.UtcNow);

        await _repository.AddAuditLogAsync(auditLog, cancellationToken);
    }
}
