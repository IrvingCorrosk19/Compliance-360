using Compliance360.Domain.Audit;
using Compliance360.Shared;
using Microsoft.Extensions.Options;

namespace Compliance360.Application.Audit;

public sealed class AuditService : IAuditService
{
    private readonly IAuditRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditPermissionEvaluator _permissionEvaluator;
    private readonly IClock _clock;
    private readonly AuditOptions _options;

    public AuditService(
        IAuditRepository repository,
        IApplicationDbContext dbContext,
        IAuditPermissionEvaluator permissionEvaluator,
        IClock clock,
        IOptions<AuditOptions> options)
    {
        _repository = repository;
        _dbContext = dbContext;
        _permissionEvaluator = permissionEvaluator;
        _clock = clock;
        _options = options.Value;
    }

    public async Task<Result<Guid>> RecordAsync(RecordAuditCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.AuditEvent.EntityName))
        {
            return Result<Guid>.Failure("Audit entity name is required.");
        }

        var auditLog = AuditLog.FromEvent(command.AuditEvent, _clock.UtcNow);
        await _repository.AddAsync(auditLog, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(auditLog.Id);
    }

    public async Task<Result<AuditSearchResult>> SearchAsync(AuditSearchQuery query, CancellationToken cancellationToken = default)
    {
        if (!_permissionEvaluator.CanReadAudit(query.Principal))
        {
            return Result<AuditSearchResult>.Failure("User is not authorized to read audit logs.");
        }

        if (query.Principal.TenantId != query.TenantId)
        {
            return Result<AuditSearchResult>.Failure("Audit queries must be scoped to the current tenant.");
        }

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, _options.MaxPageSize);
        var criteria = new AuditSearchCriteria(
            query.TenantId,
            query.Action,
            query.Category,
            query.EntityName,
            query.EntityId,
            query.SearchText,
            query.FromUtc,
            query.ToUtc,
            page,
            pageSize);

        return Result<AuditSearchResult>.Success(await _repository.SearchAsync(criteria, cancellationToken));
    }

    public async Task<Result<int>> ApplyRetentionAsync(AuditRetentionCommand command, CancellationToken cancellationToken = default)
    {
        if (!_permissionEvaluator.CanReadAudit(command.Principal))
        {
            return Result<int>.Failure("User is not authorized to evaluate audit retention.");
        }

        if (command.Principal.TenantId != command.TenantId)
        {
            return Result<int>.Failure("Audit retention must be scoped to the current tenant.");
        }

        var retentionDays = command.RetentionDays > 0 ? command.RetentionDays : _options.DefaultRetentionDays;
        var olderThanUtc = _clock.UtcNow.AddDays(-retentionDays);
        return Result<int>.Success(await _repository.CountOlderThanAsync(command.TenantId, olderThanUtc, cancellationToken));
    }
}

public sealed class AuditPermissionEvaluator : IAuditPermissionEvaluator
{
    private readonly AuditOptions _options;

    public AuditPermissionEvaluator(IOptions<AuditOptions> options)
    {
        _options = options.Value;
    }

    public bool CanReadAudit(AuditQueryPrincipal principal)
    {
        return principal.Permissions.Contains(_options.ReadPermission, StringComparer.OrdinalIgnoreCase)
            || principal.Permissions.Contains("AUDIT.MANAGE", StringComparer.OrdinalIgnoreCase);
    }
}

public sealed class AuditContextAccessor : IAuditContextAccessor
{
    private static readonly AsyncLocal<AuditContext?> CurrentContext = new();

    public AuditContext Current => CurrentContext.Value ?? new AuditContext(null, null, null, null, null, null, null, null, null);

    public void Set(AuditContext context)
    {
        CurrentContext.Value = context;
    }
}
