using Compliance360.Application.Notifications;
using Compliance360.Domain.Notifications;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Notifications;

public sealed class EfAlertEventRepository : IAlertEventRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfAlertEventRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AlertOccurrence?> GetOccurrenceAsync(Guid tenantId, Guid occurrenceId, CancellationToken cancellationToken) =>
        _dbContext.AlertOccurrences.SingleOrDefaultAsync(
            item => item.TenantId == tenantId && item.Id == occurrenceId,
            cancellationToken);

    public Task<AlertDefinition?> GetDefinitionAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken) =>
        _dbContext.AlertDefinitions.AsNoTracking().SingleOrDefaultAsync(
            item => item.TenantId == tenantId && item.Id == definitionId,
            cancellationToken);

    public Task<AlertDefinitionVersion?> GetVersionAsync(
        Guid tenantId,
        Guid definitionId,
        Guid versionId,
        CancellationToken cancellationToken) =>
        _dbContext.AlertDefinitionVersions.AsNoTracking().SingleOrDefaultAsync(
            item => item.TenantId == tenantId && item.DefinitionId == definitionId && item.Id == versionId,
            cancellationToken);

    public Task<NotificationTemplateVersion?> GetPublishedTemplateAsync(
        Guid tenantId,
        string code,
        NotificationChannel channel,
        string? locale,
        CancellationToken cancellationToken)
    {
        var query =
            from version in _dbContext.NotificationTemplateVersions.AsNoTracking()
            join template in _dbContext.NotificationTemplates.AsNoTracking()
                on new { version.TenantId, Id = version.NotificationTemplateId }
                equals new { template.TenantId, template.Id }
            where version.TenantId == tenantId
                && template.Code == code.ToUpper()
                && template.Channel == channel
                && version.Lifecycle == NotificationTemplateLifecycle.Published
                && (locale == null || version.Locale == locale)
            orderby version.Version descending
            select version;
        return query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> HasRecentOccurrenceAsync(
        Guid tenantId,
        Guid definitionVersionId,
        string dedupeKey,
        Guid excludedOccurrenceId,
        DateTimeOffset sinceUtc,
        CancellationToken cancellationToken) =>
        _dbContext.AlertOccurrences.AsNoTracking().AnyAsync(
            item => item.TenantId == tenantId
                && item.DefinitionVersionId == definitionVersionId
                && item.DedupeKey == dedupeKey
                && item.Id != excludedOccurrenceId
                && item.OccurredAtUtc >= sinceUtc
                && item.Status != AlertOccurrenceStatus.Failed,
            cancellationToken);

    public Task<bool> MessageExistsAsync(Guid tenantId, string idempotencyKey, CancellationToken cancellationToken) =>
        _dbContext.NotificationMessages.AsNoTracking().AnyAsync(
            item => item.TenantId == tenantId && item.IdempotencyKey == idempotencyKey,
            cancellationToken);

    public Task<AlertEventType?> GetEventTypeAsync(
        Guid tenantId,
        string eventCode,
        CancellationToken cancellationToken) =>
        _dbContext.AlertEventTypes.AsNoTracking().SingleOrDefaultAsync(
            item => item.TenantId == tenantId
                && item.Code == eventCode.ToLower()
                && item.IsActive,
            cancellationToken);

    public async Task<IReadOnlyCollection<(AlertDefinition Definition, AlertDefinitionVersion Version)>> ListPublishedRulesAsync(
        Guid tenantId,
        Guid eventTypeId,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from definition in _dbContext.AlertDefinitions.AsNoTracking()
            join version in _dbContext.AlertDefinitionVersions.AsNoTracking()
                on new { definition.TenantId, Id = definition.CurrentPublishedVersionId }
                equals new { version.TenantId, Id = (Guid?)version.Id }
            where definition.TenantId == tenantId
                && definition.EventTypeId == eventTypeId
                && definition.Lifecycle == AlertDefinitionLifecycle.Published
                && version.Lifecycle == AlertDefinitionLifecycle.Published
            select new { Definition = definition, Version = version })
            .ToArrayAsync(cancellationToken);
        return rows.Select(item => (item.Definition, item.Version)).ToArray();
    }

    public async Task AddOccurrencesAsync(
        IReadOnlyCollection<AlertOccurrence> occurrences,
        IReadOnlyCollection<NotificationOutboxEvent> outboxEvents,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _dbContext.AlertOccurrences.AddRangeAsync(occurrences, cancellationToken);
        await _dbContext.NotificationOutbox.AddRangeAsync(outboxEvents, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task CompleteOccurrenceIfTerminalAsync(
        Guid tenantId,
        Guid occurrenceId,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken)
    {
        var occurrence = await _dbContext.AlertOccurrences.SingleOrDefaultAsync(
            item => item.TenantId == tenantId && item.Id == occurrenceId,
            cancellationToken);
        if (occurrence is null || occurrence.Status != AlertOccurrenceStatus.Queued)
        {
            return;
        }

        var statuses = await _dbContext.NotificationMessages.AsNoTracking()
            .Where(item => item.TenantId == tenantId && item.AlertOccurrenceId == occurrenceId)
            .Select(item => item.Status)
            .ToArrayAsync(cancellationToken);
        if (statuses.Length == 0 || statuses.Any(item => item is NotificationStatus.Queued or NotificationStatus.Processing or NotificationStatus.Retried))
        {
            return;
        }

        occurrence.CompleteEvaluation(
            statuses.Any(item => item is NotificationStatus.Failed or NotificationStatus.DeadLetter)
                ? AlertOccurrenceStatus.Failed
                : AlertOccurrenceStatus.Completed,
            completedAtUtc,
            statuses.Any(item => item is NotificationStatus.Failed or NotificationStatus.DeadLetter)
                ? "One or more notification messages failed terminally."
                : null);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
