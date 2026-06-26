using Compliance360.Application.TechnicalSheets;
using Compliance360.Domain.Audit;
using Compliance360.Domain.TechnicalSheets;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.TechnicalSheets;

public sealed class EfTechnicalSheetRepository : ITechnicalSheetRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfTechnicalSheetRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);
    }

    public Task<Product?> GetProductAsync(Guid tenantId, Guid productId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Products.FirstOrDefaultAsync(product => product.TenantId == tenantId && product.Id == productId, cancellationToken);
    }

    public Task<bool> ProductSkuExistsAsync(Guid tenantId, string sku, CancellationToken cancellationToken = default)
    {
        return _dbContext.Products.AnyAsync(product => product.TenantId == tenantId && product.Sku == sku.ToUpperInvariant(), cancellationToken);
    }

    public async Task AddSheetAsync(TechnicalSheet sheet, CancellationToken cancellationToken = default)
    {
        await _dbContext.TechnicalSheets.AddAsync(sheet, cancellationToken);
    }

    public Task<TechnicalSheet?> GetSheetAsync(Guid tenantId, Guid sheetId, CancellationToken cancellationToken = default)
    {
        return _dbContext.TechnicalSheets
            .Include(sheet => sheet.Ingredients)
            .Include(sheet => sheet.Nutrients)
            .Include(sheet => sheet.Certifications)
            .Include(sheet => sheet.Versions)
            .Include(sheet => sheet.Approvals)
            .AsSplitQuery()
            .FirstOrDefaultAsync(sheet => sheet.TenantId == tenantId && sheet.Id == sheetId, cancellationToken);
    }

    public async Task<TechnicalSheetSearchResult> SearchAsync(TechnicalSheetSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TechnicalSheets.AsNoTracking().Where(sheet => sheet.TenantId == criteria.TenantId);

        if (criteria.Status.HasValue)
        {
            query = query.Where(sheet => sheet.Status == criteria.Status.Value);
        }

        if (criteria.ProductId.HasValue)
        {
            query = query.Where(sheet => sheet.ProductId == criteria.ProductId.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            query = query.Where(sheet => sheet.Title.Contains(criteria.SearchText));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(sheet => sheet.Title)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(sheet => new TechnicalSheetSummary(sheet.Id, sheet.TenantId, sheet.ProductId, sheet.Title, sheet.Status, sheet.CurrentVersionNumber, sheet.PdfObjectKey))
            .ToListAsync(cancellationToken);

        return new TechnicalSheetSearchResult(items, total, criteria.Page, criteria.PageSize);
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }
}
