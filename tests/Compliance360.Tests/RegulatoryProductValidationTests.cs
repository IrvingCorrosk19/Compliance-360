using Compliance360.Application;
using Compliance360.Domain.Common;
using Compliance360.Domain.RegulatoryAffairs;
using Compliance360.Infrastructure.Persistence;
using Compliance360.Infrastructure.RegulatoryAffairs;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Tests;

public sealed class RegulatoryProductValidationTests
{
    [Fact]
    public async Task ProductCatalogExists_NullOrWhitespace_ReturnsFalse_WithoutThrowing()
    {
        await using var db = new Compliance360DbContext(
            new DbContextOptionsBuilder<Compliance360DbContext>()
                .UseInMemoryDatabase($"ra-catalog-{Guid.NewGuid():N}")
                .Options,
            new FixedClock());
        var repo = new EfRegulatoryAffairsRepository(db);
        var tenantId = Guid.NewGuid();

        Assert.False(await repo.ProductCatalogExistsAsync(tenantId, null!, null));
        Assert.False(await repo.ProductCatalogExistsAsync(tenantId, "   ", null));
    }

    [Fact]
    public void MedicalDeviceProduct_RequiresCatalogCode()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new MedicalDeviceProduct(
                Guid.NewGuid(),
                "PA",
                "Category",
                "Brand",
                "Name",
                null,
                null,
                "  ",
                null,
                null,
                DeviceRiskClass.A,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                Guid.NewGuid()));

        Assert.Contains("catalog", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = DateTimeOffset.Parse("2026-07-26T12:00:00Z");
    }
}
