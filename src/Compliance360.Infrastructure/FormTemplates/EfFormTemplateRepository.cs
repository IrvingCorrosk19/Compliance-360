using Compliance360.Application.FormTemplates;
using Compliance360.Domain.FormTemplates;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.FormTemplates;

public sealed class EfFormTemplateRepository : IFormTemplateRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfFormTemplateRepository(Compliance360DbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(FormTemplate template, CancellationToken cancellationToken = default)
        => await _dbContext.FormTemplates.AddAsync(template, cancellationToken);

    public Task<FormTemplate?> GetAsync(Guid tenantId, Guid templateId, CancellationToken cancellationToken = default)
        => _dbContext.FormTemplates
            .Include(t => t.Versions)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == templateId, cancellationToken);

    public Task<bool> CodeExistsAsync(Guid tenantId, string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return _dbContext.FormTemplates.AnyAsync(
            t => t.TenantId == tenantId && t.Code == normalized && !t.IsDeleted && (excludeId == null || t.Id != excludeId),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<FormTemplateSummaryDto>> SearchAsync(FormTemplateSearchQuery query, CancellationToken cancellationToken = default)
    {
        var q = _dbContext.FormTemplates.AsNoTracking().Where(t => t.TenantId == query.TenantId);
        if (!query.IncludeDeleted)
        {
            q = q.Where(t => !t.IsDeleted);
        }

        if (query.Status is { } status)
        {
            q = q.Where(t => t.Status == status);
        }

        if (query.Kind is { } kind)
        {
            q = q.Where(t => t.Kind == kind);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var text = query.SearchText.Trim().ToUpperInvariant();
            q = q.Where(t => t.Name.ToUpper().Contains(text) || t.Code.Contains(text) || t.Category.ToUpper().Contains(text));
        }

        return await q.OrderByDescending(t => t.UpdatedAtUtc ?? t.CreatedAtUtc)
            .Select(t => new FormTemplateSummaryDto(
                t.Id,
                t.TenantId,
                t.Name,
                t.Code,
                t.Category,
                t.Kind,
                t.Status,
                t.PublishedVersionNumber,
                t.CreatedByUserId,
                t.CreatedAtUtc,
                t.UpdatedAtUtc))
            .Take(200)
            .ToListAsync(cancellationToken);
    }
}
