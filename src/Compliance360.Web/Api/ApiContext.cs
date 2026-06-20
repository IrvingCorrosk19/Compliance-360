using System.Security.Claims;

namespace Compliance360.Web.Api;

public static class ApiContext
{
    public static Guid UserId(HttpContext httpContext)
    {
        var userId = ReadGuidClaim(httpContext.User, ClaimTypes.NameIdentifier)
            ?? ReadGuidClaim(httpContext.User, "sub")
            ?? ReadGuidHeader(httpContext, "X-User-Id");

        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("Authenticated user id is required.");
        }

        return userId.Value;
    }

    public static Guid TenantId(HttpContext httpContext, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }

        var claimTenantId = ReadGuidClaim(httpContext.User, "tenant_id");
        if (claimTenantId.HasValue && claimTenantId.Value != tenantId)
        {
            throw new UnauthorizedAccessException("Tenant context does not match authenticated user.");
        }

        return tenantId;
    }

    public static string? IpAddress(HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }

    public static string? UserAgent(HttpContext httpContext)
    {
        return httpContext.Request.Headers.UserAgent.ToString();
    }

    private static Guid? ReadGuidClaim(ClaimsPrincipal principal, string claimType)
    {
        var value = principal.FindFirstValue(claimType);
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }

    private static Guid? ReadGuidHeader(HttpContext httpContext, string headerName)
    {
        return httpContext.Request.Headers.TryGetValue(headerName, out var value) && Guid.TryParse(value, out var parsed)
            ? parsed
            : null;
    }
}
