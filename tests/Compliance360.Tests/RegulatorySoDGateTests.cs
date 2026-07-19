using Compliance360.Application;
using Compliance360.Application.Audit;
using Compliance360.Application.RegulatoryAffairs;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Identity;
using Compliance360.Domain.RegulatoryAffairs;
using Compliance360.Shared;

namespace Compliance360.Tests;

public sealed class RegulatorySoDGateTests
{
    [Fact]
    public async Task SOD001_creator_cannot_accept_own_requirements_when_PreventSelfReview()
    {
        var tenantId = Guid.NewGuid();
        var creator = Guid.NewGuid();
        var (dossier, req) = BuildDossierReady(tenantId, creator);
        var permissions = new FixedPermissions(PermissionCatalog.RegulatoryDossierReview);
        var gate = BuildGate(tenantId, permissions);

        var result = await gate.EnsureRequirementReviewAllowedAsync(
            tenantId, dossier, req, DossierRequirementStatus.Accepted, creator, null);

        Assert.True(result.IsFailure);
        Assert.Contains("PreventSelfReview", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SOD002_creator_cannot_approve_for_submission()
    {
        var tenantId = Guid.NewGuid();
        var creator = Guid.NewGuid();
        var (dossier, _) = BuildDossierReady(tenantId, creator);
        dossier.TransitionTo(RegistrationDossierStatus.Planning, DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.WaitingManufacturerDocuments, DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.DocumentsReceived, DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.Assembling, DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.ReadyForSubmission, DateTimeOffset.UtcNow);

        var permissions = new FixedPermissions(PermissionCatalog.RegulatoryDossierApproveForSubmission);
        var gate = BuildGate(tenantId, permissions);

        var result = await gate.EnsureApproveForSubmissionAllowedAsync(tenantId, dossier, creator, null);
        Assert.True(result.IsFailure);
        Assert.Contains("PreventSelfApproval", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SOD004_approver_cannot_submit_when_separated()
    {
        var tenantId = Guid.NewGuid();
        var preparer = Guid.NewGuid();
        var approver = Guid.NewGuid();
        var (dossier, _) = BuildDossierReady(tenantId, preparer);
        AcceptCritical(dossier, Guid.NewGuid());
        AdvanceToReady(dossier);
        dossier.TransitionTo(RegistrationDossierStatus.ApprovedForSubmission, DateTimeOffset.UtcNow);
        dossier.MarkInternallyApproved(approver, DateTimeOffset.UtcNow);

        var permissions = new FixedPermissions(PermissionCatalog.RegulatoryDossierSubmit);
        var gate = BuildGate(tenantId, permissions);

        var result = await gate.EnsureSubmitAllowedAsync(tenantId, dossier, approver, null);
        Assert.True(result.IsFailure);
        Assert.Contains("SeparateApproverAndSubmitter", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SOD003_reviewer_cannot_self_approve_for_submission()
    {
        var tenantId = Guid.NewGuid();
        var preparer = Guid.NewGuid();
        var reviewer = Guid.NewGuid();
        var (dossier, _) = BuildDossierReady(tenantId, preparer);
        AcceptCritical(dossier, reviewer);
        AdvanceToReady(dossier);

        var permissions = new FixedPermissions(PermissionCatalog.RegulatoryDossierApproveForSubmission);
        var gate = BuildGate(tenantId, permissions);

        var result = await gate.EnsureApproveForSubmissionAllowedAsync(tenantId, dossier, reviewer, null);
        Assert.True(result.IsFailure);
        Assert.Contains("reviewer cannot approve", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Submit_denied_without_internal_clearance()
    {
        var tenantId = Guid.NewGuid();
        var preparer = Guid.NewGuid();
        var submitter = Guid.NewGuid();
        var (dossier, _) = BuildDossierReady(tenantId, preparer);
        AcceptCritical(dossier, Guid.NewGuid());
        AdvanceToReady(dossier);

        var permissions = new FixedPermissions(PermissionCatalog.RegulatoryDossierSubmit);
        var gate = BuildGate(tenantId, permissions);

        var result = await gate.EnsureSubmitAllowedAsync(tenantId, dossier, submitter, null);
        Assert.True(result.IsFailure);
        Assert.Contains("ApprovedForSubmission", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Catalog_separates_internal_and_external_approve_permissions()
    {
        Assert.Equal("REGULATORY.DOSSIER.APPROVE_FOR_SUBMISSION", PermissionCatalog.RegulatoryDossierApproveForSubmission);
        Assert.Equal("REGULATORY.DOSSIER.APPROVE", PermissionCatalog.RegulatoryDossierApprove);
        Assert.Contains(RoleCatalog.All, r => r.Name == RoleCatalog.RegulatoryApprover);
        Assert.Contains(RoleCatalog.All, r => r.Name == RoleCatalog.RegulatorySubmitter);
        Assert.Contains(RoleCatalog.All, r => r.Name == RoleCatalog.RegulatoryManager);

        var tenantAdmin = RoleCatalog.Find(RoleCatalog.TenantAdministrator)!;
        Assert.DoesNotContain(PermissionCatalog.RegulatoryDossierApprove, tenantAdmin.PermissionCodes);
        Assert.DoesNotContain(PermissionCatalog.RegulatoryDossierSubmit, tenantAdmin.PermissionCodes);

        var specialist = RoleCatalog.Find(RoleCatalog.RegulatorySpecialist)!;
        Assert.DoesNotContain(PermissionCatalog.RegulatoryDossierSubmit, specialist.PermissionCodes);
        Assert.DoesNotContain(PermissionCatalog.RegulatoryDossierApprove, specialist.PermissionCodes);
    }

    private static IRegulatorySoDGate BuildGate(Guid tenantId, ICurrentUserPermissions permissions)
    {
        var settings = RegulatorySoDSettings.CreateRegulatedDefaults(tenantId);
        var repo = new FakeRaRepo(settings);
        var db = new FakeDb();
        var audit = new FakeAudit();
        return new RegulatorySoDGate(repo, db, permissions, audit, new SystemClock());
    }

    private static (RegistrationDossier dossier, DossierRequirement req) BuildDossierReady(Guid tenantId, Guid creator)
    {
        var pack = BuildPack(tenantId, creator);
        var dossier = new RegistrationDossier(
            tenantId, "RA-SOD-1", Guid.NewGuid(), Guid.NewGuid(), RegistrationProcessType.NewRegistration,
            null, null, creator, null, null, "USD", null, pack.Id, pack.VersionLabel, creator);
        dossier.ApplyRequirementPack(pack);
        return (dossier, dossier.Requirements.First());
    }

    private static void AcceptCritical(RegistrationDossier dossier, Guid reviewer)
    {
        foreach (var req in dossier.Requirements.Where(r => r.IsCritical))
        {
            req.SetStatus(DossierRequirementStatus.Accepted, "ok", reviewer);
        }
    }

    private static void AdvanceToReady(RegistrationDossier dossier)
    {
        dossier.TransitionTo(RegistrationDossierStatus.Planning, DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.WaitingManufacturerDocuments, DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.DocumentsReceived, DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.Assembling, DateTimeOffset.UtcNow);
        dossier.TransitionTo(RegistrationDossierStatus.ReadyForSubmission, DateTimeOffset.UtcNow);
    }

    private static RegulatoryRequirementPack BuildPack(Guid tenantId, Guid userId)
    {
        var pack = new RegulatoryRequirementPack(tenantId, "SOD-PACK", "SoD", "PA", null, null, null, null, userId);
        var order = 0;
        foreach (var item in RegutrackRequirementCatalog.Items)
        {
            pack.AddDefinition(item.Code, item.Name, item.Category, true, item.Critical, order++);
        }

        pack.Publish(DateTimeOffset.UtcNow);
        return pack;
    }

    private sealed class FixedPermissions : ICurrentUserPermissions
    {
        private readonly HashSet<string> _codes;
        public FixedPermissions(params string[] codes) => _codes = new HashSet<string>(codes, StringComparer.OrdinalIgnoreCase);
        public bool Has(string permissionCode) => _codes.Contains(permissionCode);
    }

    private sealed class SystemClock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }

    private sealed class FakeDb : IApplicationDbContext
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private sealed class FakeAudit : IAuditRepository
    {
        public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<AuditSearchResult> SearchAsync(AuditSearchCriteria criteria, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AuditSearchResult([], 0, 1, 50));
        public Task<int> CountOlderThanAsync(Guid tenantId, DateTimeOffset olderThanUtc, CancellationToken cancellationToken = default) =>
            Task.FromResult(0);
    }

    private sealed class FakeRaRepo : IRegulatoryAffairsRepository
    {
        private readonly RegulatorySoDSettings _settings;
        public FakeRaRepo(RegulatorySoDSettings settings) => _settings = settings;

        public Task<RegulatorySoDSettings?> GetSoDSettingsAsync(Guid tenantId, CancellationToken ct = default) =>
            Task.FromResult<RegulatorySoDSettings?>(_settings);

        public Task AddSoDSettingsAsync(RegulatorySoDSettings settings, CancellationToken ct = default) => Task.CompletedTask;

        public Task AddAuthorityAsync(RegulatoryAuthority authority, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<RegulatoryAuthority>> ListAuthoritiesAsync(Guid tenantId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<RegulatoryAuthority?> GetAuthorityAsync(Guid tenantId, Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<RegulatoryAuthority?> GetAuthorityByCodeAsync(Guid tenantId, string code, CancellationToken ct = default) => throw new NotImplementedException();
        public Task AddManufacturerAsync(ManufacturerProfile manufacturer, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<ManufacturerProfile?> GetManufacturerAsync(Guid tenantId, Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<ManufacturerProfile>> SearchManufacturersAsync(Guid tenantId, string? search, CancellationToken ct = default) => throw new NotImplementedException();
        public Task AddCertificateAsync(ManufacturerCertificate certificate, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<ManufacturerCertificate>> ListCertificatesAsync(Guid tenantId, Guid? manufacturerId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task AddCertificateLinkAsync(ManufacturerCertificateDossierLink link, CancellationToken ct = default) => throw new NotImplementedException();
        public Task AddProductAsync(MedicalDeviceProduct product, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<MedicalDeviceProduct?> GetProductAsync(Guid tenantId, Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<MedicalDeviceProduct>> SearchProductsAsync(ProductSearchQuery query, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<bool> ProductCatalogExistsAsync(Guid tenantId, string catalogCode, Guid? excludeId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task AddPackAsync(RegulatoryRequirementPack pack, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<RegulatoryRequirementPack?> GetPackAsync(Guid tenantId, Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<RegulatoryRequirementPack?> GetPublishedPackByCodeAsync(Guid tenantId, string code, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<RegulatoryRequirementPack>> ListPacksAsync(Guid tenantId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task AddDossierAsync(RegistrationDossier dossier, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<RegistrationDossier?> GetDossierAsync(Guid tenantId, Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<RegistrationDossier>> SearchDossiersAsync(DossierSearchQuery query, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<bool> CaseNumberExistsAsync(Guid tenantId, string caseNumber, CancellationToken ct = default) => throw new NotImplementedException();
        public Task AddRegistrationAsync(SanitaryRegistration registration, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<SanitaryRegistration>> ListRegistrationsAsync(Guid tenantId, string? search, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<SanitaryRegistration?> GetCurrentRegistrationAsync(Guid tenantId, Guid productId, Guid authorityId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task AddOperatingLicenseAsync(OperatingLicense license, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<OperatingLicense>> ListOperatingLicensesAsync(Guid tenantId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<OperatingLicense?> GetOperatingLicenseAsync(Guid tenantId, Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task AddLicenseCaseAsync(LicenseRenewalCase renewalCase, CancellationToken ct = default) => throw new NotImplementedException();
        public Task AddImportJobAsync(RegutrackImportJob job, CancellationToken ct = default) => throw new NotImplementedException();
        public Task AddImportRowAsync(RegutrackImportRow row, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<RegutrackImportJob?> GetImportJobAsync(Guid tenantId, Guid jobId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<RegutrackImportRow>> ListImportRowsAsync(Guid tenantId, Guid jobId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<RegutrackImportJob>> ListImportJobsAsync(Guid tenantId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<RegulatoryAlertSettings?> GetAlertSettingsAsync(Guid tenantId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task AddAlertSettingsAsync(RegulatoryAlertSettings settings, CancellationToken ct = default) => throw new NotImplementedException();
        public Task AddAlertLogAsync(RegulatoryAlertLog log, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<bool> AlertExistsAsync(Guid tenantId, string alertType, Guid entityId, int daysRemaining, DateTimeOffset sinceUtc, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<RegulatoryDashboardDto> BuildDashboardAsync(Guid tenantId, DateTimeOffset now, CancellationToken ct = default) => throw new NotImplementedException();
    }
}
