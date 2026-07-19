using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using Compliance360.Application;
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
    private readonly IProviderCenterRepository? _providerRepository;
    private readonly IProviderEndpointResolver? _endpointResolver;
    private readonly IClock? _clock;

    public EnterpriseNotificationDispatcher(
        INotificationProviderFactory providerFactory,
        IOptions<NotificationProviderOptions> options,
        ILogger<EnterpriseNotificationDispatcher> logger)
    {
        _providerFactory = providerFactory;
        _options = options;
        _logger = logger;
    }

    public EnterpriseNotificationDispatcher(
        INotificationProviderFactory providerFactory,
        IOptions<NotificationProviderOptions> options,
        ILogger<EnterpriseNotificationDispatcher> logger,
        IProviderCenterRepository providerRepository,
        IProviderEndpointResolver endpointResolver,
        IClock clock)
        : this(providerFactory, options, logger)
    {
        _providerRepository = providerRepository;
        _endpointResolver = endpointResolver;
        _clock = clock;
    }

    public async Task<NotificationDispatchResult> DispatchAsync(NotificationDispatchRequest request, CancellationToken cancellationToken = default)
    {
        if (_providerRepository is not null && _endpointResolver is not null && _clock is not null)
        {
            var tenantProviders = await _providerRepository.ListAsync(request.TenantId, cancellationToken);
            if (tenantProviders.Count > 0)
            {
                foreach (var configured in tenantProviders.Where(x => x.IsEnabled).OrderBy(x => x.Priority))
                {
                    if (!configured.TryAcquire(_clock.UtcNow))
                    {
                        continue;
                    }

                    NotificationDispatchResult result;
                    try
                    {
                        var endpoint = await _endpointResolver.ResolveAsync(configured, cancellationToken);
                        result = await _providerFactory.GetProvider(configured.Provider).SendAsync(request, endpoint, cancellationToken);
                    }
                    catch (Exception exception) when (exception is not OperationCanceledException)
                    {
                        _logger.LogWarning("Tenant notification provider {ProviderId} failed for message {MessageId}; details were suppressed.", configured.Id, request.MessageId);
                        result = NotificationDispatchResult.Failed("Provider dispatch failed.", configured.Provider);
                    }

                    if (result.Success)
                    {
                        configured.RecordSuccess(_clock.UtcNow);
                        await _providerRepository.SaveChangesAsync(cancellationToken);
                        return result;
                    }

                    configured.RecordFailure("dispatch_failed", _clock.UtcNow);
                    await _providerRepository.SaveChangesAsync(cancellationToken);
                }

                return NotificationDispatchResult.Failed("All tenant notification providers were unavailable or failed.");
            }
        }

        // Transitional fallback for tenants without Provider Center records.
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

public sealed class GmailSmtpNotificationProvider : SmtpCompatibleNotificationProvider
{
    public override NotificationProvider Provider => NotificationProvider.GmailSmtp;
}

public sealed class Microsoft365NotificationProvider : INotificationProvider
{
    private readonly IHttpClientFactory _httpClientFactory;

    public Microsoft365NotificationProvider(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public NotificationProvider Provider => NotificationProvider.Microsoft365;

    public async Task<NotificationDispatchResult> SendAsync(NotificationDispatchRequest request, NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        if (endpoint.Authentication != NotificationProviderAuthentication.OAuth2ClientCredentials
            || string.IsNullOrWhiteSpace(endpoint.ClientId)
            || string.IsNullOrWhiteSpace(endpoint.DirectoryTenantId)
            || HttpNotificationProvider.IsPlaceholder(endpoint.Secret)
            || string.IsNullOrWhiteSpace(endpoint.FromAddress))
        {
            return NotificationDispatchResult.Failed("Microsoft 365 Graph OAuth2 configuration is incomplete.", Provider);
        }

        using var client = _httpClientFactory.CreateClient("notifications-Microsoft365");
        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post,
            $"https://login.microsoftonline.com/{Uri.EscapeDataString(endpoint.DirectoryTenantId)}/oauth2/v2.0/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = endpoint.ClientId,
                ["client_secret"] = endpoint.Secret!,
                ["scope"] = "https://graph.microsoft.com/.default",
                ["grant_type"] = "client_credentials"
            })
        };
        using var tokenResponse = await client.SendAsync(tokenRequest, cancellationToken);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            return NotificationDispatchResult.Failed($"Microsoft identity returned HTTP {(int)tokenResponse.StatusCode}.", Provider);
        }
        using var tokenJson = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync(cancellationToken));
        if (!tokenJson.RootElement.TryGetProperty("access_token", out var tokenElement))
        {
            return NotificationDispatchResult.Failed("Microsoft identity response did not contain an access token.", Provider);
        }

        var graphBase = endpoint.BaseUrl?.TrimEnd('/') ?? "https://graph.microsoft.com/v1.0";
        using var graphRequest = new HttpRequestMessage(HttpMethod.Post,
            $"{graphBase}/users/{Uri.EscapeDataString(endpoint.FromAddress)}/sendMail");
        graphRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenElement.GetString());
        graphRequest.Content = HttpNotificationProvider.Json(new
        {
            message = new
            {
                subject = request.Subject,
                body = new { contentType = "HTML", content = request.Body },
                toRecipients = new[] { new { emailAddress = new { address = request.Recipient } } }
            },
            saveToSentItems = true
        });
        using var response = await client.SendAsync(graphRequest, cancellationToken);
        return response.IsSuccessStatusCode
            ? NotificationDispatchResult.Sent(Provider, request.MessageId.ToString())
            : NotificationDispatchResult.Failed($"Microsoft Graph returned HTTP {(int)response.StatusCode}.", Provider);
    }

    public async Task<NotificationProviderHealth> CheckHealthAsync(NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        var healthy = endpoint.Authentication == NotificationProviderAuthentication.OAuth2ClientCredentials
            && !string.IsNullOrWhiteSpace(endpoint.ClientId) && !string.IsNullOrWhiteSpace(endpoint.DirectoryTenantId)
            && !HttpNotificationProvider.IsPlaceholder(endpoint.Secret) && !string.IsNullOrWhiteSpace(endpoint.FromAddress);
        if (!healthy)
        {
            return new NotificationProviderHealth(Provider, false, "Microsoft Graph OAuth2 configuration is incomplete.");
        }
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post,
                $"https://login.microsoftonline.com/{Uri.EscapeDataString(endpoint.DirectoryTenantId!)}/oauth2/v2.0/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = endpoint.ClientId!,
                    ["client_secret"] = endpoint.Secret!,
                    ["scope"] = "https://graph.microsoft.com/.default",
                    ["grant_type"] = "client_credentials"
                })
            };
            using var response = await _httpClientFactory.CreateClient("notifications-Microsoft365").SendAsync(request, cancellationToken);
            return new NotificationProviderHealth(Provider, response.IsSuccessStatusCode,
                response.IsSuccessStatusCode ? "Microsoft identity accepted the Graph credentials." : $"Microsoft identity returned HTTP {(int)response.StatusCode}.");
        }
        catch (Exception exception) when (exception is HttpRequestException or OperationCanceledException)
        {
            return new NotificationProviderHealth(Provider, false, "Microsoft Graph connection test failed.");
        }
    }
}

public sealed class ExchangeOnlineNotificationProvider : SmtpCompatibleNotificationProvider
{
    public override NotificationProvider Provider => NotificationProvider.ExchangeOnline;
}

public abstract class SmtpCompatibleNotificationProvider : INotificationProvider
{
    public abstract NotificationProvider Provider { get; }

    public async Task<NotificationDispatchResult> SendAsync(NotificationDispatchRequest request, NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        var smtp = new SmtpNotificationProvider();
        var result = await smtp.SendAsync(request, endpoint, cancellationToken);
        return result.Success
            ? NotificationDispatchResult.Sent(Provider, result.ProviderMessageId)
            : NotificationDispatchResult.Failed(result.FailureReason ?? $"{Provider} failed.", Provider);
    }

    public Task<NotificationProviderHealth> CheckHealthAsync(NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        var healthy = !string.IsNullOrWhiteSpace(endpoint.Host) && endpoint.Port.HasValue && !string.IsNullOrWhiteSpace(endpoint.FromAddress);
        return Task.FromResult(new NotificationProviderHealth(Provider, healthy, healthy ? $"{Provider} SMTP configuration is valid." : $"{Provider} SMTP configuration is incomplete."));
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

public sealed class AmazonSesNotificationProvider : INotificationProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    public AmazonSesNotificationProvider(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;
    public NotificationProvider Provider => NotificationProvider.AmazonSes;

    public async Task<NotificationDispatchResult> SendAsync(NotificationDispatchRequest request, NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        if (endpoint.Authentication != NotificationProviderAuthentication.AwsSignatureV4
            || string.IsNullOrWhiteSpace(endpoint.Username) || HttpNotificationProvider.IsPlaceholder(endpoint.Secret)
            || string.IsNullOrWhiteSpace(endpoint.FromAddress))
        {
            return NotificationDispatchResult.Failed("Amazon SES SigV4 configuration is incomplete.", Provider);
        }

        var region = string.IsNullOrWhiteSpace(endpoint.Region) ? "us-east-1" : endpoint.Region;
        var uri = new Uri(endpoint.BaseUrl?.TrimEnd('/') ?? $"https://email.{region}.amazonaws.com/v2/email/outbound-emails");
        var payload = JsonSerializer.Serialize(new
        {
            FromEmailAddress = endpoint.FromAddress,
            Destination = new { ToAddresses = new[] { request.Recipient } },
            Content = new { Simple = new { Subject = new { Data = request.Subject }, Body = new { Html = new { Data = request.Body }, Text = new { Data = request.TextBody ?? request.Body } } } }
        });
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri) { Content = new StringContent(payload, Encoding.UTF8, "application/json") };
        AwsSigV4.Sign(httpRequest, payload, endpoint.Username, endpoint.Secret!, region, "ses", DateTimeOffset.UtcNow);
        using var response = await _httpClientFactory.CreateClient("notifications-AmazonSes").SendAsync(httpRequest, cancellationToken);
        var messageId = response.Headers.TryGetValues("x-amzn-requestid", out var values) ? values.FirstOrDefault() : request.MessageId.ToString();
        return response.IsSuccessStatusCode
            ? NotificationDispatchResult.Sent(Provider, messageId)
            : NotificationDispatchResult.Failed($"Amazon SES returned HTTP {(int)response.StatusCode}.", Provider);
    }

    public async Task<NotificationProviderHealth> CheckHealthAsync(NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        var healthy = endpoint.Authentication == NotificationProviderAuthentication.AwsSignatureV4
            && !string.IsNullOrWhiteSpace(endpoint.Username) && !HttpNotificationProvider.IsPlaceholder(endpoint.Secret)
            && !string.IsNullOrWhiteSpace(endpoint.FromAddress);
        if (!healthy)
        {
            return new NotificationProviderHealth(Provider, false, "Amazon SES SigV4 configuration is incomplete.");
        }
        var region = string.IsNullOrWhiteSpace(endpoint.Region) ? "us-east-1" : endpoint.Region;
        var uri = new Uri(endpoint.BaseUrl?.TrimEnd('/') ?? $"https://email.{region}.amazonaws.com/v2/email/account");
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        AwsSigV4.Sign(request, string.Empty, endpoint.Username!, endpoint.Secret!, region, "ses", DateTimeOffset.UtcNow);
        try
        {
            using var response = await _httpClientFactory.CreateClient("notifications-AmazonSes").SendAsync(request, cancellationToken);
            return new NotificationProviderHealth(Provider, response.IsSuccessStatusCode,
                response.IsSuccessStatusCode ? "Amazon SES accepted the SigV4 credentials." : $"Amazon SES returned HTTP {(int)response.StatusCode}.");
        }
        catch (Exception exception) when (exception is HttpRequestException or OperationCanceledException)
        {
            return new NotificationProviderHealth(Provider, false, "Amazon SES connection test failed.");
        }
    }
}

internal static class AwsSigV4
{
    public static void Sign(HttpRequestMessage request, string payload, string accessKey, string secretKey, string region, string service, DateTimeOffset now)
    {
        var timestamp = now.UtcDateTime.ToString("yyyyMMdd'T'HHmmss'Z'");
        var date = now.UtcDateTime.ToString("yyyyMMdd");
        var payloadHash = Hex(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
        request.Headers.Host = request.RequestUri!.Authority;
        request.Headers.TryAddWithoutValidation("x-amz-date", timestamp);
        request.Headers.TryAddWithoutValidation("x-amz-content-sha256", payloadHash);

        const string signedHeaders = "host;x-amz-content-sha256;x-amz-date";
        var canonical = $"{request.Method.Method}\n{request.RequestUri.AbsolutePath}\n{request.RequestUri.Query.TrimStart('?')}\n" +
            $"host:{request.RequestUri.Authority}\nx-amz-content-sha256:{payloadHash}\nx-amz-date:{timestamp}\n\n{signedHeaders}\n{payloadHash}";
        var scope = $"{date}/{region}/{service}/aws4_request";
        var stringToSign = $"AWS4-HMAC-SHA256\n{timestamp}\n{scope}\n{Hex(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))}";
        var signingKey = Hmac(Hmac(Hmac(Hmac(Encoding.UTF8.GetBytes("AWS4" + secretKey), date), region), service), "aws4_request");
        var signature = Hex(Hmac(signingKey, stringToSign));
        request.Headers.TryAddWithoutValidation("Authorization",
            $"AWS4-HMAC-SHA256 Credential={accessKey}/{scope}, SignedHeaders={signedHeaders}, Signature={signature}");
    }

    private static byte[] Hmac(byte[] key, string value) => new HMACSHA256(key).ComputeHash(Encoding.UTF8.GetBytes(value));
    private static string Hex(byte[] bytes) => Convert.ToHexString(bytes).ToLowerInvariant();
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
        if (IsPlaceholder(endpoint.Secret) || string.IsNullOrWhiteSpace(endpoint.FromAddress))
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

    public async Task<NotificationProviderHealth> CheckHealthAsync(NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        if (IsPlaceholder(endpoint.Secret) || string.IsNullOrWhiteSpace(endpoint.FromAddress))
        {
            return new NotificationProviderHealth(Provider, false, $"{Provider} provider is missing API secret or from address.");
        }

        var baseUrl = endpoint.BaseUrl?.TrimEnd('/');
        var uri = Provider switch
        {
            NotificationProvider.SendGrid => $"{baseUrl ?? "https://api.sendgrid.com"}/v3/user/profile",
            NotificationProvider.Mailgun => $"{baseUrl ?? "https://api.mailgun.net"}/v3/domains/{Uri.EscapeDataString(endpoint.Domain ?? string.Empty)}",
            NotificationProvider.Resend => $"{baseUrl ?? "https://api.resend.com"}/domains",
            _ => baseUrl
        };
        if (string.IsNullOrWhiteSpace(uri))
        {
            return new NotificationProviderHealth(Provider, false, $"{Provider} connection test endpoint is unavailable.");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = Provider == NotificationProvider.Mailgun
                ? new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{endpoint.Secret}")))
                : new AuthenticationHeaderValue("Bearer", endpoint.Secret);
            using var response = await _httpClientFactory.CreateClient($"notifications-{Provider}").SendAsync(request, cancellationToken);
            return new NotificationProviderHealth(Provider, response.IsSuccessStatusCode,
                response.IsSuccessStatusCode ? $"{Provider} credentials were accepted." : $"{Provider} connection test returned HTTP {(int)response.StatusCode}.");
        }
        catch (Exception exception) when (exception is HttpRequestException or OperationCanceledException)
        {
            return new NotificationProviderHealth(Provider, false, $"{Provider} connection test failed.");
        }
    }

    protected abstract HttpRequestMessage CreateRequest(NotificationDispatchRequest request, NotificationProviderEndpoint endpoint);

    internal static StringContent Json(object payload)
    {
        return new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
    }

    internal static bool IsPlaceholder(string? secret) =>
        string.IsNullOrWhiteSpace(secret)
        || secret.Contains("placeholder", StringComparison.OrdinalIgnoreCase)
        || secret.Equals("not-configured", StringComparison.OrdinalIgnoreCase);
}
