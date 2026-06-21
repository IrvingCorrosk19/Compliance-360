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

    public async Task<IReadOnlyCollection<NotificationMessage>> ListMessagesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationMessages
            .Where(message => message.TenantId == tenantId)
            .OrderByDescending(message => message.QueuedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddDeliveryAsync(NotificationDelivery delivery, CancellationToken cancellationToken = default)
    {
        await _dbContext.NotificationDeliveries.AddAsync(delivery, cancellationToken);
    }

    public async Task AddRetryAsync(NotificationRetry retry, CancellationToken cancellationToken = default)
    {
        await _dbContext.NotificationRetries.AddAsync(retry, cancellationToken);
    }

    public async Task AddHistoryAsync(NotificationHistory history, CancellationToken cancellationToken = default)
    {
        await _dbContext.NotificationHistory.AddAsync(history, cancellationToken);
    }

    public async Task AddDeadLetterAsync(NotificationDeadLetter deadLetter, CancellationToken cancellationToken = default)
    {
        await _dbContext.NotificationDeadLetters.AddAsync(deadLetter, cancellationToken);
    }

    public async Task<IReadOnlyCollection<NotificationDeadLetter>> ListDeadLettersAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationDeadLetters
            .Where(deadLetter => deadLetter.TenantId == tenantId)
            .OrderByDescending(deadLetter => deadLetter.DeadLetteredAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddProviderConfigurationAsync(NotificationProviderConfiguration configuration, CancellationToken cancellationToken = default)
    {
        await _dbContext.NotificationProviderConfigurations.AddAsync(configuration, cancellationToken);
    }

    public async Task<IReadOnlyCollection<NotificationProviderConfiguration>> ListProviderConfigurationsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationProviderConfigurations
            .Where(configuration => configuration.TenantId == tenantId)
            .OrderBy(configuration => configuration.Priority)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }
}
