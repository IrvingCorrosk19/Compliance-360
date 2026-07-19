using Compliance360.Application;
using Compliance360.Application.Notifications;
using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Tests;

public sealed class AlertEventProcessingTests
{
    [Fact]
    public async Task Published_Rule_Resolves_Renders_And_Queues_Idempotent_Message()
    {
        var tenantId = Guid.NewGuid();
        var maker = Guid.NewGuid();
        var recipientUserId = Guid.NewGuid();
        var definition = new AlertDefinition(
            tenantId,
            Guid.NewGuid(),
            "DOSSIER_OVERDUE",
            "Dossier overdue",
            "Notify the owner.",
            maker,
            NotificationPriority.High);
        var version = new AlertDefinitionVersion(
            tenantId,
            definition.Id,
            1,
            """{"type":"Compare","left":{"type":"VariableReference","path":"status"},"operator":"Equal","right":{"type":"Constant","value":"Overdue"}}""",
            """[{"kind":"Owner","routing":"To"}]""",
            """[{"channel":"InApp","templateCode":"DOSSIER_OVERDUE"}]""",
            "{{entityId}}",
            60,
            120,
            AlertUnknownPolicy.TreatAsFalse,
            maker);
        version.SubmitForReview(DateTimeOffset.UtcNow);
        version.Review(Guid.NewGuid(), DateTimeOffset.UtcNow);
        version.Approve(Guid.NewGuid(), DateTimeOffset.UtcNow);
        version.Publish(Guid.NewGuid(), DateTimeOffset.UtcNow);
        definition.SetPublishedVersion(version.Id, DateTimeOffset.UtcNow);

        var template = PublishedTemplate(tenantId, maker);
        var occurrence = new AlertOccurrence(
            tenantId,
            definition.Id,
            version.Id,
            definition.EventTypeId,
            "dossier:1",
            $$"""{"status":"Overdue","entityId":"{{Guid.NewGuid()}}","ownerUserId":"{{recipientUserId}}","caseNumber":"RA-100"}""",
            "corr-1",
            "RegulatoryAffairs",
            "RegistrationDossier",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
        var outbox = new NotificationOutboxEvent(
            tenantId,
            "AlertOccurrenceCreated",
            nameof(AlertOccurrence),
            occurrence.Id,
            occurrence.PayloadJson,
            occurrence.CorrelationId,
            DateTimeOffset.UtcNow);
        var repository = new FakeEventRepository(definition, version, occurrence, template);
        var notifications = new FakeNotificationService(repository);
        var processor = new AlertEventProcessor(
            repository,
            new AlertRuleEvaluator(),
            new FakeRecipientResolver(recipientUserId),
            new NotificationTemplateEngine(),
            notifications,
            new FixedClock());

        var result = await processor.ProcessAsync(outbox);
        var repeated = await processor.ProcessAsync(outbox);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(1, result.Value);
        Assert.True(repeated.IsSuccess);
        Assert.Equal(0, repeated.Value);
        var queued = Assert.Single(notifications.Commands);
        Assert.Equal(NotificationChannel.InApp, queued.Channel);
        Assert.Equal(recipientUserId, queued.TargetUserId);
        Assert.Equal(occurrence.Id, queued.AlertOccurrenceId);
        Assert.Equal(RecipientRouting.To, queued.Routing);
        Assert.Contains("RA-100", queued.Body);
        Assert.Equal(AlertOccurrenceStatus.Queued, occurrence.Status);
    }

    private static NotificationTemplateVersion PublishedTemplate(Guid tenantId, Guid maker)
    {
        var template = new NotificationTemplateVersion(
            tenantId,
            Guid.NewGuid(),
            1,
            "es-PA",
            "Dossier {{caseNumber}}",
            "<p>El dossier {{caseNumber}} está vencido.</p>",
            "El dossier {{caseNumber}} está vencido.",
            """["caseNumber"]""",
            null,
            maker,
            DateTimeOffset.UtcNow);
        template.SubmitForReview(DateTimeOffset.UtcNow);
        template.RecordReview(Guid.NewGuid(), DateTimeOffset.UtcNow);
        template.Approve(Guid.NewGuid(), DateTimeOffset.UtcNow);
        template.Publish(Guid.NewGuid(), DateTimeOffset.UtcNow);
        return template;
    }

    private sealed class FakeEventRepository : IAlertEventRepository
    {
        private readonly AlertDefinition _definition;
        private readonly AlertDefinitionVersion _version;
        private readonly AlertOccurrence _occurrence;
        private readonly NotificationTemplateVersion _template;
        public HashSet<string> MessageKeys { get; } = [];

        public FakeEventRepository(AlertDefinition definition, AlertDefinitionVersion version, AlertOccurrence occurrence, NotificationTemplateVersion template)
        {
            _definition = definition;
            _version = version;
            _occurrence = occurrence;
            _template = template;
        }

        public Task<AlertOccurrence?> GetOccurrenceAsync(Guid tenantId, Guid occurrenceId, CancellationToken cancellationToken) => Task.FromResult<AlertOccurrence?>(_occurrence);
        public Task<AlertDefinition?> GetDefinitionAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken) => Task.FromResult<AlertDefinition?>(_definition);
        public Task<AlertDefinitionVersion?> GetVersionAsync(Guid tenantId, Guid definitionId, Guid versionId, CancellationToken cancellationToken) => Task.FromResult<AlertDefinitionVersion?>(_version);
        public Task<NotificationTemplateVersion?> GetPublishedTemplateAsync(Guid tenantId, string code, NotificationChannel channel, string? locale, CancellationToken cancellationToken) => Task.FromResult<NotificationTemplateVersion?>(_template);
        public Task<bool> HasRecentOccurrenceAsync(Guid tenantId, Guid definitionVersionId, string dedupeKey, Guid excludedOccurrenceId, DateTimeOffset sinceUtc, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<bool> MessageExistsAsync(Guid tenantId, string idempotencyKey, CancellationToken cancellationToken) => Task.FromResult(MessageKeys.Contains(idempotencyKey));
        public Task<AlertEventType?> GetEventTypeAsync(Guid tenantId, string eventCode, CancellationToken cancellationToken) => Task.FromResult<AlertEventType?>(null);
        public Task<IReadOnlyCollection<(AlertDefinition Definition, AlertDefinitionVersion Version)>> ListPublishedRulesAsync(Guid tenantId, Guid eventTypeId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<(AlertDefinition, AlertDefinitionVersion)>>([]);
        public Task AddOccurrencesAsync(IReadOnlyCollection<AlertOccurrence> occurrences, IReadOnlyCollection<NotificationOutboxEvent> outboxEvents, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task CompleteOccurrenceIfTerminalAsync(Guid tenantId, Guid occurrenceId, DateTimeOffset completedAtUtc, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeRecipientResolver : IRecipientResolverService
    {
        private readonly Guid _userId;
        public FakeRecipientResolver(Guid userId) => _userId = userId;
        public Task<Result<RecipientPreview>> PreviewAsync(PreviewRecipientsCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<RecipientPreview>.Success(new RecipientPreview(
                command.VersionId,
                [new ResolvedRecipient(_userId, "owner@example.com", "Owner", RecipientRouting.To, RecipientKind.Owner, false)],
                [],
                [],
                [],
                false)));
        public Task<Result> SetPreferenceAsync(SetRecipientPreferenceCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<Guid>> CreateGroupAsync(CreateRecipientGroupCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> AddGroupMemberAsync(AddRecipientGroupMemberCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<Guid>> CreateDepartmentAsync(CreateRecipientDepartmentCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> SetDirectoryProfileAsync(SetRecipientDirectoryProfileCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<Guid>> AuthorizeExternalAsync(AuthorizeExternalRecipientCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<Guid>> CreateDistributionListAsync(CreateRecipientDistributionListCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> AddDistributionListMemberAsync(AddRecipientDistributionListMemberCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> SetFallbackAsync(SetRecipientFallbackCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<RecipientDirectorySummary>> GetDirectoryAsync(Guid tenantId, Guid requestedByUserId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeNotificationService : INotificationService
    {
        private readonly FakeEventRepository _repository;
        public FakeNotificationService(FakeEventRepository repository) => _repository = repository;
        public List<QueueNotificationCommand> Commands { get; } = [];
        public Task<Result<NotificationMessageSummary>> QueueAsync(QueueNotificationCommand command, CancellationToken cancellationToken = default)
        {
            Commands.Add(command);
            _repository.MessageKeys.Add(command.IdempotencyKey!);
            return Task.FromResult(Result<NotificationMessageSummary>.Success(new NotificationMessageSummary(
                Guid.NewGuid(), command.TenantId, command.Channel, command.Recipient, command.Subject!, command.Priority,
                NotificationStatus.Queued, DateTimeOffset.UtcNow, null, null)));
        }
        public Task<Result<NotificationTemplateSummary>> CreateTemplateAsync(CreateNotificationTemplateCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<NotificationTemplateSummary>> PreviewTemplateAsync(PreviewNotificationTemplateCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<NotificationMessageSummary>> SendAsync(SendNotificationCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<NotificationMessageSummary>> RetryAsync(RetryNotificationCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> CancelAsync(CancelNotificationCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<NotificationDashboardSummary>> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<IReadOnlyCollection<NotificationMessageSummary>>> GetHistoryAsync(Guid tenantId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<IReadOnlyCollection<NotificationDeadLetterSummary>>> GetDeadLettersAsync(Guid tenantId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<NotificationProviderConfigurationSummary>> ConfigureProviderAsync(ConfigureNotificationProviderCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 7, 19, 20, 0, 0, TimeSpan.Zero);
    }
}
