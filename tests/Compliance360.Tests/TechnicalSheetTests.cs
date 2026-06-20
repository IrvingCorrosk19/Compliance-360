using Compliance360.Application;
using Compliance360.Application.TechnicalSheets;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.TechnicalSheets;
using Compliance360.Infrastructure.Persistence;
using Compliance360.Infrastructure.TechnicalSheets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Compliance360.Tests;

public sealed class TechnicalSheetTests
{
    [Fact]
    public async Task Technical_Sheet_Flow_Creates_Approves_And_Attaches_Pdf()
    {
        var fixture = TechnicalSheetFixture.Create();
        var product = await fixture.Service.CreateProductAsync(new CreateProductCommand(fixture.TenantId, "Yogurt", "YOG-001", "Dairy product", fixture.UserId));
        var sheet = await fixture.Service.CreateSheetAsync(new CreateTechnicalSheetCommand(fixture.TenantId, product.Value!.Id, "Yogurt Technical Sheet", fixture.UserId));
        var version = await fixture.Service.CreateVersionAsync(new CreateTechnicalSheetVersionCommand(fixture.TenantId, sheet.Value!.Id, "Initial version", fixture.UserId));
        var ingredient = await fixture.Service.AddIngredientAsync(new AddIngredientCommand(fixture.TenantId, sheet.Value.Id, "Milk", 95m, "Milk", fixture.UserId));
        var nutrient = await fixture.Service.AddNutrientAsync(new AddNutrientCommand(fixture.TenantId, sheet.Value.Id, "Protein", 4.2m, "g", fixture.UserId));
        var certification = await fixture.Service.AddCertificationAsync(new AddCertificationCommand(fixture.TenantId, sheet.Value.Id, "HACCP", "QA Authority", fixture.Clock.UtcNow.AddYears(1), fixture.UserId));
        var submitted = await fixture.Service.SubmitAsync(new TechnicalSheetActionCommand(fixture.TenantId, sheet.Value.Id, fixture.UserId));
        var approved = await fixture.Service.DecideAsync(new DecideTechnicalSheetCommand(fixture.TenantId, sheet.Value.Id, TechnicalSheetApprovalDecision.Approved, "Approved", fixture.UserId));
        var pdf = await fixture.Service.AttachPdfAsync(new AttachTechnicalSheetPdfCommand(fixture.TenantId, sheet.Value.Id, "pdf/yogurt.pdf", fixture.UserId));

        Assert.True(version.IsSuccess);
        Assert.True(ingredient.IsSuccess);
        Assert.True(nutrient.IsSuccess);
        Assert.True(certification.IsSuccess);
        Assert.True(submitted.IsSuccess);
        Assert.True(approved.IsSuccess);
        Assert.True(pdf.IsSuccess);
        Assert.Equal(TechnicalSheetStatus.Approved, fixture.Repository.Sheets.Single().Status);
        Assert.Equal("pdf/yogurt.pdf", fixture.Repository.Sheets.Single().PdfObjectKey);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.TechnicalSheetUpdated);
    }

    [Fact]
    public async Task Technical_Sheet_Service_Rejects_Missing_References_And_Invalid_State()
    {
        var fixture = TechnicalSheetFixture.Create();
        var missingProduct = await fixture.Service.CreateSheetAsync(new CreateTechnicalSheetCommand(fixture.TenantId, Guid.NewGuid(), "Missing", fixture.UserId));
        var product = await fixture.Service.CreateProductAsync(new CreateProductCommand(fixture.TenantId, "Sauce", "SAU-001", null, fixture.UserId));
        var duplicate = await fixture.Service.CreateProductAsync(new CreateProductCommand(fixture.TenantId, "Sauce 2", "SAU-001", null, fixture.UserId));
        var sheet = await fixture.Service.CreateSheetAsync(new CreateTechnicalSheetCommand(fixture.TenantId, product.Value!.Id, "Sauce Sheet", fixture.UserId));
        var submitWithoutVersion = await fixture.Service.SubmitAsync(new TechnicalSheetActionCommand(fixture.TenantId, sheet.Value!.Id, fixture.UserId));
        var missingSheet = await fixture.Service.CreateVersionAsync(new CreateTechnicalSheetVersionCommand(fixture.TenantId, Guid.NewGuid(), "Missing", fixture.UserId));
        var missingDecision = await fixture.Service.DecideAsync(new DecideTechnicalSheetCommand(fixture.TenantId, Guid.NewGuid(), TechnicalSheetApprovalDecision.Approved, "Missing", fixture.UserId));
        var missingChange = await fixture.Service.AddIngredientAsync(new AddIngredientCommand(fixture.TenantId, Guid.NewGuid(), "Salt", 1m, null, fixture.UserId));
        var invalidProduct = await fixture.Service.CreateProductAsync(new CreateProductCommand(fixture.TenantId, "", "BAD", null, fixture.UserId));
        var invalidSheet = await fixture.Service.CreateSheetAsync(new CreateTechnicalSheetCommand(fixture.TenantId, product.Value.Id, "", fixture.UserId));
        var invalidDecisionState = await fixture.Service.DecideAsync(new DecideTechnicalSheetCommand(fixture.TenantId, sheet.Value.Id, TechnicalSheetApprovalDecision.Approved, "Not submitted", fixture.UserId));

        Assert.True(missingProduct.IsFailure);
        Assert.True(duplicate.IsFailure);
        Assert.True(submitWithoutVersion.IsFailure);
        Assert.True(missingSheet.IsFailure);
        Assert.True(missingDecision.IsFailure);
        Assert.True(missingChange.IsFailure);
        Assert.True(invalidProduct.IsFailure);
        Assert.True(invalidSheet.IsFailure);
        Assert.True(invalidDecisionState.IsFailure);
    }

    [Fact]
    public async Task Technical_Sheet_Can_Be_Rejected_And_Marked_Obsolete()
    {
        var fixture = TechnicalSheetFixture.CreateWithSheet();
        await fixture.Service.CreateVersionAsync(new CreateTechnicalSheetVersionCommand(fixture.TenantId, fixture.SheetId, "Initial", fixture.UserId));
        await fixture.Service.SubmitAsync(new TechnicalSheetActionCommand(fixture.TenantId, fixture.SheetId, fixture.UserId));

        var rejection = await fixture.Service.DecideAsync(new DecideTechnicalSheetCommand(fixture.TenantId, fixture.SheetId, TechnicalSheetApprovalDecision.Rejected, "Needs work", fixture.UserId));
        var obsolete = await fixture.Service.MarkObsoleteAsync(new TechnicalSheetActionCommand(fixture.TenantId, fixture.SheetId, fixture.UserId));
        var versionAfterObsolete = await fixture.Service.CreateVersionAsync(new CreateTechnicalSheetVersionCommand(fixture.TenantId, fixture.SheetId, "Blocked", fixture.UserId));

        Assert.True(rejection.IsSuccess);
        Assert.True(obsolete.IsSuccess);
        Assert.True(versionAfterObsolete.IsFailure);
        Assert.Equal(TechnicalSheetStatus.Obsolete, fixture.Repository.Sheets.Single().Status);
    }

    [Fact]
    public async Task Technical_Sheet_Search_Is_Tenant_Isolated()
    {
        var fixture = TechnicalSheetFixture.CreateWithSheet();
        fixture.Repository.Sheets.Add(new TechnicalSheet(Guid.NewGuid(), Guid.NewGuid(), "Other Tenant Sheet", fixture.UserId, fixture.Clock.UtcNow));

        var search = await fixture.Service.SearchAsync(new TechnicalSheetSearchQuery(fixture.TenantId, "Sheet", TechnicalSheetStatus.Draft, fixture.ProductId, 1, 20));

        Assert.True(search.IsSuccess);
        Assert.Single(search.Value!.Items);
        Assert.Equal(fixture.SheetId, search.Value.Items.Single().Id);
    }

    [Fact]
    public void Technical_Sheet_Domain_Validates_Rules()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sheet = new TechnicalSheet(tenantId, Guid.NewGuid(), "Sheet", userId, DateTimeOffset.UtcNow);

        Assert.Throws<DomainException>(() => sheet.SubmitForApproval(userId));
        Assert.Throws<DomainException>(() => sheet.AddIngredient("Salt", 101m, null));
        Assert.Throws<DomainException>(() => sheet.AddNutrient("Fat", -1m, "g"));
        Assert.Throws<DomainException>(() => new Product(tenantId, "", "SKU", null));
        Assert.Throws<DomainException>(() => new TechnicalSheet(tenantId, Guid.Empty, "Sheet", userId, DateTimeOffset.UtcNow));
        Assert.Throws<DomainException>(() => new TechnicalSheetVersion(tenantId, sheet.Id, 0, "Bad", userId, DateTimeOffset.UtcNow));
        Assert.Throws<DomainException>(() => new TechnicalSheetApproval(tenantId, sheet.Id, 1, TechnicalSheetApprovalDecision.Approved, "", userId, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Technical_Sheet_Domain_Exposes_Component_State()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var product = new Product(tenantId, "Juice", "JUI-001", "Fresh");
        var sheet = new TechnicalSheet(tenantId, product.Id, "Juice Sheet", userId, DateTimeOffset.UtcNow);
        sheet.CreateVersion("Initial", userId, DateTimeOffset.UtcNow);
        sheet.AddIngredient("Orange", 80m, null);
        sheet.AddIngredient("Sugar", 10m, "None");
        sheet.AddNutrient("Vitamin C", 30m, "mg");
        sheet.AddCertification("BPM", "Authority", DateTimeOffset.UtcNow.AddYears(1));
        sheet.AttachPdf("pdf/juice.pdf");

        var ingredient = sheet.Ingredients.First(item => item.Name == "Orange");
        var allergenIngredient = sheet.Ingredients.First(item => item.Name == "Sugar");
        var nutrient = sheet.Nutrients.Single();
        var certification = sheet.Certifications.Single();
        var version = sheet.Versions.Single();
        sheet.SubmitForApproval(userId);
        var approval = sheet.Decide(TechnicalSheetApprovalDecision.Rejected, "Rejected", userId, DateTimeOffset.UtcNow);

        Assert.Equal("Fresh", product.Description);
        Assert.True(product.IsActive);
        Assert.Null(new Product(tenantId, "Water", "WAT-001", null).Description);
        Assert.Equal("Orange", ingredient.Name);
        Assert.Equal(80m, ingredient.Percentage);
        Assert.Null(ingredient.Allergen);
        Assert.Equal("None", allergenIngredient.Allergen);
        Assert.Equal("Vitamin C", nutrient.Name);
        Assert.Equal(30m, nutrient.Amount);
        Assert.Equal("mg", nutrient.Unit);
        Assert.Equal("BPM", certification.Name);
        Assert.Equal("Authority", certification.Issuer);
        Assert.True(certification.ExpiresAtUtc > DateTimeOffset.UtcNow);
        Assert.Equal(1, version.VersionNumber);
        Assert.Equal(userId, version.CreatedByUserId);
        Assert.Equal(sheet.Id, version.TechnicalSheetId);
        Assert.Equal(TechnicalSheetApprovalDecision.Rejected, approval.Decision);
        Assert.Equal("Rejected", approval.Comments);
        Assert.Equal(userId, approval.DecidedByUserId);
        Assert.Equal("pdf/juice.pdf", sheet.PdfObjectKey);
    }

    [Fact]
    public async Task EfTechnicalSheetRepository_Persists_And_Searches()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfTechnicalSheetRepository(dbContext);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var product = new Product(tenantId, "Bread", "BRD-001", null);
        var sheet = new TechnicalSheet(tenantId, product.Id, "Bread Sheet", userId, DateTimeOffset.UtcNow);
        sheet.CreateVersion("Initial", userId, DateTimeOffset.UtcNow);
        sheet.AddIngredient("Flour", 70m, "Gluten");
        sheet.AddNutrient("Carbs", 30m, "g");
        sheet.AddCertification("BPM", "QA", DateTimeOffset.UtcNow.AddYears(1));

        await repository.AddProductAsync(product);
        await repository.AddSheetAsync(sheet);
        await repository.AddAuditLogAsync(AuditLog.Create(tenantId, userId, nameof(TechnicalSheet), sheet.Id, AuditAction.TechnicalSheetCreated, DateTimeOffset.UtcNow));
        await dbContext.SaveChangesAsync();

        var loadedProduct = await repository.GetProductAsync(tenantId, product.Id);
        var loadedSheet = await repository.GetSheetAsync(tenantId, sheet.Id);
        var exists = await repository.ProductSkuExistsAsync(tenantId, "brd-001");
        var search = await repository.SearchAsync(new TechnicalSheetSearchCriteria(tenantId, "Bread", TechnicalSheetStatus.Draft, product.Id, 1, 10));

        Assert.NotNull(loadedProduct);
        Assert.NotNull(loadedSheet);
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

    private sealed class TechnicalSheetFixture
    {
        private TechnicalSheetFixture()
        {
            TenantId = Guid.NewGuid();
            UserId = Guid.NewGuid();
            Clock = new FixedClock();
            Repository = new InMemoryTechnicalSheetRepository();
            Service = new TechnicalSheetService(Repository, new FakeApplicationDbContext(), Clock, Options.Create(new TechnicalSheetOptions()));
        }

        public Guid TenantId { get; }
        public Guid UserId { get; }
        public Guid ProductId { get; private set; }
        public Guid SheetId { get; private set; }
        public FixedClock Clock { get; }
        public InMemoryTechnicalSheetRepository Repository { get; }
        public TechnicalSheetService Service { get; }

        public static TechnicalSheetFixture Create()
        {
            return new TechnicalSheetFixture();
        }

        public static TechnicalSheetFixture CreateWithSheet()
        {
            var fixture = Create();
            var product = new Product(fixture.TenantId, "Product", "PROD-001", null);
            var sheet = new TechnicalSheet(fixture.TenantId, product.Id, "Product Sheet", fixture.UserId, fixture.Clock.UtcNow);
            fixture.ProductId = product.Id;
            fixture.SheetId = sheet.Id;
            fixture.Repository.Products.Add(product);
            fixture.Repository.Sheets.Add(sheet);
            return fixture;
        }
    }

    private sealed class InMemoryTechnicalSheetRepository : ITechnicalSheetRepository
    {
        public List<Product> Products { get; } = [];
        public List<TechnicalSheet> Sheets { get; } = [];
        public List<AuditLog> AuditLogs { get; } = [];

        public Task AddProductAsync(Product product, CancellationToken cancellationToken = default)
        {
            Products.Add(product);
            return Task.CompletedTask;
        }

        public Task<Product?> GetProductAsync(Guid tenantId, Guid productId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Products.SingleOrDefault(product => product.TenantId == tenantId && product.Id == productId));
        }

        public Task<bool> ProductSkuExistsAsync(Guid tenantId, string sku, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Products.Any(product => product.TenantId == tenantId && product.Sku == sku.ToUpperInvariant()));
        }

        public Task AddSheetAsync(TechnicalSheet sheet, CancellationToken cancellationToken = default)
        {
            Sheets.Add(sheet);
            return Task.CompletedTask;
        }

        public Task<TechnicalSheet?> GetSheetAsync(Guid tenantId, Guid sheetId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Sheets.SingleOrDefault(sheet => sheet.TenantId == tenantId && sheet.Id == sheetId));
        }

        public Task<TechnicalSheetSearchResult> SearchAsync(TechnicalSheetSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            var sheets = Sheets
                .Where(sheet => sheet.TenantId == criteria.TenantId)
                .Where(sheet => criteria.SearchText is null || sheet.Title.Contains(criteria.SearchText))
                .Where(sheet => !criteria.Status.HasValue || sheet.Status == criteria.Status.Value)
                .Where(sheet => !criteria.ProductId.HasValue || sheet.ProductId == criteria.ProductId.Value)
                .Select(sheet => new TechnicalSheetSummary(sheet.Id, sheet.TenantId, sheet.ProductId, sheet.Title, sheet.Status, sheet.CurrentVersionNumber, sheet.PdfObjectKey))
                .ToArray();

            return Task.FromResult(new TechnicalSheetSearchResult(sheets, sheets.Length, criteria.Page, criteria.PageSize));
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
        public DateTimeOffset UtcNow => new(2026, 6, 20, 19, 15, 0, TimeSpan.Zero);
    }
}
