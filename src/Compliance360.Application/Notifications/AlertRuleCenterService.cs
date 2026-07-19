using System.Text.Json;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public sealed class AlertRuleCenterService : IAlertRuleCenterService
{
    private readonly IAlertRuleCenterRepository _repository;
    private readonly IAlertRuleEvaluator _evaluator;
    private readonly IClock _clock;
    private readonly INotificationAuditService? _audit;

    public AlertRuleCenterService(
        IAlertRuleCenterRepository repository,
        IAlertRuleEvaluator evaluator,
        IClock clock,
        INotificationAuditService? audit = null)
    {
        _repository = repository;
        _evaluator = evaluator;
        _clock = clock;
        _audit = audit;
    }

    public async Task<IReadOnlyCollection<AlertEventTypeSummary>> ListEventTypesAsync(
        Guid tenantId,
        string? module,
        CancellationToken cancellationToken = default)
    {
        var items = await _repository.ListEventTypesAsync(tenantId, module, cancellationToken);
        return items.Select(item => new AlertEventTypeSummary(
            item.Id, item.Code, item.Name, item.Module, item.SchemaJson, item.SchemaVersion, item.IsActive)).ToArray();
    }

    public async Task<AlertDefinitionSearchResult> SearchAsync(
        AlertDefinitionSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalized = query with
        {
            Page = Math.Max(1, query.Page),
            PageSize = Math.Clamp(query.PageSize, 1, 100)
        };
        var (items, total) = await _repository.SearchAsync(normalized, cancellationToken);
        return new AlertDefinitionSearchResult(items.Select(MapSummary).ToArray(), total, normalized.Page, normalized.PageSize);
    }

    public async Task<Result<AlertDefinitionDetail>> GetAsync(
        Guid tenantId,
        Guid definitionId,
        CancellationToken cancellationToken = default)
    {
        var definition = await _repository.GetDefinitionAsync(tenantId, definitionId, cancellationToken);
        if (definition is null)
        {
            return Result<AlertDefinitionDetail>.Failure("Alert definition was not found.");
        }

        return Result<AlertDefinitionDetail>.Success(await MapDetailAsync(definition, cancellationToken));
    }

    public async Task<Result<AlertDefinitionDetail>> CreateAsync(
        CreateAlertDefinitionCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateRuleConfiguration(command.ConditionJson, command.RecipientRulesJson, command.ChannelPoliciesJson);
            var eventTypes = await _repository.ListEventTypesAsync(command.TenantId, null, cancellationToken);
            if (!eventTypes.Any(item => item.Id == command.EventTypeId && item.IsActive))
            {
                return Result<AlertDefinitionDetail>.Failure("Active alert event type was not found for this tenant.");
            }

            var definition = new AlertDefinition(
                command.TenantId,
                command.EventTypeId,
                command.Code,
                command.Name,
                command.Description,
                command.OwnerUserId,
                command.Priority);
            var version = CreateVersion(command, definition.Id, 1);
            await _repository.AddAsync(definition, version, cancellationToken);
            if (_audit is not null)
            {
                await _audit.AppendAsync(command.TenantId, command.RequestedByUserId, nameof(AlertDefinition), definition.Id, AuditAction.AlertConfigurationChanged, true, null, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);
            }
            return Result<AlertDefinitionDetail>.Success(new AlertDefinitionDetail(MapSummary(definition), [MapVersion(version)]));
        }
        catch (Exception exception) when (exception is DomainException or InvalidOperationException or JsonException)
        {
            return Result<AlertDefinitionDetail>.Failure(exception.Message);
        }
    }

    public async Task<Result<AlertDefinitionDetail>> CreateVersionAsync(
        CreateAlertDefinitionVersionCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateRuleConfiguration(command.ConditionJson, command.RecipientRulesJson, command.ChannelPoliciesJson);
            var definition = await _repository.GetDefinitionAsync(command.TenantId, command.DefinitionId, cancellationToken);
            if (definition is null)
            {
                return Result<AlertDefinitionDetail>.Failure("Alert definition was not found.");
            }

            var versionNumber = await _repository.NextVersionAsync(command.TenantId, command.DefinitionId, cancellationToken);
            var version = new AlertDefinitionVersion(
                command.TenantId,
                command.DefinitionId,
                versionNumber,
                command.ConditionJson,
                command.RecipientRulesJson,
                command.ChannelPoliciesJson,
                command.DedupeExpression,
                command.SilenceWindowMinutes,
                command.SlaMinutes,
                command.UnknownPolicy,
                command.RequestedByUserId);
            await _repository.AddVersionAsync(version, cancellationToken);
            if (_audit is not null)
            {
                await _audit.AppendAsync(command.TenantId, command.RequestedByUserId, nameof(AlertDefinitionVersion), version.Id, AuditAction.AlertConfigurationChanged, true, null, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);
            }
            return Result<AlertDefinitionDetail>.Success(await MapDetailAsync(definition, cancellationToken));
        }
        catch (Exception exception) when (exception is DomainException or InvalidOperationException or JsonException)
        {
            return Result<AlertDefinitionDetail>.Failure(exception.Message);
        }
    }

    public async Task<Result<AlertDefinitionDetail>> ApplyLifecycleActionAsync(
        AlertDefinitionLifecycleCommand command,
        CancellationToken cancellationToken = default)
    {
        var definition = await _repository.GetDefinitionAsync(command.TenantId, command.DefinitionId, cancellationToken);
        if (definition is null)
        {
            return Result<AlertDefinitionDetail>.Failure("Alert definition was not found.");
        }

        var version = await _repository.GetVersionAsync(command.TenantId, command.DefinitionId, command.VersionId, cancellationToken);
        if (version is null)
        {
            return Result<AlertDefinitionDetail>.Failure("Alert definition version was not found.");
        }

        try
        {
            var now = _clock.UtcNow;
            switch (command.Action.Trim().ToLowerInvariant())
            {
                case "submit":
                    version.SubmitForReview(now);
                    break;
                case "review":
                    version.Review(command.RequestedByUserId, now);
                    break;
                case "approve":
                    version.Approve(command.RequestedByUserId, now);
                    break;
                case "publish":
                    EnsurePublishable(version);
                    version.Publish(command.RequestedByUserId, now);
                    definition.SetPublishedVersion(version.Id, now);
                    break;
                case "disable":
                    definition.Disable(now);
                    break;
                case "enable":
                    definition.Enable(now);
                    break;
                default:
                    return Result<AlertDefinitionDetail>.Failure("Unsupported alert rule lifecycle action.");
            }

            await _repository.SaveChangesAsync(cancellationToken);
            if (_audit is not null)
            {
                await _audit.AppendAsync(command.TenantId, command.RequestedByUserId, nameof(AlertDefinitionVersion), version.Id, AuditAction.AlertConfigurationChanged, true, command.Action, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);
            }
            return Result<AlertDefinitionDetail>.Success(await MapDetailAsync(definition, cancellationToken));
        }
        catch (DomainException exception)
        {
            return Result<AlertDefinitionDetail>.Failure(exception.Message);
        }
    }

    public async Task<Result<AlertRuleSimulationResult>> SimulateAsync(
        AlertRuleSimulationCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var conditionJson = command.ConditionJson;
            var unknownPolicy = command.UnknownPolicy;
            if (command.VersionId.HasValue)
            {
                if (!command.DefinitionId.HasValue)
                {
                    return Result<AlertRuleSimulationResult>.Failure("DefinitionId is required when simulating a stored version.");
                }

                var version = await _repository.GetVersionAsync(
                    command.TenantId,
                    command.DefinitionId.Value,
                    command.VersionId.Value,
                    cancellationToken);
                if (version is null)
                {
                    return Result<AlertRuleSimulationResult>.Failure("Alert definition version was not found.");
                }

                conditionJson = version.ConditionJson;
                unknownPolicy = version.UnknownPolicy;
            }

            if (string.IsNullOrWhiteSpace(conditionJson))
            {
                return Result<AlertRuleSimulationResult>.Failure("Condition JSON is required.");
            }

            var evaluation = _evaluator.Evaluate(conditionJson, command.EventPayloadJson);
            var matched = evaluation.Value switch
            {
                AlertTruthValue.True => true,
                AlertTruthValue.False => false,
                AlertTruthValue.Unknown when unknownPolicy == AlertUnknownPolicy.TreatAsTrue => true,
                AlertTruthValue.Unknown when unknownPolicy == AlertUnknownPolicy.FailEvaluation =>
                    throw new InvalidOperationException("Rule evaluation returned Unknown and policy requires failure."),
                _ => false
            };
            return Result<AlertRuleSimulationResult>.Success(new AlertRuleSimulationResult(
                evaluation.Value,
                matched,
                evaluation.NodesEvaluated,
                evaluation.Explanation,
                unknownPolicy));
        }
        catch (Exception exception) when (exception is InvalidOperationException or JsonException)
        {
            return Result<AlertRuleSimulationResult>.Failure(exception.Message);
        }
    }

    private AlertDefinitionVersion CreateVersion(CreateAlertDefinitionCommand command, Guid definitionId, int version)
    {
        return new AlertDefinitionVersion(
            command.TenantId,
            definitionId,
            version,
            command.ConditionJson,
            command.RecipientRulesJson,
            command.ChannelPoliciesJson,
            command.DedupeExpression,
            command.SilenceWindowMinutes,
            command.SlaMinutes,
            command.UnknownPolicy,
            command.RequestedByUserId);
    }

    private void ValidateRuleConfiguration(string conditionJson, string recipientRulesJson, string channelPoliciesJson)
    {
        _evaluator.Evaluate(conditionJson, "{}");
        EnsureJsonArray(recipientRulesJson, "Recipient rules");
        EnsureJsonArray(channelPoliciesJson, "Channel policies");
    }

    private static void EnsurePublishable(AlertDefinitionVersion version)
    {
        EnsureNonEmptyJsonArray(version.RecipientRulesJson, "Recipient rules");
        EnsureNonEmptyJsonArray(version.ChannelPoliciesJson, "Channel policies");
    }

    private static void EnsureJsonArray(string json, string label)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException($"{label} must be a JSON array.");
        }
    }

    private static void EnsureNonEmptyJsonArray(string json, string label)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array || document.RootElement.GetArrayLength() == 0)
        {
            throw new DomainException($"{label} must contain at least one item before publication.");
        }
    }

    private async Task<AlertDefinitionDetail> MapDetailAsync(AlertDefinition definition, CancellationToken cancellationToken)
    {
        var versions = await _repository.ListVersionsAsync(definition.TenantId, definition.Id, cancellationToken);
        return new AlertDefinitionDetail(MapSummary(definition), versions.Select(MapVersion).ToArray());
    }

    private static AlertDefinitionSummary MapSummary(AlertDefinition definition) =>
        new(
            definition.Id,
            definition.EventTypeId,
            definition.Code,
            definition.Name,
            definition.Description,
            definition.Priority,
            definition.Lifecycle,
            definition.OwnerUserId,
            definition.CurrentPublishedVersionId,
            definition.CreatedAtUtc,
            definition.UpdatedAtUtc);

    private static AlertDefinitionVersionDetail MapVersion(AlertDefinitionVersion version) =>
        new(
            version.Id,
            version.Version,
            version.ConditionJson,
            version.RecipientRulesJson,
            version.ChannelPoliciesJson,
            version.DedupeExpression,
            version.SilenceWindowMinutes,
            version.SlaMinutes,
            version.UnknownPolicy,
            version.Lifecycle,
            version.CreatedByUserId,
            version.ReviewedByUserId,
            version.ApprovedByUserId,
            version.PublishedByUserId,
            version.CreatedAtUtc,
            version.PublishedAtUtc);
}
