using System.Security.Claims;
using Compliance360.Domain.Identity;

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
        if (claimTenantId.HasValue && claimTenantId.Value != tenantId && !HasSupportAccess(httpContext.User))
        {
            throw new UnauthorizedAccessException("Tenant context does not match authenticated user.");
        }

        return tenantId;
    }

    /// <summary>
    /// Resolves the tenant id for Tenant Administration Center routes. Platform
    /// operators with <see cref="PermissionCatalog.PlatformTenantRead"/> may
    /// administer any tenant without the break-glass support permission.
    /// </summary>
    public static Guid AdministrationTenantId(HttpContext httpContext, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }

        var claimTenantId = ReadGuidClaim(httpContext.User, "tenant_id");
        if (claimTenantId.HasValue
            && claimTenantId.Value != tenantId
            && !HasSupportAccess(httpContext.User)
            && !HasPlatformTenantAdministrationAccess(httpContext.User))
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

    // Cross-tenant access is only allowed through the explicit, audited
    // break-glass permission granted to the Support Operator role. There is no
    // implicit SuperAdmin role bypass anymore.
    private static bool HasSupportAccess(ClaimsPrincipal principal)
    {
        return HasPermission(principal, PermissionCatalog.PlatformSupportAccess);
    }

    private static bool HasPlatformTenantAdministrationAccess(ClaimsPrincipal principal)
    {
        return HasPermission(principal, PermissionCatalog.PlatformTenantRead);
    }

    private static bool HasPermission(ClaimsPrincipal principal, string permissionCode)
    {
        return principal.Claims.Any(claim =>
            string.Equals(claim.Type, "permission", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(claim.Value, permissionCode, StringComparison.OrdinalIgnoreCase));
    }

    private static Guid? ReadGuidHeader(HttpContext httpContext, string headerName)
    {
        return httpContext.Request.Headers.TryGetValue(headerName, out var value) && Guid.TryParse(value, out var parsed)
            ? parsed
            : null;
    }
}
