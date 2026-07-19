using Compliance360.Application.RegulatoryAffairs;
using Compliance360.Domain.RegulatoryAffairs;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.RegulatoryAffairs;

public sealed class EfRegulatoryWorkflowV2Repository : IRegulatoryWorkflowV2Repository
{
    private readonly Compliance360DbContext _db;
    public EfRegulatoryWorkflowV2Repository(Compliance360DbContext db) => _db = db;

    public Task<RegistrationDossier?> GetDossierAsync(Guid tenantId, Guid dossierId, CancellationToken ct) =>
        _db.RegistrationDossiers.Include(x => x.Requirements).Include(x => x.Milestones).Include(x => x.Observations)
            .Include(x => x.History).FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == dossierId, ct);
    public Task AddCorrectionAsync(DossierCorrectionRequest entity, CancellationToken ct) => _db.DossierCorrectionRequests.AddAsync(entity, ct).AsTask();
    public Task<DossierCorrectionRequest?> GetOpenCorrectionAsync(Guid tenantId, Guid dossierId, CancellationToken ct) =>
        _db.DossierCorrectionRequests.Include(x => x.ScopeItems)
            .OrderByDescending(x => x.RequestedAtUtc)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.DossierId == dossierId
                && (x.Status == DossierCorrectionStatus.Open || x.Status == DossierCorrectionStatus.ResponseSubmitted), ct);
    public Task<DossierCorrectionRequest?> GetCorrectionAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        _db.DossierCorrectionRequests.Include(x => x.ScopeItems).FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
    public Task AddEvidenceAsync(DossierEvidenceRevision entity, CancellationToken ct) => _db.DossierEvidenceRevisions.AddAsync(entity, ct).AsTask();
    public async Task<IReadOnlyList<DossierEvidenceRevision>> ListEvidenceAsync(Guid tenantId, Guid dossierId, Guid requirementId, CancellationToken ct) =>
        await _db.DossierEvidenceRevisions.Where(x => x.TenantId == tenantId && x.DossierId == dossierId && x.RequirementId == requirementId).OrderBy(x => x.VersionNumber).ToListAsync(ct);
    public Task AddReopenAsync(DossierReopenRequest entity, CancellationToken ct) => _db.DossierReopenRequests.AddAsync(entity, ct).AsTask();
    public Task<DossierReopenRequest?> GetReopenAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        _db.DossierReopenRequests.Include(x => x.Approvals).FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
    public Task AddOverrideAsync(DossierOverrideRequest entity, CancellationToken ct) => _db.DossierOverrideRequests.AddAsync(entity, ct).AsTask();
    public Task<DossierOverrideRequest?> GetOverrideAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        _db.DossierOverrideRequests.Include(x => x.Approvals).FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
    public Task AddChangeEventAsync(DossierChangeEvent entity, CancellationToken ct) => _db.DossierChangeEvents.AddAsync(entity, ct).AsTask();
    public async Task<long> NextSequenceAsync(Guid tenantId, Guid dossierId, CancellationToken ct) =>
        (await _db.DossierChangeEvents.Where(x => x.TenantId == tenantId && x.DossierId == dossierId).MaxAsync(x => (long?)x.Sequence, ct) ?? 0) + 1;
    public async Task<IReadOnlyList<DossierChangeEvent>> ListTimelineAsync(Guid tenantId, Guid dossierId, CancellationToken ct) =>
        await _db.DossierChangeEvents.AsNoTracking().Where(x => x.TenantId == tenantId && x.DossierId == dossierId).OrderBy(x => x.Sequence).ToListAsync(ct);
}
