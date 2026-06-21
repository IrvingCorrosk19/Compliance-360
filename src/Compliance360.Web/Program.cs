using System.Text;
using System.Threading.RateLimiting;
using Compliance360.Application;
using Compliance360.Infrastructure;
using Compliance360.Web.Api;
using Compliance360.Web.Audit;
using Compliance360.Web.Errors;
using Compliance360.Web.Observability;
using Compliance360.Web.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("ServiceName", ObservabilityTelemetry.ServiceName)
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .WriteTo.Console(new RenderedCompactJsonFormatter());
});

builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
    options.ParseStateValues = true;
    options.SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService(ObservabilityTelemetry.ServiceName, serviceVersion: "v0.19.0-observability-enterprise")
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName
        }));
});

if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Compliance360")))
{
    throw new InvalidOperationException("ConnectionStrings:Compliance360 must be configured before starting the API.");
}

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IObservabilityTelemetry, ObservabilityTelemetry>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT access token."
    });
});
builder.Services.AddHealthChecks()
    .AddCheck<ApplicationHealthCheck>("application", tags: ["live", "ready"])
    .AddCheck<PostgreSqlHealthCheck>("postgresql", tags: ["ready", "database"])
    .AddCheck<StorageHealthCheck>("storage", tags: ["ready", "storage"])
    .AddCheck<NotificationHealthCheck>("notification", tags: ["ready", "notification"])
    .AddCheck<DataProtectionHealthCheck>("data-protection", tags: ["ready", "security"])
    .AddCheck<ReportingHealthCheck>("reporting", tags: ["ready", "reporting"])
    .AddCheck<WorkflowHealthCheck>("workflow", tags: ["ready", "workflow"]);
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(ObservabilityTelemetry.ServiceName, serviceVersion: "v0.19.0-observability-enterprise")
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName
        }))
    .WithTracing(tracing => tracing
        .AddSource(ObservabilityTelemetry.ActivitySourceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation())
    .WithMetrics(metrics => metrics
        .AddMeter(ObservabilityTelemetry.MeterName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());
builder.Services.AddCors(options =>
{
    options.AddPolicy("compliance360-cors", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (allowedOrigins.Length > 0)
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("api", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});
builder.Services.AddAuthorization(options => options.AddCompliancePolicies());

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
if (string.IsNullOrWhiteSpace(jwtOptions?.SigningKey))
{
    throw new InvalidOperationException("Jwt:SigningKey must be configured through secure configuration before starting the API.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
        };
    });

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseSerilogRequestLogging();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseCors("compliance360-cors");
app.UseRateLimiter();
app.UseMiddleware<AuditContextMiddleware>();
app.UseAuthentication();
app.UseMiddleware<ObservabilityMiddleware>();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new
{
    service = "Compliance360.Enterprise",
    status = "Healthy",
    utc = DateTimeOffset.UtcNow
}));
app.MapPrometheusScrapingEndpoint("/metrics");

app.MapFoundationApi();
app.MapObservabilityEndpoints();
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
