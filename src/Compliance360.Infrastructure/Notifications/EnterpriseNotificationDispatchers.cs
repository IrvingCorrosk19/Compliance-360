using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using Compliance360.Application.Notifications;
using Compliance360.Domain.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Compliance360.Infrastructure.Notifications;

public sealed class EnterpriseNotificationDispatcher : INotificationDispatcher
{
    private readonly INotificationProviderFactory _providerFactory;
    private readonly IOptions<NotificationProviderOptions> _options;
    private readonly ILogger<EnterpriseNotificationDispatcher> _logger;

    public EnterpriseNotificationDispatcher(
        INotificationProviderFactory providerFactory,
        IOptions<NotificationProviderOptions> options,
        ILogger<EnterpriseNotificationDispatcher> logger)
    {
        _providerFactory = providerFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<NotificationDispatchResult> DispatchAsync(NotificationDispatchRequest request, CancellationToken cancellationToken = default)
    {
        var configuredProviders = OrderedProviders();
        foreach (var provider in configuredProviders)
        {
            var endpoint = Endpoint(provider);
            var dispatchProvider = _providerFactory.GetProvider(provider);
            var result = await dispatchProvider.SendAsync(request, endpoint, cancellationToken);
            if (result.Success)
            {
                return result;
            }

            _logger.LogWarning("Notification provider {Provider} failed for message {MessageId}: {FailureReason}", provider, request.MessageId, result.FailureReason);
        }

        return NotificationDispatchResult.Failed("All configured notification providers failed.");
    }

    private IReadOnlyCollection<NotificationProvider> OrderedProviders()
    {
        var providers = _options.Value.Providers
            .OrderBy(provider => provider.Value == null ? int.MaxValue : 0)
            .Select(provider => provider.Key)
            .ToList();

        if (!providers.Contains(_options.Value.DefaultProvider))
        {
            providers.Insert(0, _options.Value.DefaultProvider);
        }

        if (_options.Value.FailoverProvider.HasValue && !providers.Contains(_options.Value.FailoverProvider.Value))
        {
            providers.Add(_options.Value.FailoverProvider.Value);
        }

        return providers;
    }

    private NotificationProviderEndpoint Endpoint(NotificationProvider provider)
    {
        _options.Value.Providers.TryGetValue(provider, out var options);
        return new NotificationProviderEndpoint(
            provider,
            options?.Host,
            options?.Port,
            options?.Username,
            options?.Secret,
            options?.FromAddress,
            options?.FromName,
            options?.BaseUrl,
            options?.Domain,
            options?.UseSsl ?? true);
    }
}

public sealed class NotificationProviderFactory : INotificationProviderFactory
{
    private readonly IReadOnlyDictionary<NotificationProvider, INotificationProvider> _providers;

    public NotificationProviderFactory(IEnumerable<INotificationProvider> providers)
    {
        _providers = providers.ToDictionary(provider => provider.Provider);
    }

    public INotificationProvider GetProvider(NotificationProvider provider)
    {
        return _providers.TryGetValue(provider, out var implementation)
            ? implementation
            : throw new InvalidOperationException($"Notification provider '{provider}' is not registered.");
    }
}

public sealed class SmtpNotificationProvider : INotificationProvider
{
    public NotificationProvider Provider => NotificationProvider.Smtp;

    public async Task<NotificationDispatchResult> SendAsync(NotificationDispatchRequest request, NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(endpoint.Host) || !endpoint.Port.HasValue || string.IsNullOrWhiteSpace(endpoint.FromAddress))
        {
            return NotificationDispatchResult.Failed("SMTP provider is not fully configured.", Provider);
        }

        using var message = new MailMessage
        {
            From = new MailAddress(endpoint.FromAddress, endpoint.FromName),
            Subject = request.Subject,
            Body = request.Body,
            IsBodyHtml = true
        };
        message.To.Add(request.Recipient);
        if (!string.IsNullOrWhiteSpace(request.TextBody))
        {
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(request.TextBody, Encoding.UTF8, "text/plain"));
        }

        using var client = new SmtpClient(endpoint.Host, endpoint.Port.Value)
        {
            EnableSsl = endpoint.UseSsl
        };
        if (!string.IsNullOrWhiteSpace(endpoint.Username))
        {
            client.Credentials = new NetworkCredential(endpoint.Username, endpoint.Secret);
        }

        await client.SendMailAsync(message, cancellationToken);
        return NotificationDispatchResult.Sent(Provider, request.MessageId.ToString());
    }

    public Task<NotificationProviderHealth> CheckHealthAsync(NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        var healthy = !string.IsNullOrWhiteSpace(endpoint.Host) && endpoint.Port.HasValue && !string.IsNullOrWhiteSpace(endpoint.FromAddress);
        return Task.FromResult(new NotificationProviderHealth(Provider, healthy, healthy ? "SMTP provider is configured." : "SMTP provider is missing host, port, or from address."));
    }
}

public sealed class SendGridNotificationProvider : HttpNotificationProvider
{
    public SendGridNotificationProvider(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory)
    {
    }

    public override NotificationProvider Provider => NotificationProvider.SendGrid;

    protected override HttpRequestMessage CreateRequest(NotificationDispatchRequest request, NotificationProviderEndpoint endpoint)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{endpoint.BaseUrl?.TrimEnd('/') ?? "https://api.sendgrid.com"}/v3/mail/send");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", endpoint.Secret);
        httpRequest.Content = Json(new
        {
            personalizations = new[] { new { to = new[] { new { email = request.Recipient } } } },
            from = new { email = endpoint.FromAddress, name = endpoint.FromName },
            subject = request.Subject,
            content = new[] { new { type = "text/html", value = request.Body } }
        });
        return httpRequest;
    }
}

public sealed class MailgunNotificationProvider : HttpNotificationProvider
{
    public MailgunNotificationProvider(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory)
    {
    }

    public override NotificationProvider Provider => NotificationProvider.Mailgun;

    protected override HttpRequestMessage CreateRequest(NotificationDispatchRequest request, NotificationProviderEndpoint endpoint)
    {
        var domain = endpoint.Domain ?? throw new InvalidOperationException("Mailgun domain is required.");
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{endpoint.BaseUrl?.TrimEnd('/') ?? "https://api.mailgun.net"}/v3/{domain}/messages");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{endpoint.Secret}")));
        httpRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["from"] = $"{endpoint.FromName} <{endpoint.FromAddress}>",
            ["to"] = request.Recipient,
            ["subject"] = request.Subject,
            ["html"] = request.Body,
            ["text"] = request.TextBody ?? request.Body
        });
        return httpRequest;
    }
}

public sealed class ResendNotificationProvider : HttpNotificationProvider
{
    public ResendNotificationProvider(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory)
    {
    }

    public override NotificationProvider Provider => NotificationProvider.Resend;

    protected override HttpRequestMessage CreateRequest(NotificationDispatchRequest request, NotificationProviderEndpoint endpoint)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{endpoint.BaseUrl?.TrimEnd('/') ?? "https://api.resend.com"}/emails");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", endpoint.Secret);
        httpRequest.Content = Json(new
        {
            from = $"{endpoint.FromName} <{endpoint.FromAddress}>",
            to = new[] { request.Recipient },
            subject = request.Subject,
            html = request.Body,
            text = request.TextBody
        });
        return httpRequest;
    }
}

public abstract class HttpNotificationProvider : INotificationProvider
{
    private readonly IHttpClientFactory _httpClientFactory;

    protected HttpNotificationProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public abstract NotificationProvider Provider { get; }

    public async Task<NotificationDispatchResult> SendAsync(NotificationDispatchRequest request, NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(endpoint.Secret) || string.IsNullOrWhiteSpace(endpoint.FromAddress))
        {
            return NotificationDispatchResult.Failed($"{Provider} provider is not fully configured.", Provider);
        }

        using var client = _httpClientFactory.CreateClient($"notifications-{Provider}");
        using var httpRequest = CreateRequest(request, endpoint);
        using var response = await client.SendAsync(httpRequest, cancellationToken);
        var providerMessageId = response.Headers.TryGetValues("X-Message-Id", out var values)
            ? values.FirstOrDefault()
            : request.MessageId.ToString();

        return response.IsSuccessStatusCode
            ? NotificationDispatchResult.Sent(Provider, providerMessageId)
            : NotificationDispatchResult.Failed($"{Provider} returned HTTP {(int)response.StatusCode}.", Provider);
    }

    public Task<NotificationProviderHealth> CheckHealthAsync(NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        var healthy = !string.IsNullOrWhiteSpace(endpoint.Secret) && !string.IsNullOrWhiteSpace(endpoint.FromAddress);
        return Task.FromResult(new NotificationProviderHealth(Provider, healthy, healthy ? $"{Provider} provider is configured." : $"{Provider} provider is missing API secret or from address."));
    }

    protected abstract HttpRequestMessage CreateRequest(NotificationDispatchRequest request, NotificationProviderEndpoint endpoint);

    protected static StringContent Json(object payload)
    {
        return new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
    }
}
