using Compliance360.Application.RegulatoryAffairs;

namespace Compliance360.Web.Security;

public sealed class HttpContextCurrentUserPermissions : ICurrentUserPermissions
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserPermissions(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool Has(string permissionCode)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        return user.Claims.Any(claim =>
            string.Equals(claim.Type, "permission", StringComparison.OrdinalIgnoreCase)
            && string.Equals(claim.Value, permissionCode, StringComparison.OrdinalIgnoreCase));
    }
}
