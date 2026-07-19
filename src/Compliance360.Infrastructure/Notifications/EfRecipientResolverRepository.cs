using Compliance360.Application.Notifications;
using Compliance360.Domain.Identity;
using Compliance360.Domain.Notifications;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Notifications;

public sealed class EfRecipientResolverRepository : IRecipientResolverRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfRecipientResolverRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AlertDefinitionVersion?> GetVersionAsync(Guid tenantId, Guid definitionId, Guid versionId, CancellationToken cancellationToken = default) =>
        _dbContext.AlertDefinitionVersions.SingleOrDefaultAsync(
            item => item.TenantId == tenantId && item.DefinitionId == definitionId && item.Id == versionId, cancellationToken);

    public async Task<IReadOnlyCollection<RecipientUserRecord>> GetUsersAsync(Guid tenantId, IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default) =>
        await ActiveUsers(tenantId).Where(user => userIds.Contains(user.Id)).Select(ToUser()).ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<RecipientUserRecord>> GetUsersByRoleAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default) =>
        await (from user in ActiveUsers(tenantId)
               join assignment in _dbContext.UserRoles.AsNoTracking()
                   on new { user.TenantId, UserId = user.Id } equals new { assignment.TenantId, assignment.UserId }
               where assignment.TenantId == tenantId && assignment.RoleId == roleId
               select new RecipientUserRecord(user.Id, user.Email, user.FullName))
            .Distinct().ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<RecipientUserRecord>> GetUsersByGroupAsync(Guid tenantId, Guid groupId, CancellationToken cancellationToken = default) =>
        await (from user in ActiveUsers(tenantId)
               join member in _dbContext.RecipientGroupMembers.AsNoTracking()
                   on new { user.TenantId, UserId = user.Id } equals new { member.TenantId, member.UserId }
               where member.TenantId == tenantId && member.GroupId == groupId
               select new RecipientUserRecord(user.Id, user.Email, user.FullName))
            .Distinct().ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<RecipientUserRecord>> GetUsersByDepartmentAsync(Guid tenantId, Guid departmentId, CancellationToken cancellationToken = default) =>
        await (from user in ActiveUsers(tenantId)
               join profile in _dbContext.RecipientDirectoryProfiles.AsNoTracking()
                   on new { user.TenantId, UserId = user.Id } equals new { profile.TenantId, profile.UserId }
               where profile.TenantId == tenantId && profile.DepartmentId == departmentId
               select new RecipientUserRecord(user.Id, user.Email, user.FullName))
            .ToArrayAsync(cancellationToken);

    public async Task<RecipientUserRecord?> GetSupervisorAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        await (from profile in _dbContext.RecipientDirectoryProfiles.AsNoTracking()
               join user in ActiveUsers(tenantId)
                   on new { profile.TenantId, UserId = profile.SupervisorUserId!.Value } equals new { user.TenantId, UserId = user.Id }
               where profile.TenantId == tenantId && profile.UserId == userId && profile.SupervisorUserId.HasValue
               select new RecipientUserRecord(user.Id, user.Email, user.FullName))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<AuthorizedExternalRecipient?> GetExternalAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _dbContext.AuthorizedExternalRecipients.AsNoTracking().SingleOrDefaultAsync(
            item => item.TenantId == tenantId && item.Email == normalized && item.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyCollection<RecipientAddressRecord>> GetDistributionListAsync(Guid tenantId, Guid listId, CancellationToken cancellationToken = default)
    {
        var internalMembers = await (
            from member in _dbContext.RecipientDistributionListMembers.AsNoTracking()
            join user in ActiveUsers(tenantId)
                on new { member.TenantId, UserId = member.UserId!.Value } equals new { user.TenantId, UserId = user.Id }
            where member.TenantId == tenantId && member.DistributionListId == listId && member.UserId.HasValue
            select new RecipientAddressRecord(user.Id, user.Email, user.FullName, false)).ToArrayAsync(cancellationToken);
        var externalMembers = await (
            from member in _dbContext.RecipientDistributionListMembers.AsNoTracking()
            join external in _dbContext.AuthorizedExternalRecipients.AsNoTracking()
                on new { member.TenantId, ExternalId = member.ExternalRecipientId!.Value } equals new { external.TenantId, ExternalId = external.Id }
            where member.TenantId == tenantId && member.DistributionListId == listId && member.ExternalRecipientId.HasValue && external.IsActive
            select new RecipientAddressRecord(null, external.Email, external.DisplayName, true)).ToArrayAsync(cancellationToken);
        return internalMembers.Concat(externalMembers).ToArray();
    }

    public async Task<IReadOnlyCollection<RecipientAddressRecord>> GetSubscriptionsAsync(Guid tenantId, string topic, NotificationChannel channel, CancellationToken cancellationToken = default)
    {
        var addresses = await _dbContext.NotificationSubscriptions.AsNoTracking()
            .Where(item => item.TenantId == tenantId && item.Topic == topic && item.Channel == channel && item.IsActive)
            .Select(item => item.Recipient.ToLower())
            .Distinct()
            .ToArrayAsync(cancellationToken);
        var users = await ActiveUsers(tenantId).Where(user => addresses.Contains(user.Email))
            .Select(user => new RecipientAddressRecord(user.Id, user.Email, user.FullName, false)).ToArrayAsync(cancellationToken);
        var externals = await _dbContext.AuthorizedExternalRecipients.AsNoTracking()
            .Where(item => item.TenantId == tenantId && item.IsActive && addresses.Contains(item.Email))
            .Select(item => new RecipientAddressRecord(null, item.Email, item.DisplayName, true)).ToArrayAsync(cancellationToken);
        return users.Concat(externals).ToArray();
    }

    public async Task<IReadOnlyDictionary<Guid, bool>> GetPreferencesAsync(Guid tenantId, IReadOnlyCollection<Guid> userIds, NotificationChannel channel, CancellationToken cancellationToken = default) =>
        await _dbContext.NotificationPreferences.AsNoTracking()
            .Where(item => item.TenantId == tenantId && userIds.Contains(item.UserId) && item.Channel == channel)
            .ToDictionaryAsync(item => item.UserId, item => item.Enabled, cancellationToken);

    public Task<RecipientFallbackConfiguration?> GetFallbackAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _dbContext.RecipientFallbackConfigurations.SingleOrDefaultAsync(item => item.TenantId == tenantId, cancellationToken);

    public Task<NotificationPreference?> GetPreferenceAsync(Guid tenantId, Guid userId, NotificationChannel channel, CancellationToken cancellationToken = default) =>
        _dbContext.NotificationPreferences.SingleOrDefaultAsync(item => item.TenantId == tenantId && item.UserId == userId && item.Channel == channel, cancellationToken);

    public Task<RecipientDirectoryProfile?> GetProfileAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        _dbContext.RecipientDirectoryProfiles.SingleOrDefaultAsync(item => item.TenantId == tenantId && item.UserId == userId, cancellationToken);

    public async Task<IReadOnlyCollection<RecipientGroup>> ListGroupsAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _dbContext.RecipientGroups.AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .OrderBy(item => item.Name)
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<RecipientDepartment>> ListDepartmentsAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _dbContext.RecipientDepartments.AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .OrderBy(item => item.Name)
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<AuthorizedExternalRecipient>> ListExternalRecipientsAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _dbContext.AuthorizedExternalRecipients.AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .OrderBy(item => item.DisplayName)
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<RecipientDistributionList>> ListDistributionListsAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _dbContext.RecipientDistributionLists.AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .OrderBy(item => item.Name)
            .ToArrayAsync(cancellationToken);

    public async Task AddAsync(object entity, CancellationToken cancellationToken = default) =>
        await _dbContext.AddAsync(entity, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<User> ActiveUsers(Guid tenantId) =>
        _dbContext.Users.AsNoTracking().Where(user => user.TenantId == tenantId && user.Status == UserStatus.Active);

    private static System.Linq.Expressions.Expression<Func<User, RecipientUserRecord>> ToUser() =>
        user => new RecipientUserRecord(user.Id, user.Email, user.FullName);
}
