using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public interface IAlertRuleCenterService
{
    Task<IReadOnlyCollection<AlertEventTypeSummary>> ListEventTypesAsync(Guid tenantId, string? module, CancellationToken cancellationToken = default);
    Task<AlertDefinitionSearchResult> SearchAsync(AlertDefinitionSearchQuery query, CancellationToken cancellationToken = default);
    Task<Result<AlertDefinitionDetail>> GetAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken = default);
    Task<Result<AlertDefinitionDetail>> CreateAsync(CreateAlertDefinitionCommand command, CancellationToken cancellationToken = default);
    Task<Result<AlertDefinitionDetail>> CreateVersionAsync(CreateAlertDefinitionVersionCommand command, CancellationToken cancellationToken = default);
    Task<Result<AlertDefinitionDetail>> ApplyLifecycleActionAsync(AlertDefinitionLifecycleCommand command, CancellationToken cancellationToken = default);
    Task<Result<AlertRuleSimulationResult>> SimulateAsync(AlertRuleSimulationCommand command, CancellationToken cancellationToken = default);
}

public interface IAlertRuleCenterRepository
{
    Task<IReadOnlyCollection<AlertEventType>> ListEventTypesAsync(Guid tenantId, string? module, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<AlertDefinition> Items, int Total)> SearchAsync(AlertDefinitionSearchQuery query, CancellationToken cancellationToken);
    Task<AlertDefinition?> GetDefinitionAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken);
    Task<AlertDefinitionVersion?> GetVersionAsync(Guid tenantId, Guid definitionId, Guid versionId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AlertDefinitionVersion>> ListVersionsAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken);
    Task<int> NextVersionAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken);
    Task AddAsync(AlertDefinition definition, AlertDefinitionVersion version, CancellationToken cancellationToken);
    Task AddVersionAsync(AlertDefinitionVersion version, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record AlertEventTypeSummary(Guid Id, string Code, string Name, string Module, string SchemaJson, int SchemaVersion, bool IsActive);

public sealed record AlertDefinitionSearchQuery(
    Guid TenantId,
    string? Search,
    Guid? EventTypeId,
    AlertDefinitionLifecycle? Lifecycle,
    int Page,
    int PageSize);

public sealed record AlertDefinitionSummary(
    Guid Id,
    Guid EventTypeId,
    string Code,
    string Name,
    string Description,
    NotificationPriority Priority,
    AlertDefinitionLifecycle Lifecycle,
    Guid OwnerUserId,
    Guid? CurrentPublishedVersionId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

public sealed record AlertDefinitionVersionDetail(
    Guid Id,
    int Version,
    string ConditionJson,
    string RecipientRulesJson,
    string ChannelPoliciesJson,
    string DedupeExpression,
    int SilenceWindowMinutes,
    int? SlaMinutes,
    AlertUnknownPolicy UnknownPolicy,
    AlertDefinitionLifecycle Lifecycle,
    Guid CreatedByUserId,
    Guid? ReviewedByUserId,
    Guid? ApprovedByUserId,
    Guid? PublishedByUserId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? PublishedAtUtc);

public sealed record AlertDefinitionDetail(AlertDefinitionSummary Definition, IReadOnlyCollection<AlertDefinitionVersionDetail> Versions);

public sealed record AlertDefinitionSearchResult(IReadOnlyCollection<AlertDefinitionSummary> Items, int Total, int Page, int PageSize);

public sealed record CreateAlertDefinitionCommand(
    Guid TenantId,
    Guid EventTypeId,
    string Code,
    string Name,
    string Description,
    Guid OwnerUserId,
    NotificationPriority Priority,
    string ConditionJson,
    string RecipientRulesJson,
    string ChannelPoliciesJson,
    string DedupeExpression,
    int SilenceWindowMinutes,
    int? SlaMinutes,
    AlertUnknownPolicy UnknownPolicy,
    Guid RequestedByUserId);

public sealed record CreateAlertDefinitionVersionCommand(
    Guid TenantId,
    Guid DefinitionId,
    string ConditionJson,
    string RecipientRulesJson,
    string ChannelPoliciesJson,
    string DedupeExpression,
    int SilenceWindowMinutes,
    int? SlaMinutes,
    AlertUnknownPolicy UnknownPolicy,
    Guid RequestedByUserId);

public sealed record AlertDefinitionLifecycleCommand(
    Guid TenantId,
    Guid DefinitionId,
    Guid VersionId,
    string Action,
    Guid RequestedByUserId);

public sealed record AlertRuleSimulationCommand(
    Guid TenantId,
    Guid? DefinitionId,
    Guid? VersionId,
    string? ConditionJson,
    AlertUnknownPolicy UnknownPolicy,
    string EventPayloadJson);

public sealed record AlertRuleSimulationResult(
    AlertTruthValue RawValue,
    bool Matched,
    int NodesEvaluated,
    string Explanation,
    AlertUnknownPolicy UnknownPolicy);
