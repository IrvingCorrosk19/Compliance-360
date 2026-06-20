using Compliance360.Application.Documents;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Documents;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Documents;

public sealed class EfDocumentRepository : IDocumentRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfDocumentRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddDocumentAsync(Document document, CancellationToken cancellationToken = default)
    {
        await _dbContext.Documents.AddAsync(document, cancellationToken);
    }

    public Task<Document?> GetDocumentAsync(Guid tenantId, Guid documentId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Documents
            .Include(document => document.Versions)
            .Include(document => document.Approvals)
            .Include(document => document.History)
            .Include(document => document.Permissions)
            .FirstOrDefaultAsync(document => document.TenantId == tenantId && document.Id == documentId, cancellationToken);
    }

    public Task<bool> DocumentCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default)
    {
        return _dbContext.Documents.AnyAsync(document => document.TenantId == tenantId && document.Code == code.ToUpperInvariant(), cancellationToken);
    }

    public async Task AddTypeAsync(DocumentType documentType, CancellationToken cancellationToken = default)
    {
        await _dbContext.DocumentTypes.AddAsync(documentType, cancellationToken);
    }

    public Task<DocumentType?> GetTypeAsync(Guid tenantId, Guid documentTypeId, CancellationToken cancellationToken = default)
    {
        return _dbContext.DocumentTypes.FirstOrDefaultAsync(documentType => documentType.TenantId == tenantId && documentType.Id == documentTypeId, cancellationToken);
    }

    public async Task AddCategoryAsync(DocumentCategory category, CancellationToken cancellationToken = default)
    {
        await _dbContext.DocumentCategories.AddAsync(category, cancellationToken);
    }

    public Task<DocumentCategory?> GetCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        return _dbContext.DocumentCategories.FirstOrDefaultAsync(category => category.TenantId == tenantId && category.Id == categoryId, cancellationToken);
    }

    public async Task<DocumentSearchResult> SearchAsync(DocumentSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Documents.AsNoTracking().Where(document => document.TenantId == criteria.TenantId);

        if (criteria.Status.HasValue)
        {
            query = query.Where(document => document.Status == criteria.Status.Value);
        }

        if (criteria.DocumentTypeId.HasValue)
        {
            query = query.Where(document => document.DocumentTypeId == criteria.DocumentTypeId.Value);
        }

        if (criteria.CategoryId.HasValue)
        {
            query = query.Where(document => document.CategoryId == criteria.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            query = query.Where(document => document.Title.Contains(criteria.SearchText) || document.Code.Contains(criteria.SearchText));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(document => document.Code)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(document => new DocumentSummary(
                document.Id,
                document.TenantId,
                document.DocumentTypeId,
                document.CategoryId,
                document.Title,
                document.Code,
                document.Status,
                document.CurrentVersionId,
                document.ApprovedAtUtc,
                document.ExpiresAtUtc))
            .ToListAsync(cancellationToken);

        return new DocumentSearchResult(items, total, criteria.Page, criteria.PageSize);
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }
}
