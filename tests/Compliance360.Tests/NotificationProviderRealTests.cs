using System.Net;
using Compliance360.Application;
using Compliance360.Application.Notifications;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Notifications;
using Compliance360.Infrastructure.Notifications;
using Compliance360.Web.Observability;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Compliance360.Tests;

public sealed class NotificationProviderRealTests
{
    [Fact]
    public void TemplateEngine_Renders_Variables_And_Tenant_Branding()
    {
        var engine = new NotificationTemplateEngine();

        var rendered = engine.Render(
            "Hello {{Name}}",
            "<img src='{{TenantLogo}}'><b>{{PrimaryColor}}</b>",
            "Tenant {{TenantName}}",
            new Dictionary<string, string> { ["Name"] = "QA" },
            new TenantNotificationBranding("https://cdn/logo.png", "#0052CC", "#172B4D", "Acme"));

        Assert.Equal("Hello QA", rendered.Subject);
        Assert.Contains("https://cdn/logo.png", rendered.HtmlBody);
        Assert.Equal("Tenant Acme", rendered.TextBody);
    }

    [Fact]
    public void RetryService_Uses_Exponential_Backoff_And_DeadLetter_Limit()
    {
        var service = new NotificationRetryService(Options.Create(new NotificationProviderOptions
        {
            MaxRetryCount = 3,
            RetryBaseDelaySeconds = 10
        }));
        var now = new DateTimeOffset(2026, 6, 21, 10, 0, 0, TimeSpan.Zero);

        Assert.Equal(now.AddSeconds(10), service.GetNextRetryAt(now, 0));
        Assert.Equal(now.AddSeconds(40), service.GetNextRetryAt(now, 2));
        Assert.False(service.ShouldDeadLetter(2));
        Assert.True(service.ShouldDeadLetter(3));
    }

    [Fact]
    public async Task Http_Providers_Send_Real_Http_Payloads()
    {
        var handler = new CapturingHandler(HttpStatusCode.Accepted);
        var factory = new FakeHttpClientFactory(handler);
        var provider = new SendGridNotificationProvider(factory);
        var endpoint = new NotificationProviderEndpoint(NotificationProvider.SendGrid, null, null, null, "secret", "qa@example.com", "QA", "https://sendgrid.local", null, true);

        var result = await provider.SendAsync(new NotificationDispatchRequest(Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.Email, "to@example.com", "Subject", "<b>Body</b>"), endpoint);

        Assert.True(result.Success);
        Assert.Equal(NotificationProvider.SendGrid, result.Provider);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Contains("/v3/mail/send", handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Mailgun_And_Resend_Send_Real_Http_Payloads()
    {
        var mailgunHandler = new CapturingHandler(HttpStatusCode.OK);
        var resendHandler = new CapturingHandler(HttpStatusCode.OK);
        var mailgun = new MailgunNotificationProvider(new FakeHttpClientFactory(mailgunHandler));
        var resend = new ResendNotificationProvider(new FakeHttpClientFactory(resendHandler));

        var mailgunResult = await mailgun.SendAsync(new NotificationDispatchRequest(Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.Email, "to@example.com", "Subject", "<b>Body</b>", "Body"), new NotificationProviderEndpoint(NotificationProvider.Mailgun, null, null, null, "secret", "from@example.com", "QA", "https://mailgun.local", "mg.example.com", true));
        var resendResult = await resend.SendAsync(new NotificationDispatchRequest(Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.Email, "to@example.com", "Subject", "<b>Body</b>", "Body"), new NotificationProviderEndpoint(NotificationProvider.Resend, null, null, null, "secret", "from@example.com", "QA", "https://resend.local", null, true));

        Assert.True(mailgunResult.Success);
        Assert.True(resendResult.Success);
        Assert.Contains("/v3/mg.example.com/messages", mailgunHandler.LastRequest!.RequestUri!.ToString());
        Assert.Contains("/emails", resendHandler.LastRequest!.RequestUri!.ToString());
    }

    [Fact]
    public async Task Dispatcher_Uses_Failover_When_Default_Provider_Fails()
    {
        var dispatcher = new EnterpriseNotificationDispatcher(
            new NotificationProviderFactory([
                new FailingProvider(NotificationProvider.Smtp),
                new SuccessfulProvider(NotificationProvider.Resend)
            ]),
            Options.Create(new NotificationProviderOptions
            {
                DefaultProvider = NotificationProvider.Smtp,
                FailoverProvider = NotificationProvider.Resend,
                Providers = new Dictionary<NotificationProvider, NotificationProviderEndpointOptions>
                {
                    [NotificationProvider.Smtp] = new(),
                    [NotificationProvider.Resend] = new()
                }
            }),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<EnterpriseNotificationDispatcher>.Instance);

        var result = await dispatcher.DispatchAsync(new NotificationDispatchRequest(Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.Email, "to@example.com", "Subject", "Body"));

        Assert.True(result.Success);
        Assert.Equal(NotificationProvider.Resend, result.Provider);
    }

    [Fact]
    public async Task Http_Provider_Fails_When_Not_Configured_Or_Provider_Returns_Error()
    {
        var provider = new ResendNotificationProvider(new FakeHttpClientFactory(new CapturingHandler(HttpStatusCode.BadGateway)));
        var missing = await provider.SendAsync(new NotificationDispatchRequest(Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.Email, "to@example.com", "Subject", "Body"), new NotificationProviderEndpoint(NotificationProvider.Resend, null, null, null, null, null, null, null, null, true));
        var failed = await provider.SendAsync(new NotificationDispatchRequest(Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.Email, "to@example.com", "Subject", "Body"), new NotificationProviderEndpoint(NotificationProvider.Resend, null, null, null, "secret", "from@example.com", "QA", "https://resend.local", null, true));

        Assert.False(missing.Success);
        Assert.False(failed.Success);
    }

    [Fact]
    public async Task Mailgun_And_Smtp_Health_Checks_Report_Configured_State()
    {
        var mailgun = new MailgunNotificationProvider(new FakeHttpClientFactory(new CapturingHandler(HttpStatusCode.OK)));
        var smtp = new SmtpNotificationProvider();

        var mailgunHealth = await mailgun.CheckHealthAsync(new NotificationProviderEndpoint(NotificationProvider.Mailgun, null, null, null, "secret", "from@example.com", "QA", "https://mailgun.local", "mg.example.com", true));
        var smtpHealth = await smtp.CheckHealthAsync(new NotificationProviderEndpoint(NotificationProvider.Smtp, "smtp.example.com", 587, "user", "secret", "from@example.com", "QA", null, null, true));

        Assert.True(mailgunHealth.Healthy);
        Assert.True(smtpHealth.Healthy);
    }

    [Fact]
    public void ProviderFactory_Rejects_Unregistered_Provider()
    {
        var factory = new NotificationProviderFactory([new SmtpNotificationProvider()]);

        Assert.Throws<InvalidOperationException>(() => factory.GetProvider(NotificationProvider.Resend));
    }

    [Fact]
    public async Task NotificationProviderHealthCheck_Returns_Degraded_For_Unconfigured_Provider()
    {
        var healthCheck = new NotificationProviderHealthCheck(
            new NotificationProviderFactory([new SmtpNotificationProvider()]),
            Options.Create(new NotificationProviderOptions()),
            NotificationProvider.Smtp);

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Degraded, result.Status);
    }

    [Fact]
    public async Task Service_Configures_Provider_And_Builds_Dashboard()
    {
        var fixture = EnterpriseNotificationFixture.Create();
        await fixture.Service.ConfigureProviderAsync(new ConfigureNotificationProviderCommand(fixture.TenantId, fixture.UserId, NotificationProvider.Smtp, "Primary SMTP", 1, true, true));
        var queued = await fixture.QueueDirectAsync();
        await fixture.Service.SendAsync(new SendNotificationCommand(fixture.TenantId, queued.Id, fixture.UserId));

        var dashboard = await fixture.Service.GetDashboardAsync(fixture.TenantId);
        var history = await fixture.Service.GetHistoryAsync(fixture.TenantId);

        Assert.True(dashboard.IsSuccess);
        Assert.Equal(1, dashboard.Value!.Sent);
        Assert.True(dashboard.Value.ProviderHealth[NotificationProvider.Smtp]);
        Assert.Single(history.Value!);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.NotificationProviderChanged);
    }

    [Fact]
    public async Task Service_DeadLetters_After_Retry_Limit()
    {
        var fixture = EnterpriseNotificationFixture.Create(NotificationDispatchResult.Failed("provider down"), maxRetryCount: 1);
        var queued = await fixture.QueueDirectAsync();

        await fixture.Service.SendAsync(new SendNotificationCommand(fixture.TenantId, queued.Id, fixture.UserId));
        var retry = await fixture.Service.SendAsync(new SendNotificationCommand(fixture.TenantId, queued.Id, fixture.UserId));
        var deadLetters = await fixture.Service.GetDeadLettersAsync(fixture.TenantId);

        Assert.Equal(NotificationStatus.DeadLetter, retry.Value!.Status);
        Assert.Single(deadLetters.Value!);
    }

    [Fact]
    public void Domain_Models_Cover_Enterprise_Notification_State()
    {
        var tenantId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var template = new NotificationTemplate(tenantId, "renewal", NotificationChannel.Email, "Subject {{TenantName}}", "<b>Body</b>");
        template.ConfigureEnterpriseContent("Body", "en-US", "{\"primaryColor\":\"#0052CC\"}");
        template.UpdateContent("Updated", "Updated body");
        template.Disable();

        var message = new NotificationMessage(tenantId, NotificationChannel.Email, "qa@example.com", "Subject", "Body", NotificationPriority.Critical, DateTimeOffset.UtcNow);
        message.LinkTemplate(template.Id);
        message.TargetUser(Guid.NewGuid());
        message.MarkSent(DateTimeOffset.UtcNow);
        message.MarkDelivered(DateTimeOffset.UtcNow);

        var retry = new NotificationRetry(tenantId, messageId, 1, DateTimeOffset.UtcNow.AddMinutes(1), "temporary failure");
        retry.MarkExecuted(DateTimeOffset.UtcNow.AddMinutes(2));
        var subscription = new NotificationSubscription(tenantId, "CAPA", NotificationChannel.Email, "qa@example.com");
        var preference = new NotificationPreference(tenantId, Guid.NewGuid(), NotificationChannel.Email, true);
        var history = new NotificationHistory(tenantId, messageId, NotificationDeliveryStatus.Delivered, "Delivered", DateTimeOffset.UtcNow);
        var attachment = new NotificationAttachment(tenantId, messageId, "evidence.pdf", "application/pdf", "tenant/file.pdf");
        var providerConfiguration = new NotificationProviderConfiguration(tenantId, NotificationProvider.Smtp, "Primary", 1, true, true);
        var deadLetter = new NotificationDeadLetter(tenantId, messageId, "failed", "{\"id\":\"1\"}", DateTimeOffset.UtcNow);
        var delivery = new NotificationDelivery(tenantId, messageId, NotificationProvider.Smtp, NotificationDeliveryStatus.Sent, "provider-id", DateTimeOffset.UtcNow);

        Assert.False(template.IsActive);
        Assert.Equal(2, template.Version);
        Assert.Equal(NotificationStatus.Delivered, message.Status);
        Assert.NotNull(retry.ExecutedAtUtc);
        Assert.True(subscription.IsActive);
        Assert.True(preference.Enabled);
        Assert.Equal("Delivered", history.EventName);
        Assert.Equal("evidence.pdf", attachment.FileName);
        Assert.True(providerConfiguration.IsDefault);
        Assert.Equal("failed", deadLetter.Reason);
        Assert.Equal(NotificationProvider.Smtp, delivery.Provider);
        Assert.Throws<Compliance360.Domain.Common.DomainException>(() => message.Cancel());
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        public CapturingHandler(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            var response = new HttpResponseMessage(_statusCode);
            response.Headers.Add("X-Message-Id", "provider-message-1");
            return Task.FromResult(response);
        }
    }

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public FakeHttpClientFactory(HttpMessageHandler handler)
        {
            _handler = handler;
        }

        public HttpClient CreateClient(string name)
        {
            return new HttpClient(_handler, disposeHandler: false);
        }
    }

    private sealed class FailingProvider : INotificationProvider
    {
        public FailingProvider(NotificationProvider provider)
        {
            Provider = provider;
        }

        public NotificationProvider Provider { get; }

        public Task<NotificationDispatchResult> SendAsync(NotificationDispatchRequest request, NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default) =>
            Task.FromResult(NotificationDispatchResult.Failed("failed", Provider));

        public Task<NotificationProviderHealth> CheckHealthAsync(NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default) =>
            Task.FromResult(new NotificationProviderHealth(Provider, false, "failed"));
    }

    private sealed class SuccessfulProvider : INotificationProvider
    {
        public SuccessfulProvider(NotificationProvider provider)
        {
            Provider = provider;
        }

        public NotificationProvider Provider { get; }

        public Task<NotificationDispatchResult> SendAsync(NotificationDispatchRequest request, NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default) =>
            Task.FromResult(NotificationDispatchResult.Sent(Provider, "ok"));

        public Task<NotificationProviderHealth> CheckHealthAsync(NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default) =>
            Task.FromResult(new NotificationProviderHealth(Provider, true, "ok"));
    }

    private sealed class EnterpriseNotificationFixture
    {
        private EnterpriseNotificationFixture(NotificationDispatchResult dispatchResult, int maxRetryCount)
        {
            TenantId = Guid.NewGuid();
            UserId = Guid.NewGuid();
            Clock = new FixedClock();
            Repository = new InMemoryNotificationRepository();
            Service = new NotificationService(
                Repository,
                new FakeNotificationDispatcher(dispatchResult),
                new NotificationTemplateEngine(),
                new NotificationRetryService(Options.Create(new NotificationProviderOptions { MaxRetryCount = maxRetryCount, RetryBaseDelaySeconds = 1 })),
                new NotificationTrackingService(Repository),
                new NotificationAuditService(Repository, Clock),
                new FakeApplicationDbContext(),
                Clock);
        }

        public Guid TenantId { get; }
        public Guid UserId { get; }
        public FixedClock Clock { get; }
        public InMemoryNotificationRepository Repository { get; }
        public NotificationService Service { get; }

        public static EnterpriseNotificationFixture Create(NotificationDispatchResult? dispatchResult = null, int maxRetryCount = 3) =>
            new(dispatchResult ?? NotificationDispatchResult.Sent(NotificationProvider.Smtp, "smtp-1"), maxRetryCount);

        public async Task<NotificationMessageSummary> QueueDirectAsync()
        {
            var result = await Service.QueueAsync(new QueueNotificationCommand(TenantId, UserId, NotificationChannel.Email, "qa@example.com", "Subject", "Body", null, new Dictionary<string, string>(), NotificationPriority.Normal, null));
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

        public Task AddTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default) { Templates.Add(template); return Task.CompletedTask; }
        public Task<NotificationTemplate?> GetTemplateAsync(Guid tenantId, string code, NotificationChannel channel, CancellationToken cancellationToken = default) => Task.FromResult(Templates.SingleOrDefault(template => template.TenantId == tenantId && template.Code == code.ToUpperInvariant() && template.Channel == channel));
        public Task AddMessageAsync(NotificationMessage message, CancellationToken cancellationToken = default) { Messages.Add(message); return Task.CompletedTask; }
        public Task<NotificationMessage?> GetMessageAsync(Guid tenantId, Guid messageId, CancellationToken cancellationToken = default) => Task.FromResult(Messages.SingleOrDefault(message => message.TenantId == tenantId && message.Id == messageId));
        public Task<IReadOnlyCollection<NotificationMessage>> ListMessagesAsync(Guid tenantId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<NotificationMessage>>(Messages.Where(message => message.TenantId == tenantId).ToArray());
        public Task AddDeliveryAsync(NotificationDelivery delivery, CancellationToken cancellationToken = default) { Deliveries.Add(delivery); return Task.CompletedTask; }
        public Task AddRetryAsync(NotificationRetry retry, CancellationToken cancellationToken = default) { Retries.Add(retry); return Task.CompletedTask; }
        public Task AddHistoryAsync(NotificationHistory history, CancellationToken cancellationToken = default) { History.Add(history); return Task.CompletedTask; }
        public Task AddDeadLetterAsync(NotificationDeadLetter deadLetter, CancellationToken cancellationToken = default) { DeadLetters.Add(deadLetter); return Task.CompletedTask; }
        public Task<IReadOnlyCollection<NotificationDeadLetter>> ListDeadLettersAsync(Guid tenantId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<NotificationDeadLetter>>(DeadLetters.Where(deadLetter => deadLetter.TenantId == tenantId).ToArray());
        public Task AddProviderConfigurationAsync(NotificationProviderConfiguration configuration, CancellationToken cancellationToken = default) { ProviderConfigurations.Add(configuration); return Task.CompletedTask; }
        public Task<IReadOnlyCollection<NotificationProviderConfiguration>> ListProviderConfigurationsAsync(Guid tenantId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<NotificationProviderConfiguration>>(ProviderConfigurations.Where(configuration => configuration.TenantId == tenantId).ToArray());
        public Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default) { AuditLogs.Add(auditLog); return Task.CompletedTask; }
    }

    private sealed class FakeNotificationDispatcher : INotificationDispatcher
    {
        private readonly NotificationDispatchResult _result;
        public FakeNotificationDispatcher(NotificationDispatchResult result) { _result = result; }
        public Task<NotificationDispatchResult> DispatchAsync(NotificationDispatchRequest request, CancellationToken cancellationToken = default) => Task.FromResult(_result);
    }

    private sealed class FakeApplicationDbContext : IApplicationDbContext
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 6, 21, 10, 0, 0, TimeSpan.Zero);
    }
}
