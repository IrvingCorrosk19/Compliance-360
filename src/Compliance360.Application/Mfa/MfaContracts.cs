using Compliance360.Domain.Audit;
using Compliance360.Domain.Identity;
using Compliance360.Shared;

namespace Compliance360.Application.Mfa;

public interface IMfaService
{
    Task<Result<MfaSetupResult>> BeginSetupAsync(BeginMfaSetupCommand command, CancellationToken cancellationToken = default);

    Task<Result> EnableAsync(EnableMfaCommand command, CancellationToken cancellationToken = default);

    Task<Result> VerifyAsync(VerifyMfaCommand command, CancellationToken cancellationToken = default);

    Task<Result> DisableAsync(DisableMfaCommand command, CancellationToken cancellationToken = default);
}

public interface IMfaRepository
{
    Task<User?> GetUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<MfaConfiguration?> GetConfigurationAsync(Guid tenantId, Guid userId, MfaMethod method, CancellationToken cancellationToken = default);

    Task AddConfigurationAsync(MfaConfiguration configuration, CancellationToken cancellationToken = default);

    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

public interface IMfaSecretProtector
{
    string Protect(string secret);

    string Unprotect(string encryptedSecret);
}

public interface ITotpService
{
    string GenerateSecret();

    string GenerateCode(string secret, DateTimeOffset timestampUtc);

    bool VerifyCode(string secret, string code, DateTimeOffset timestampUtc, int allowedDriftSteps = 1);
}

public sealed record BeginMfaSetupCommand(Guid TenantId, Guid UserId, MfaMethod Method, Guid RequestedByUserId);

public sealed record EnableMfaCommand(Guid TenantId, Guid UserId, MfaMethod Method, string VerificationCode, Guid RequestedByUserId);

public sealed record VerifyMfaCommand(Guid TenantId, Guid UserId, MfaMethod Method, string VerificationCode, string? IpAddress, string? UserAgent);

public sealed record DisableMfaCommand(Guid TenantId, Guid UserId, MfaMethod Method, Guid RequestedByUserId);

public sealed record MfaSetupResult(Guid UserId, Guid TenantId, MfaMethod Method, string SharedSecret, string ManualEntryKey);
