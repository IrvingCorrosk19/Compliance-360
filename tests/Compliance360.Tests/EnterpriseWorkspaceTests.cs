using Compliance360.Application;
using Compliance360.Application.Enterprise;
using Compliance360.Domain.Enterprise;
using Compliance360.Infrastructure.Enterprise;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Tests;

public sealed class EnterpriseWorkspaceTests
{
    [Fact]
    public async Task Enterprise_Workspace_Creates_Searches_Completes_And_Reopens_Items()
    {
        await using var dbContext = CreateDbContext();
        var service = new EnterpriseWorkspaceService(new EfEnterpriseWorkspaceRepository(dbContext), dbContext, new FixedClock());
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var created = await service.CreateAsync(new CreateEnterpriseWorkspaceItemCommand(
            tenantId,
            EnterpriseWorkspaceType.Regulatory,
            "Habilitacion sanitaria",
            "REG-001",
            "Control regulator principal",
            userId,
            userId,
            new FixedClock().UtcNow.AddDays(7),
            "{\"source\":\"test\"}"));
        var duplicate = await service.CreateAsync(new CreateEnterpriseWorkspaceItemCommand(tenantId, EnterpriseWorkspaceType.Regulatory, "Duplicate", "REG-001", "Duplicate", userId, null, null, "{}"));
        var search = await service.SearchAsync(new EnterpriseWorkspaceSearchQuery(tenantId, EnterpriseWorkspaceType.Regulatory, EnterpriseWorkspaceStatus.Active, "REG"));
        var dashboard = await service.GetDashboardAsync(tenantId);
        var complete = await service.CompleteAsync(new EnterpriseWorkspaceActionCommand(tenantId, created.Value!.Id, userId));
        var reopen = await service.ReopenAsync(new EnterpriseWorkspaceActionCommand(tenantId, created.Value.Id, userId));

        Assert.True(created.IsSuccess);
        Assert.True(duplicate.IsFailure);
        Assert.Single(search.Value!);
        Assert.Equal(1, dashboard.Value!.TotalItems);
        Assert.Equal(1, dashboard.Value.ActiveItems);
        Assert.True(complete.IsSuccess);
        Assert.True(reopen.IsSuccess);
        Assert.Equal(EnterpriseWorkspaceStatus.Active, (await service.SearchAsync(new EnterpriseWorkspaceSearchQuery(tenantId, null, null, null))).Value!.Single().Status);
    }

    [Fact]
    public async Task Enterprise_Workspace_Is_Tenant_Isolated_And_Dashboard_Counts_By_Type()
    {
        await using var dbContext = CreateDbContext();
        var service = new EnterpriseWorkspaceService(new EfEnterpriseWorkspaceRepository(dbContext), dbContext, new FixedClock());
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await service.CreateAsync(new CreateEnterpriseWorkspaceItemCommand(tenantId, EnterpriseWorkspaceType.Training, "BPM anual", "TRN-001", "Training", userId, null, null, "{}"));
        await service.CreateAsync(new CreateEnterpriseWorkspaceItemCommand(tenantId, EnterpriseWorkspaceType.TemplateBuilder, "Formato auditoria", "TPL-001", "Template", userId, null, null, "{}"));
        await service.CreateAsync(new CreateEnterpriseWorkspaceItemCommand(otherTenantId, EnterpriseWorkspaceType.Training, "Other", "TRN-001", "Other", userId, null, null, "{}"));

        var tenantItems = await service.SearchAsync(new EnterpriseWorkspaceSearchQuery(tenantId, null, null, null));
        var otherItems = await service.SearchAsync(new EnterpriseWorkspaceSearchQuery(otherTenantId, null, null, null));
        var dashboard = await service.GetDashboardAsync(tenantId);

        Assert.Equal(2, tenantItems.Value!.Count);
        Assert.Single(otherItems.Value!);
        Assert.Equal(2, dashboard.Value!.TotalItems);
        Assert.Equal(1, dashboard.Value.ItemsByType[EnterpriseWorkspaceType.Training]);
        Assert.Equal(1, dashboard.Value.ItemsByType[EnterpriseWorkspaceType.TemplateBuilder]);
    }

    private static Compliance360DbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<Compliance360DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new Compliance360DbContext(options, new FixedClock());
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
    }
}
