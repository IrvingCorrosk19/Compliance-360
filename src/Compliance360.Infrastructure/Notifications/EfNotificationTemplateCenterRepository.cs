using Compliance360.Application.Notifications;
using Compliance360.Domain.Notifications;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Notifications;

public sealed class EfNotificationTemplateCenterRepository : INotificationTemplateCenterRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfNotificationTemplateCenterRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<NotificationTemplate?> GetTemplateAsync(Guid tenantId, Guid templateId, CancellationToken cancellationToken = default)
    {
        return _dbContext.NotificationTemplates.SingleOrDefaultAsync(
            template => template.TenantId == tenantId && template.Id == templateId,
            cancellationToken);
    }

    public Task<NotificationTemplateVersion?> GetVersionAsync(Guid tenantId, Guid versionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.NotificationTemplateVersions.SingleOrDefaultAsync(
            version => version.TenantId == tenantId && version.Id == versionId,
            cancellationToken);
    }

    public async Task<int> GetNextVersionAsync(Guid tenantId, Guid templateId, string locale, CancellationToken cancellationToken = default)
    {
        var maximum = await _dbContext.NotificationTemplateVersions
            .Where(version => version.TenantId == tenantId
                && version.NotificationTemplateId == templateId
                && version.Locale == locale)
            .Select(version => (int?)version.Version)
            .MaxAsync(cancellationToken);
        return (maximum ?? 0) + 1;
    }

    public async Task AddVersionAsync(NotificationTemplateVersion version, CancellationToken cancellationToken = default)
    {
        await _dbContext.NotificationTemplateVersions.AddAsync(version, cancellationToken);
    }

    public async Task<(IReadOnlyCollection<NotificationTemplateCenterRecord> Items, long Total)> SearchAsync(
        NotificationTemplateCenterQuery query,
        CancellationToken cancellationToken = default)
    {
        var source =
            from version in _dbContext.NotificationTemplateVersions.AsNoTracking()
            join template in _dbContext.NotificationTemplates.AsNoTracking()
                on new { version.TenantId, TemplateId = version.NotificationTemplateId }
                equals new { template.TenantId, TemplateId = template.Id }
            where version.TenantId == query.TenantId
            select new { Version = version, Template = template };

        if (query.Channel.HasValue)
        {
            source = source.Where(row => row.Template.Channel == query.Channel.Value);
        }

        if (query.Lifecycle.HasValue)
        {
            source = source.Where(row => row.Version.Lifecycle == query.Lifecycle.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Locale))
        {
            source = source.Where(row => row.Version.Locale == query.Locale);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{EscapeLikePattern(query.Search)}%";
            source = source.Where(row =>
                EF.Functions.ILike(row.Template.Code, pattern, "\\")
                || EF.Functions.ILike(row.Version.Subject, pattern, "\\"));
        }

        var total = await source.LongCountAsync(cancellationToken);
        var items = await source
            .OrderByDescending(row => row.Version.CreatedAtUtc)
            .ThenByDescending(row => row.Version.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(row => new NotificationTemplateCenterRecord(
                row.Template.Id,
                row.Template.Code,
                row.Template.Channel,
                row.Version.Id,
                row.Version.Version,
                row.Version.Locale,
                row.Version.Subject,
                row.Version.Lifecycle,
                row.Version.ReviewedByUserId.HasValue,
                row.Version.CreatedAtUtc,
                row.Version.PublishedAtUtc))
            .ToArrayAsync(cancellationToken);
        return (items, total);
    }

    public async Task RetirePublishedVersionsAsync(
        Guid tenantId,
        Guid templateId,
        string locale,
        Guid exceptVersionId,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var published = await _dbContext.NotificationTemplateVersions
            .Where(version => version.TenantId == tenantId
                && version.NotificationTemplateId == templateId
                && version.Locale == locale
                && version.Id != exceptVersionId
                && version.Lifecycle == NotificationTemplateLifecycle.Published)
            .ToArrayAsync(cancellationToken);
        foreach (var version in published)
        {
            version.Retire(nowUtc);
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string EscapeLikePattern(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}
