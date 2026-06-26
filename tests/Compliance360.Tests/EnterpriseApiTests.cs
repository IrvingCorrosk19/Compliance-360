using System.Net;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Compliance360.Tests;

public sealed class EnterpriseApiTests
{
    [Fact]
    public async Task Health_Endpoint_Returns_Enterprise_Status_And_Security_Headers()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.True(response.Headers.Contains("Referrer-Policy"));
        Assert.True(response.Headers.Contains("Permissions-Policy"));
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("Compliance360.Enterprise", json);
    }

    [Fact]
    public async Task Hsts_And_Api_NoStore_Cache_Headers_Are_Present_For_Sensitive_Routes()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        var health = await client.GetAsync("/health");
        var protectedApi = await client.PostAsJsonAsync("/api/v1/tenants", new CreateTenantPayload("Acme", "acme"));

        Assert.True(health.Headers.Contains("Strict-Transport-Security"));
        Assert.True(protectedApi.Headers.CacheControl?.NoStore);
        Assert.Contains("no-cache", protectedApi.Headers.Pragma.ToString());
    }

    [Fact]
    public async Task Cors_Does_Not_Allow_Unconfigured_Origin()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/tenants");
        request.Headers.Add("Origin", "https://evil.example");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public void Frontend_Csp_Surface_Does_Not_Use_Inline_Handlers_Or_Styles()
    {
        var appJs = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Compliance360.Web", "wwwroot", "app.js"));

        Assert.DoesNotContain("onclick=", appJs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("style=", appJs, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Design_Time_DbContext_Factory_Does_Not_Contain_Hardcoded_Postgres_Password()
    {
        var factoryCode = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Compliance360.Infrastructure", "Persistence", "Compliance360DbContextFactory.cs"));

        Assert.DoesNotContain("Password=postgres", factoryCode, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Username=postgres;Password", factoryCode, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Swagger_Endpoint_Is_Available()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("/api/v1/auth/login", json);
        Assert.Contains("/api/v1/tenants", json);
        Assert.Contains("/api/v1/superadmin/platform-center", json);
        Assert.Contains("/api/v1/observability/telemetry", json);
    }

    [Fact]
    public async Task Protected_Api_Endpoint_Requires_Authentication()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.PostAsJsonAsync("/api/v1/tenants", new CreateTenantPayload("Acme", "acme"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Observability_Api_Requires_Authentication()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/api/v1/observability/telemetry");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SuperAdmin_Platform_Center_Requires_Authentication()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/api/v1/superadmin/platform-center");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Observability_Api_Returns_Correlation_Trace_And_Resource_Metadata()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateToken("OBSERVABILITY.READ"));
        client.DefaultRequestHeaders.Add("X-Correlation-Id", "corr-observability-test");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TestTenantId.ToString());

        var response = await client.GetAsync("/api/v1/observability/telemetry");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-Id"));
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("corr-observability-test", json);
        Assert.Contains("Compliance360.Enterprise", json);
        Assert.Contains("traceId", json);
        Assert.Contains("spanId", json);
    }

    [Fact]
    public async Task Metrics_Endpoint_Exposes_OpenTelemetry_Prometheus_Metrics()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        await client.GetAsync("/health");
        var response = await client.GetAsync("/metrics");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var metrics = await response.Content.ReadAsStringAsync();
        Assert.Contains("compliance360_api_requests_total", metrics);
    }

    [Fact]
    public async Task Health_Live_And_Ready_Endpoints_Are_Available()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var live = await client.GetAsync("/health/live");
        var ready = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, live.StatusCode);
        Assert.True(ready.StatusCode is HttpStatusCode.OK or HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Observability_Dashboards_Alerts_And_Tenant_Metrics_Are_Available()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateToken("OBSERVABILITY.READ"));
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TestTenantId.ToString());

        await client.GetAsync("/health");
        var metrics = await client.GetAsync("/api/v1/observability/metrics/summary");
        var operationalDashboard = await client.GetAsync("/api/v1/observability/dashboards/operational");
        var performanceDashboard = await client.GetAsync("/api/v1/observability/dashboards/performance");
        var securityDashboard = await client.GetAsync("/api/v1/observability/dashboards/security");
        var tenantDashboard = await client.GetAsync("/api/v1/observability/dashboards/tenants");
        var alerts = await client.GetAsync("/api/v1/observability/alerts");

        Assert.Equal(HttpStatusCode.OK, metrics.StatusCode);
        Assert.Equal(HttpStatusCode.OK, operationalDashboard.StatusCode);
        Assert.Equal(HttpStatusCode.OK, performanceDashboard.StatusCode);
        Assert.Equal(HttpStatusCode.OK, securityDashboard.StatusCode);
        Assert.Equal(HttpStatusCode.OK, tenantDashboard.StatusCode);
        Assert.Equal(HttpStatusCode.OK, alerts.StatusCode);
        Assert.Contains(TestTenantId.ToString(), await tenantDashboard.Content.ReadAsStringAsync());
        Assert.Contains("High Error Rate", await alerts.Content.ReadAsStringAsync());
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "test-signing-key-with-at-least-32-characters");
        Environment.SetEnvironmentVariable("ConnectionStrings__Compliance360", "Host=localhost;Database=compliance360_tests;Username=test;Password=test");

        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Jwt:SigningKey"] = "test-signing-key-with-at-least-32-characters",
                        ["ConnectionStrings:Compliance360"] = "Host=localhost;Database=compliance360_tests;Username=test;Password=test"
                    });
                });
            });
    }

    private static readonly Guid TestTenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TestUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static string CreateToken(params string[] permissions)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, TestUserId.ToString()),
            new("sub", TestUserId.ToString()),
            new("tenant_id", TestTenantId.ToString()),
            new("session_id", Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc").ToString())
        };
        claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-signing-key-with-at-least-32-characters"));
        var token = new JwtSecurityToken(
            issuer: "Compliance360",
            audience: "Compliance360.Web",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record CreateTenantPayload(string Name, string Slug);
}
