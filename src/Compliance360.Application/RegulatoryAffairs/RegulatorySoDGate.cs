using Compliance360.Application.Audit;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Identity;
using Compliance360.Domain.RegulatoryAffairs;
using Compliance360.Shared;

namespace Compliance360.Application.RegulatoryAffairs;

public interface ICurrentUserPermissions
{
    bool Has(string permissionCode);
}

/// <summary>Null permissions for unit/domain paths that do not carry HTTP claims.</summary>
public sealed class AllowAllUserPermissions : ICurrentUserPermissions
{
    public bool Has(string permissionCode) => true;
}

/// <summary>Evaluates resource-level SoD after RBAC permission checks.</summary>
public interface IRegulatorySoDGate
{
    Task<RegulatorySoDSettings> GetOrCreateSettingsAsync(Guid tenantId, CancellationToken ct = default);

    Task<Result> EnsureRequirementReviewAllowedAsync(
        Guid tenantId,
        RegistrationDossier dossier,
        DossierRequirement requirement,
        DossierRequirementStatus targetStatus,
        Guid actorUserId,
        string? emergencyOverrideReason,
        CancellationToken ct = default);

    Task<Result> EnsureApproveForSubmissionAllowedAsync(
        Guid tenantId,
        RegistrationDossier dossier,
        Guid actorUserId,
        string? emergencyOverrideReason,
        CancellationToken ct = default);

    Task<Result> EnsureSubmitAllowedAsync(
        Guid tenantId,
        RegistrationDossier dossier,
        Guid actorUserId,
        string? emergencyOverrideReason,
        CancellationToken ct = default);

    Task<Result> EnsureResubmitAllowedAsync(
        Guid tenantId,
        RegistrationDossier dossier,
        Guid actorUserId,
        CancellationToken ct = default) => Task.FromResult(Result.Success());

    Task<Result> EnsureExternalDecisionAllowedAsync(
        Guid tenantId,
        RegistrationDossier dossier,
        Guid actorUserId,
        string? emergencyOverrideReason,
        CancellationToken ct = default);
}

public sealed class RegulatorySoDGate : IRegulatorySoDGate
{
    private readonly IRegulatoryAffairsRepository _repo;
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserPermissions _permissions;
    private readonly IAuditRepository _audit;
    private readonly IClock _clock;

    public RegulatorySoDGate(
        IRegulatoryAffairsRepository repo,
        IApplicationDbContext db,
        ICurrentUserPermissions permissions,
        IAuditRepository audit,
        IClock clock)
    {
        _repo = repo;
        _db = db;
        _permissions = permissions;
        _audit = audit;
        _clock = clock;
    }

    public async Task<RegulatorySoDSettings> GetOrCreateSettingsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var settings = await _repo.GetSoDSettingsAsync(tenantId, ct);
        if (settings is not null)
        {
            return settings;
        }

        settings = RegulatorySoDSettings.CreateRegulatedDefaults(tenantId);
        await _repo.AddSoDSettingsAsync(settings, ct);
        await _db.SaveChangesAsync(ct);
        return settings;
    }

    public async Task<Result> EnsureRequirementReviewAllowedAsync(
        Guid tenantId,
        RegistrationDossier dossier,
        DossierRequirement requirement,
        DossierRequirementStatus targetStatus,
        Guid actorUserId,
        string? emergencyOverrideReason,
        CancellationToken ct = default)
    {
        var isReviewDecision = targetStatus is DossierRequirementStatus.Accepted
            or DossierRequirementStatus.Rejected
            or DossierRequirementStatus.Waived;

        if (!isReviewDecision)
        {
            return Result.Success();
        }

        if (!_permissions.Has(PermissionCatalog.RegulatoryDossierReview)
            && !_permissions.Has(PermissionCatalog.RegulatoryDossierApproveForSubmission)
            && !_permissions.Has(PermissionCatalog.RegulatorySoDEmergencyOverride))
        {
            return await Deny(tenantId, actorUserId, dossier.Id, "Requirement review requires REGULATORY.DOSSIER.REVIEW.", ct);
        }

        var settings = await GetOrCreateSettingsAsync(tenantId, ct);
        if (settings.PreventSelfReview
            && (dossier.CreatedByUserId == actorUserId || dossier.RegulatoryOwnerUserId == actorUserId))
        {
            return await DenyOrOverride(
                tenantId, actorUserId, dossier.Id, settings, emergencyOverrideReason,
                "SoD PreventSelfReview: creator/owner cannot accept/reject/waive requirements on this dossier.", ct);
        }

        if (targetStatus == DossierRequirementStatus.Waived
            && requirement.IsCritical
            && settings.RequireSecondApprovalForCriticalWaiver
            && !_permissions.Has(PermissionCatalog.RegulatoryDossierApproveForSubmission)
            && !_permissions.Has(PermissionCatalog.RegulatorySoDEmergencyOverride))
        {
            return await Deny(tenantId, actorUserId, dossier.Id,
                "Critical waiver requires Approver clearance (REGULATORY.DOSSIER.APPROVE_FOR_SUBMISSION) or emergency override.", ct);
        }

        return Result.Success();
    }

    public async Task<Result> EnsureApproveForSubmissionAllowedAsync(
        Guid tenantId,
        RegistrationDossier dossier,
        Guid actorUserId,
        string? emergencyOverrideReason,
        CancellationToken ct = default)
    {
        var settings = await GetOrCreateSettingsAsync(tenantId, ct);
        if (settings.PreventSelfApproval
            && (dossier.CreatedByUserId == actorUserId || dossier.RegulatoryOwnerUserId == actorUserId))
        {
            return await DenyOrOverride(
                tenantId, actorUserId, dossier.Id, settings, emergencyOverrideReason,
                "SoD PreventSelfApproval: creator/owner cannot grant internal clearance on this dossier.", ct);
        }

        var reviewedByActor = dossier.Requirements.Any(r =>
            r.LastStatusChangedByUserId == actorUserId
            && r.Status is DossierRequirementStatus.Accepted or DossierRequirementStatus.Rejected);
        if (settings.PreventSelfApproval && reviewedByActor)
        {
            return await DenyOrOverride(
                tenantId, actorUserId, dossier.Id, settings, emergencyOverrideReason,
                "SoD PreventSelfApproval: reviewer cannot approve their own technical review for submission.", ct);
        }

        return Result.Success();
    }

    public async Task<Result> EnsureSubmitAllowedAsync(
        Guid tenantId,
        RegistrationDossier dossier,
        Guid actorUserId,
        string? emergencyOverrideReason,
        CancellationToken ct = default)
    {
        var settings = await GetOrCreateSettingsAsync(tenantId, ct);
        if (settings.RequireInternalApprovalBeforeSubmission)
        {
            if (dossier.Status != RegistrationDossierStatus.ApprovedForSubmission)
            {
                return await Deny(tenantId, actorUserId, dossier.Id,
                    "Submission requires prior internal clearance (ApprovedForSubmission).", ct);
            }
        }
        else if (dossier.Status is not (RegistrationDossierStatus.ApprovedForSubmission or RegistrationDossierStatus.ReadyForSubmission))
        {
            return await Deny(tenantId, actorUserId, dossier.Id,
                "Dossier must be ReadyForSubmission or ApprovedForSubmission before submit.", ct);
        }

        if (settings.SeparateApproverAndSubmitter
            && dossier.InternallyApprovedByUserId.HasValue
            && dossier.InternallyApprovedByUserId.Value == actorUserId)
        {
            return await DenyOrOverride(
                tenantId, actorUserId, dossier.Id, settings, emergencyOverrideReason,
                "SoD SeparateApproverAndSubmitter: internal approver cannot record the submission.", ct);
        }

        return Result.Success();
    }

    public async Task<Result> EnsureResubmitAllowedAsync(
        Guid tenantId,
        RegistrationDossier dossier,
        Guid actorUserId,
        CancellationToken ct = default)
    {
        var settings = await GetOrCreateSettingsAsync(tenantId, ct);
        if (settings.SeparateApproverAndSubmitter
            && dossier.InternallyApprovedByUserId.HasValue
            && dossier.InternallyApprovedByUserId.Value == actorUserId)
        {
            return await Deny(
                tenantId,
                actorUserId,
                dossier.Id,
                "SoD SeparateApproverAndSubmitter: internal approver cannot record the resubmission.",
                ct);
        }

        return Result.Success();
    }

    public async Task<Result> EnsureExternalDecisionAllowedAsync(
        Guid tenantId,
        RegistrationDossier dossier,
        Guid actorUserId,
        string? emergencyOverrideReason,
        CancellationToken ct = default)
    {
        var settings = await GetOrCreateSettingsAsync(tenantId, ct);
        if (settings.PreventSelfApproval
            && (dossier.CreatedByUserId == actorUserId || dossier.RegulatoryOwnerUserId == actorUserId))
        {
            return await DenyOrOverride(
                tenantId, actorUserId, dossier.Id, settings, emergencyOverrideReason,
                "SoD: creator/owner cannot record external authority approval on this dossier.", ct);
        }

        return Result.Success();
    }

    private async Task<Result> DenyOrOverride(
        Guid tenantId,
        Guid actorUserId,
        Guid dossierId,
        RegulatorySoDSettings settings,
        string? reason,
        string message,
        CancellationToken ct)
    {
        if (settings.AllowEmergencyOverride
            && _permissions.Has(PermissionCatalog.RegulatorySoDEmergencyOverride)
            && (!settings.EmergencyOverrideRequiresReason || IsValidOverrideReason(reason)))
        {
            await WriteAudit(tenantId, actorUserId, dossierId, AuditAction.RegulatorySoDEmergencyOverrideUsed,
                $"Emergency override: {message}. Reason: {reason}", ct);
            return Result.Success();
        }

        return await Deny(tenantId, actorUserId, dossierId, message, ct);
    }

    private async Task<Result> Deny(Guid tenantId, Guid actorUserId, Guid dossierId, string message, CancellationToken ct)
    {
        await WriteAudit(tenantId, actorUserId, dossierId, AuditAction.RegulatorySoDActionDenied, message, ct);
        return Result.Failure(message);
    }

    private async Task WriteAudit(Guid tenantId, Guid userId, Guid entityId, AuditAction action, string summary, CancellationToken ct)
    {
        var context = new AuditContext(tenantId, userId, null, null, null, null, null, null, null);
        var evt = new AuditEvent(
            nameof(RegistrationDossier),
            entityId,
            action,
            AuditCategory.RegulatoryAffairs,
            context,
            new AuditSnapshot(null, summary),
            new AuditMetadata(null),
            true,
            null);
        await _audit.AddAsync(AuditLog.FromEvent(evt, _clock.UtcNow), ct);
        await _db.SaveChangesAsync(ct);
    }

    private static bool IsValidOverrideReason(string? reason) =>
        !string.IsNullOrWhiteSpace(reason) && reason.Trim().Length >= 15;
}
