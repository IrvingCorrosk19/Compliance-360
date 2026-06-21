using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

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
    }

    [Fact]
    public async Task Protected_Api_Endpoint_Requires_Authentication()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.PostAsJsonAsync("/api/v1/tenants", new CreateTenantPayload("Acme", "acme"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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

    private sealed record CreateTenantPayload(string Name, string Slug);
}
