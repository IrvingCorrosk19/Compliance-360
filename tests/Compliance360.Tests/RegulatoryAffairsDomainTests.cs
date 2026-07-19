using Compliance360.Domain.Common;
using Compliance360.Domain.RegulatoryAffairs;

namespace Compliance360.Tests;

public sealed class RegulatoryAffairsDomainTests
{
    [Fact]
    public void Submit_blocked_when_critical_requirements_incomplete()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var pack = BuildPack(tenantId, userId);
        var dossier = new RegistrationDossier(
            tenantId, "RA-TEST-1", Guid.NewGuid(), Guid.NewGuid(), RegistrationProcessType.NewRegistration,
            null, null, userId, null, null, "USD", null, pack.Id, pack.VersionLabel, userId);
        dossier.ApplyRequirementPack(pack);
        dossier.TransitionTo(RegistrationDossierStatus.Planning, DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.WaitingManufacturerDocuments, DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.DocumentsReceived, DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.Assembling, DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.ReadyForSubmission, DateTimeOffset.UtcNow);

        var ex = Assert.Throws<DomainException>(() =>
            dossier.TransitionTo(RegistrationDossierStatus.Submitted, DateTimeOffset.UtcNow));
        Assert.Contains("critical requirements", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Submit_allowed_when_critical_requirements_accepted()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var pack = BuildPack(tenantId, userId);
        var dossier = new RegistrationDossier(
            tenantId, "RA-TEST-2", Guid.NewGuid(), Guid.NewGuid(), RegistrationProcessType.NewRegistration,
            null, null, userId, null, null, "USD", null, pack.Id, pack.VersionLabel, userId);
        dossier.ApplyRequirementPack(pack);
        foreach (var req in dossier.Requirements.Where(r => r.IsCritical))
        {
            req.SetStatus(DossierRequirementStatus.Accepted, "ok", userId);
        }

        dossier.TransitionTo(RegistrationDossierStatus.Planning, DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.WaitingManufacturerDocuments, DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.DocumentsReceived, DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.Assembling, DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.ReadyForSubmission, DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.ApprovedForSubmission, DateTimeOffset.UtcNow);
        dossier.MarkInternallyApproved(Guid.NewGuid(), DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.Submitted, DateTimeOffset.UtcNow);

        Assert.Equal(RegistrationDossierStatus.Submitted, dossier.Status);
        Assert.NotNull(dossier.SubmittedOn);
    }

    [Fact]
    public void Approved_means_external_authority_not_internal_clearance()
    {
        Assert.NotEqual(RegistrationDossierStatus.Approved, RegistrationDossierStatus.ApprovedForSubmission);
        Assert.Equal(11, (int)RegistrationDossierStatus.Approved);
        Assert.Equal(16, (int)RegistrationDossierStatus.ApprovedForSubmission);
    }

    [Fact]
    public void Sanitary_registration_requires_number_to_activate()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new SanitaryRegistration(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), " ",
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1), null, Guid.NewGuid(), activate: true));
        Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Risk_class_enum_is_device_specific_not_org_risk()
    {
        Assert.Equal(0, (int)DeviceRiskClass.A);
        Assert.Equal(1, (int)DeviceRiskClass.B);
        Assert.Equal(2, (int)DeviceRiskClass.C);
        Assert.Equal(22, RegutrackRequirementCatalog.Items.Count);
    }

    [Fact]
    public void Default_pack_contains_regutrack_checklist()
    {
        var pack = BuildPack(Guid.NewGuid(), Guid.NewGuid());
        Assert.Equal(RequirementPackStatus.Published, pack.Status);
        Assert.Contains(pack.Definitions, d => d.Code == "CLV_FDA");
        Assert.Contains(pack.Definitions, d => d.Code == "ISO" && d.IsCritical);
    }

    private static RegulatoryRequirementPack BuildPack(Guid tenantId, Guid userId)
    {
        var pack = new RegulatoryRequirementPack(tenantId, "TEST-PACK", "Test", "PA", null, null, null, null, userId);
        var order = 0;
        foreach (var item in RegutrackRequirementCatalog.Items)
        {
            pack.AddDefinition(item.Code, item.Name, item.Category, true, item.Critical, order++);
        }

        pack.Publish(DateTimeOffset.UtcNow);
        return pack;
    }
}
