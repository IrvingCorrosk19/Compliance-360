using Compliance360.Application;
using Compliance360.Application.Notifications;
using Compliance360.Domain.Common;
using Compliance360.Domain.Notifications;

namespace Compliance360.Tests;

public sealed class AlertRuleCenterServiceTests
{
    private const string Condition =
        """{"type":"Compare","left":{"type":"VariableReference","path":"status"},"operator":"Equal","right":{"type":"Constant","value":"Overdue"}}""";

    [Fact]
    public async Task Creates_Simulates_And_Publishes_A_Tenant_Rule_With_Maker_Checker()
    {
        var tenantId = Guid.NewGuid();
        var maker = Guid.NewGuid();
        var reviewer = Guid.NewGuid();
        var repository = new FakeRepository(
            new AlertEventType(tenantId, "dossier.overdue", "Dossier overdue", "Regulatory", "{}", 1));
        var service = new AlertRuleCenterService(repository, new AlertRuleEvaluator(), new FixedClock());

        var created = await service.CreateAsync(new CreateAlertDefinitionCommand(
            tenantId,
            repository.EventType.Id,
            "DOSSIER_OVERDUE",
            "Dossier overdue",
            "Notify accountable users when a dossier is overdue.",
            maker,
            NotificationPriority.High,
            Condition,
            """[{"type":"Owner","kind":"To"}]""",
            """[{"channel":"InApp","templateCode":"DOSSIER_OVERDUE"}]""",
            "{{entityId}}",
            60,
            1440,
            AlertUnknownPolicy.TreatAsFalse,
            maker));

        Assert.True(created.IsSuccess, created.Error);
        var detail = created.Value!;
        var version = Assert.Single(detail.Versions);

        var simulation = await service.SimulateAsync(new AlertRuleSimulationCommand(
            tenantId,
            detail.Definition.Id,
            version.Id,
            null,
            AlertUnknownPolicy.TreatAsFalse,
            """{"status":"Overdue"}"""));
        Assert.True(simulation.IsSuccess, simulation.Error);
        Assert.True(simulation.Value!.Matched);

        Assert.True((await Act("submit", maker)).IsSuccess);
        Assert.False((await Act("review", maker)).IsSuccess);
        Assert.True((await Act("review", reviewer)).IsSuccess);
        Assert.True((await Act("approve", reviewer)).IsSuccess);
        var published = await Act("publish", reviewer);

        Assert.True(published.IsSuccess, published.Error);
        Assert.Equal(AlertDefinitionLifecycle.Published, published.Value!.Definition.Lifecycle);
        Assert.Equal(version.Id, published.Value.Definition.CurrentPublishedVersionId);

        Task<Compliance360.Shared.Result<AlertDefinitionDetail>> Act(string action, Guid userId) =>
            service.ApplyLifecycleActionAsync(new AlertDefinitionLifecycleCommand(
                tenantId,
                detail.Definition.Id,
                version.Id,
                action,
                userId));
    }

    [Fact]
    public async Task Rejects_Event_Type_From_Another_Tenant()
    {
        var tenantId = Guid.NewGuid();
        var repository = new FakeRepository(
            new AlertEventType(Guid.NewGuid(), "dossier.overdue", "Dossier overdue", "Regulatory", "{}", 1));
        var service = new AlertRuleCenterService(repository, new AlertRuleEvaluator(), new FixedClock());

        var result = await service.CreateAsync(new CreateAlertDefinitionCommand(
            tenantId,
            repository.EventType.Id,
            "CROSS_TENANT",
            "Cross tenant",
            "Must be rejected by tenant boundary.",
            Guid.NewGuid(),
            NotificationPriority.Normal,
            Condition,
            "[]",
            "[]",
            "{{entityId}}",
            0,
            null,
            AlertUnknownPolicy.TreatAsFalse,
            Guid.NewGuid()));

        Assert.True(result.IsFailure);
    }

    private sealed class FakeRepository : IAlertRuleCenterRepository
    {
        private readonly List<AlertDefinition> _definitions = [];
        private readonly List<AlertDefinitionVersion> _versions = [];

        public FakeRepository(AlertEventType eventType) => EventType = eventType;

        public AlertEventType EventType { get; }

        public Task<IReadOnlyCollection<AlertEventType>> ListEventTypesAsync(Guid tenantId, string? module, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<AlertEventType>>(
                EventType.TenantId == tenantId ? [EventType] : []);

        public Task<(IReadOnlyCollection<AlertDefinition> Items, int Total)> SearchAsync(AlertDefinitionSearchQuery query, CancellationToken cancellationToken)
        {
            var items = _definitions.Where(item => item.TenantId == query.TenantId).ToArray();
            return Task.FromResult<(IReadOnlyCollection<AlertDefinition>, int)>((items, items.Length));
        }

        public Task<AlertDefinition?> GetDefinitionAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken) =>
            Task.FromResult(_definitions.SingleOrDefault(item => item.TenantId == tenantId && item.Id == definitionId));

        public Task<AlertDefinitionVersion?> GetVersionAsync(Guid tenantId, Guid definitionId, Guid versionId, CancellationToken cancellationToken) =>
            Task.FromResult(_versions.SingleOrDefault(item => item.TenantId == tenantId && item.DefinitionId == definitionId && item.Id == versionId));

        public Task<IReadOnlyCollection<AlertDefinitionVersion>> ListVersionsAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<AlertDefinitionVersion>>(
                _versions.Where(item => item.TenantId == tenantId && item.DefinitionId == definitionId).OrderByDescending(item => item.Version).ToArray());

        public Task<int> NextVersionAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken) =>
            Task.FromResult(_versions.Where(item => item.TenantId == tenantId && item.DefinitionId == definitionId).Select(item => item.Version).DefaultIfEmpty().Max() + 1);

        public Task AddAsync(AlertDefinition definition, AlertDefinitionVersion version, CancellationToken cancellationToken)
        {
            _definitions.Add(definition);
            _versions.Add(version);
            return Task.CompletedTask;
        }

        public Task AddVersionAsync(AlertDefinitionVersion version, CancellationToken cancellationToken)
        {
            _versions.Add(version);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 7, 19, 18, 0, 0, TimeSpan.Zero);
    }
}
