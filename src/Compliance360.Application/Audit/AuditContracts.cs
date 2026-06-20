using Compliance360.Domain.Audit;
using Compliance360.Shared;

namespace Compliance360.Application.Audit;

public interface IAuditService
{
    Task<Result<Guid>> RecordAsync(RecordAuditCommand command, CancellationToken cancellationToken = default);

    Task<Result<AuditSearchResult>> SearchAsync(AuditSearchQuery query, CancellationToken cancellationToken = default);

    Task<Result<int>> ApplyRetentionAsync(AuditRetentionCommand command, CancellationToken cancellationToken = default);
}

public interface IAuditRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    Task<AuditSearchResult> SearchAsync(AuditSearchCriteria criteria, CancellationToken cancellationToken = default);

    Task<int> CountOlderThanAsync(Guid tenantId, DateTimeOffset olderThanUtc, CancellationToken cancellationToken = default);
}

public interface IAuditContextAccessor
{
    AuditContext Current { get; }

    void Set(AuditContext context);
}

public interface IAuditPermissionEvaluator
{
    bool CanReadAudit(AuditQueryPrincipal principal);
}

public sealed record RecordAuditCommand(AuditEvent AuditEvent);

public sealed record AuditSearchQuery(
    Guid TenantId,
    AuditQueryPrincipal Principal,
    AuditAction? Action,
    AuditCategory? Category,
    string? EntityName,
    Guid? EntityId,
    string? SearchText,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    int Page,
    int PageSize);

public sealed record AuditSearchCriteria(
    Guid TenantId,
    AuditAction? Action,
    AuditCategory? Category,
    string? EntityName,
    Guid? EntityId,
    string? SearchText,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    int Page,
    int PageSize);

public sealed record AuditSearchResult(
    IReadOnlyCollection<AuditLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record AuditLogDto(
    Guid Id,
    Guid? TenantId,
    Guid? UserId,
    string? UserName,
    string? Role,
    string EntityName,
    Guid? EntityId,
    AuditAction Action,
    AuditCategory Category,
    DateTimeOffset TimestampUtc,
    string? IpAddress,
    string? UserAgent,
    string? CorrelationId,
    string? RequestId,
    Guid? SessionId,
    bool Success,
    string? ErrorMessage);

public sealed record AuditQueryPrincipal(
    Guid UserId,
    Guid TenantId,
    IReadOnlyCollection<string> Permissions);

public sealed record AuditRetentionCommand(
    Guid TenantId,
    int RetentionDays,
    AuditQueryPrincipal Principal);

public sealed class AuditOptions
{
    public const string SectionName = "Audit";

    public int DefaultRetentionDays { get; set; } = 2_555;

    public int MaxPageSize { get; set; } = 200;

    public string ReadPermission { get; set; } = "AUDIT.READ";
}
