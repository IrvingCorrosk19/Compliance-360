using Compliance360.Application;
using Compliance360.Domain.Common;
using Compliance360.Domain.Enterprise;
using Compliance360.Shared;

namespace Compliance360.Application.Enterprise;

public sealed class EnterpriseWorkspaceService : IEnterpriseWorkspaceService
{
    private readonly IEnterpriseWorkspaceRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;

    public EnterpriseWorkspaceService(IEnterpriseWorkspaceRepository repository, IApplicationDbContext dbContext, IClock clock)
    {
        _repository = repository;
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<Result<EnterpriseWorkspaceItemSummary>> CreateAsync(CreateEnterpriseWorkspaceItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _repository.CodeExistsAsync(command.TenantId, command.Code, cancellationToken))
            {
                return Result<EnterpriseWorkspaceItemSummary>.Failure("Enterprise workspace code already exists.");
            }

            var item = new EnterpriseWorkspaceItem(command.TenantId, command.Type, command.Title, command.Code, command.Description, command.RequestedByUserId, command.MetadataJson);
            if (command.OwnerUserId.HasValue)
            {
                item.Assign(command.OwnerUserId.Value, command.DueAtUtc);
            }

            await _repository.AddAsync(item, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<EnterpriseWorkspaceItemSummary>.Success(ToSummary(item));
        }
        catch (DomainException exception)
        {
            return Result<EnterpriseWorkspaceItemSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<IReadOnlyCollection<EnterpriseWorkspaceItemSummary>>> SearchAsync(EnterpriseWorkspaceSearchQuery query, CancellationToken cancellationToken = default)
    {
        var items = await _repository.SearchAsync(new EnterpriseWorkspaceSearchCriteria(query.TenantId, query.Type, query.Status, query.SearchText), cancellationToken);
        return Result<IReadOnlyCollection<EnterpriseWorkspaceItemSummary>>.Success(items);
    }

    public async Task<Result<EnterpriseWorkspaceDashboardDto>> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        Result<EnterpriseWorkspaceDashboardDto>.Success(await _repository.GetDashboardAsync(tenantId, cancellationToken));

    public async Task<Result> CompleteAsync(EnterpriseWorkspaceActionCommand command, CancellationToken cancellationToken = default) =>
        await ChangeAsync(command.TenantId, command.ItemId, item => item.Complete(_clock.UtcNow), cancellationToken);

    public async Task<Result> ReopenAsync(EnterpriseWorkspaceActionCommand command, CancellationToken cancellationToken = default) =>
        await ChangeAsync(command.TenantId, command.ItemId, item => item.Reopen(), cancellationToken);

    private async Task<Result> ChangeAsync(Guid tenantId, Guid itemId, Action<EnterpriseWorkspaceItem> change, CancellationToken cancellationToken)
    {
        var item = await _repository.GetAsync(tenantId, itemId, cancellationToken);
        if (item is null)
        {
            return Result.Failure("Enterprise workspace item not found.");
        }

        try
        {
            change(item);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    private static EnterpriseWorkspaceItemSummary ToSummary(EnterpriseWorkspaceItem item) =>
        new(item.Id, item.TenantId, item.Type, item.Title, item.Code, item.Description, item.Status, item.OwnerUserId, item.DueAtUtc, item.CompletedAtUtc, item.MetadataJson);
}
