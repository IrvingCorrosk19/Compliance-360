using Compliance360.Application;
using Compliance360.Application.Audit;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.TenantManagement;
using Compliance360.Infrastructure.Audit;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace Compliance360.Tests;

public sealed class AuditFoundationTests
{
    [Fact]
    public async Task RecordAsync_Creates_Sanitized_Audit_Log()
    {
        var fixture = AuditFixture.Create();
        var result = await fixture.Service.RecordAsync(new RecordAuditCommand(CreateEvent(fixture.TenantId, fixture.UserId)));

        Assert.True(result.IsSuccess);
        var auditLog = fixture.Repository.AuditLogs.Single();
        Assert.Equal(fixture.TenantId, auditLog.TenantId);
        Assert.Equal(fixture.UserId, auditLog.UserId);
        Assert.Equal(AuditAction.DocumentCreated, auditLog.Action);
        Assert.Equal(AuditCategory.DocumentManagement, auditLog.Category);
        Assert.Equal("{\"redacted\":true}", auditLog.AfterValuesJson);
        Assert.Equal("{\"redacted\":true}", auditLog.MetadataJson);
    }

    [Fact]
    public async Task SearchAsync_Requires_Audit_Read_Permission()
    {
        var fixture = AuditFixture.Create();
        var query = CreateQuery(fixture.TenantId, fixture.UserId, permissions: []);

        var result = await fixture.Service.SearchAsync(query);

        Assert.True(result.IsFailure);
        Assert.Equal("User is not authorized to read audit logs.", result.Error);
    }

    [Fact]
    public async Task SearchAsync_Enforces_Tenant_Isolation()
    {
        var fixture = AuditFixture.Create();
        var query = new AuditSearchQuery(
            Guid.NewGuid(),
            new AuditQueryPrincipal(fixture.UserId, fixture.TenantId, ["AUDIT.READ"]),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            1,
            20);

        var result = await fixture.Service.SearchAsync(query);

        Assert.True(result.IsFailure);
        Assert.Equal("Audit queries must be scoped to the current tenant.", result.Error);
    }

    [Fact]
    public async Task SearchAsync_Filters_And_Paginates_Audit_Logs()
    {
        var fixture = AuditFixture.Create();
        await fixture.Service.RecordAsync(new RecordAuditCommand(CreateEvent(fixture.TenantId, fixture.UserId)));
        await fixture.Service.RecordAsync(new RecordAuditCommand(CreateEvent(fixture.TenantId, fixture.UserId) with
        {
            EntityName = "Supplier",
            Action = AuditAction.SupplierCreated,
            Category = AuditCategory.SupplierManagement
        }));

        var result = await fixture.Service.SearchAsync(CreateQuery(
            fixture.TenantId,
            fixture.UserId,
            ["AUDIT.READ"],
            action: AuditAction.SupplierCreated,
            searchText: "Supplier"));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(AuditAction.SupplierCreated, result.Value.Items.Single().Action);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Equal(1, result.Value.Page);
    }

    [Fact]
    public async Task ApplyRetentionAsync_Counts_Logs_Older_Than_Retention()
    {
        var fixture = AuditFixture.Create();
        fixture.Repository.AuditLogs.Add(AuditLog.FromEvent(CreateEvent(fixture.TenantId, fixture.UserId), fixture.Clock.UtcNow.AddDays(-10)));
        fixture.Repository.AuditLogs.Add(AuditLog.FromEvent(CreateEvent(fixture.TenantId, fixture.UserId), fixture.Clock.UtcNow));

        var result = await fixture.Service.ApplyRetentionAsync(new AuditRetentionCommand(
            fixture.TenantId,
            RetentionDays: 5,
            new AuditQueryPrincipal(fixture.UserId, fixture.TenantId, ["AUDIT.MANAGE"])));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
    }

    [Fact]
    public void AuditLog_InferCategory_Covers_Required_Actions()
    {
        Assert.Equal(AuditCategory.Authentication, AuditLog.InferCategory(AuditAction.LoginSucceeded));
        Assert.Equal(AuditCategory.Authorization, AuditLog.InferCategory(AuditAction.PermissionChanged));
        Assert.Equal(AuditCategory.Identity, AuditLog.InferCategory(AuditAction.PasswordChanged));
        Assert.Equal(AuditCategory.DocumentManagement, AuditLog.InferCategory(AuditAction.DocumentApproved));
        Assert.Equal(AuditCategory.Workflow, AuditLog.InferCategory(AuditAction.WorkflowRejected));
        Assert.Equal(AuditCategory.SupplierManagement, AuditLog.InferCategory(AuditAction.SupplierUpdated));
        Assert.Equal(AuditCategory.TechnicalSheets, AuditLog.InferCategory(AuditAction.TechnicalSheetCreated));
        Assert.Equal(AuditCategory.TenantManagement, AuditLog.InferCategory(AuditAction.TenantChanged));
        Assert.Equal(AuditCategory.Configuration, AuditLog.InferCategory(AuditAction.ConfigurationChanged));
        Assert.Equal(AuditCategory.DataExchange, AuditLog.InferCategory(AuditAction.Exported));
        Assert.Equal(AuditCategory.FileStorage, AuditLog.InferCategory(AuditAction.FileUploaded));
        Assert.Equal(AuditCategory.Security, AuditLog.InferCategory(AuditAction.SecurityEvent));
        Assert.Equal(AuditCategory.Administration, AuditLog.InferCategory(AuditAction.AdministrativeEvent));
    }

    [Fact]
    public void AuditLog_Captures_Outcome_Request_And_Truncates_Error()
    {
        var log = AuditLog
            .Create(Guid.NewGuid(), Guid.NewGuid(), "User", Guid.NewGuid(), AuditAction.SecurityEvent, DateTimeOffset.UtcNow)
            .WithRequestContext("127.0.0.1", "agent", "corr", "req", Guid.NewGuid())
            .WithOutcome(false, new string('x', 2_000));

        Assert.False(log.Success);
        Assert.Equal("127.0.0.1", log.IpAddress);
        Assert.Equal("agent", log.UserAgent);
        Assert.Equal("corr", log.CorrelationId);
        Assert.Equal("req", log.RequestId);
        Assert.NotNull(log.SessionId);
        Assert.Equal(1_000, log.ErrorMessage!.Length);
    }

    [Fact]
    public async Task DbContext_Prevents_AuditLog_Update_And_Delete()
    {
        await using var dbContext = CreateDbContext();
        var auditLog = AuditLog.FromEvent(CreateEvent(Guid.NewGuid(), Guid.NewGuid()), DateTimeOffset.UtcNow);
        dbContext.AuditLogs.Add(auditLog);
        await dbContext.SaveChangesAsync();

        auditLog.WithOutcome(false, "changed");
        await Assert.ThrowsAsync<DomainException>(() => dbContext.SaveChangesAsync());

        dbContext.Entry(auditLog).State = EntityState.Deleted;
        await Assert.ThrowsAsync<DomainException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task EfAuditRepository_Searches_By_Tenant_And_Text()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfAuditRepository(dbContext);
        var tenantId = Guid.NewGuid();
        await repository.AddAsync(AuditLog.FromEvent(CreateEvent(tenantId, Guid.NewGuid()), DateTimeOffset.UtcNow));
        await repository.AddAsync(AuditLog.FromEvent(CreateEvent(Guid.NewGuid(), Guid.NewGuid()), DateTimeOffset.UtcNow));
        await dbContext.SaveChangesAsync();

        var result = await repository.SearchAsync(new AuditSearchCriteria(
            tenantId,
            AuditAction.DocumentCreated,
            AuditCategory.DocumentManagement,
            "Document",
            null,
            "Document",
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            1,
            20));

        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(0, await repository.CountOlderThanAsync(tenantId, DateTimeOffset.UtcNow.AddDays(-1)));
    }

    [Fact]
    public async Task AuditSaveChangesInterceptor_Creates_Audit_For_Tenant_Entity()
    {
        var accessor = new AuditContextAccessor();
        var tenantId = Guid.NewGuid();
        accessor.Set(new AuditContext(tenantId, Guid.NewGuid(), "QA", "Admin", "127.0.0.1", "test", "corr", "req", Guid.NewGuid()));
        await using var dbContext = CreateDbContext(new AuditSaveChangesInterceptor(accessor, new FixedClock()));

        dbContext.Tenants.Add(new Tenant("Acme Quality", "acme"));
        await dbContext.SaveChangesAsync();

        Assert.Contains(dbContext.AuditLogs, auditLog => auditLog.EntityName == nameof(Tenant) && auditLog.Action == AuditAction.Created);
    }

    [Fact]
    public void AuditPermissionEvaluator_Allows_Read_And_Manage()
    {
        var evaluator = new AuditPermissionEvaluator(Options.Create(new AuditOptions()));
        var tenantId = Guid.NewGuid();

        Assert.True(evaluator.CanReadAudit(new AuditQueryPrincipal(Guid.NewGuid(), tenantId, ["AUDIT.READ"])));
        Assert.True(evaluator.CanReadAudit(new AuditQueryPrincipal(Guid.NewGuid(), tenantId, ["AUDIT.MANAGE"])));
        Assert.False(evaluator.CanReadAudit(new AuditQueryPrincipal(Guid.NewGuid(), tenantId, ["TENANTS.READ"])));
    }

    private static AuditEvent CreateEvent(Guid tenantId, Guid userId)
    {
        return new AuditEvent(
            "Document",
            Guid.NewGuid(),
            AuditAction.DocumentCreated,
            AuditCategory.DocumentManagement,
            new AuditContext(tenantId, userId, "Quality User", "Admin", "127.0.0.1", "unit-test", "corr", "req", Guid.NewGuid()),
            new AuditSnapshot("{\"name\":\"old\"}", "{\"password\":\"secret\"}"),
            new AuditMetadata("{\"token\":\"secret\"}"),
            Success: true,
            ErrorMessage: null);
    }

    private static AuditSearchQuery CreateQuery(
        Guid tenantId,
        Guid userId,
        IReadOnlyCollection<string> permissions,
        AuditAction? action = null,
        string? searchText = null)
    {
        return new AuditSearchQuery(
            tenantId,
            new AuditQueryPrincipal(userId, tenantId, permissions),
            action,
            null,
            null,
            null,
            searchText,
            null,
            null,
            1,
            20);
    }

    private static Compliance360DbContext CreateDbContext(params IInterceptor[] interceptors)
    {
        var optionsBuilder = new DbContextOptionsBuilder<Compliance360DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        if (interceptors.Length > 0)
        {
            optionsBuilder.AddInterceptors(interceptors);
        }

        return new Compliance360DbContext(optionsBuilder.Options, new FixedClock());
    }

    private sealed class AuditFixture
    {
        private AuditFixture()
        {
            TenantId = Guid.NewGuid();
            UserId = Guid.NewGuid();
            Clock = new FixedClock();
            Repository = new InMemoryAuditRepository();
            Service = new AuditService(
                Repository,
                new FakeApplicationDbContext(),
                new AuditPermissionEvaluator(Options.Create(new AuditOptions())),
                Clock,
                Options.Create(new AuditOptions { MaxPageSize = 50, DefaultRetentionDays = 2_555 }));
        }

        public Guid TenantId { get; }

        public Guid UserId { get; }

        public FixedClock Clock { get; }

        public InMemoryAuditRepository Repository { get; }

        public AuditService Service { get; }

        public static AuditFixture Create()
        {
            return new AuditFixture();
        }
    }

    private sealed class InMemoryAuditRepository : IAuditRepository
    {
        public List<AuditLog> AuditLogs { get; } = [];

        public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            AuditLogs.Add(auditLog);
            return Task.CompletedTask;
        }

        public Task<AuditSearchResult> SearchAsync(AuditSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            var query = AuditLogs.Where(auditLog => auditLog.TenantId == criteria.TenantId);
            if (criteria.Action.HasValue)
            {
                query = query.Where(auditLog => auditLog.Action == criteria.Action);
            }

            if (!string.IsNullOrWhiteSpace(criteria.SearchText))
            {
                query = query.Where(auditLog => auditLog.EntityName.Contains(criteria.SearchText));
            }

            var items = query
                .Skip((criteria.Page - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .Select(auditLog => new AuditLogDto(
                    auditLog.Id,
                    auditLog.TenantId,
                    auditLog.UserId,
                    auditLog.UserName,
                    auditLog.Role,
                    auditLog.EntityName,
                    auditLog.EntityId,
                    auditLog.Action,
                    auditLog.Category,
                    auditLog.OccurredAtUtc,
                    auditLog.IpAddress,
                    auditLog.UserAgent,
                    auditLog.CorrelationId,
                    auditLog.RequestId,
                    auditLog.SessionId,
                    auditLog.Success,
                    auditLog.ErrorMessage))
                .ToList();

            return Task.FromResult(new AuditSearchResult(items, items.Count, criteria.Page, criteria.PageSize));
        }

        public Task<int> CountOlderThanAsync(Guid tenantId, DateTimeOffset olderThanUtc, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(AuditLogs.Count(auditLog => auditLog.TenantId == tenantId && auditLog.OccurredAtUtc < olderThanUtc));
        }
    }

    private sealed class FakeApplicationDbContext : IApplicationDbContext
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 6, 20, 16, 0, 0, TimeSpan.Zero);
    }
}
