using Compliance360.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Compliance360.Infrastructure.Persistence;

public sealed class Compliance360DbContextFactory : IDesignTimeDbContextFactory<Compliance360DbContext>
{
    public Compliance360DbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<Compliance360DbContext>()
            .UseNpgsql("Host=localhost;Database=compliance360_design;Username=postgres;Password=postgres")
            .Options;

        return new Compliance360DbContext(options, new SystemClock());
    }
}
