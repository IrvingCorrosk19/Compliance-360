using Compliance360.Application.RegulatoryAffairs;
using Compliance360.Domain.RegulatoryAffairs;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.RegulatoryAffairs;

public sealed class EfRegulatoryAffairsRepository : IRegulatoryAffairsRepository
{
    private readonly Compliance360DbContext _db;

    public EfRegulatoryAffairsRepository(Compliance360DbContext db) => _db = db;

    public Task AddAuthorityAsync(RegulatoryAuthority authority, CancellationToken ct = default) =>
        _db.RegulatoryAuthorities.AddAsync(authority, ct).AsTask();

    public async Task<IReadOnlyList<RegulatoryAuthority>> ListAuthoritiesAsync(Guid tenantId, CancellationToken ct = default) =>
        await _db.RegulatoryAuthorities.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Code)
            .ToListAsync(ct);

    public Task<RegulatoryAuthority?> GetAuthorityAsync(Guid tenantId, Guid id, CancellationToken ct = default) =>
        _db.RegulatoryAuthorities.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public Task<RegulatoryAuthority?> GetAuthorityByCodeAsync(Guid tenantId, string code, CancellationToken ct = default) =>
        _db.RegulatoryAuthorities.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == code.ToUpperInvariant(), ct);

    public Task AddManufacturerAsync(ManufacturerProfile manufacturer, CancellationToken ct = default) =>
        _db.ManufacturerProfiles.AddAsync(manufacturer, ct).AsTask();

    public Task<ManufacturerProfile?> GetManufacturerAsync(Guid tenantId, Guid id, CancellationToken ct = default) =>
        _db.ManufacturerProfiles.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public async Task<IReadOnlyList<ManufacturerProfile>> SearchManufacturersAsync(Guid tenantId, string? search, CancellationToken ct = default)
    {
        var q = _db.ManufacturerProfiles.AsNoTracking().Where(x => x.TenantId == tenantId && x.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{EscapeLike(search.Trim())}%";
            q = q.Where(x =>
                EF.Functions.ILike(x.LegalName, pattern) ||
                (x.CommercialName != null && EF.Functions.ILike(x.CommercialName, pattern)));
        }

        return await q.OrderBy(x => x.LegalName).Take(200).ToListAsync(ct);
    }

    public Task AddCertificateAsync(ManufacturerCertificate certificate, CancellationToken ct = default) =>
        _db.ManufacturerCertificates.AddAsync(certificate, ct).AsTask();

    public async Task<IReadOnlyList<ManufacturerCertificate>> ListCertificatesAsync(Guid tenantId, Guid? manufacturerId, CancellationToken ct = default)
    {
        var q = _db.ManufacturerCertificates.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (manufacturerId.HasValue)
        {
            q = q.Where(x => x.ManufacturerId == manufacturerId);
        }

        return await q.OrderByDescending(x => x.CreatedAtUtc).Take(500).ToListAsync(ct);
    }

    public Task AddCertificateLinkAsync(ManufacturerCertificateDossierLink link, CancellationToken ct = default) =>
        _db.ManufacturerCertificateDossierLinks.AddAsync(link, ct).AsTask();

    public Task AddProductAsync(MedicalDeviceProduct product, CancellationToken ct = default) =>
        _db.MedicalDeviceProducts.AddAsync(product, ct).AsTask();

    public Task<MedicalDeviceProduct?> GetProductAsync(Guid tenantId, Guid id, CancellationToken ct = default) =>
        _db.MedicalDeviceProducts.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public async Task<IReadOnlyList<MedicalDeviceProduct>> SearchProductsAsync(ProductSearchQuery query, CancellationToken ct = default)
    {
        var q = _db.MedicalDeviceProducts.AsNoTracking().Where(x => x.TenantId == query.TenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var pattern = $"%{EscapeLike(query.SearchText.Trim())}%";
            q = q.Where(x =>
                EF.Functions.ILike(x.RegulatoryName, pattern) ||
                EF.Functions.ILike(x.Brand, pattern) ||
                EF.Functions.ILike(x.CatalogCode, pattern));
        }

        if (query.RiskClass.HasValue)
        {
            q = q.Where(x => x.RiskClass == query.RiskClass);
        }

        if (query.ManufacturerId.HasValue)
        {
            q = q.Where(x => x.ManufacturerId == query.ManufacturerId);
        }

        if (query.CommercializableOnly == true)
        {
            q = q.Where(x => x.IsCommercializable);
        }

        return await q.OrderBy(x => x.RegulatoryName).Take(500).ToListAsync(ct);
    }

    public Task<bool> ProductCatalogExistsAsync(Guid tenantId, string catalogCode, Guid? excludeId, CancellationToken ct = default)
    {
        var code = catalogCode.Trim().ToUpperInvariant();
        return _db.MedicalDeviceProducts.AsNoTracking().AnyAsync(
            x => x.TenantId == tenantId && !x.IsDeleted && x.CatalogCode == code && (excludeId == null || x.Id != excludeId), ct);
    }

    public Task AddPackAsync(RegulatoryRequirementPack pack, CancellationToken ct = default) =>
        _db.RegulatoryRequirementPacks.AddAsync(pack, ct).AsTask();

    public Task<RegulatoryRequirementPack?> GetPackAsync(Guid tenantId, Guid id, CancellationToken ct = default) =>
        _db.RegulatoryRequirementPacks.Include(x => x.Definitions).FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public Task<RegulatoryRequirementPack?> GetPublishedPackByCodeAsync(Guid tenantId, string code, CancellationToken ct = default) =>
        _db.RegulatoryRequirementPacks.Include(x => x.Definitions)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == code.ToUpperInvariant() && x.Status == RequirementPackStatus.Published, ct);

    public async Task<IReadOnlyList<RegulatoryRequirementPack>> ListPacksAsync(Guid tenantId, CancellationToken ct = default) =>
        await _db.RegulatoryRequirementPacks.AsNoTracking()
            .Include(x => x.Definitions)
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Code)
            .ToListAsync(ct);

    public Task AddDossierAsync(RegistrationDossier dossier, CancellationToken ct = default) =>
        _db.RegistrationDossiers.AddAsync(dossier, ct).AsTask();

    public Task<RegistrationDossier?> GetDossierAsync(Guid tenantId, Guid id, CancellationToken ct = default) =>
        _db.RegistrationDossiers
            .Include(x => x.Requirements)
            .Include(x => x.Milestones)
            .Include(x => x.Observations).ThenInclude(o => o.LinkedRequirements)
            .Include(x => x.History)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public async Task<IReadOnlyList<RegistrationDossier>> SearchDossiersAsync(DossierSearchQuery query, CancellationToken ct = default)
    {
        var q = _db.RegistrationDossiers.AsNoTracking().Where(x => x.TenantId == query.TenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var pattern = $"%{EscapeLike(query.SearchText.Trim())}%";
            q = q.Where(x =>
                EF.Functions.ILike(x.CaseNumber, pattern) ||
                (x.Comments != null && EF.Functions.ILike(x.Comments, pattern)));
        }

        if (query.Status.HasValue)
        {
            q = q.Where(x => x.Status == query.Status);
        }

        if (query.AuthorityId.HasValue)
        {
            q = q.Where(x => x.AuthorityId == query.AuthorityId);
        }

        if (query.ProductId.HasValue)
        {
            q = q.Where(x => x.ProductId == query.ProductId);
        }

        if (query.ProcessType.HasValue)
        {
            q = q.Where(x => x.ProcessType == query.ProcessType);
        }

        return await q.OrderByDescending(x => x.CreatedAtUtc).Take(500).ToListAsync(ct);
    }

    public Task<bool> CaseNumberExistsAsync(Guid tenantId, string caseNumber, CancellationToken ct = default) =>
        _db.RegistrationDossiers.AsNoTracking().AnyAsync(x => x.TenantId == tenantId && x.CaseNumber == caseNumber.ToUpperInvariant(), ct);

    public Task AddRegistrationAsync(SanitaryRegistration registration, CancellationToken ct = default) =>
        _db.SanitaryRegistrations.AddAsync(registration, ct).AsTask();

    public async Task<IReadOnlyList<SanitaryRegistration>> ListRegistrationsAsync(Guid tenantId, string? search, CancellationToken ct = default)
    {
        var q = _db.SanitaryRegistrations.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{EscapeLike(search.Trim())}%";
            q = q.Where(x => EF.Functions.ILike(x.RegistrationNumber, pattern));
        }

        return await q.OrderByDescending(x => x.CreatedAtUtc).Take(500).ToListAsync(ct);
    }

    public Task<SanitaryRegistration?> GetCurrentRegistrationAsync(Guid tenantId, Guid productId, Guid authorityId, CancellationToken ct = default) =>
        _db.SanitaryRegistrations.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.ProductId == productId && x.AuthorityId == authorityId && x.IsCurrent, ct);

    public Task AddOperatingLicenseAsync(OperatingLicense license, CancellationToken ct = default) =>
        _db.OperatingLicenses.AddAsync(license, ct).AsTask();

    public async Task<IReadOnlyList<OperatingLicense>> ListOperatingLicensesAsync(Guid tenantId, CancellationToken ct = default) =>
        await _db.OperatingLicenses.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.CompanyName)
            .ToListAsync(ct);

    public Task<OperatingLicense?> GetOperatingLicenseAsync(Guid tenantId, Guid id, CancellationToken ct = default) =>
        _db.OperatingLicenses.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public Task AddLicenseCaseAsync(LicenseRenewalCase renewalCase, CancellationToken ct = default) =>
        _db.LicenseRenewalCases.AddAsync(renewalCase, ct).AsTask();

    public Task AddImportJobAsync(RegutrackImportJob job, CancellationToken ct = default) =>
        _db.RegutrackImportJobs.AddAsync(job, ct).AsTask();

    public Task AddImportRowAsync(RegutrackImportRow row, CancellationToken ct = default) =>
        _db.RegutrackImportRows.AddAsync(row, ct).AsTask();

    public Task<RegutrackImportJob?> GetImportJobAsync(Guid tenantId, Guid jobId, CancellationToken ct = default) =>
        _db.RegutrackImportJobs.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == jobId, ct);

    public async Task<IReadOnlyList<RegutrackImportRow>> ListImportRowsAsync(Guid tenantId, Guid jobId, CancellationToken ct = default) =>
        await _db.RegutrackImportRows.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.JobId == jobId)
            .OrderBy(x => x.RowNumber)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<RegutrackImportJob>> ListImportJobsAsync(Guid tenantId, CancellationToken ct = default) =>
        await _db.RegutrackImportJobs.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(100)
            .ToListAsync(ct);

    public Task<RegulatoryAlertSettings?> GetAlertSettingsAsync(Guid tenantId, CancellationToken ct = default) =>
        _db.RegulatoryAlertSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

    public Task AddAlertSettingsAsync(RegulatoryAlertSettings settings, CancellationToken ct = default) =>
        _db.RegulatoryAlertSettings.AddAsync(settings, ct).AsTask();

    public Task AddAlertLogAsync(RegulatoryAlertLog log, CancellationToken ct = default) =>
        _db.RegulatoryAlertLogs.AddAsync(log, ct).AsTask();

    public Task<bool> AlertExistsAsync(Guid tenantId, string alertType, Guid entityId, int daysRemaining, DateTimeOffset sinceUtc, CancellationToken ct = default) =>
        _db.RegulatoryAlertLogs.AsNoTracking().AnyAsync(
            x => x.TenantId == tenantId && x.AlertType == alertType && x.EntityId == entityId && x.DaysRemaining == daysRemaining && x.DeliveredAtUtc >= sinceUtc, ct);

    public Task<RegulatorySoDSettings?> GetSoDSettingsAsync(Guid tenantId, CancellationToken ct = default) =>
        _db.RegulatorySoDSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

    public Task AddSoDSettingsAsync(RegulatorySoDSettings settings, CancellationToken ct = default) =>
        _db.RegulatorySoDSettings.AddAsync(settings, ct).AsTask();

    public async Task<RegulatoryDashboardDto> BuildDashboardAsync(Guid tenantId, DateTimeOffset now, CancellationToken ct = default)
    {
        var today = now.Date;
        var expiringCutoff = today.AddDays(90);
        var staleCutoff = now.AddDays(-14);

        var productsQ = _db.MedicalDeviceProducts.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        var productCount = await productsQ.CountAsync(ct);
        var productOpportunity = await productsQ.SumAsync(x => x.OpportunityAmount ?? 0m, ct);
        var byRisk = await productsQ
            .GroupBy(p => p.RiskClass)
            .Select(g => new { Key = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

        var dossiersQ = _db.RegistrationDossiers.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        var dossierRows = await dossiersQ
            .Select(d => new
            {
                d.Status,
                d.AuthorityId,
                d.OpportunityAmount,
                d.MaximumReceptionOn,
                ActivityAt = d.UpdatedAtUtc ?? d.CreatedAtUtc
            })
            .ToListAsync(ct);

        var byAuthority = await (
            from d in dossiersQ
            join a in _db.RegulatoryAuthorities.AsNoTracking().Where(x => x.TenantId == tenantId)
                on d.AuthorityId equals a.Id
            group d by a.Code into g
            select new { g.Key, Count = g.Count() }
        ).ToDictionaryAsync(x => x.Key, x => x.Count, ct);

        var criticalOpen = await (
            from r in _db.DossierRequirements.AsNoTracking()
            join d in dossiersQ on r.DossierId equals d.Id
            where r.TenantId == tenantId
                  && r.IsCritical
                  && r.Status != DossierRequirementStatus.Accepted
                  && r.Status != DossierRequirementStatus.Waived
                  && r.Status != DossierRequirementStatus.NotRequired
            select r.Id
        ).CountAsync(ct);

        var regRows = await _db.SanitaryRegistrations.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .Select(r => new { r.Status, r.IsCurrent, r.ExpiresOn })
            .ToListAsync(ct);

        var certRows = await _db.ManufacturerCertificates.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .Select(c => new { c.Status, c.ExpiresOn })
            .ToListAsync(ct);

        var licenseRows = await _db.OperatingLicenses.AsNoTracking()
            .Where(l => l.TenantId == tenantId)
            .Select(l => new { l.Status, l.ExpiresOn })
            .ToListAsync(ct);

        var byStatus = dossierRows
            .GroupBy(d => d.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
        var opportunityByStatus = dossierRows
            .GroupBy(d => d.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(x => x.OpportunityAmount ?? 0m));
        var dossierOpportunity = dossierRows.Sum(d => d.OpportunityAmount ?? 0m);

        var inProgress = new HashSet<RegistrationDossierStatus>
        {
            RegistrationDossierStatus.Planning,
            RegistrationDossierStatus.WaitingManufacturerDocuments,
            RegistrationDossierStatus.DocumentsReceived,
            RegistrationDossierStatus.Assembling,
            RegistrationDossierStatus.ReadyForSubmission,
            RegistrationDossierStatus.Submitted,
            RegistrationDossierStatus.UnderAuthorityReview,
            RegistrationDossierStatus.Observed,
            RegistrationDossierStatus.CorrectingObservation,
            RegistrationDossierStatus.Resubmitted
        };

        var activeRegs = 0;
        var expiringRegs = 0;
        var expiredRegs = 0;
        var renewalsThisMonth = 0;
        foreach (var r in regRows)
        {
            var (status, isCurrent) = EffectiveRegistration(r.Status, r.IsCurrent, r.ExpiresOn, today, expiringCutoff);
            if (isCurrent && status is SanitaryRegistrationStatus.Active or SanitaryRegistrationStatus.Expiring)
            {
                activeRegs++;
            }

            if (isCurrent && status == SanitaryRegistrationStatus.Expiring)
            {
                expiringRegs++;
            }

            if (status == SanitaryRegistrationStatus.Expired)
            {
                expiredRegs++;
            }

            if (isCurrent && r.ExpiresOn.HasValue && r.ExpiresOn.Value.Year == now.Year && r.ExpiresOn.Value.Month == now.Month)
            {
                renewalsThisMonth++;
            }
        }

        var certAlerts = certRows.Count(c =>
        {
            var status = EffectiveCertificate(c.Status, c.ExpiresOn, today, expiringCutoff);
            return status is ManufacturerCertificateStatus.Expiring or ManufacturerCertificateStatus.Expired;
        });

        var licenseAlerts = licenseRows.Count(l =>
        {
            var status = EffectiveLicense(l.Status, l.ExpiresOn, today, expiringCutoff);
            return status is OperatingLicenseStatus.Expiring or OperatingLicenseStatus.Expired;
        });

        return new RegulatoryDashboardDto(
            productCount,
            activeRegs,
            expiringRegs,
            expiredRegs,
            renewalsThisMonth,
            dossierRows.Count(d => inProgress.Contains(d.Status)),
            dossierRows.Count(d => d.MaximumReceptionOn.HasValue && d.MaximumReceptionOn < now && d.Status == RegistrationDossierStatus.WaitingManufacturerDocuments),
            dossierRows.Count(d => inProgress.Contains(d.Status) && d.ActivityAt < staleCutoff),
            criticalOpen,
            certAlerts,
            licenseAlerts,
            productOpportunity + dossierOpportunity,
            byStatus.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).FirstOrDefault(),
            byStatus.OrderByDescending(kv => kv.Value).Select(kv => kv.Value).FirstOrDefault(),
            byAuthority,
            byRisk,
            byStatus,
            opportunityByStatus);
    }

    private static (SanitaryRegistrationStatus Status, bool IsCurrent) EffectiveRegistration(
        SanitaryRegistrationStatus status,
        bool isCurrent,
        DateTimeOffset? expiresOn,
        DateTime today,
        DateTime expiringCutoff)
    {
        if (status is SanitaryRegistrationStatus.Cancelled or SanitaryRegistrationStatus.Replaced
            or SanitaryRegistrationStatus.Suspended or SanitaryRegistrationStatus.Draft
            || !expiresOn.HasValue)
        {
            return (status, isCurrent);
        }

        if (expiresOn.Value.Date < today)
        {
            return (SanitaryRegistrationStatus.Expired, false);
        }

        if (expiresOn.Value.Date <= expiringCutoff)
        {
            return (SanitaryRegistrationStatus.Expiring, isCurrent);
        }

        return (status == SanitaryRegistrationStatus.Expiring ? SanitaryRegistrationStatus.Active : status, isCurrent);
    }

    private static ManufacturerCertificateStatus EffectiveCertificate(
        ManufacturerCertificateStatus status,
        DateTimeOffset? expiresOn,
        DateTime today,
        DateTime expiringCutoff)
    {
        if (status is ManufacturerCertificateStatus.Revoked or ManufacturerCertificateStatus.Replaced)
        {
            return status;
        }

        if (!expiresOn.HasValue)
        {
            return ManufacturerCertificateStatus.Active;
        }

        if (expiresOn.Value.Date < today)
        {
            return ManufacturerCertificateStatus.Expired;
        }

        if (expiresOn.Value.Date <= expiringCutoff)
        {
            return ManufacturerCertificateStatus.Expiring;
        }

        return ManufacturerCertificateStatus.Active;
    }

    private static OperatingLicenseStatus EffectiveLicense(
        OperatingLicenseStatus status,
        DateTimeOffset? expiresOn,
        DateTime today,
        DateTime expiringCutoff)
    {
        if (status is OperatingLicenseStatus.Cancelled or OperatingLicenseStatus.Suspended or OperatingLicenseStatus.InRenewal)
        {
            return status;
        }

        if (!expiresOn.HasValue)
        {
            return OperatingLicenseStatus.Active;
        }

        if (expiresOn.Value.Date < today)
        {
            return OperatingLicenseStatus.Expired;
        }

        if (expiresOn.Value.Date <= expiringCutoff)
        {
            return OperatingLicenseStatus.Expiring;
        }

        return OperatingLicenseStatus.Active;
    }

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}
