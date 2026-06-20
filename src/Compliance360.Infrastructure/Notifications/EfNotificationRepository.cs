using Compliance360.Application.Notifications;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Notifications;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Notifications;

public sealed class EfNotificationRepository : INotificationRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfNotificationRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        await _dbContext.NotificationTemplates.AddAsync(template, cancellationToken);
    }

    public Task<NotificationTemplate?> GetTemplateAsync(Guid tenantId, string code, NotificationChannel channel, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.ToUpperInvariant();
        return _dbContext.NotificationTemplates.FirstOrDefaultAsync(
            template => template.TenantId == tenantId && template.Code == normalizedCode && template.Channel == channel,
            cancellationToken);
    }

    public async Task AddMessageAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        await _dbContext.NotificationMessages.AddAsync(message, cancellationToken);
    }

    public Task<NotificationMessage?> GetMessageAsync(Guid tenantId, Guid messageId, CancellationToken cancellationToken = default)
    {
        return _dbContext.NotificationMessages.FirstOrDefaultAsync(message => message.TenantId == tenantId && message.Id == messageId, cancellationToken);
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }
}
