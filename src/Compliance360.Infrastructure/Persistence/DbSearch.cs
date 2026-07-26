using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Persistence;

internal static class DbSearch
{
    public static bool SupportsILike(DbContext dbContext) =>
        dbContext.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true;

    public static string ContainsPattern(string searchText) =>
        $"%{searchText.Trim()}%";
}
