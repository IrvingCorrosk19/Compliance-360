using Compliance360.Application;
using Compliance360.Application.Notifications;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Notifications;

namespace Compliance360.Tests;

public sealed class RecipientResolverTests
{
    [Fact]
    public async Task Preview_Resolves_Routing_Preferences_And_Authorized_External_Recipients()
    {
        var tenantId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var version = Version(tenantId,
            $$"""[{"kind":"Owner","routing":"To"},{"kind":"Role","routing":"Cc","value":"{{roleId}}"},{"kind":"ExternalEmail","routing":"Bcc","value":"partner@example.com"},{"kind":"ExternalEmail","routing":"To","value":"attacker@example.com"}]""");
        var repository = new FakeRepository(version)
        {
            Users = [new(ownerId, "owner@example.com", "Owner")],
            RoleUsers = [new(Guid.NewGuid(), "reviewer@example.com", "Reviewer")],
            External = new AuthorizedExternalRecipient(tenantId, "partner@example.com", "Partner"),
            Preferences = new Dictionary<Guid, bool> { [ownerId] = false }
        };
        var service = new RecipientResolverService(repository, new NoOpAudit(), new FixedClock());

        var result = await service.PreviewAsync(new PreviewRecipientsCommand(
            tenantId, version.DefinitionId, version.Id, Guid.NewGuid(), NotificationChannel.Email, null,
            new Dictionary<RecipientKind, Guid?> { [RecipientKind.Owner] = ownerId }));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.To);
        Assert.Single(result.Value.Cc);
        Assert.Single(result.Value.Bcc);
        Assert.Equal("partner@example.com", result.Value.Bcc.Single().Address);
        Assert.Contains(result.Value.Warnings, warning => warning.Contains("not authorized", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Value.Warnings, warning => warning.Contains("disabled", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Preview_Applies_Configured_Fallback_When_Rules_Resolve_Nothing()
    {
        var tenantId = Guid.NewGuid();
        var fallbackUser = Guid.NewGuid();
        var version = Version(tenantId, """[{"kind":"Responsible","routing":"To"}]""");
        var repository = new FakeRepository(version)
        {
            Users = [new(fallbackUser, "fallback@example.com", "Fallback")],
            Fallback = new RecipientFallbackConfiguration(tenantId, RecipientFallbackMode.User, fallbackUser, RecipientRouting.Cc)
        };
        var service = new RecipientResolverService(repository, new NoOpAudit(), new FixedClock());

        var result = await service.PreviewAsync(new PreviewRecipientsCommand(
            tenantId, version.DefinitionId, version.Id, Guid.NewGuid(), NotificationChannel.Email, null,
            new Dictionary<RecipientKind, Guid?>()));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.FallbackApplied);
        Assert.Single(result.Value.Cc);
        Assert.Equal(fallbackUser, result.Value.Cc.Single().UserId);
    }

    [Fact]
    public void Distribution_List_Member_Requires_Exactly_One_Tenant_Principal()
    {
        var tenantId = Guid.NewGuid();
        Assert.Throws<Compliance360.Domain.Common.DomainException>(() =>
            new RecipientDistributionListMember(tenantId, Guid.NewGuid(), null, null));
        Assert.Throws<Compliance360.Domain.Common.DomainException>(() =>
            new RecipientDistributionListMember(tenantId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task Preview_Rejects_A_Version_From_Another_Tenant()
    {
        var version = Version(Guid.NewGuid(), """[{"kind":"Owner","routing":"To"}]""");
        var service = new RecipientResolverService(new FakeRepository(version), new NoOpAudit(), new FixedClock());

        var result = await service.PreviewAsync(new PreviewRecipientsCommand(
            Guid.NewGuid(), version.DefinitionId, version.Id, Guid.NewGuid(), NotificationChannel.Email, null,
            new Dictionary<RecipientKind, Guid?>()));

        Assert.True(result.IsFailure);
        Assert.Equal("Alert definition version not found.", result.Error);
    }

    private static AlertDefinitionVersion Version(Guid tenantId, string rules) =>
        new(tenantId, Guid.NewGuid(), 1, """{"type":"Constant","value":true}""", rules, "[]", "entity.id", 0, null,
            AlertUnknownPolicy.TreatAsFalse, Guid.NewGuid());

    private sealed class FakeRepository : IRecipientResolverRepository
    {
        private readonly AlertDefinitionVersion _version;
        public FakeRepository(AlertDefinitionVersion version) => _version = version;
        public IReadOnlyCollection<RecipientUserRecord> Users { get; init; } = [];
        public IReadOnlyCollection<RecipientUserRecord> RoleUsers { get; init; } = [];
        public AuthorizedExternalRecipient? External { get; init; }
        public IReadOnlyDictionary<Guid, bool> Preferences { get; init; } = new Dictionary<Guid, bool>();
        public RecipientFallbackConfiguration? Fallback { get; init; }

        public Task<AlertDefinitionVersion?> GetVersionAsync(Guid tenantId, Guid definitionId, Guid versionId, CancellationToken cancellationToken = default) =>
            Task.FromResult<AlertDefinitionVersion?>(_version.TenantId == tenantId && _version.DefinitionId == definitionId && _version.Id == versionId ? _version : null);
        public Task<IReadOnlyCollection<RecipientUserRecord>> GetUsersAsync(Guid tenantId, IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<RecipientUserRecord>>(Users.Where(item => userIds.Contains(item.UserId)).ToArray());
        public Task<IReadOnlyCollection<RecipientUserRecord>> GetUsersByRoleAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default) => Task.FromResult(RoleUsers);
        public Task<IReadOnlyCollection<RecipientUserRecord>> GetUsersByGroupAsync(Guid tenantId, Guid groupId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<RecipientUserRecord>>([]);
        public Task<IReadOnlyCollection<RecipientUserRecord>> GetUsersByDepartmentAsync(Guid tenantId, Guid departmentId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<RecipientUserRecord>>([]);
        public Task<RecipientUserRecord?> GetSupervisorAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<RecipientUserRecord?>(null);
        public Task<AuthorizedExternalRecipient?> GetExternalAsync(Guid tenantId, string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(External?.TenantId == tenantId && External.Email == email ? External : null);
        public Task<IReadOnlyCollection<RecipientAddressRecord>> GetDistributionListAsync(Guid tenantId, Guid listId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<RecipientAddressRecord>>([]);
        public Task<IReadOnlyCollection<RecipientAddressRecord>> GetSubscriptionsAsync(Guid tenantId, string topic, NotificationChannel channel, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<RecipientAddressRecord>>([]);
        public Task<IReadOnlyDictionary<Guid, bool>> GetPreferencesAsync(Guid tenantId, IReadOnlyCollection<Guid> userIds, NotificationChannel channel, CancellationToken cancellationToken = default) => Task.FromResult(Preferences);
        public Task<RecipientFallbackConfiguration?> GetFallbackAsync(Guid tenantId, CancellationToken cancellationToken = default) => Task.FromResult(Fallback?.TenantId == tenantId ? Fallback : null);
        public Task<NotificationPreference?> GetPreferenceAsync(Guid tenantId, Guid userId, NotificationChannel channel, CancellationToken cancellationToken = default) => Task.FromResult<NotificationPreference?>(null);
        public Task<RecipientDirectoryProfile?> GetProfileAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<RecipientDirectoryProfile?>(null);
        public Task<IReadOnlyCollection<RecipientGroup>> ListGroupsAsync(Guid tenantId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<RecipientGroup>>([]);
        public Task<IReadOnlyCollection<RecipientDepartment>> ListDepartmentsAsync(Guid tenantId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<RecipientDepartment>>([]);
        public Task<IReadOnlyCollection<AuthorizedExternalRecipient>> ListExternalRecipientsAsync(Guid tenantId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<AuthorizedExternalRecipient>>([]);
        public Task<IReadOnlyCollection<RecipientDistributionList>> ListDistributionListsAsync(Guid tenantId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<RecipientDistributionList>>([]);
        public Task AddAsync(object entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class NoOpAudit : INotificationAuditService
    {
        public Task AppendAsync(Guid tenantId, Guid userId, string entityName, Guid entityId, AuditAction action, bool success, string? error, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 7, 19, 17, 45, 0, TimeSpan.Zero);
    }
}
