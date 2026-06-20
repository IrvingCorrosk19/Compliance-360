using Compliance360.Application.Mfa;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Identity;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Mfa;

public sealed class EfMfaRepository : IMfaRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfMfaRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(user => user.TenantId == tenantId && user.Id == userId, cancellationToken);
    }

    public Task<MfaConfiguration?> GetConfigurationAsync(Guid tenantId, Guid userId, MfaMethod method, CancellationToken cancellationToken = default)
    {
        return _dbContext.MfaConfigurations.FirstOrDefaultAsync(
            configuration => configuration.TenantId == tenantId && configuration.UserId == userId && configuration.Method == method,
            cancellationToken);
    }

    public async Task AddConfigurationAsync(MfaConfiguration configuration, CancellationToken cancellationToken = default)
    {
        await _dbContext.MfaConfigurations.AddAsync(configuration, cancellationToken);
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }
}
