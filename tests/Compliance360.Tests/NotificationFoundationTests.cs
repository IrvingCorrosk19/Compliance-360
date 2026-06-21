using Compliance360.Application;
using Compliance360.Application.Notifications;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Notifications;
using Compliance360.Infrastructure.Notifications;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Compliance360.Tests;

public sealed class NotificationFoundationTests
{
    [Fact]
    public async Task CreateTemplateAsync_Creates_Tenant_Template_And_Audit()
    {
        var fixture = NotificationFixture.Create();

        var result = await fixture.Service.CreateTemplateAsync(new CreateNotificationTemplateCommand(
            fixture.TenantId,
            fixture.UserId,
            "welcome",
            NotificationChannel.Email,
            "Welcome {{Name}}",
            "Hello {{Name}}"));

        Assert.True(result.IsSuccess);
        Assert.Equal("WELCOME", result.Value!.Code);
        Assert.Single(fixture.Repository.Templates);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.NotificationTemplateCreated);
    }

    [Fact]
    public async Task CreateTemplateAsync_Rejects_Duplicate_Template()
    {
        var fixture = NotificationFixture.Create();
        var command = new CreateNotificationTemplateCommand(fixture.TenantId, fixture.UserId, "welcome", NotificationChannel.Email, "Subject", "Body");

        await fixture.Service.CreateTemplateAsync(command);
        var duplicate = await fixture.Service.CreateTemplateAsync(command);

        Assert.True(duplicate.IsFailure);
    }

    [Fact]
    public async Task QueueAsync_Renders_Template_And_Targets_User()
    {
        var fixture = NotificationFixture.Create();
        await fixture.Service.CreateTemplateAsync(new CreateNotificationTemplateCommand(fixture.TenantId, fixture.UserId, "welcome", NotificationChannel.Email, "Welcome {{Name}}", "Hello {{Name}}"));
        var targetUserId = Guid.NewGuid();

        var result = await fixture.Service.QueueAsync(new QueueNotificationCommand(
            fixture.TenantId,
            fixture.UserId,
            NotificationChannel.Email,
            "qa@example.com",
            null,
            null,
            "welcome",
            new Dictionary<string, string> { ["Name"] = "QA" },
            NotificationPriority.High,
            targetUserId));

        Assert.True(result.IsSuccess);
        Assert.Equal("Welcome QA", result.Value!.Subject);
        Assert.Equal(NotificationStatus.Queued, result.Value.Status);
        Assert.Equal(targetUserId, fixture.Repository.Messages.Single().TargetUserId);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.NotificationQueued);
    }

    [Fact]
    public async Task QueueAsync_Rejects_Missing_Template_And_Invalid_Content()
    {
        var fixture = NotificationFixture.Create();

        var missingTemplate = await fixture.Service.QueueAsync(new QueueNotificationCommand(fixture.TenantId, fixture.UserId, NotificationChannel.Email, "qa@example.com", null, null, "missing", new Dictionary<string, string>(), NotificationPriority.Normal, null));
        var invalidDirect = await fixture.Service.QueueAsync(new QueueNotificationCommand(fixture.TenantId, fixture.UserId, NotificationChannel.Email, "qa@example.com", null, "body", null, new Dictionary<string, string>(), NotificationPriority.Normal, null));

        Assert.True(missingTemplate.IsFailure);
        Assert.True(invalidDirect.IsFailure);
    }

    [Fact]
    public async Task SendAsync_Marks_Message_Sent_And_Audits()
    {
        var fixture = NotificationFixture.Create();
        var queued = await fixture.QueueDirectAsync();

        var sent = await fixture.Service.SendAsync(new SendNotificationCommand(fixture.TenantId, queued.Id, fixture.UserId));

        Assert.True(sent.IsSuccess);
        Assert.Equal(NotificationStatus.Sent, sent.Value!.Status);
        Assert.Equal(fixture.Clock.UtcNow, sent.Value.SentAtUtc);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.NotificationSent);
    }

    [Fact]
    public async Task SendAsync_Marks_Message_Failed_When_Dispatcher_Fails()
    {
        var fixture = NotificationFixture.Create(NotificationDispatchResult.Failed("smtp unavailable"));
        var queued = await fixture.QueueDirectAsync();

        var sent = await fixture.Service.SendAsync(new SendNotificationCommand(fixture.TenantId, queued.Id, fixture.UserId));

        Assert.True(sent.IsSuccess);
        Assert.Equal(NotificationStatus.Retried, sent.Value!.Status);
        Assert.Equal("smtp unavailable", sent.Value.FailureReason);
        Assert.Single(fixture.Repository.Retries);
        Assert.Contains(fixture.Repository.History, history => history.Status == NotificationDeliveryStatus.Retried);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.NotificationFailed && !audit.Success);
    }

    [Fact]
    public async Task SendAsync_Rejects_Missing_Cancelled_Or_CrossTenant_Message()
    {
        var fixture = NotificationFixture.Create();
        var queued = await fixture.QueueDirectAsync();
        await fixture.Service.CancelAsync(new CancelNotificationCommand(fixture.TenantId, queued.Id, fixture.UserId));

        var missing = await fixture.Service.SendAsync(new SendNotificationCommand(fixture.TenantId, Guid.NewGuid(), fixture.UserId));
        var cancelled = await fixture.Service.SendAsync(new SendNotificationCommand(fixture.TenantId, queued.Id, fixture.UserId));
        var crossTenant = await fixture.Service.SendAsync(new SendNotificationCommand(Guid.NewGuid(), queued.Id, fixture.UserId));

        Assert.True(missing.IsFailure);
        Assert.True(cancelled.IsFailure);
        Assert.True(crossTenant.IsFailure);
    }

    [Fact]
    public async Task CancelAsync_Rejects_Sent_Message()
    {
        var fixture = NotificationFixture.Create();
        var queued = await fixture.QueueDirectAsync();
        await fixture.Service.SendAsync(new SendNotificationCommand(fixture.TenantId, queued.Id, fixture.UserId));

        var cancel = await fixture.Service.CancelAsync(new CancelNotificationCommand(fixture.TenantId, queued.Id, fixture.UserId));

        Assert.True(cancel.IsFailure);
    }

    [Fact]
    public void Notification_Domain_Rules_Handle_Template_And_Message_State()
    {
        var tenantId = Guid.NewGuid();
        var template = new NotificationTemplate(tenantId, "alert", NotificationChannel.InApp, "Subject", "Body");
        var message = new NotificationMessage(tenantId, NotificationChannel.InApp, "user", "Subject", "Body", NotificationPriority.Critical, DateTimeOffset.UtcNow);

        template.UpdateContent("Updated", "Updated body");
        template.Disable();
        message.LinkTemplate(template.Id);
        message.TargetUser(Guid.NewGuid());
        message.MarkFailed("failed", DateTimeOffset.UtcNow);
        message.Cancel();

        Assert.False(template.IsActive);
        Assert.Equal(NotificationStatus.Cancelled, message.Status);
        Assert.Throws<DomainException>(() => new NotificationMessage(tenantId, NotificationChannel.Email, "", "Subject", "Body", NotificationPriority.Normal, DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task EfNotificationRepository_Persists_And_Loads_TenantScoped_Data()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfNotificationRepository(dbContext);
        var tenantId = Guid.NewGuid();
        var template = new NotificationTemplate(tenantId, "welcome", NotificationChannel.Email, "Subject", "Body");
        var message = new NotificationMessage(tenantId, NotificationChannel.Email, "qa@example.com", "Subject", "Body", NotificationPriority.Normal, DateTimeOffset.UtcNow);
        var audit = AuditLog.Create(tenantId, Guid.NewGuid(), nameof(NotificationMessage), message.Id, AuditAction.NotificationQueued, DateTimeOffset.UtcNow);

        await repository.AddTemplateAsync(template);
        await repository.AddMessageAsync(message);
        await repository.AddAuditLogAsync(audit);
        await dbContext.SaveChangesAsync();

        var loadedTemplate = await repository.GetTemplateAsync(tenantId, "welcome", NotificationChannel.Email);
        var loadedMessage = await repository.GetMessageAsync(tenantId, message.Id);
        var wrongTenantMessage = await repository.GetMessageAsync(Guid.NewGuid(), message.Id);

        Assert.NotNull(loadedTemplate);
        Assert.NotNull(loadedMessage);
        Assert.Null(wrongTenantMessage);
        Assert.Single(dbContext.AuditLogs);
    }

    [Fact]
    public async Task EnterpriseNotificationDispatcher_Fails_When_All_Providers_Are_Unconfigured()
    {
        var dispatcher = new EnterpriseNotificationDispatcher(
            new NotificationProviderFactory([new SmtpNotificationProvider()]),
            Options.Create(new NotificationProviderOptions
            {
                DefaultProvider = NotificationProvider.Smtp,
                Providers = new Dictionary<NotificationProvider, NotificationProviderEndpointOptions>
                {
                    [NotificationProvider.Smtp] = new()
                }
            }),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<EnterpriseNotificationDispatcher>.Instance);

        var result = await dispatcher.DispatchAsync(new NotificationDispatchRequest(Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.Email, "qa@example.com", "Subject", "Body"));

        Assert.False(result.Success);
    }

    private static Compliance360DbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<Compliance360DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new Compliance360DbContext(options, new FixedClock());
    }

    private sealed class NotificationFixture
    {
        private NotificationFixture(NotificationDispatchResult dispatchResult)
        {
            TenantId = Guid.NewGuid();
            UserId = Guid.NewGuid();
            Clock = new FixedClock();
            Repository = new InMemoryNotificationRepository();
            var templateEngine = new NotificationTemplateEngine();
            var retryService = new NotificationRetryService(Options.Create(new NotificationProviderOptions()));
            var trackingService = new NotificationTrackingService(Repository);
            var auditService = new NotificationAuditService(Repository, Clock);
            Service = new NotificationService(Repository, new FakeNotificationDispatcher(dispatchResult), templateEngine, retryService, trackingService, auditService, new FakeApplicationDbContext(), Clock);
        }

        public Guid TenantId { get; }
        public Guid UserId { get; }
        public FixedClock Clock { get; }
        public InMemoryNotificationRepository Repository { get; }
        public NotificationService Service { get; }

        public static NotificationFixture Create(NotificationDispatchResult? dispatchResult = null)
        {
            return new NotificationFixture(dispatchResult ?? NotificationDispatchResult.Sent());
        }

        public async Task<NotificationMessageSummary> QueueDirectAsync()
        {
            var result = await Service.QueueAsync(new QueueNotificationCommand(
                TenantId,
                UserId,
                NotificationChannel.Email,
                "qa@example.com",
                "Subject",
                "Body",
                null,
                new Dictionary<string, string>(),
                NotificationPriority.Normal,
                null));

            return result.Value!;
        }
    }

    private sealed class InMemoryNotificationRepository : INotificationRepository
    {
        public List<NotificationTemplate> Templates { get; } = [];
        public List<NotificationMessage> Messages { get; } = [];
        public List<NotificationDelivery> Deliveries { get; } = [];
        public List<NotificationRetry> Retries { get; } = [];
        public List<NotificationHistory> History { get; } = [];
        public List<NotificationDeadLetter> DeadLetters { get; } = [];
        public List<NotificationProviderConfiguration> ProviderConfigurations { get; } = [];
        public List<AuditLog> AuditLogs { get; } = [];

        public Task AddTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
        {
            Templates.Add(template);
            return Task.CompletedTask;
        }

        public Task<NotificationTemplate?> GetTemplateAsync(Guid tenantId, string code, NotificationChannel channel, CancellationToken cancellationToken = default)
        {
            var normalizedCode = code.ToUpperInvariant();
            return Task.FromResult(Templates.SingleOrDefault(template => template.TenantId == tenantId && template.Code == normalizedCode && template.Channel == channel));
        }

        public Task AddMessageAsync(NotificationMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }

        public Task<NotificationMessage?> GetMessageAsync(Guid tenantId, Guid messageId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Messages.SingleOrDefault(message => message.TenantId == tenantId && message.Id == messageId));
        }

        public Task<IReadOnlyCollection<NotificationMessage>> ListMessagesAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<NotificationMessage>>(Messages.Where(message => message.TenantId == tenantId).ToArray());
        }

        public Task AddDeliveryAsync(NotificationDelivery delivery, CancellationToken cancellationToken = default)
        {
            Deliveries.Add(delivery);
            return Task.CompletedTask;
        }

        public Task AddRetryAsync(NotificationRetry retry, CancellationToken cancellationToken = default)
        {
            Retries.Add(retry);
            return Task.CompletedTask;
        }

        public Task AddHistoryAsync(NotificationHistory history, CancellationToken cancellationToken = default)
        {
            History.Add(history);
            return Task.CompletedTask;
        }

        public Task AddDeadLetterAsync(NotificationDeadLetter deadLetter, CancellationToken cancellationToken = default)
        {
            DeadLetters.Add(deadLetter);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<NotificationDeadLetter>> ListDeadLettersAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<NotificationDeadLetter>>(DeadLetters.Where(deadLetter => deadLetter.TenantId == tenantId).ToArray());
        }

        public Task AddProviderConfigurationAsync(NotificationProviderConfiguration configuration, CancellationToken cancellationToken = default)
        {
            ProviderConfigurations.Add(configuration);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<NotificationProviderConfiguration>> ListProviderConfigurationsAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<NotificationProviderConfiguration>>(ProviderConfigurations.Where(configuration => configuration.TenantId == tenantId).ToArray());
        }

        public Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            AuditLogs.Add(auditLog);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeNotificationDispatcher : INotificationDispatcher
    {
        private readonly NotificationDispatchResult _result;

        public FakeNotificationDispatcher(NotificationDispatchResult result)
        {
            _result = result;
        }

        public Task<NotificationDispatchResult> DispatchAsync(NotificationDispatchRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_result);
        }
    }

    private sealed class FakeApplicationDbContext : IApplicationDbContext
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 6, 20, 19, 0, 0, TimeSpan.Zero);
    }
}
