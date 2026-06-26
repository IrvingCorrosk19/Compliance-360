using Compliance360.Application.Suppliers;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Suppliers;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Suppliers;

public sealed class EfSupplierRepository : ISupplierRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfSupplierRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        await _dbContext.Suppliers.AddAsync(supplier, cancellationToken);
    }

    public Task<Supplier?> GetAsync(Guid tenantId, Guid supplierId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Suppliers
            .Include(supplier => supplier.Documents)
            .Include(supplier => supplier.Evaluations)
            .Include(supplier => supplier.Alerts)
            .AsSplitQuery()
            .FirstOrDefaultAsync(supplier => supplier.TenantId == tenantId && supplier.Id == supplierId, cancellationToken);
    }

    public Task<bool> TaxIdentifierExistsAsync(Guid tenantId, string taxIdentifier, CancellationToken cancellationToken = default)
    {
        return _dbContext.Suppliers.AnyAsync(supplier => supplier.TenantId == tenantId && supplier.TaxIdentifier == taxIdentifier.ToUpperInvariant(), cancellationToken);
    }

    public async Task<SupplierSearchResult> SearchAsync(SupplierSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Suppliers.AsNoTracking().Where(supplier => supplier.TenantId == criteria.TenantId);

        if (criteria.Status.HasValue)
        {
            query = query.Where(supplier => supplier.Status == criteria.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            query = query.Where(supplier => supplier.LegalName.Contains(criteria.SearchText) || supplier.TaxIdentifier.Contains(criteria.SearchText));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(supplier => supplier.LegalName)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(supplier => new SupplierSummary(supplier.Id, supplier.TenantId, supplier.LegalName, supplier.TaxIdentifier, supplier.CountryCode, supplier.Status, supplier.HomologatedAtUtc))
            .ToListAsync(cancellationToken);

        return new SupplierSearchResult(items, total, criteria.Page, criteria.PageSize);
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }
}
