using System.Net;
using System.Net.Sockets;
using System.Text;
using Compliance360.Application;
using Compliance360.Application.Notifications;
using Compliance360.Domain.Notifications;
using Compliance360.Infrastructure.Notifications;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Compliance360.Tests;

public sealed class ProviderCenterTests
{
    [Fact]
    public void Provider_Enforces_Durable_Rate_And_Circuit_State()
    {
        var now = new DateTimeOffset(2026, 7, 19, 12, 0, 0, TimeSpan.Zero);
        var provider = NewProvider(rateLimit: 1, threshold: 2);

        Assert.True(provider.TryAcquire(now));
        Assert.False(provider.TryAcquire(now.AddSeconds(1)));
        provider.RecordFailure("timeout", now);
        provider.RecordFailure("timeout", now.AddSeconds(1));

        Assert.False(provider.TryAcquire(now.AddMinutes(1)));
        Assert.Equal(now.AddSeconds(301), provider.CircuitOpenUntilUtc);
        Assert.True(provider.TryAcquire(now.AddSeconds(302)));
    }

    [Fact]
    public void Data_Protection_Encrypts_Secrets_And_Summary_Masks_Them()
    {
        var protector = new DataProtectionProviderSecretProtector(new EphemeralDataProtectionProvider());
        var encrypted = protector.Protect(new ProviderSecretSettings(Secret: "never-return-this"));

        Assert.DoesNotContain("never-return-this", encrypted);
        Assert.Equal("never-return-this", protector.Unprotect(encrypted).Secret);
    }

    [Fact]
    public async Task Dispatcher_Uses_Tenant_Priority_Failover_And_Persists_Health()
    {
        var tenantId = Guid.NewGuid();
        var first = NewProvider(tenantId, NotificationProvider.SendGrid, priority: 1);
        var second = NewProvider(tenantId, NotificationProvider.Resend, priority: 2);
        var repository = new MemoryRepository(first, second);
        var dispatcher = new EnterpriseNotificationDispatcher(
            new NotificationProviderFactory([
                new StubProvider(NotificationProvider.SendGrid, false),
                new StubProvider(NotificationProvider.Resend, true)
            ]),
            Options.Create(new NotificationProviderOptions()),
            NullLogger<EnterpriseNotificationDispatcher>.Instance,
            repository,
            new StubResolver(),
            new FixedClock());

        var result = await dispatcher.DispatchAsync(new NotificationDispatchRequest(
            tenantId, Guid.NewGuid(), NotificationChannel.Email, "to@example.com", "subject", "body"));

        Assert.True(result.Success);
        Assert.Equal(NotificationProvider.Resend, result.Provider);
        Assert.Equal(1, first.ConsecutiveFailures);
        Assert.NotNull(second.LastSucceededAtUtc);
        Assert.True(repository.SaveCount >= 2);
    }

    [Fact]
    public async Task Amazon_Ses_Uses_SigV4_Not_Bearer()
    {
        var handler = new CaptureHandler();
        var provider = new AmazonSesNotificationProvider(new ClientFactory(handler));
        var endpoint = new NotificationProviderEndpoint(NotificationProvider.AmazonSes, null, null,
            "AKIATEST", "secret-key", "from@example.com", "Compliance", null, null, true,
            NotificationProviderAuthentication.AwsSignatureV4, Region: "us-east-1");

        var result = await provider.SendAsync(new NotificationDispatchRequest(
            Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.Email, "to@example.com", "subject", "body"), endpoint);

        Assert.True(result.Success);
        Assert.StartsWith("AWS4-HMAC-SHA256 ", handler.Authorization);
        Assert.DoesNotContain("Bearer", handler.Authorization);
    }

    [Fact]
    public async Task Smtp_Provider_Delivers_Real_Message_To_Local_Sandbox()
    {
        await using var sandbox = new LocalSmtpSandbox();
        var provider = new SmtpNotificationProvider();
        var request = new NotificationDispatchRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            NotificationChannel.Email,
            "recipient@example.test",
            "Alert Center SMTP certification",
            "<p>Durable delivery verified.</p>",
            "Durable delivery verified.");
        var endpoint = new NotificationProviderEndpoint(
            NotificationProvider.Smtp,
            "127.0.0.1",
            sandbox.Port,
            null,
            null,
            "no-reply@compliance360.test",
            "Compliance 360",
            null,
            null,
            false);

        var result = await provider.SendAsync(request, endpoint);
        var message = await sandbox.Message.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(result.Success);
        Assert.Contains("Subject: Alert Center SMTP certification", message);
        Assert.Contains("recipient@example.test", message);
        Assert.Contains("Durable delivery verified", message);
    }

    private static TenantNotificationProvider NewProvider(
        int rateLimit = 60, int threshold = 5) =>
        NewProvider(Guid.NewGuid(), NotificationProvider.SendGrid, 1, rateLimit, threshold);

    private static TenantNotificationProvider NewProvider(
        Guid tenantId, NotificationProvider kind, int priority, int rateLimit = 60, int threshold = 5) =>
        new(tenantId, kind, kind.ToString(), priority, true, NotificationProviderAuthentication.ApiKey,
            "from@example.com", "Compliance", "protected", rateLimit, threshold, 300);

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 7, 19, 12, 0, 0, TimeSpan.Zero);
    }

    private sealed class MemoryRepository : IProviderCenterRepository
    {
        private readonly List<TenantNotificationProvider> _providers;
        public MemoryRepository(params TenantNotificationProvider[] providers) => _providers = [.. providers];
        public int SaveCount { get; private set; }
        public Task<IReadOnlyCollection<TenantNotificationProvider>> ListAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<TenantNotificationProvider>>(_providers.Where(x => x.TenantId == tenantId).OrderBy(x => x.Priority).ToArray());
        public Task<TenantNotificationProvider?> GetAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_providers.SingleOrDefault(x => x.TenantId == tenantId && x.Id == id));
        public Task AddAsync(TenantNotificationProvider provider, CancellationToken cancellationToken = default) { _providers.Add(provider); return Task.CompletedTask; }
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) { SaveCount++; return Task.CompletedTask; }
    }

    private sealed class StubResolver : IProviderEndpointResolver
    {
        public Task<NotificationProviderEndpoint> ResolveAsync(TenantNotificationProvider provider, CancellationToken cancellationToken = default) =>
            Task.FromResult(new NotificationProviderEndpoint(provider.Provider, null, null, null, "masked",
                provider.FromAddress, provider.FromName, null, null, true));
    }

    private sealed class StubProvider : INotificationProvider
    {
        private readonly bool _success;
        public StubProvider(NotificationProvider provider, bool success) { Provider = provider; _success = success; }
        public NotificationProvider Provider { get; }
        public Task<NotificationDispatchResult> SendAsync(NotificationDispatchRequest request, NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default) =>
            Task.FromResult(_success ? NotificationDispatchResult.Sent(Provider, "accepted") : NotificationDispatchResult.Failed("failed", Provider));
        public Task<NotificationProviderHealth> CheckHealthAsync(NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default) =>
            Task.FromResult(new NotificationProviderHealth(Provider, _success, "test"));
    }

    private sealed class ClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;
        public ClientFactory(HttpMessageHandler handler) => _handler = handler;
        public HttpClient CreateClient(string name) => new(_handler, false);
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        public string Authorization { get; private set; } = string.Empty;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Authorization = request.Headers.GetValues("Authorization").Single();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    private sealed class LocalSmtpSandbox : IAsyncDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource _stop = new();
        private readonly Task _server;
        private readonly TaskCompletionSource<string> _message = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public LocalSmtpSandbox()
        {
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            _server = ServeAsync();
        }

        public int Port { get; }
        public Task<string> Message => _message.Task;

        private async Task ServeAsync()
        {
            try
            {
                using var client = await _listener.AcceptTcpClientAsync(_stop.Token);
                await using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.ASCII, false, 1024, leaveOpen: true);
                await using var writer = new StreamWriter(stream, Encoding.ASCII, 1024, leaveOpen: true) { NewLine = "\r\n", AutoFlush = true };
                await writer.WriteLineAsync("220 localhost Compliance360 sandbox");
                while (!_stop.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(_stop.Token);
                    if (line is null) break;
                    if (line.StartsWith("EHLO", StringComparison.OrdinalIgnoreCase)
                        || line.StartsWith("HELO", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("250-localhost");
                        await writer.WriteLineAsync("250 8BITMIME");
                    }
                    else if (line.StartsWith("MAIL FROM:", StringComparison.OrdinalIgnoreCase)
                        || line.StartsWith("RCPT TO:", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("250 OK");
                    }
                    else if (line.Equals("DATA", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("354 End data with <CRLF>.<CRLF>");
                        var data = new StringBuilder();
                        while (true)
                        {
                            var dataLine = await reader.ReadLineAsync(_stop.Token);
                            if (dataLine is null || dataLine == ".") break;
                            data.AppendLine(dataLine);
                        }
                        _message.TrySetResult(data.ToString());
                        await writer.WriteLineAsync("250 queued");
                    }
                    else if (line.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("221 bye");
                        break;
                    }
                    else
                    {
                        await writer.WriteLineAsync("250 OK");
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                _message.TrySetException(exception);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _stop.Cancel();
            _listener.Stop();
            try { await _server; } catch (OperationCanceledException) { }
            _stop.Dispose();
        }
    }
}
