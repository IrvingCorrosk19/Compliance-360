using Compliance360.Domain.Common;
using Compliance360.Domain.RegulatoryAffairs;
using Compliance360.Domain.Storage;
using Compliance360.Application;
using Compliance360.Application.RegulatoryAffairs;
using Compliance360.Application.Storage;

namespace Compliance360.Tests;

public sealed class RegulatoryWorkflowV2DomainTests
{
    [Fact]
    public void Draft_persists_after_pack_application()
    {
        var d = Dossier();
        var p = Pack(d.TenantId);
        d.ApplyRequirementPack(p);
        Assert.Equal(RegistrationDossierStatus.Draft, d.Status);
        Assert.NotEmpty(d.Requirements);
    }

    [Fact]
    public void Technical_review_can_return_only_to_scoped_correction()
    {
        var d = Dossier();
        d.TransitionTo(RegistrationDossierStatus.Planning, Now);
        d.TransitionTo(RegistrationDossierStatus.WaitingManufacturerDocuments, Now);
        d.TransitionTo(RegistrationDossierStatus.DocumentsReceived, Now);
        d.TransitionTo(RegistrationDossierStatus.Assembling, Now);
        d.TransitionTo(RegistrationDossierStatus.UnderTechnicalReview, Now);
        d.TransitionTo(RegistrationDossierStatus.CorrectionRequested, Now);
        Assert.Equal(RegistrationDossierStatus.CorrectionRequested, d.Status);
        Assert.Throws<DomainException>(() => d.TransitionTo(RegistrationDossierStatus.Planning, Now));
    }

    [Fact]
    public void Correction_scope_does_not_open_entire_dossier()
    {
        var request = new DossierCorrectionRequest(Guid.NewGuid(), Guid.NewGuid(), "Fix requirement evidence", DossierCorrectionSeverity.High, Guid.NewGuid(), Now);
        var inScope = Guid.NewGuid();
        request.AddRequirement(inScope);
        request.AddField("metadata.priority");
        Assert.True(request.IncludesRequirement(inScope));
        Assert.False(request.IncludesRequirement(Guid.NewGuid()));
        Assert.False(request.IncludesField("metadata.comments"));
    }

    [Fact]
    public void Evidence_replacement_preserves_superseded_version()
    {
        var tenant = Guid.NewGuid(); var dossier = Guid.NewGuid(); var req = Guid.NewGuid();
        var first = new DossierEvidenceRevision(tenant, dossier, req, null, null, Guid.NewGuid(), new string('a', 64), "one.pdf", "", Guid.NewGuid(), Now, 1);
        first.Supersede();
        var second = new DossierEvidenceRevision(tenant, dossier, req, null, null, Guid.NewGuid(), new string('b', 64), "two.pdf", "Revised document", Guid.NewGuid(), Now, 2);
        Assert.Equal(DossierEvidenceRevisionStatus.Superseded, first.Status);
        Assert.False(first.IsCurrent);
        Assert.True(second.IsCurrent);
    }

    [Fact]
    public void Submitted_correction_can_be_closed_only_after_response()
    {
        var request = new DossierCorrectionRequest(Guid.NewGuid(), Guid.NewGuid(), "Correct scoped evidence",
            DossierCorrectionSeverity.Medium, Guid.NewGuid(), Now);
        request.AddRequirement(Guid.NewGuid());

        Assert.Throws<DomainException>(() => request.Close());
        request.MarkResponseSubmitted(Now);
        request.Close();

        Assert.Equal(DossierCorrectionStatus.Closed, request.Status);
    }

    [Fact]
    public void Stale_revision_is_rejected_clearly()
    {
        var d = Dossier();
        d.IncrementRevision();
        var ex = Assert.Throws<DomainException>(() => d.EnsureExpectedRevision(0));
        Assert.Contains("Revision conflict", ex.Message);
    }

    [Fact]
    public async Task Service_returns_failure_for_revision_conflict()
    {
        var d = Dossier();
        d.IncrementRevision();
        var service = new RegulatoryWorkflowV2Service(new StubRepository(d), null!, null!, new TestClock());
        var result = await service.UpdateMetadataAsync(new(d.TenantId, d.Id, 0, "Metadata revision", Guid.NewGuid(),
            null, null, null, null, null, null, null, null, null, null, null, null));
        Assert.False(result.IsSuccess);
        Assert.Contains("Revision conflict", result.Error);
    }

    [Fact]
    public async Task Technical_review_rejects_received_requirement_without_persisted_evidence()
    {
        var d = Dossier();
        d.ApplyRequirementPack(Pack(d.TenantId));
        var requirement = Assert.Single(d.Requirements);
        requirement.SetStatus(DossierRequirementStatus.Received, "Received without evidence", Guid.NewGuid());
        d.TransitionTo(RegistrationDossierStatus.Planning, Now);
        d.TransitionTo(RegistrationDossierStatus.WaitingManufacturerDocuments, Now);
        d.TransitionTo(RegistrationDossierStatus.DocumentsReceived, Now);
        d.TransitionTo(RegistrationDossierStatus.Assembling, Now);
        var service = new RegulatoryWorkflowV2Service(
            new StubRepository(d), null!, null!, new TestClock(), new StubStorageRepository());

        var result = await service.StartTechnicalReviewAsync(
            new(d.TenantId, d.Id, d.Revision, "Start review", Guid.NewGuid(), "Regulatory Specialist"));

        Assert.False(result.IsSuccess);
        Assert.Contains("incomplete or invalid", result.Error);
        Assert.Equal(RegistrationDossierStatus.Assembling, d.Status);
    }

    [Fact]
    public void Reopen_requires_two_distinct_non_requester_approvals()
    {
        var requester = Guid.NewGuid();
        var request = new DossierReopenRequest(Guid.NewGuid(), Guid.NewGuid(), "Controlled reopening", requester, Now);
        Assert.Throws<DomainException>(() => request.Approve(requester, Now));
        request.Approve(Guid.NewGuid(), Now);
        Assert.Equal(DossierGovernanceRequestStatus.Pending, request.Status);
        request.Approve(Guid.NewGuid(), Now);
        Assert.Equal(DossierGovernanceRequestStatus.Approved, request.Status);
        request.Execute(Now);
        Assert.Equal(DossierGovernanceRequestStatus.Executed, request.Status);
    }

    [Fact]
    public void Override_requires_two_approvals_and_is_one_time()
    {
        var request = new DossierOverrideRequest(Guid.NewGuid(), Guid.NewGuid(), "archive", "Exceptional controlled action", Guid.NewGuid(), Now);
        request.Approve(Guid.NewGuid(), Now);
        request.Approve(Guid.NewGuid(), Now);
        request.Consume(Guid.NewGuid(), "archive", Now);
        Assert.Throws<DomainException>(() => request.Consume(Guid.NewGuid(), "archive", Now));
    }

    [Fact]
    public void Archive_soft_deletes_and_preserves_history()
    {
        var d = Dossier();
        d.RecordHistory("BeforeArchive", "Persistent history", Guid.NewGuid(), Now);
        MoveToClosed(d);
        var count = d.History.Count;
        d.Archive(Now);
        Assert.True(d.IsDeleted);
        Assert.Equal(RegistrationDossierStatus.Archived, d.Status);
        Assert.True(d.History.Count > count);
    }

    [Fact]
    public void Cancellation_is_a_terminal_soft_state_that_preserves_history()
    {
        var d = Dossier();
        d.TransitionTo(RegistrationDossierStatus.Planning, Now);
        d.TransitionTo(RegistrationDossierStatus.WaitingManufacturerDocuments, Now);
        d.TransitionTo(RegistrationDossierStatus.DocumentsReceived, Now);
        d.TransitionTo(RegistrationDossierStatus.Assembling, Now);
        d.RecordHistory("BeforeCancellation", "Evidence remains traceable", Guid.NewGuid(), Now);
        var count = d.History.Count;

        d.TransitionTo(RegistrationDossierStatus.Cancelled, Now);

        Assert.Equal(RegistrationDossierStatus.Cancelled, d.Status);
        Assert.False(d.IsDeleted);
        Assert.Equal(count + 1, d.History.Count);
        Assert.Throws<DomainException>(() => d.TransitionTo(RegistrationDossierStatus.Planning, Now));
    }

    private static DateTimeOffset Now => DateTimeOffset.UtcNow;
    private static RegistrationDossier Dossier() => new(Guid.NewGuid(), $"RA-{Guid.NewGuid():N}"[..20], Guid.NewGuid(), Guid.NewGuid(),
        RegistrationProcessType.NewRegistration, null, null, Guid.NewGuid(), null, null, "USD", null, null, null, Guid.NewGuid());
    private static RegulatoryRequirementPack Pack(Guid tenant)
    {
        var p = new RegulatoryRequirementPack(tenant, $"P-{Guid.NewGuid():N}"[..20], "Pack", "PA", null, null, null, null, Guid.NewGuid());
        p.AddDefinition("REQ", "Requirement", "General", true, false, 0);
        p.Publish(Now);
        return p;
    }
    private static void MoveToClosed(RegistrationDossier d)
    {
        d.TransitionTo(RegistrationDossierStatus.Planning, Now);
        d.TransitionTo(RegistrationDossierStatus.WaitingManufacturerDocuments, Now);
        d.TransitionTo(RegistrationDossierStatus.DocumentsReceived, Now);
        d.TransitionTo(RegistrationDossierStatus.Assembling, Now);
        d.TransitionTo(RegistrationDossierStatus.ReadyForSubmission, Now);
        d.TransitionTo(RegistrationDossierStatus.ApprovedForSubmission, Now);
        d.MarkInternallyApproved(Guid.NewGuid(), Now);
        d.TransitionTo(RegistrationDossierStatus.Submitted, Now);
        d.TransitionTo(RegistrationDossierStatus.UnderAuthorityReview, Now);
        d.TransitionTo(RegistrationDossierStatus.Approved, Now);
        d.TransitionTo(RegistrationDossierStatus.Closed, Now);
    }

    private sealed class TestClock : IClock { public DateTimeOffset UtcNow => Now; }
    private sealed class StubStorageRepository : IStorageRepository
    {
        public Task AddAsync(StoredFile storedFile, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<StoredFile?> GetByIdAsync(Guid tenantId, Guid storedFileId, CancellationToken cancellationToken = default) =>
            Task.FromResult<StoredFile?>(null);
        public Task AddProviderConfigurationAsync(StorageProviderConfiguration configuration, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<StorageProviderConfiguration?> GetProviderConfigurationAsync(Guid tenantId, Guid providerConfigurationId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<StorageProviderConfiguration>> ListProviderConfigurationsAsync(Guid tenantId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AddAuditLogAsync(Compliance360.Domain.Audit.AuditLog auditLog, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class StubRepository(RegistrationDossier dossier) : IRegulatoryWorkflowV2Repository
    {
        public Task<RegistrationDossier?> GetDossierAsync(Guid tenantId, Guid dossierId, CancellationToken ct) => Task.FromResult<RegistrationDossier?>(dossier);
        public Task AddCorrectionAsync(DossierCorrectionRequest entity, CancellationToken ct) => throw new NotSupportedException();
        public Task<DossierCorrectionRequest?> GetOpenCorrectionAsync(Guid tenantId, Guid dossierId, CancellationToken ct) => throw new NotSupportedException();
        public Task<DossierCorrectionRequest?> GetCorrectionAsync(Guid tenantId, Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task AddEvidenceAsync(DossierEvidenceRevision entity, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<DossierEvidenceRevision>> ListEvidenceAsync(Guid tenantId, Guid dossierId, Guid requirementId, CancellationToken ct) => throw new NotSupportedException();
        public Task AddReopenAsync(DossierReopenRequest entity, CancellationToken ct) => throw new NotSupportedException();
        public Task<DossierReopenRequest?> GetReopenAsync(Guid tenantId, Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task AddOverrideAsync(DossierOverrideRequest entity, CancellationToken ct) => throw new NotSupportedException();
        public Task<DossierOverrideRequest?> GetOverrideAsync(Guid tenantId, Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task AddChangeEventAsync(DossierChangeEvent entity, CancellationToken ct) => throw new NotSupportedException();
        public Task<long> NextSequenceAsync(Guid tenantId, Guid dossierId, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<DossierChangeEvent>> ListTimelineAsync(Guid tenantId, Guid dossierId, CancellationToken ct) => throw new NotSupportedException();
    }
}
