using Compliance360.Domain.Enterprise;
using Compliance360.Shared;

namespace Compliance360.Application.Enterprise;

public interface IEnterpriseWorkspaceService
{
    Task<Result<EnterpriseWorkspaceItemSummary>> CreateAsync(CreateEnterpriseWorkspaceItemCommand command, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<EnterpriseWorkspaceItemSummary>>> SearchAsync(EnterpriseWorkspaceSearchQuery query, CancellationToken cancellationToken = default);

    Task<Result<EnterpriseWorkspaceDashboardDto>> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<Result> CompleteAsync(EnterpriseWorkspaceActionCommand command, CancellationToken cancellationToken = default);

    Task<Result> ReopenAsync(EnterpriseWorkspaceActionCommand command, CancellationToken cancellationToken = default);
}

public interface IEnterpriseWorkspaceRepository
{
    Task AddAsync(EnterpriseWorkspaceItem item, CancellationToken cancellationToken = default);

    Task<EnterpriseWorkspaceItem?> GetAsync(Guid tenantId, Guid itemId, CancellationToken cancellationToken = default);

    Task<bool> CodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<EnterpriseWorkspaceItemSummary>> SearchAsync(EnterpriseWorkspaceSearchCriteria criteria, CancellationToken cancellationToken = default);

    Task<EnterpriseWorkspaceDashboardDto> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public sealed record CreateEnterpriseWorkspaceItemCommand(
    Guid TenantId,
    EnterpriseWorkspaceType Type,
    string Title,
    string Code,
    string Description,
    Guid RequestedByUserId,
    Guid? OwnerUserId,
    DateTimeOffset? DueAtUtc,
    string MetadataJson);

public sealed record EnterpriseWorkspaceActionCommand(Guid TenantId, Guid ItemId, Guid RequestedByUserId);

public sealed record EnterpriseWorkspaceSearchQuery(
    Guid TenantId,
    EnterpriseWorkspaceType? Type,
    EnterpriseWorkspaceStatus? Status,
    string? SearchText);

public sealed record EnterpriseWorkspaceSearchCriteria(
    Guid TenantId,
    EnterpriseWorkspaceType? Type,
    EnterpriseWorkspaceStatus? Status,
    string? SearchText);

public sealed record EnterpriseWorkspaceItemSummary(
    Guid Id,
    Guid TenantId,
    EnterpriseWorkspaceType Type,
    string Title,
    string Code,
    string Description,
    EnterpriseWorkspaceStatus Status,
    Guid? OwnerUserId,
    DateTimeOffset? DueAtUtc,
    DateTimeOffset? CompletedAtUtc,
    string MetadataJson);

public sealed record EnterpriseWorkspaceDashboardDto(
    int TotalItems,
    int ActiveItems,
    int CompletedItems,
    int OverdueItems,
    IReadOnlyDictionary<EnterpriseWorkspaceType, int> ItemsByType);
