using Compliance360.Application;
using Compliance360.Application.Suppliers;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Suppliers;
using Compliance360.Infrastructure.Persistence;
using Compliance360.Infrastructure.Suppliers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Compliance360.Tests;

public sealed class SupplierManagementTests
{
    [Fact]
    public async Task Supplier_Flow_Homologates_With_Required_Documents_And_Evaluation()
    {
        var fixture = SupplierFixture.Create();
        var supplier = await fixture.Service.CreateSupplierAsync(new CreateSupplierCommand(fixture.TenantId, "ACME Foods", "ruc-001", "pa", fixture.UserId));
        var documentIds = new List<Guid>();
        foreach (var type in SupplierFixture.RequiredDocuments)
        {
            var document = await fixture.Service.AddDocumentAsync(new AddSupplierDocumentCommand(fixture.TenantId, supplier.Value!.Id, type, $"{type}-001", Guid.NewGuid(), fixture.Clock.UtcNow.AddDays(-30), fixture.Clock.UtcNow.AddYears(1), fixture.UserId));
            documentIds.Add(document.Value!.Id);
            await fixture.Service.ValidateDocumentAsync(new ReviewSupplierDocumentCommand(fixture.TenantId, supplier.Value.Id, document.Value.Id, fixture.UserId));
        }

        var evaluation = await fixture.Service.AddEvaluationAsync(new AddSupplierEvaluationCommand(fixture.TenantId, supplier.Value!.Id, 92, "Strong supplier", fixture.UserId));
        var homologated = await fixture.Service.HomologateAsync(new SupplierActionCommand(fixture.TenantId, supplier.Value.Id, fixture.UserId));
        var alert = await fixture.Service.CreateExpirationAlertAsync(new CreateSupplierExpirationAlertCommand(fixture.TenantId, supplier.Value.Id, documentIds.First(), fixture.UserId));

        Assert.True(evaluation.IsSuccess);
        Assert.True(homologated.IsSuccess);
        Assert.True(alert.IsSuccess);
        Assert.Equal(SupplierStatus.Homologated, fixture.Repository.Suppliers.Single().Status);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.SupplierUpdated);
    }

    [Fact]
    public async Task Supplier_Service_Rejects_Duplicates_Missing_And_Invalid_Homologation()
    {
        var fixture = SupplierFixture.Create();
        var supplier = await fixture.Service.CreateSupplierAsync(new CreateSupplierCommand(fixture.TenantId, "ACME", "RUC-001", "PA", fixture.UserId));
        var duplicate = await fixture.Service.CreateSupplierAsync(new CreateSupplierCommand(fixture.TenantId, "ACME 2", "RUC-001", "PA", fixture.UserId));
        var missingDocument = await fixture.Service.AddDocumentAsync(new AddSupplierDocumentCommand(fixture.TenantId, Guid.NewGuid(), SupplierDocumentType.Ruc, "DOC", Guid.NewGuid(), fixture.Clock.UtcNow, fixture.Clock.UtcNow.AddDays(1), fixture.UserId));
        var invalidHomologation = await fixture.Service.HomologateAsync(new SupplierActionCommand(fixture.TenantId, supplier.Value!.Id, fixture.UserId));
        var missingChange = await fixture.Service.SuspendAsync(new SuspendSupplierCommand(fixture.TenantId, Guid.NewGuid(), "Missing", fixture.UserId));
        var missingEvaluation = await fixture.Service.AddEvaluationAsync(new AddSupplierEvaluationCommand(fixture.TenantId, Guid.NewGuid(), 80, "Missing", fixture.UserId));
        var missingAlert = await fixture.Service.CreateExpirationAlertAsync(new CreateSupplierExpirationAlertCommand(fixture.TenantId, Guid.NewGuid(), Guid.NewGuid(), fixture.UserId));
        var invalidSupplier = await fixture.Service.CreateSupplierAsync(new CreateSupplierCommand(fixture.TenantId, "", "BAD", "PA", fixture.UserId));

        Assert.True(supplier.IsSuccess);
        Assert.True(duplicate.IsFailure);
        Assert.True(missingDocument.IsFailure);
        Assert.True(invalidHomologation.IsFailure);
        Assert.True(missingChange.IsFailure);
        Assert.True(missingEvaluation.IsFailure);
        Assert.True(missingAlert.IsFailure);
        Assert.True(invalidSupplier.IsFailure);
    }

    [Fact]
    public async Task Supplier_Homologation_Rejects_Low_Score_And_Expired_Document()
    {
        var lowScore = SupplierFixture.Create();
        var supplier = await lowScore.Service.CreateSupplierAsync(new CreateSupplierCommand(lowScore.TenantId, "Low Score", "RUC-LOW", "PA", lowScore.UserId));
        foreach (var type in SupplierFixture.RequiredDocuments)
        {
            var document = await lowScore.Service.AddDocumentAsync(new AddSupplierDocumentCommand(lowScore.TenantId, supplier.Value!.Id, type, $"{type}-LOW", Guid.NewGuid(), lowScore.Clock.UtcNow.AddDays(-10), lowScore.Clock.UtcNow.AddYears(1), lowScore.UserId));
            await lowScore.Service.ValidateDocumentAsync(new ReviewSupplierDocumentCommand(lowScore.TenantId, supplier.Value.Id, document.Value!.Id, lowScore.UserId));
        }

        await lowScore.Service.AddEvaluationAsync(new AddSupplierEvaluationCommand(lowScore.TenantId, supplier.Value!.Id, 69, "Below threshold", lowScore.UserId));
        var lowScoreHomologation = await lowScore.Service.HomologateAsync(new SupplierActionCommand(lowScore.TenantId, supplier.Value.Id, lowScore.UserId));

        var expired = SupplierFixture.Create();
        var expiredSupplier = new Supplier(expired.TenantId, "Expired", "RUC-EXP", "PA", expired.UserId, expired.Clock.UtcNow);
        foreach (var type in SupplierFixture.RequiredDocuments)
        {
            var doc = expiredSupplier.AddDocument(type, $"{type}-EXP", Guid.NewGuid(), expired.Clock.UtcNow.AddYears(-2), expired.Clock.UtcNow.AddDays(-1), expired.UserId);
            expiredSupplier.ValidateDocument(doc.Id, expired.UserId, expired.Clock.UtcNow);
        }

        expiredSupplier.AddEvaluation(90, "Good", expired.UserId, expired.Clock.UtcNow);
        expired.Repository.Suppliers.Add(expiredSupplier);
        var expiredHomologation = await expired.Service.HomologateAsync(new SupplierActionCommand(expired.TenantId, expiredSupplier.Id, expired.UserId));

        Assert.True(lowScoreHomologation.IsFailure);
        Assert.True(expiredHomologation.IsFailure);
    }

    [Fact]
    public async Task Supplier_Document_Can_Be_Rejected_And_Supplier_Suspended()
    {
        var fixture = SupplierFixture.Create();
        var supplier = await fixture.Service.CreateSupplierAsync(new CreateSupplierCommand(fixture.TenantId, "Rejected Supplier", "RUC-002", "PA", fixture.UserId));
        var document = await fixture.Service.AddDocumentAsync(new AddSupplierDocumentCommand(fixture.TenantId, supplier.Value!.Id, SupplierDocumentType.Ruc, "RUC-002", Guid.NewGuid(), fixture.Clock.UtcNow.AddDays(-1), fixture.Clock.UtcNow.AddYears(1), fixture.UserId));

        var rejected = await fixture.Service.RejectDocumentAsync(new RejectSupplierDocumentCommand(fixture.TenantId, supplier.Value.Id, document.Value!.Id, "Unreadable", fixture.UserId));
        var suspended = await fixture.Service.SuspendAsync(new SuspendSupplierCommand(fixture.TenantId, supplier.Value.Id, "Quality incident", fixture.UserId));

        Assert.True(rejected.IsSuccess);
        Assert.True(suspended.IsSuccess);
        Assert.Equal(SupplierStatus.Suspended, fixture.Repository.Suppliers.Single().Status);
        Assert.Equal(SupplierDocumentStatus.Rejected, fixture.Repository.Suppliers.Single().Documents.Single().Status);
    }

    [Fact]
    public async Task Supplier_Search_Is_Tenant_Isolated()
    {
        var fixture = SupplierFixture.Create();
        await fixture.Service.CreateSupplierAsync(new CreateSupplierCommand(fixture.TenantId, "Tenant Supplier", "RUC-003", "PA", fixture.UserId));
        fixture.Repository.Suppliers.Add(new Supplier(Guid.NewGuid(), "Other Supplier", "RUC-004", "PA", fixture.UserId, fixture.Clock.UtcNow));

        var search = await fixture.Service.SearchAsync(new SupplierSearchQuery(fixture.TenantId, "Tenant", SupplierStatus.Draft, 1, 10));

        Assert.True(search.IsSuccess);
        Assert.Single(search.Value!.Items);
        Assert.Equal("RUC-003", search.Value.Items.Single().TaxIdentifier);
    }

    [Fact]
    public void Supplier_Domain_Validates_Rules_And_Exposes_State()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var supplier = new Supplier(tenantId, "Supplier", "RUC-005", "PA", userId, DateTimeOffset.UtcNow);
        var document = supplier.AddDocument(SupplierDocumentType.Ruc, "RUC-005", Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(-10), DateTimeOffset.UtcNow.AddYears(1), userId);
        document.MarkValid(userId, DateTimeOffset.UtcNow);
        var evaluation = supplier.AddEvaluation(70, "Meets criteria", userId, DateTimeOffset.UtcNow);
        var alert = supplier.CreateExpirationAlert(document.Id, DateTimeOffset.UtcNow);
        alert.Acknowledge();

        Assert.Equal("Supplier", supplier.LegalName);
        Assert.Equal("PA", supplier.CountryCode);
        Assert.Equal(userId, supplier.CreatedByUserId);
        Assert.Equal(SupplierAlertStatus.Acknowledged, alert.Status);
        Assert.Equal(document.Id, alert.SupplierDocumentId);
        Assert.Equal(SupplierDocumentType.Ruc, alert.DocumentType);
        Assert.Equal(70, evaluation.Score);
        Assert.Equal("Meets criteria", evaluation.Comments);
        Assert.Equal(userId, evaluation.EvaluatedByUserId);
        Assert.Equal(SupplierDocumentStatus.Valid, document.Status);
        Assert.Equal("RUC-005", document.DocumentNumber);
        Assert.Equal(userId, document.UploadedByUserId);
        Assert.Equal(userId, document.ReviewedByUserId);
        Assert.Throws<DomainException>(() => new Supplier(tenantId, "", "RUC", "PA", userId, DateTimeOffset.UtcNow));
        Assert.Throws<DomainException>(() => supplier.AddDocument(SupplierDocumentType.Bpm, "BPM", Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(-1), userId));
        Assert.Throws<DomainException>(() => new SupplierEvaluation(tenantId, supplier.Id, 101, "Bad", userId, DateTimeOffset.UtcNow));
        Assert.Throws<DomainException>(() => supplier.RejectDocument(Guid.NewGuid(), "Missing", userId, DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task EfSupplierRepository_Persists_And_Searches()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfSupplierRepository(dbContext);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var supplier = new Supplier(tenantId, "DB Supplier", "RUC-DB", "PA", userId, DateTimeOffset.UtcNow);
        supplier.AddDocument(SupplierDocumentType.Ruc, "RUC-DB", Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1), userId);
        supplier.AddEvaluation(88, "Good", userId, DateTimeOffset.UtcNow);

        await repository.AddAsync(supplier);
        await repository.AddAuditLogAsync(AuditLog.Create(tenantId, userId, nameof(Supplier), supplier.Id, AuditAction.SupplierCreated, DateTimeOffset.UtcNow));
        await dbContext.SaveChangesAsync();

        var loaded = await repository.GetAsync(tenantId, supplier.Id);
        var exists = await repository.TaxIdentifierExistsAsync(tenantId, "ruc-db");
        var search = await repository.SearchAsync(new SupplierSearchCriteria(tenantId, "DB", SupplierStatus.PendingHomologation, 1, 10));

        Assert.NotNull(loaded);
        Assert.True(exists);
        Assert.Single(search.Items);
        Assert.Single(dbContext.AuditLogs);
    }

    private static Compliance360DbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<Compliance360DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new Compliance360DbContext(options, new FixedClock());
    }

    private sealed class SupplierFixture
    {
        public static readonly SupplierDocumentType[] RequiredDocuments =
        [
            SupplierDocumentType.Ruc,
            SupplierDocumentType.OperationsNotice,
            SupplierDocumentType.Bpm,
            SupplierDocumentType.Haccp,
            SupplierDocumentType.SanitaryRegistration
        ];

        private SupplierFixture()
        {
            TenantId = Guid.NewGuid();
            UserId = Guid.NewGuid();
            Clock = new FixedClock();
            Repository = new InMemorySupplierRepository();
            Service = new SupplierManagementService(Repository, new FakeApplicationDbContext(), Clock, Options.Create(new SupplierManagementOptions()));
        }

        public Guid TenantId { get; }
        public Guid UserId { get; }
        public FixedClock Clock { get; }
        public InMemorySupplierRepository Repository { get; }
        public SupplierManagementService Service { get; }

        public static SupplierFixture Create()
        {
            return new SupplierFixture();
        }
    }

    private sealed class InMemorySupplierRepository : ISupplierRepository
    {
        public List<Supplier> Suppliers { get; } = [];
        public List<AuditLog> AuditLogs { get; } = [];

        public Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
        {
            Suppliers.Add(supplier);
            return Task.CompletedTask;
        }

        public Task<Supplier?> GetAsync(Guid tenantId, Guid supplierId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Suppliers.SingleOrDefault(supplier => supplier.TenantId == tenantId && supplier.Id == supplierId));
        }

        public Task<bool> TaxIdentifierExistsAsync(Guid tenantId, string taxIdentifier, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Suppliers.Any(supplier => supplier.TenantId == tenantId && supplier.TaxIdentifier == taxIdentifier.ToUpperInvariant()));
        }

        public Task<SupplierSearchResult> SearchAsync(SupplierSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            var suppliers = Suppliers
                .Where(supplier => supplier.TenantId == criteria.TenantId)
                .Where(supplier => criteria.SearchText is null || supplier.LegalName.Contains(criteria.SearchText) || supplier.TaxIdentifier.Contains(criteria.SearchText))
                .Where(supplier => !criteria.Status.HasValue || supplier.Status == criteria.Status.Value)
                .Select(supplier => new SupplierSummary(supplier.Id, supplier.TenantId, supplier.LegalName, supplier.TaxIdentifier, supplier.CountryCode, supplier.Status, supplier.HomologatedAtUtc))
                .ToArray();

            return Task.FromResult(new SupplierSearchResult(suppliers, suppliers.Length, criteria.Page, criteria.PageSize));
        }

        public Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            AuditLogs.Add(auditLog);
            return Task.CompletedTask;
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
        public DateTimeOffset UtcNow => new(2026, 6, 20, 20, 0, 0, TimeSpan.Zero);
    }
}
