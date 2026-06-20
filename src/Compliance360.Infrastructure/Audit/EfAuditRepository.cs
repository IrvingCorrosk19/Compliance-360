using Compliance360.Application.Audit;
using Compliance360.Domain.Audit;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Audit;

public sealed class EfAuditRepository : IAuditRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfAuditRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }

    public async Task<AuditSearchResult> SearchAsync(AuditSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AuditLogs
            .AsNoTracking()
            .Where(auditLog => auditLog.TenantId == criteria.TenantId);

        if (criteria.Action.HasValue)
        {
            query = query.Where(auditLog => auditLog.Action == criteria.Action.Value);
        }

        if (criteria.Category.HasValue)
        {
            query = query.Where(auditLog => auditLog.Category == criteria.Category.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.EntityName))
        {
            query = query.Where(auditLog => auditLog.EntityName == criteria.EntityName);
        }

        if (criteria.EntityId.HasValue)
        {
            query = query.Where(auditLog => auditLog.EntityId == criteria.EntityId.Value);
        }

        if (criteria.FromUtc.HasValue)
        {
            query = query.Where(auditLog => auditLog.OccurredAtUtc >= criteria.FromUtc.Value);
        }

        if (criteria.ToUtc.HasValue)
        {
            query = query.Where(auditLog => auditLog.OccurredAtUtc <= criteria.ToUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            query = query.Where(auditLog =>
                auditLog.EntityName.Contains(criteria.SearchText)
                || (auditLog.UserName != null && auditLog.UserName.Contains(criteria.SearchText))
                || (auditLog.ErrorMessage != null && auditLog.ErrorMessage.Contains(criteria.SearchText)));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(auditLog => auditLog.OccurredAtUtc)
            .ThenByDescending(auditLog => auditLog.Id)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(auditLog => new AuditLogDto(
                auditLog.Id,
                auditLog.TenantId,
                auditLog.UserId,
                auditLog.UserName,
                auditLog.Role,
                auditLog.EntityName,
                auditLog.EntityId,
                auditLog.Action,
                auditLog.Category,
                auditLog.OccurredAtUtc,
                auditLog.IpAddress,
                auditLog.UserAgent,
                auditLog.CorrelationId,
                auditLog.RequestId,
                auditLog.SessionId,
                auditLog.Success,
                auditLog.ErrorMessage))
            .ToListAsync(cancellationToken);

        return new AuditSearchResult(items, total, criteria.Page, criteria.PageSize);
    }

    public Task<int> CountOlderThanAsync(Guid tenantId, DateTimeOffset olderThanUtc, CancellationToken cancellationToken = default)
    {
        return _dbContext.AuditLogs.CountAsync(
            auditLog => auditLog.TenantId == tenantId && auditLog.OccurredAtUtc < olderThanUtc,
            cancellationToken);
    }
}
