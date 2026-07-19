using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public sealed class AlertEventProcessor : IAlertEventProcessor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IAlertEventRepository _repository;
    private readonly IAlertRuleEvaluator _evaluator;
    private readonly IRecipientResolverService _recipientResolver;
    private readonly INotificationTemplateEngine _templateEngine;
    private readonly INotificationService _notificationService;
    private readonly IClock _clock;

    public AlertEventProcessor(
        IAlertEventRepository repository,
        IAlertRuleEvaluator evaluator,
        IRecipientResolverService recipientResolver,
        INotificationTemplateEngine templateEngine,
        INotificationService notificationService,
        IClock clock)
    {
        _repository = repository;
        _evaluator = evaluator;
        _recipientResolver = recipientResolver;
        _templateEngine = templateEngine;
        _notificationService = notificationService;
        _clock = clock;
    }

    public async Task<Result<int>> ProcessAsync(
        NotificationOutboxEvent outboxEvent,
        CancellationToken cancellationToken = default)
    {
        var occurrence = await _repository.GetOccurrenceAsync(
            outboxEvent.TenantId,
            outboxEvent.AggregateId,
            cancellationToken);
        if (occurrence is null)
        {
            return Result<int>.Failure("Alert occurrence was not found.");
        }

        if (occurrence.Status is AlertOccurrenceStatus.Matched
            or AlertOccurrenceStatus.Suppressed
            or AlertOccurrenceStatus.NoRecipients
            or AlertOccurrenceStatus.Queued
            or AlertOccurrenceStatus.Completed)
        {
            return Result<int>.Success(0);
        }

        try
        {
            var definition = await _repository.GetDefinitionAsync(
                occurrence.TenantId,
                occurrence.DefinitionId,
                cancellationToken);
            var version = await _repository.GetVersionAsync(
                occurrence.TenantId,
                occurrence.DefinitionId,
                occurrence.DefinitionVersionId,
                cancellationToken);
            if (definition is null || version is null || definition.Lifecycle != AlertDefinitionLifecycle.Published)
            {
                return await FailAsync(occurrence, "Published alert definition or version was not found.", cancellationToken);
            }

            var evaluation = _evaluator.Evaluate(version.ConditionJson, occurrence.PayloadJson);
            var matched = evaluation.Value switch
            {
                AlertTruthValue.True => true,
                AlertTruthValue.False => false,
                AlertTruthValue.Unknown when version.UnknownPolicy == AlertUnknownPolicy.TreatAsTrue => true,
                AlertTruthValue.Unknown when version.UnknownPolicy == AlertUnknownPolicy.FailEvaluation =>
                    throw new InvalidOperationException("Alert condition returned Unknown and policy requires failure."),
                _ => false
            };
            if (!matched)
            {
                occurrence.CompleteEvaluation(AlertOccurrenceStatus.Suppressed, _clock.UtcNow);
                await _repository.SaveChangesAsync(cancellationToken);
                return Result<int>.Success(0);
            }

            if (version.SilenceWindowMinutes > 0
                && await _repository.HasRecentOccurrenceAsync(
                    occurrence.TenantId,
                    occurrence.DefinitionVersionId,
                    occurrence.DedupeKey,
                    occurrence.Id,
                    occurrence.OccurredAtUtc.AddMinutes(-version.SilenceWindowMinutes),
                    cancellationToken))
            {
                occurrence.CompleteEvaluation(AlertOccurrenceStatus.Suppressed, _clock.UtcNow, "Suppressed by silence window.");
                await _repository.SaveChangesAsync(cancellationToken);
                return Result<int>.Success(0);
            }

            var policies = JsonSerializer.Deserialize<AlertChannelPolicy[]>(version.ChannelPoliciesJson, JsonOptions) ?? [];
            var relationships = ParseRelationships(occurrence.PayloadJson);
            relationships.TryAdd(RecipientKind.Owner, definition.OwnerUserId);
            var variables = FlattenVariables(occurrence.PayloadJson);
            var queued = 0;

            foreach (var policy in policies.Where(item => item.Enabled))
            {
                var preview = await _recipientResolver.PreviewAsync(
                    new PreviewRecipientsCommand(
                        occurrence.TenantId,
                        definition.Id,
                        version.Id,
                        definition.OwnerUserId,
                        policy.Channel,
                        definition.Code,
                        relationships),
                    cancellationToken);
                if (preview.IsFailure)
                {
                    return await FailAsync(occurrence, preview.Error ?? "Recipient resolution failed.", cancellationToken);
                }

                var template = await _repository.GetPublishedTemplateAsync(
                    occurrence.TenantId,
                    policy.TemplateCode,
                    policy.Channel,
                    policy.Locale,
                    cancellationToken);
                if (template is null)
                {
                    return await FailAsync(
                        occurrence,
                        $"Published template '{policy.TemplateCode}' for channel '{policy.Channel}' was not found.",
                        cancellationToken);
                }

                var rendered = _templateEngine.Render(
                    template.Subject,
                    template.HtmlBody,
                    template.TextBody,
                    variables,
                    null);
                foreach (var recipient in AllRecipients(preview.Value!))
                {
                    if (policy.Channel == NotificationChannel.InApp && !recipient.UserId.HasValue)
                    {
                        continue;
                    }

                    var idempotencyKey = MessageIdempotencyKey(occurrence.Id, policy.Channel, recipient);
                    if (await _repository.MessageExistsAsync(occurrence.TenantId, idempotencyKey, cancellationToken))
                    {
                        continue;
                    }

                    var queuedResult = await _notificationService.QueueAsync(
                        new QueueNotificationCommand(
                            occurrence.TenantId,
                            definition.OwnerUserId,
                            policy.Channel,
                            recipient.Address,
                            rendered.Subject,
                            rendered.HtmlBody,
                            null,
                            new Dictionary<string, string>(),
                            definition.Priority,
                            recipient.UserId,
                            occurrence.Id,
                            definition.Id,
                            version.Id,
                            idempotencyKey,
                            recipient.Routing),
                        cancellationToken);
                    if (queuedResult.IsFailure)
                    {
                        return await FailAsync(occurrence, queuedResult.Error ?? "Notification queueing failed.", cancellationToken);
                    }
                    queued++;
                }
            }

            occurrence.CompleteEvaluation(
                queued == 0 ? AlertOccurrenceStatus.NoRecipients : AlertOccurrenceStatus.Queued,
                _clock.UtcNow,
                queued == 0 ? "No eligible recipients were resolved." : null);
            await _repository.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(queued);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            return await FailAsync(occurrence, exception.Message, cancellationToken);
        }
    }

    private async Task<Result<int>> FailAsync(
        AlertOccurrence occurrence,
        string reason,
        CancellationToken cancellationToken)
    {
        occurrence.CompleteEvaluation(AlertOccurrenceStatus.Failed, _clock.UtcNow, reason);
        await _repository.SaveChangesAsync(cancellationToken);
        return Result<int>.Failure(reason);
    }

    private static Dictionary<RecipientKind, Guid?> ParseRelationships(string payloadJson)
    {
        using var document = JsonDocument.Parse(payloadJson);
        var result = new Dictionary<RecipientKind, Guid?>();
        var names = new Dictionary<RecipientKind, string[]>
        {
            [RecipientKind.Owner] = ["ownerUserId", "ownerId"],
            [RecipientKind.Creator] = ["creatorUserId", "createdByUserId"],
            [RecipientKind.Responsible] = ["responsibleUserId", "assignedUserId"],
            [RecipientKind.Reviewer] = ["reviewerUserId"],
            [RecipientKind.Approver] = ["approverUserId"],
            [RecipientKind.Submitter] = ["submitterUserId", "submittedByUserId"]
        };
        foreach (var (kind, candidates) in names)
        {
            foreach (var candidate in candidates)
            {
                if (document.RootElement.ValueKind == JsonValueKind.Object
                    && document.RootElement.TryGetProperty(candidate, out var value)
                    && value.ValueKind == JsonValueKind.String
                    && Guid.TryParse(value.GetString(), out var parsed))
                {
                    result[kind] = parsed;
                    break;
                }
            }
        }
        return result;
    }

    private static IReadOnlyDictionary<string, string> FlattenVariables(string payloadJson)
    {
        using var document = JsonDocument.Parse(payloadJson);
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Flatten(document.RootElement, string.Empty, values, 0);
        return values;
    }

    private static void Flatten(JsonElement element, string path, Dictionary<string, string> values, int depth)
    {
        if (depth > 20) return;
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                Flatten(property.Value, string.IsNullOrEmpty(path) ? property.Name : $"{path}.{property.Name}", values, depth + 1);
            }
            return;
        }

        if (!string.IsNullOrEmpty(path) && element.ValueKind is not (JsonValueKind.Object or JsonValueKind.Array))
        {
            values[path] = element.ValueKind == JsonValueKind.String ? element.GetString() ?? string.Empty : element.GetRawText();
        }
    }

    private static IEnumerable<ResolvedRecipient> AllRecipients(RecipientPreview preview) =>
        preview.To.Concat(preview.Cc).Concat(preview.Bcc);

    private static string MessageIdempotencyKey(Guid occurrenceId, NotificationChannel channel, ResolvedRecipient recipient)
    {
        var addressHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(recipient.Address.ToLowerInvariant())))[..24];
        return $"alert:{occurrenceId:N}:{channel}:{recipient.Routing}:{addressHash}";
    }
}
