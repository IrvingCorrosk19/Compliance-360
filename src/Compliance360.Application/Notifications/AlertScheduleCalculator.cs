using System.Text.Json;
using Cronos;

namespace Compliance360.Application.Notifications;

public interface IAlertScheduleCalculator
{
    DateTimeOffset NextOccurrence(string cronExpression, string timeZoneId, DateTimeOffset fromUtc, bool inclusive = false);
    AlertSchedulePreviewOccurrence ApplyCalendar(DateTimeOffset scheduledAtUtc, string timeZoneId, string businessCalendarJson, string quietHoursJson);
}

public sealed class AlertScheduleCalculator : IAlertScheduleCalculator
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public DateTimeOffset NextOccurrence(string cronExpression, string timeZoneId, DateTimeOffset fromUtc, bool inclusive = false)
    {
        var cron = CronExpression.Parse(cronExpression, CronFormat.Standard);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var next = cron.GetNextOccurrence(fromUtc.UtcDateTime, timeZone, inclusive);
        if (!next.HasValue)
        {
            throw new InvalidOperationException("Cron expression has no next occurrence.");
        }

        return new DateTimeOffset(DateTime.SpecifyKind(next.Value, DateTimeKind.Utc));
    }

    public AlertSchedulePreviewOccurrence ApplyCalendar(
        DateTimeOffset scheduledAtUtc,
        string timeZoneId,
        string businessCalendarJson,
        string quietHoursJson)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var business = Deserialize<BusinessCalendarSettings>(businessCalendarJson) ?? new BusinessCalendarSettings();
        var quiet = Deserialize<QuietHoursSettings>(quietHoursJson) ?? new QuietHoursSettings();
        var local = TimeZoneInfo.ConvertTime(scheduledAtUtc, timeZone).DateTime;
        var effective = local;
        var reasons = new List<string>();

        if (TryReadTime(quiet.Start, out var quietStart) && TryReadTime(quiet.End, out var quietEnd)
            && IsInside(effective.TimeOfDay, quietStart, quietEnd))
        {
            effective = effective.Date.Add(quietEnd);
            if (quietStart > quietEnd && local.TimeOfDay >= quietStart)
            {
                effective = effective.AddDays(1);
            }
            reasons.Add("quiet-hours");
        }

        var allowedDays = ParseDays(business.Days);
        var holidays = new HashSet<DateOnly>(
            business.Holidays
                .Select(value => DateOnly.TryParse(value, out var date) ? date : (DateOnly?)null)
                .Where(value => value.HasValue)
                .Select(value => value!.Value));
        var hasStart = TryReadTime(business.Start, out var businessStart);
        var hasEnd = TryReadTime(business.End, out var businessEnd);

        for (var guard = 0; guard < 370; guard++)
        {
            var date = DateOnly.FromDateTime(effective);
            if ((allowedDays.Count > 0 && !allowedDays.Contains(effective.DayOfWeek)) || holidays.Contains(date))
            {
                effective = effective.Date.AddDays(1).Add(hasStart ? businessStart : TimeSpan.Zero);
                reasons.Add(holidays.Contains(date) ? "holiday" : "non-business-day");
                continue;
            }

            if (hasStart && effective.TimeOfDay < businessStart)
            {
                effective = effective.Date.Add(businessStart);
                reasons.Add("before-business-hours");
            }
            else if (hasEnd && effective.TimeOfDay >= businessEnd)
            {
                effective = effective.Date.AddDays(1).Add(hasStart ? businessStart : TimeSpan.Zero);
                reasons.Add("after-business-hours");
                continue;
            }

            break;
        }

        while (timeZone.IsInvalidTime(effective))
        {
            effective = effective.AddMinutes(1);
            reasons.Add("dst-invalid-time");
        }

        var effectiveUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(effective, DateTimeKind.Unspecified), timeZone);
        return new AlertSchedulePreviewOccurrence(
            scheduledAtUtc,
            new DateTimeOffset(effectiveUtc),
            reasons.Count == 0 ? "scheduled" : string.Join(",", reasons.Distinct()));
    }

    private static T? Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    private static HashSet<DayOfWeek> ParseDays(IEnumerable<string>? days)
    {
        var result = new HashSet<DayOfWeek>();
        foreach (var day in days ?? [])
        {
            if (Enum.TryParse<DayOfWeek>(day, true, out var parsed))
            {
                result.Add(parsed);
            }
        }

        return result;
    }

    private static bool TryReadTime(string? value, out TimeSpan parsed) =>
        TimeSpan.TryParse(value, out parsed);

    private static bool IsInside(TimeSpan value, TimeSpan start, TimeSpan end) =>
        start <= end ? value >= start && value < end : value >= start || value < end;

    private sealed record BusinessCalendarSettings
    {
        public IReadOnlyCollection<string> Days { get; init; } = [];
        public IReadOnlyCollection<string> Holidays { get; init; } = [];
        public string? Start { get; init; }
        public string? End { get; init; }
    }

    private sealed record QuietHoursSettings
    {
        public string? Start { get; init; }
        public string? End { get; init; }
    }
}
