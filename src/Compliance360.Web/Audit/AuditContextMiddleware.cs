using System.Security.Claims;
using Compliance360.Application.Audit;
using Compliance360.Domain.Audit;

namespace Compliance360.Web.Audit;

public sealed class AuditContextMiddleware
{
    private readonly RequestDelegate _next;

    public AuditContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IAuditContextAccessor auditContextAccessor)
    {
        auditContextAccessor.Set(new AuditContext(
            TenantId: ReadGuidClaim(httpContext.User, "tenant_id"),
            UserId: ReadGuidClaim(httpContext.User, ClaimTypes.NameIdentifier) ?? ReadGuidClaim(httpContext.User, "sub"),
            UserName: httpContext.User.FindFirstValue(ClaimTypes.Email) ?? httpContext.User.FindFirstValue("email"),
            Role: httpContext.User.FindFirstValue(ClaimTypes.Role),
            IpAddress: httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent: httpContext.Request.Headers.UserAgent.ToString(),
            CorrelationId: httpContext.TraceIdentifier,
            RequestId: httpContext.Request.Headers.TryGetValue("X-Request-Id", out var requestId) ? requestId.ToString() : null,
            SessionId: ReadGuidClaim(httpContext.User, "session_id")));

        await _next(httpContext);
    }

    private static Guid? ReadGuidClaim(ClaimsPrincipal principal, string claimType)
    {
        var value = principal.FindFirstValue(claimType);
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }
}
