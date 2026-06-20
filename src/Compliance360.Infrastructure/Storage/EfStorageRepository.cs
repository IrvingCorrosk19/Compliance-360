using Compliance360.Application.Storage;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Storage;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Storage;

public sealed class EfStorageRepository : IStorageRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfStorageRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(StoredFile storedFile, CancellationToken cancellationToken = default)
    {
        await _dbContext.StoredFiles.AddAsync(storedFile, cancellationToken);
    }

    public Task<StoredFile?> GetByIdAsync(Guid tenantId, Guid storedFileId, CancellationToken cancellationToken = default)
    {
        return _dbContext.StoredFiles.FirstOrDefaultAsync(file => file.TenantId == tenantId && file.Id == storedFileId, cancellationToken);
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }
}
