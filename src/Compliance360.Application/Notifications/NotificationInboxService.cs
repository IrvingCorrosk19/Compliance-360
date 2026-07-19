using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public sealed class NotificationInboxService : INotificationInboxService
{
    private readonly INotificationInboxRepository _repository;
    private readonly INotificationAuditService _auditService;
    private readonly IClock _clock;

    public NotificationInboxService(
        INotificationInboxRepository repository,
        INotificationAuditService auditService,
        IClock clock)
    {
        _repository = repository;
        _auditService = auditService;
        _clock = clock;
    }

    public async Task<Result<NotificationInboxPage>> SearchAsync(NotificationInboxQuery query, CancellationToken cancellationToken = default)
    {
        if (query.TenantId == Guid.Empty || query.UserId == Guid.Empty)
        {
            return Result<NotificationInboxPage>.Failure("Tenant and user are required.");
        }

        var normalized = query with
        {
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            Page = Math.Max(1, query.Page),
            PageSize = Math.Clamp(query.PageSize, 1, 100)
        };
        var (items, total) = await _repository.SearchAsync(normalized, cancellationToken);
        return Result<NotificationInboxPage>.Success(new NotificationInboxPage(items, total, normalized.Page, normalized.PageSize));
    }

    public async Task<Result<NotificationInboxCounts>> GetCountsAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty || userId == Guid.Empty)
        {
            return Result<NotificationInboxCounts>.Failure("Tenant and user are required.");
        }

        return Result<NotificationInboxCounts>.Success(await _repository.GetCountsAsync(tenantId, userId, cancellationToken));
    }

    public async Task<Result> ApplyActionAsync(NotificationInboxActionCommand command, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetAsync(command.TenantId, command.UserId, command.InboxItemId, cancellationToken);
        if (item is null)
        {
            return Result.Failure("Inbox item not found.");
        }

        try
        {
            Apply(item, command.Action);
            await _auditService.AppendAsync(
                command.TenantId,
                command.UserId,
                nameof(NotificationInboxItem),
                item.Id,
                AuditAction.Updated,
                true,
                null,
                cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    public async Task<Result<NotificationInboxBulkResult>> ApplyBulkActionAsync(NotificationInboxBulkActionCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.All && command.InboxItemIds.Count == 0)
        {
            return Result<NotificationInboxBulkResult>.Failure("At least one inbox item is required.");
        }

        var items = command.All
            ? await _repository.GetAllActiveAsync(command.TenantId, command.UserId, cancellationToken)
            : await _repository.GetManyAsync(command.TenantId, command.UserId, command.InboxItemIds.Distinct().ToArray(), cancellationToken);

        try
        {
            foreach (var item in items)
            {
                Apply(item, command.Action);
            }

            await _auditService.AppendAsync(
                command.TenantId,
                command.UserId,
                nameof(NotificationInboxItem),
                Guid.Empty,
                AuditAction.Updated,
                true,
                null,
                cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return Result<NotificationInboxBulkResult>.Success(new NotificationInboxBulkResult(
                command.All ? items.Count : command.InboxItemIds.Count,
                items.Count));
        }
        catch (DomainException exception)
        {
            return Result<NotificationInboxBulkResult>.Failure(exception.Message);
        }
    }

    private void Apply(NotificationInboxItem item, NotificationInboxAction action)
    {
        switch (action)
        {
            case NotificationInboxAction.MarkRead:
                item.MarkRead(_clock.UtcNow);
                break;
            case NotificationInboxAction.MarkUnread:
                item.MarkUnread(_clock.UtcNow);
                break;
            case NotificationInboxAction.Archive:
                item.Archive(_clock.UtcNow);
                break;
            case NotificationInboxAction.Delete:
                item.Delete(_clock.UtcNow);
                break;
            case NotificationInboxAction.Restore:
                item.Restore(_clock.UtcNow);
                break;
            case NotificationInboxAction.Favorite:
                item.SetFavorite(true, _clock.UtcNow);
                break;
            case NotificationInboxAction.Unfavorite:
                item.SetFavorite(false, _clock.UtcNow);
                break;
            default:
                throw new DomainException("Unsupported inbox action.");
        }
    }
}
