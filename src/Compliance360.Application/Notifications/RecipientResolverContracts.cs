using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public interface IRecipientResolverService
{
    Task<Result<RecipientPreview>> PreviewAsync(PreviewRecipientsCommand command, CancellationToken cancellationToken = default);
    Task<Result> SetPreferenceAsync(SetRecipientPreferenceCommand command, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateGroupAsync(CreateRecipientGroupCommand command, CancellationToken cancellationToken = default);
    Task<Result> AddGroupMemberAsync(AddRecipientGroupMemberCommand command, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateDepartmentAsync(CreateRecipientDepartmentCommand command, CancellationToken cancellationToken = default);
    Task<Result> SetDirectoryProfileAsync(SetRecipientDirectoryProfileCommand command, CancellationToken cancellationToken = default);
    Task<Result<Guid>> AuthorizeExternalAsync(AuthorizeExternalRecipientCommand command, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateDistributionListAsync(CreateRecipientDistributionListCommand command, CancellationToken cancellationToken = default);
    Task<Result> AddDistributionListMemberAsync(AddRecipientDistributionListMemberCommand command, CancellationToken cancellationToken = default);
    Task<Result> SetFallbackAsync(SetRecipientFallbackCommand command, CancellationToken cancellationToken = default);
    Task<Result<RecipientDirectorySummary>> GetDirectoryAsync(Guid tenantId, Guid requestedByUserId, CancellationToken cancellationToken = default);
}

public interface IRecipientResolverRepository
{
    Task<AlertDefinitionVersion?> GetVersionAsync(Guid tenantId, Guid definitionId, Guid versionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RecipientUserRecord>> GetUsersAsync(Guid tenantId, IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RecipientUserRecord>> GetUsersByRoleAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RecipientUserRecord>> GetUsersByGroupAsync(Guid tenantId, Guid groupId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RecipientUserRecord>> GetUsersByDepartmentAsync(Guid tenantId, Guid departmentId, CancellationToken cancellationToken = default);
    Task<RecipientUserRecord?> GetSupervisorAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task<AuthorizedExternalRecipient?> GetExternalAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RecipientAddressRecord>> GetDistributionListAsync(Guid tenantId, Guid listId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RecipientAddressRecord>> GetSubscriptionsAsync(Guid tenantId, string topic, NotificationChannel channel, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, bool>> GetPreferencesAsync(Guid tenantId, IReadOnlyCollection<Guid> userIds, NotificationChannel channel, CancellationToken cancellationToken = default);
    Task<RecipientFallbackConfiguration?> GetFallbackAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<NotificationPreference?> GetPreferenceAsync(Guid tenantId, Guid userId, NotificationChannel channel, CancellationToken cancellationToken = default);
    Task<RecipientDirectoryProfile?> GetProfileAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RecipientGroup>> ListGroupsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RecipientDepartment>> ListDepartmentsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AuthorizedExternalRecipient>> ListExternalRecipientsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RecipientDistributionList>> ListDistributionListsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(object entity, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed record PreviewRecipientsCommand(
    Guid TenantId,
    Guid DefinitionId,
    Guid VersionId,
    Guid RequestedByUserId,
    NotificationChannel Channel,
    string? Topic,
    IReadOnlyDictionary<RecipientKind, Guid?> RelationshipUsers);

public sealed record SetRecipientPreferenceCommand(Guid TenantId, Guid RequestedByUserId, NotificationChannel Channel, bool Enabled);
public sealed record CreateRecipientGroupCommand(Guid TenantId, Guid RequestedByUserId, string Name);
public sealed record AddRecipientGroupMemberCommand(Guid TenantId, Guid RequestedByUserId, Guid GroupId, Guid UserId);
public sealed record CreateRecipientDepartmentCommand(Guid TenantId, Guid RequestedByUserId, string Name);
public sealed record SetRecipientDirectoryProfileCommand(Guid TenantId, Guid RequestedByUserId, Guid UserId, Guid? DepartmentId, Guid? SupervisorUserId);
public sealed record AuthorizeExternalRecipientCommand(Guid TenantId, Guid RequestedByUserId, string Email, string DisplayName);
public sealed record CreateRecipientDistributionListCommand(Guid TenantId, Guid RequestedByUserId, string Name);
public sealed record AddRecipientDistributionListMemberCommand(Guid TenantId, Guid RequestedByUserId, Guid DistributionListId, Guid? UserId, Guid? ExternalRecipientId);
public sealed record SetRecipientFallbackCommand(Guid TenantId, Guid RequestedByUserId, RecipientFallbackMode Mode, Guid? TargetId, RecipientRouting Routing);

public sealed record RecipientRule(RecipientKind Kind, RecipientRouting Routing, string? Value, bool RespectPreferences = true);
public sealed record RecipientUserRecord(Guid UserId, string Email, string DisplayName);
public sealed record RecipientAddressRecord(Guid? UserId, string Email, string DisplayName, bool IsExternal);
public sealed record ResolvedRecipient(Guid? UserId, string Address, string DisplayName, RecipientRouting Routing, RecipientKind Source, bool IsExternal);
public sealed record RecipientPreview(
    Guid VersionId,
    IReadOnlyCollection<ResolvedRecipient> To,
    IReadOnlyCollection<ResolvedRecipient> Cc,
    IReadOnlyCollection<ResolvedRecipient> Bcc,
    IReadOnlyCollection<string> Warnings,
    bool FallbackApplied);

public sealed record RecipientDirectoryItem(Guid Id, string Name, string? Address = null, bool IsActive = true);
public sealed record RecipientFallbackSummary(RecipientFallbackMode Mode, Guid? TargetId, RecipientRouting Routing);
public sealed record RecipientDirectorySummary(
    IReadOnlyCollection<RecipientDirectoryItem> Groups,
    IReadOnlyCollection<RecipientDirectoryItem> Departments,
    IReadOnlyCollection<RecipientDirectoryItem> ExternalRecipients,
    IReadOnlyCollection<RecipientDirectoryItem> DistributionLists,
    RecipientFallbackSummary? Fallback);
