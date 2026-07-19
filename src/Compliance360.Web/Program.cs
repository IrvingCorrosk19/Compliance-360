using System.Text;
using System.Text.Json.Serialization;
using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using Compliance360.Application;
using Compliance360.Application.Notifications;
using Compliance360.Infrastructure;
using Compliance360.Infrastructure.Persistence;
using Compliance360.Web.Api;
using Compliance360.Web.Audit;
using Compliance360.Web.Development;
using Compliance360.Web.Errors;
using Compliance360.Web.Observability;
using Compliance360.Web.Security;
using Compliance360.Domain.Identity;
using Compliance360.Domain.Notifications;
using Compliance360.Domain.TenantManagement;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment() && !DevelopmentBootstrapRuntime.IsTestHost)
{
    DevelopmentBootstrapLogging.ConfigureBootstrapLogger(builder.Environment.EnvironmentName);
    var precheck = DevelopmentBootstrapPrecheck.Run(builder.Configuration);
    if (!precheck.CanContinue)
    {
        return;
    }
}

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
    if (builder.Environment.IsDevelopment())
    {
        DevelopmentBootstrapConsole.WriteFatal("ConnectionStrings:Compliance360 is not configured. Development bootstrap cannot continue.");
        return;
    }

    throw new InvalidOperationException("ConnectionStrings:Compliance360 must be configured before starting the API.");
}

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Compliance360.Application.RegulatoryAffairs.ICurrentUserPermissions, HttpContextCurrentUserPermissions>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddDevelopmentBootstrap();
builder.Services.AddSingleton<IObservabilityTelemetry, ObservabilityTelemetry>();
builder.Services.AddLocalization();
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
    // External delivery providers are operational integrations, not core
    // process-readiness dependencies. They remain visible through the
    // dedicated notification health endpoint without taking the API offline.
    .Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration("notification-smtp", provider => new NotificationProviderHealthCheck(provider.GetRequiredService<INotificationProviderFactory>(), provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<NotificationProviderOptions>>(), NotificationProvider.Smtp), null, ["notification", "smtp"]))
    .Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration("notification-sendgrid", provider => new NotificationProviderHealthCheck(provider.GetRequiredService<INotificationProviderFactory>(), provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<NotificationProviderOptions>>(), NotificationProvider.SendGrid), null, ["notification", "sendgrid"]))
    .Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration("notification-mailgun", provider => new NotificationProviderHealthCheck(provider.GetRequiredService<INotificationProviderFactory>(), provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<NotificationProviderOptions>>(), NotificationProvider.Mailgun), null, ["notification", "mailgun"]))
    .Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration("notification-resend", provider => new NotificationProviderHealthCheck(provider.GetRequiredService<INotificationProviderFactory>(), provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<NotificationProviderOptions>>(), NotificationProvider.Resend), null, ["notification", "resend"]))
    .AddCheck<NotificationQueueHealthCheck>("notification-queue", tags: ["ready", "notification", "queue"])
    .AddCheck<AlertCenterWorkerHealthCheck>("alert-center-worker", tags: ["notification", "worker"])
    .AddCheck<NotificationDeadLetterHealthCheck>("notification-dead-letter", tags: ["ready", "notification", "dead-letter"])
    .AddCheck<DataProtectionHealthCheck>("data-protection", tags: ["ready", "security"]);
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
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = FileUploadSecurity.MaximumFileSizeBytes + (1024 * 1024);
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    // The published application port is loopback-only. Trust the host proxy and
    // Docker bridge networks, but never arbitrary external forwarding hops.
    options.KnownProxies.Add(IPAddress.Loopback);
    options.KnownProxies.Add(IPAddress.IPv6Loopback);
    options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("172.16.0.0"), 12));
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("api", httpContext =>
    {
        var sessionId = httpContext.User.FindFirst("session_id")?.Value;
        var isAuthenticatedSession = httpContext.User.Identity?.IsAuthenticated == true
            && !string.IsNullOrWhiteSpace(sessionId);
        var partitionKey = isAuthenticatedSession
            ? $"session:{sessionId}"
            : $"ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous"}";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Environment.IsDevelopment() ? 2000 : isAuthenticatedSession ? 600 : 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
    options.AddPolicy("authentication", httpContext =>
    {
        // Keep credential endpoints bounded without treating every user behind
        // the same corporate NAT/proxy as one login attempt stream. Endpoint
        // partitioning also prevents refresh traffic from starving new logins.
        var endpoint = httpContext.Request.Path.Value?.ToLowerInvariant() ?? "/auth";
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        return RateLimitPartition.GetSlidingWindowLimiter(
            $"auth:{endpoint}:ip:{clientIp}",
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = builder.Environment.IsDevelopment() ? 300 : 120,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueLimit = 0
            });
    });
});
builder.Services.AddAuthorization(options => options.AddCompliancePolicies());

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
if (string.IsNullOrWhiteSpace(jwtOptions?.SigningKey))
{
    if (builder.Environment.IsDevelopment())
    {
        DevelopmentBootstrapConsole.WriteFatal("Jwt:SigningKey is not configured. Development bootstrap cannot continue.");
        return;
    }

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
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var tenantClaim = context.Principal?.FindFirst("tenant_id")?.Value;
                var sessionClaim = context.Principal?.FindFirst("session_id")?.Value;
                if (!Guid.TryParse(sessionClaim, out var sessionId))
                {
                    if (builder.Environment.IsProduction())
                    {
                        context.Fail("A valid session-bound token is required.");
                    }

                    return;
                }

                if (!Guid.TryParse(tenantClaim, out var tenantId))
                {
                    context.Fail("Invalid tenant context.");
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<Compliance360DbContext>();
                var now = DateTimeOffset.UtcNow;
                var valid = await (
                    from session in db.UserSessions.AsNoTracking()
                    join user in db.Users.AsNoTracking() on session.UserId equals user.Id
                    join tenant in db.Tenants.AsNoTracking() on session.TenantId equals tenant.Id
                    where session.Id == sessionId
                        && session.TenantId == tenantId
                        && session.RevokedAtUtc == null
                        && session.ExpiresAtUtc > now
                        && user.Status == UserStatus.Active
                        && tenant.Status == TenantStatus.Active
                    select session.Id).AnyAsync(context.HttpContext.RequestAborted);
                if (!valid)
                {
                    context.Fail("Session is no longer active.");
                }
            }
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment() && !DevelopmentBootstrapRuntime.IsTestHost)
{
    var bootstrap = app.Services.GetRequiredService<DevelopmentBootstrapRunner>();
    var bootstrapResult = await bootstrap.RunAsync(app.Lifetime.ApplicationStopping);
    if (!bootstrapResult.CanStart)
    {
        return;
    }
}

app.UseForwardedHeaders();
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
var supportedCultures = new[] { new CultureInfo("es-PA"), new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-PA"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    RequestCultureProviders =
    [
        new CustomRequestCultureProvider(context =>
        {
            var language = context.Request.Cookies["c360.language"];
            var culture = language == "en" ? "en-US" : "es-PA";
            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture, culture));
        }),
        new AcceptLanguageHeaderRequestCultureProvider()
    ]
});
app.UseDefaultFiles();
app.UseStaticFiles();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("compliance360-cors");
app.UseAuthentication();
app.UseRateLimiter();
app.UseMiddleware<AuditContextMiddleware>();
app.UseMiddleware<ObservabilityMiddleware>();
app.UseAuthorization();

static IResult LivenessResult() => Results.Ok(new
{
    service = "Compliance360.Enterprise",
    status = "Healthy",
    utc = DateTimeOffset.UtcNow
});

app.MapGet("/health", LivenessResult);
app.MapPrometheusScrapingEndpoint("/metrics");

app.MapFoundationApi();
app.MapObservabilityEndpoints();
app.MapFallback(async context =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = "API endpoint not found." });
        return;
    }

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
});

await app.RunAsync();

public partial class Program;
