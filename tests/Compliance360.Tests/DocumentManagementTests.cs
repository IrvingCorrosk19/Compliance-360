using Compliance360.Application;
using Compliance360.Application.Documents;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Documents;
using Compliance360.Infrastructure.Documents;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Compliance360.Tests;

public sealed class DocumentManagementTests
{
    [Fact]
    public async Task Document_Workflow_Creates_Versions_And_Approves()
    {
        var fixture = DocumentFixture.Create();
        var type = await fixture.Service.CreateTypeAsync(new CreateDocumentTypeCommand(fixture.TenantId, "Procedure", "PROC", 365, fixture.UserId));
        var category = await fixture.Service.CreateCategoryAsync(new CreateDocumentCategoryCommand(fixture.TenantId, "Quality", "QUAL", fixture.UserId));
        var document = await fixture.Service.CreateDocumentAsync(new CreateDocumentCommand(fixture.TenantId, type.Value!.Id, category.Value!.Id, "Cleaning Procedure", "SOP-001", fixture.UserId));

        var version = await fixture.Service.AddVersionAsync(new AddDocumentVersionCommand(fixture.TenantId, document.Value!.Id, Guid.NewGuid(), "Initial release", fixture.UserId));
        var submitted = await fixture.Service.SubmitForReviewAsync(new DocumentActionCommand(fixture.TenantId, document.Value.Id, fixture.UserId));
        var approval = await fixture.Service.DecideAsync(new DecideDocumentCommand(fixture.TenantId, document.Value.Id, DocumentApprovalDecision.Approved, "Approved for use", fixture.UserId));

        Assert.True(version.IsSuccess);
        Assert.True(submitted.IsSuccess);
        Assert.True(approval.IsSuccess);
        Assert.Equal(DocumentStatus.Approved, fixture.Repository.Documents.Single().Status);
        Assert.NotNull(fixture.Repository.Documents.Single().ExpiresAtUtc);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.DocumentApproved);
    }

    [Fact]
    public async Task Document_Workflow_Rejects_Review_Without_Version()
    {
        var fixture = DocumentFixture.CreateWithReferenceData();
        var document = await fixture.Service.CreateDocumentAsync(new CreateDocumentCommand(fixture.TenantId, fixture.TypeId, fixture.CategoryId, "Policy", "POL-001", fixture.UserId));

        var submitted = await fixture.Service.SubmitForReviewAsync(new DocumentActionCommand(fixture.TenantId, document.Value!.Id, fixture.UserId));

        Assert.True(submitted.IsFailure);
    }

    [Fact]
    public async Task Document_Workflow_Rejects_And_Marks_Obsolete()
    {
        var fixture = DocumentFixture.CreateWithReferenceData();
        var document = await fixture.Service.CreateDocumentAsync(new CreateDocumentCommand(fixture.TenantId, fixture.TypeId, fixture.CategoryId, "Policy", "POL-002", fixture.UserId));
        await fixture.Service.AddVersionAsync(new AddDocumentVersionCommand(fixture.TenantId, document.Value!.Id, Guid.NewGuid(), "Initial", fixture.UserId));
        await fixture.Service.SubmitForReviewAsync(new DocumentActionCommand(fixture.TenantId, document.Value.Id, fixture.UserId));

        var rejection = await fixture.Service.DecideAsync(new DecideDocumentCommand(fixture.TenantId, document.Value.Id, DocumentApprovalDecision.Rejected, "Needs correction", fixture.UserId));
        var obsolete = await fixture.Service.MarkObsoleteAsync(new DocumentActionCommand(fixture.TenantId, document.Value.Id, fixture.UserId));

        Assert.True(rejection.IsSuccess);
        Assert.True(obsolete.IsSuccess);
        Assert.Equal(DocumentStatus.Obsolete, fixture.Repository.Documents.Single().Status);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.DocumentRejected);
    }

    [Fact]
    public async Task GrantPermission_Adds_Document_Permission_Once()
    {
        var fixture = DocumentFixture.CreateWithReferenceData();
        var document = await fixture.Service.CreateDocumentAsync(new CreateDocumentCommand(fixture.TenantId, fixture.TypeId, fixture.CategoryId, "Permission Policy", "PERM-001", fixture.UserId));
        var principalId = Guid.NewGuid();

        var firstGrant = await fixture.Service.GrantPermissionAsync(new GrantDocumentPermissionCommand(fixture.TenantId, document.Value!.Id, principalId, DocumentPermissionLevel.Approve, fixture.UserId));
        var duplicateGrant = await fixture.Service.GrantPermissionAsync(new GrantDocumentPermissionCommand(fixture.TenantId, document.Value.Id, principalId, DocumentPermissionLevel.Approve, fixture.UserId));

        Assert.True(firstGrant.IsSuccess);
        Assert.True(duplicateGrant.IsSuccess);
        Assert.Single(fixture.Repository.Documents.Single().Permissions);
    }

    [Fact]
    public async Task CreateDocument_Rejects_Missing_Type_Or_Category()
    {
        var fixture = DocumentFixture.CreateWithReferenceData();

        var missingType = await fixture.Service.CreateDocumentAsync(new CreateDocumentCommand(fixture.TenantId, Guid.NewGuid(), fixture.CategoryId, "Policy", "MISS-001", fixture.UserId));
        var missingCategory = await fixture.Service.CreateDocumentAsync(new CreateDocumentCommand(fixture.TenantId, fixture.TypeId, Guid.NewGuid(), "Policy", "MISS-002", fixture.UserId));

        Assert.True(missingType.IsFailure);
        Assert.True(missingCategory.IsFailure);
    }

    [Fact]
    public async Task Document_Service_Returns_Failures_For_Missing_Document_Actions()
    {
        var fixture = DocumentFixture.CreateWithReferenceData();
        var missingDocumentId = Guid.NewGuid();

        var version = await fixture.Service.AddVersionAsync(new AddDocumentVersionCommand(fixture.TenantId, missingDocumentId, Guid.NewGuid(), "Missing", fixture.UserId));
        var approval = await fixture.Service.DecideAsync(new DecideDocumentCommand(fixture.TenantId, missingDocumentId, DocumentApprovalDecision.Approved, "Missing", fixture.UserId));
        var grant = await fixture.Service.GrantPermissionAsync(new GrantDocumentPermissionCommand(fixture.TenantId, missingDocumentId, Guid.NewGuid(), DocumentPermissionLevel.Read, fixture.UserId));
        var obsolete = await fixture.Service.MarkObsoleteAsync(new DocumentActionCommand(fixture.TenantId, missingDocumentId, fixture.UserId));

        Assert.True(version.IsFailure);
        Assert.True(approval.IsFailure);
        Assert.True(grant.IsFailure);
        Assert.True(obsolete.IsFailure);
    }

    [Fact]
    public async Task Document_Service_Returns_Failures_For_Invalid_Type_And_Category()
    {
        var fixture = DocumentFixture.Create();

        var invalidType = await fixture.Service.CreateTypeAsync(new CreateDocumentTypeCommand(fixture.TenantId, "", "TYPE", 365, fixture.UserId));
        var invalidCategory = await fixture.Service.CreateCategoryAsync(new CreateDocumentCategoryCommand(fixture.TenantId, "", "CAT", fixture.UserId));

        Assert.True(invalidType.IsFailure);
        Assert.True(invalidCategory.IsFailure);
    }

    [Fact]
    public async Task AddVersion_Returns_Failure_When_Document_Is_Obsolete()
    {
        var fixture = DocumentFixture.CreateWithReferenceData();
        var document = await fixture.Service.CreateDocumentAsync(new CreateDocumentCommand(fixture.TenantId, fixture.TypeId, fixture.CategoryId, "Obsolete Policy", "OBS-001", fixture.UserId));
        await fixture.Service.MarkObsoleteAsync(new DocumentActionCommand(fixture.TenantId, document.Value!.Id, fixture.UserId));

        var version = await fixture.Service.AddVersionAsync(new AddDocumentVersionCommand(fixture.TenantId, document.Value.Id, Guid.NewGuid(), "Blocked", fixture.UserId));

        Assert.True(version.IsFailure);
    }

    [Fact]
    public void Document_Domain_Rejects_Invalid_State_Transitions()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var document = new Document(tenantId, Guid.NewGuid(), Guid.NewGuid(), "Policy", "POL-ERR", userId, DateTimeOffset.UtcNow);

        Assert.Throws<DomainException>(() => document.Decide(DocumentApprovalDecision.Approved, "Not in review", userId, DateTimeOffset.UtcNow));

        document.MarkObsolete(userId, DateTimeOffset.UtcNow);

        Assert.Throws<DomainException>(() => document.AddVersion("Cannot add", Guid.NewGuid(), userId, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Document_Domain_Constructors_Validate_Input()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        Assert.Throws<DomainException>(() => new DocumentType(tenantId, "", "TYPE", 365));
        Assert.Throws<DomainException>(() => new DocumentType(tenantId, "Type", "TYPE", 1));
        Assert.Throws<DomainException>(() => new DocumentCategory(tenantId, "Category", ""));
        Assert.Throws<DomainException>(() => new Document(tenantId, Guid.Empty, Guid.NewGuid(), "Doc", "DOC", userId, DateTimeOffset.UtcNow));
        Assert.Throws<DomainException>(() => new DocumentVersion(tenantId, Guid.NewGuid(), 0, Guid.NewGuid(), "Change", userId, DateTimeOffset.UtcNow));
        Assert.Throws<DomainException>(() => new DocumentApproval(tenantId, Guid.NewGuid(), Guid.NewGuid(), DocumentApprovalDecision.Approved, "", userId, DateTimeOffset.UtcNow));
        Assert.Throws<DomainException>(() => new DocumentHistory(tenantId, Guid.NewGuid(), "", userId, DateTimeOffset.UtcNow));
        Assert.Throws<DomainException>(() => new DocumentPermission(tenantId, Guid.NewGuid(), Guid.Empty, DocumentPermissionLevel.Read, userId, DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task Document_Search_Is_Tenant_Isolated()
    {
        var fixture = DocumentFixture.CreateWithReferenceData();
        await fixture.Service.CreateDocumentAsync(new CreateDocumentCommand(fixture.TenantId, fixture.TypeId, fixture.CategoryId, "Food Safety", "FS-001", fixture.UserId));
        fixture.Repository.Documents.Add(new Document(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Other Tenant", "OT-001", fixture.UserId, DateTimeOffset.UtcNow));

        var result = await fixture.Service.SearchAsync(new DocumentSearchQuery(fixture.TenantId, "Food", null, null, null, 1, 50));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal("FS-001", result.Value.Items.Single().Code);
    }

    [Fact]
    public async Task EfDocumentRepository_Persists_Document_Graph()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfDocumentRepository(dbContext);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var documentType = new DocumentType(tenantId, "Manual", "MAN", 365);
        var category = new DocumentCategory(tenantId, "Operations", "OPS");
        var document = new Document(tenantId, documentType.Id, category.Id, "Operations Manual", "MAN-001", userId, DateTimeOffset.UtcNow);
        document.AddVersion("Initial", Guid.NewGuid(), userId, DateTimeOffset.UtcNow);

        await repository.AddTypeAsync(documentType);
        await repository.AddCategoryAsync(category);
        await repository.AddDocumentAsync(document);
        await repository.AddAuditLogAsync(AuditLog.Create(tenantId, userId, nameof(Document), document.Id, AuditAction.DocumentCreated, DateTimeOffset.UtcNow));
        await dbContext.SaveChangesAsync();

        var loaded = await repository.GetDocumentAsync(tenantId, document.Id);
        var search = await repository.SearchAsync(new DocumentSearchCriteria(tenantId, "Operations", null, null, null, 1, 10));

        Assert.NotNull(loaded);
        Assert.Single(loaded!.Versions);
        Assert.Single(search.Items);
        Assert.Single(dbContext.AuditLogs);
    }

    [Fact]
    public async Task EfDocumentRepository_Searches_By_Status_Type_And_Category()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfDocumentRepository(dbContext);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var documentType = new DocumentType(tenantId, "Manual", "MAN", 365);
        var category = new DocumentCategory(tenantId, "Operations", "OPS");
        var document = new Document(tenantId, documentType.Id, category.Id, "Operations Manual", "MAN-002", userId, DateTimeOffset.UtcNow);

        await repository.AddTypeAsync(documentType);
        await repository.AddCategoryAsync(category);
        await repository.AddDocumentAsync(document);
        await dbContext.SaveChangesAsync();

        var search = await repository.SearchAsync(new DocumentSearchCriteria(tenantId, null, DocumentStatus.Draft, documentType.Id, category.Id, 1, 10));
        var exists = await repository.DocumentCodeExistsAsync(tenantId, "MAN-002");
        var loadedType = await repository.GetTypeAsync(tenantId, documentType.Id);
        var loadedCategory = await repository.GetCategoryAsync(tenantId, category.Id);

        Assert.Single(search.Items);
        Assert.True(exists);
        Assert.NotNull(loadedType);
        Assert.NotNull(loadedCategory);
    }

    private static Compliance360DbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<Compliance360DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new Compliance360DbContext(options, new FixedClock());
    }

    private sealed class DocumentFixture
    {
        private DocumentFixture()
        {
            TenantId = Guid.NewGuid();
            UserId = Guid.NewGuid();
            Repository = new InMemoryDocumentRepository();
            Service = new DocumentManagementService(Repository, new FakeApplicationDbContext(), new FixedClock(), Options.Create(new DocumentManagementOptions()));
        }

        public Guid TenantId { get; }
        public Guid UserId { get; }
        public Guid TypeId { get; private set; }
        public Guid CategoryId { get; private set; }
        public InMemoryDocumentRepository Repository { get; }
        public DocumentManagementService Service { get; }

        public static DocumentFixture Create()
        {
            return new DocumentFixture();
        }

        public static DocumentFixture CreateWithReferenceData()
        {
            var fixture = Create();
            var documentType = new DocumentType(fixture.TenantId, "Procedure", "PROC", 365);
            var category = new DocumentCategory(fixture.TenantId, "Quality", "QUAL");
            fixture.TypeId = documentType.Id;
            fixture.CategoryId = category.Id;
            fixture.Repository.Types.Add(documentType);
            fixture.Repository.Categories.Add(category);
            return fixture;
        }
    }

    private sealed class InMemoryDocumentRepository : IDocumentRepository
    {
        public List<Document> Documents { get; } = [];
        public List<DocumentType> Types { get; } = [];
        public List<DocumentCategory> Categories { get; } = [];
        public List<AuditLog> AuditLogs { get; } = [];

        public Task AddDocumentAsync(Document document, CancellationToken cancellationToken = default)
        {
            Documents.Add(document);
            return Task.CompletedTask;
        }

        public Task<Document?> GetDocumentAsync(Guid tenantId, Guid documentId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Documents.SingleOrDefault(document => document.TenantId == tenantId && document.Id == documentId));
        }

        public Task<bool> DocumentCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Documents.Any(document => document.TenantId == tenantId && document.Code == code));
        }

        public Task AddTypeAsync(DocumentType documentType, CancellationToken cancellationToken = default)
        {
            Types.Add(documentType);
            return Task.CompletedTask;
        }

        public Task<DocumentType?> GetTypeAsync(Guid tenantId, Guid documentTypeId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Types.SingleOrDefault(documentType => documentType.TenantId == tenantId && documentType.Id == documentTypeId));
        }

        public Task AddCategoryAsync(DocumentCategory category, CancellationToken cancellationToken = default)
        {
            Categories.Add(category);
            return Task.CompletedTask;
        }

        public Task<DocumentCategory?> GetCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Categories.SingleOrDefault(category => category.TenantId == tenantId && category.Id == categoryId));
        }

        public Task<DocumentSearchResult> SearchAsync(DocumentSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            var documents = Documents
                .Where(document => document.TenantId == criteria.TenantId)
                .Where(document => criteria.SearchText is null || document.Title.Contains(criteria.SearchText) || document.Code.Contains(criteria.SearchText))
                .Select(document => new DocumentSummary(document.Id, document.TenantId, document.DocumentTypeId, document.CategoryId, document.Title, document.Code, document.Status, document.CurrentVersionId, document.ApprovedAtUtc, document.ExpiresAtUtc))
                .ToArray();

            return Task.FromResult(new DocumentSearchResult(documents, documents.Length, criteria.Page, criteria.PageSize));
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
        public DateTimeOffset UtcNow => new(2026, 6, 20, 17, 0, 0, TimeSpan.Zero);
    }
}
