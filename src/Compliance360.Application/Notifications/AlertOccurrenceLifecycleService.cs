using Compliance360.Application.Audit;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public interface IAlertOccurrenceLifecycleService
{
    Task<Result<AlertOccurrenceLifecycleSummary>> AcknowledgeAsync(
        AlertOccurrenceLifecycleCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<AlertOccurrenceLifecycleSummary>> ResolveAsync(
        AlertOccurrenceLifecycleCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<AlertOccurrenceLifecycleSummary>> EscalateAsync(
        AlertOccurrenceLifecycleCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<AlertOccurrenceLifecycleSummary>> GetAsync(
        Guid tenantId,
        Guid occurrenceId,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<AlertOccurrenceLifecycleSummary>>> ListAsync(
        Guid tenantId,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}

public sealed record AlertOccurrenceLifecycleCommand(
    Guid TenantId,
    Guid OccurrenceId,
    Guid RequestedByUserId,
    string? Notes);

public sealed record AlertOccurrenceLifecycleSummary(
    Guid Id,
    Guid DefinitionId,
    Guid DefinitionVersionId,
    AlertOccurrenceStatus Status,
    string DedupeKey,
    string CorrelationId,
    string SourceModule,
    string EntityType,
    Guid? EntityId,
    DateTimeOffset OccurredAtUtc,
    DateTimeOffset? EvaluatedAtUtc,
    string? Notes);

public sealed class AlertOccurrenceLifecycleService : IAlertOccurrenceLifecycleService
{
    private readonly IAlertEventRepository _occurrences;
    private readonly IApplicationDbContext _db;
    private readonly IAuditRepository _audit;
    private readonly IClock _clock;

    public AlertOccurrenceLifecycleService(
        IAlertEventRepository occurrences,
        IApplicationDbContext db,
        IAuditRepository audit,
        IClock clock)
    {
        _occurrences = occurrences;
        _db = db;
        _audit = audit;
        _clock = clock;
    }

    public async Task<Result<AlertOccurrenceLifecycleSummary>> GetAsync(
        Guid tenantId,
        Guid occurrenceId,
        CancellationToken cancellationToken = default)
    {
        var occurrence = await _occurrences.GetOccurrenceAsync(tenantId, occurrenceId, cancellationToken);
        return occurrence is null
            ? Result<AlertOccurrenceLifecycleSummary>.Failure("Alert occurrence was not found.")
            : Result<AlertOccurrenceLifecycleSummary>.Success(Map(occurrence));
    }

    public async Task<Result<IReadOnlyCollection<AlertOccurrenceLifecycleSummary>>> ListAsync(
        Guid tenantId,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var items = await _occurrences.ListOccurrencesAsync(tenantId, page, pageSize, cancellationToken);
        return Result<IReadOnlyCollection<AlertOccurrenceLifecycleSummary>>.Success(items.Select(Map).ToArray());
    }

    public Task<Result<AlertOccurrenceLifecycleSummary>> AcknowledgeAsync(
        AlertOccurrenceLifecycleCommand command,
        CancellationToken cancellationToken = default) =>
        ApplyAsync(command, (occurrence, now) => occurrence.Acknowledge(command.RequestedByUserId, now, command.Notes), cancellationToken);

    public Task<Result<AlertOccurrenceLifecycleSummary>> ResolveAsync(
        AlertOccurrenceLifecycleCommand command,
        CancellationToken cancellationToken = default) =>
        ApplyAsync(command, (occurrence, now) => occurrence.Resolve(command.RequestedByUserId, now, command.Notes), cancellationToken);

    public Task<Result<AlertOccurrenceLifecycleSummary>> EscalateAsync(
        AlertOccurrenceLifecycleCommand command,
        CancellationToken cancellationToken = default) =>
        ApplyAsync(command, (occurrence, now) => occurrence.Escalate(command.RequestedByUserId, now, command.Notes), cancellationToken);

    private async Task<Result<AlertOccurrenceLifecycleSummary>> ApplyAsync(
        AlertOccurrenceLifecycleCommand command,
        Action<AlertOccurrence, DateTimeOffset> mutate,
        CancellationToken cancellationToken)
    {
        var occurrence = await _occurrences.GetOccurrenceAsync(command.TenantId, command.OccurrenceId, cancellationToken);
        if (occurrence is null)
        {
            return Result<AlertOccurrenceLifecycleSummary>.Failure("Alert occurrence was not found.");
        }

        try
        {
            var now = _clock.UtcNow;
            mutate(occurrence, now);
            await _audit.AddAsync(AuditLog.Create(
                command.TenantId,
                command.RequestedByUserId,
                nameof(AlertOccurrence),
                occurrence.Id,
                AuditAction.AlertConfigurationChanged,
                now), cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return Result<AlertOccurrenceLifecycleSummary>.Success(Map(occurrence));
        }
        catch (DomainException ex)
        {
            return Result<AlertOccurrenceLifecycleSummary>.Failure(ex.Message);
        }
    }

    private static AlertOccurrenceLifecycleSummary Map(AlertOccurrence occurrence) =>
        new(
            occurrence.Id,
            occurrence.DefinitionId,
            occurrence.DefinitionVersionId,
            occurrence.Status,
            occurrence.DedupeKey,
            occurrence.CorrelationId,
            occurrence.SourceModule,
            occurrence.EntityType,
            occurrence.EntityId,
            occurrence.OccurredAtUtc,
            occurrence.EvaluatedAtUtc,
            occurrence.FailureReason);
}
