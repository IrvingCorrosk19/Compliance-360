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
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("Compliance360.Enterprise", json);
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
