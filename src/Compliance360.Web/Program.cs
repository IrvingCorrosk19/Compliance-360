using System.Text;
using System.Threading.RateLimiting;
using Compliance360.Application;
using Compliance360.Infrastructure;
using Compliance360.Web.Api;
using Compliance360.Web.Audit;
using Compliance360.Web.Errors;
using Compliance360.Web.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Compliance360")))
{
    throw new InvalidOperationException("ConnectionStrings:Compliance360 must be configured before starting the API.");
}

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
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
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseMiddleware<AuditContextMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live");
app.MapGet("/health", () => Results.Ok(new
{
    service = "Compliance360.Enterprise",
    status = "Healthy",
    utc = DateTimeOffset.UtcNow
}));

app.MapFoundationApi();

app.Run();

public partial class Program;
