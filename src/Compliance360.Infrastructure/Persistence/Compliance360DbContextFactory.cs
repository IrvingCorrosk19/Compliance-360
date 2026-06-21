using Compliance360.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Compliance360.Infrastructure.Persistence;

public sealed class Compliance360DbContextFactory : IDesignTimeDbContextFactory<Compliance360DbContext>
{
    public Compliance360DbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Compliance360")
            ?? Environment.GetEnvironmentVariable("COMPLIANCE360_DESIGN_CONNECTION")
            ?? throw new InvalidOperationException("Set ConnectionStrings:Compliance360 or COMPLIANCE360_DESIGN_CONNECTION before running EF design-time commands.");

        var options = new DbContextOptionsBuilder<Compliance360DbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new Compliance360DbContext(options, new SystemClock());
    }
}
