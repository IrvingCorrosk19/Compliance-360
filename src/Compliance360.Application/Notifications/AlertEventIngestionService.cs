using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Compliance360.Domain.Common;
using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public sealed partial class AlertEventIngestionService : IAlertEventIngestionService
{
    private readonly IAlertEventRepository _repository;
    private readonly IClock _clock;

    public AlertEventIngestionService(IAlertEventRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<Result<AlertEventIngestionResult>> IngestAsync(
        IngestAlertEventCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var payload = JsonDocument.Parse(command.PayloadJson);
            if (payload.RootElement.ValueKind != JsonValueKind.Object)
            {
                return Result<AlertEventIngestionResult>.Failure("Alert event payload must be a JSON object.");
            }

            var eventType = await _repository.GetEventTypeAsync(command.TenantId, command.EventCode, cancellationToken);
            if (eventType is null)
            {
                return Result<AlertEventIngestionResult>.Failure("Active alert event type was not found for this tenant.");
            }

            var rules = await _repository.ListPublishedRulesAsync(command.TenantId, eventType.Id, cancellationToken);
            var occurredAt = command.OccurredAtUtc ?? _clock.UtcNow;
            var occurrences = new List<AlertOccurrence>(rules.Count);
            var outboxEvents = new List<NotificationOutboxEvent>(rules.Count);
            foreach (var (definition, version) in rules)
            {
                var dedupeKey = ResolveDedupeKey(
                    version.DedupeExpression,
                    eventType.Code,
                    command.EntityId,
                    command.CorrelationId,
                    payload.RootElement);
                var occurrence = new AlertOccurrence(
                    command.TenantId,
                    definition.Id,
                    version.Id,
                    eventType.Id,
                    dedupeKey,
                    command.PayloadJson,
                    command.CorrelationId,
                    command.SourceModule,
                    command.EntityType,
                    command.EntityId,
                    occurredAt);
                occurrences.Add(occurrence);
                outboxEvents.Add(new NotificationOutboxEvent(
                    command.TenantId,
                    "AlertOccurrenceCreated",
                    nameof(AlertOccurrence),
                    occurrence.Id,
                    command.PayloadJson,
                    command.CorrelationId,
                    occurredAt));
            }

            if (occurrences.Count > 0)
            {
                await _repository.AddOccurrencesAsync(occurrences, outboxEvents, cancellationToken);
            }

            return Result<AlertEventIngestionResult>.Success(new AlertEventIngestionResult(
                eventType.Id,
                occurrences.Count,
                occurrences.Select(item => item.Id).ToArray()));
        }
        catch (Exception exception) when (exception is JsonException or DomainException or InvalidOperationException)
        {
            return Result<AlertEventIngestionResult>.Failure(exception.Message);
        }
    }

    private static string ResolveDedupeKey(
        string expression,
        string eventCode,
        Guid? entityId,
        string correlationId,
        JsonElement payload)
    {
        var resolved = TokenRegex().Replace(expression, match =>
        {
            var path = match.Groups[1].Value.Trim();
            if (path.Equals("eventType", StringComparison.OrdinalIgnoreCase)) return eventCode;
            if (path.Equals("entityId", StringComparison.OrdinalIgnoreCase)) return entityId?.ToString("N") ?? "none";
            if (path.Equals("correlationId", StringComparison.OrdinalIgnoreCase)) return correlationId;
            return ResolvePath(payload, path) ?? "unknown";
        });
        if (string.IsNullOrWhiteSpace(resolved))
        {
            resolved = $"{eventCode}:{entityId?.ToString("N") ?? correlationId}";
        }

        if (resolved.Length <= 500)
        {
            return resolved;
        }

        return $"sha256:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(resolved))).ToLowerInvariant()}";
    }

    private static string? ResolvePath(JsonElement payload, string path)
    {
        var current = payload;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current))
            {
                return null;
            }
        }
        return current.ValueKind == JsonValueKind.String ? current.GetString() : current.GetRawText();
    }

    [GeneratedRegex(@"\{\{\s*([A-Za-z0-9_.-]+)\s*\}\}", RegexOptions.CultureInvariant)]
    private static partial Regex TokenRegex();
}
