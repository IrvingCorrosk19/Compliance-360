using Compliance360.Application;
using Compliance360.Application.Audit;
using Compliance360.Application.FormTemplates;
using Compliance360.Domain.FormTemplates;
using Compliance360.Infrastructure.Audit;
using Compliance360.Infrastructure.FormTemplates;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Tests;

public sealed class FormTemplateBuilderTests
{
    [Fact]
    public async Task Form_Template_Creates_Draft_Publishes_And_Versions()
    {
        await using var db = CreateDb();
        var service = CreateService(db);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var created = await service.CreateAsync(new CreateFormTemplateCommand(
            tenantId,
            "Checklist auditoria higiene",
            "TPL-AUDIT-001",
            "Auditoría",
            FormTemplateKind.Audit,
            "Plantilla base de higiene",
            """{"fields":[{"id":"f1","type":"text","name":"area","label":"Área","required":true}],"rules":[]}""",
            userId));

        Assert.True(created.IsSuccess);
        Assert.Equal(FormTemplateLifecycleStatus.Draft, created.Value!.Status);
        Assert.NotNull(created.Value.WorkingVersion);
        Assert.Equal("1.0", created.Value.WorkingVersion!.VersionNumber);

        var published = await service.PublishAsync(new FormTemplateActionCommand(tenantId, created.Value.Id, created.Value.WorkingVersion.Id, userId));
        Assert.True(published.IsSuccess);
        Assert.Equal(FormTemplateLifecycleStatus.Published, published.Value!.Status);
        Assert.Equal("1.0", published.Value.PublishedVersionNumber);

        var draft2 = await service.SaveDraftAsync(new SaveFormTemplateDraftCommand(
            tenantId,
            created.Value.Id,
            published.Value.Versions.First(v => v.IsPublished).Id,
            """{"fields":[{"id":"f1","type":"text","name":"area","label":"Área","required":true},{"id":"f2","type":"textarea","name":"hallazgo","label":"Hallazgo","required":false}],"rules":[{"id":"r1","whenFieldId":"f1","operator":"equals","value":"Critico","thenAction":"show","thenFieldId":"f2"}]}""",
            "Agrega hallazgos",
            userId));
        Assert.True(draft2.IsSuccess);
        Assert.Contains(draft2.Value!.Versions, v => v.VersionNumber == "1.1" && !v.IsPublished);

        var other = await service.CreateAsync(new CreateFormTemplateCommand(tenantId, "Dup", "TPL-AUDIT-001", "x", FormTemplateKind.Checklist, "d", null, userId));
        Assert.True(other.IsFailure);

        var isolated = await service.SearchAsync(new FormTemplateSearchQuery(Guid.NewGuid(), null, null, null));
        Assert.Empty(isolated.Value!);
    }

    [Fact]
    public async Task Form_Template_Duplicates_Restores_And_Lists_Published()
    {
        await using var db = CreateDb();
        var service = CreateService(db);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var created = await service.CreateAsync(new CreateFormTemplateCommand(
            tenantId,
            "Plantilla origen",
            "TPL-SRC-001",
            "Auditoría",
            FormTemplateKind.Audit,
            "Origen",
            """{"schemaVersion":2,"components":[{"id":"c1","type":"text","name":"area","label":"Área"}],"rules":[],"expressions":[],"workflow":{"steps":[]}}""",
            userId));
        Assert.True(created.IsSuccess);

        var published = await service.PublishAsync(new FormTemplateActionCommand(
            tenantId, created.Value!.Id, created.Value.WorkingVersion!.Id, userId));
        Assert.True(published.IsSuccess);

        var publishedList = await service.SearchPublishedAsync(tenantId, FormTemplateKind.Audit);
        Assert.True(publishedList.IsSuccess);
        Assert.Contains(publishedList.Value!, x => x.Code == "TPL-SRC-001");

        var emptyKind = await service.SearchPublishedAsync(tenantId, FormTemplateKind.Capa);
        Assert.Empty(emptyKind.Value!);

        var dup = await service.DuplicateAsync(new DuplicateFormTemplateCommand(
            tenantId, created.Value.Id, "Plantilla copia", "TPL-SRC-001-COPY", userId));
        Assert.True(dup.IsSuccess);
        Assert.Equal(FormTemplateLifecycleStatus.Draft, dup.Value!.Status);
        Assert.Equal("TPL-SRC-001-COPY", dup.Value.Code);

        var draftAgain = await service.SaveDraftAsync(new SaveFormTemplateDraftCommand(
            tenantId,
            created.Value.Id,
            published.Value!.Versions.First(v => v.IsPublished).Id,
            """{"schemaVersion":2,"components":[{"id":"c1","type":"text","name":"area","label":"Área"},{"id":"c2","type":"evidence","name":"ev","label":"Evidencia"}],"rules":[],"expressions":[],"workflow":{"steps":[]}}""",
            "Nueva evidencia",
            userId));
        Assert.True(draftAgain.IsSuccess);

        var publishedVersion = draftAgain.Value!.Versions.First(v => v.IsPublished);
        var restored = await service.RestoreVersionAsync(new RestoreFormTemplateVersionCommand(
            tenantId, created.Value.Id, publishedVersion.Id, userId));
        Assert.True(restored.IsSuccess);
        Assert.Contains(restored.Value!.Versions, v => !v.IsPublished && v.ChangeLog.Contains("Restaurado", StringComparison.OrdinalIgnoreCase));
    }

    private static FormTemplateService CreateService(Compliance360DbContext db)
        => new(new EfFormTemplateRepository(db), db, new EfAuditRepository(db), new FixedClock());

    private static Compliance360DbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<Compliance360DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new Compliance360DbContext(options, new FixedClock());
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 7, 14, 12, 0, 0, TimeSpan.Zero);
    }
}
