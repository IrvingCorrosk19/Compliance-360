using Compliance360.Domain.Common;
using Compliance360.Domain.Notifications;

namespace Compliance360.Tests;

public sealed class AlertOccurrenceLifecycleTests
{
    [Fact]
    public void Acknowledge_Resolve_Escalate_Transitions_Are_Controlled()
    {
        var occurrence = NewOccurrence();
        occurrence.CompleteEvaluation(AlertOccurrenceStatus.Matched, DateTimeOffset.UtcNow);

        occurrence.Acknowledge(Guid.NewGuid(), DateTimeOffset.UtcNow, "Seen by operator");
        Assert.Equal(AlertOccurrenceStatus.Acknowledged, occurrence.Status);
        Assert.Equal("Seen by operator", occurrence.FailureReason);

        occurrence.Escalate(Guid.NewGuid(), DateTimeOffset.UtcNow, "SLA risk");
        Assert.Equal(AlertOccurrenceStatus.Escalated, occurrence.Status);

        occurrence.Resolve(Guid.NewGuid(), DateTimeOffset.UtcNow, "Closed after action");
        Assert.Equal(AlertOccurrenceStatus.Resolved, occurrence.Status);
        Assert.Equal("Closed after action", occurrence.FailureReason);
    }

    [Fact]
    public void Acknowledge_From_Pending_Is_Rejected()
    {
        var occurrence = NewOccurrence();
        var ex = Assert.Throws<DomainException>(() =>
            occurrence.Acknowledge(Guid.NewGuid(), DateTimeOffset.UtcNow, null));
        Assert.Contains("cannot transition", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static AlertOccurrence NewOccurrence() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "dedupe-key",
            """{"k":"v"}""",
            "corr-1",
            "Regulatory",
            "Dossier",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
}
