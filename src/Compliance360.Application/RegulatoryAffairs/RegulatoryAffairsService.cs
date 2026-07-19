using System.Text.Json;
using Compliance360.Application.Audit;
using Compliance360.Application.Notifications;
using Compliance360.Application.Storage;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Notifications;
using Compliance360.Domain.RegulatoryAffairs;
using Compliance360.Domain.Storage;
using Compliance360.Shared;

namespace Compliance360.Application.RegulatoryAffairs;

public sealed class RegulatoryAffairsService : IRegulatoryAffairsService
{
    private readonly IRegulatoryAffairsRepository _repo;
    private readonly IApplicationDbContext _db;
    private readonly IAuditRepository _audit;
    private readonly IClock _clock;
    private readonly IRegutrackWorkbookParser _workbookParser;
    private readonly IRegulatorySoDGate _sod;
    private readonly INotificationService _notifications;
    private readonly IStorageRepository _storage;
    private readonly IAlertEventIngestionService? _alertEvents;

    public RegulatoryAffairsService(
        IRegulatoryAffairsRepository repo,
        IApplicationDbContext db,
        IAuditRepository audit,
        IClock clock,
        IRegutrackWorkbookParser workbookParser,
        IRegulatorySoDGate sod,
        INotificationService notifications,
        IStorageRepository storage,
        IAlertEventIngestionService? alertEvents = null)
    {
        _repo = repo;
        _db = db;
        _audit = audit;
        _clock = clock;
        _workbookParser = workbookParser;
        _sod = sod;
        _notifications = notifications;
        _storage = storage;
        _alertEvents = alertEvents;
    }

    public async Task<Result<IReadOnlyCollection<AuthorityDto>>> EnsureDefaultAuthoritiesAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        await EnsureAuthority(tenantId, "MINSA", "Ministerio de Salud", RegulatoryAuthorityType.MinistryOfHealth, ct);
        await EnsureAuthority(tenantId, "CSS", "Caja de Seguro Social", RegulatoryAuthorityType.SocialSecurity, ct);
        await _db.SaveChangesAsync(ct);
        await Audit(tenantId, userId, tenantId, nameof(RegulatoryAuthority), AuditAction.RegulatoryConfigured, ct);
        return await ListAuthoritiesAsync(tenantId, ct);
    }

    public async Task<Result<IReadOnlyCollection<AuthorityDto>>> ListAuthoritiesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _repo.ListAuthoritiesAsync(tenantId, ct);
        return Result<IReadOnlyCollection<AuthorityDto>>.Success(list.Select(MapAuthority).ToList());
    }

    public async Task<Result<ManufacturerDto>> UpsertManufacturerAsync(UpsertManufacturerCommand command, CancellationToken ct = default)
    {
        try
        {
            ManufacturerProfile entity;
            if (command.ManufacturerId is Guid id)
            {
                entity = await _repo.GetManufacturerAsync(command.TenantId, id, ct)
                    ?? throw new DomainException("Manufacturer not found.");
                entity.Update(command.LegalName, command.CountryCode, command.CommercialName, command.ContactEmail, command.ContactPhone);
            }
            else
            {
                entity = new ManufacturerProfile(command.TenantId, command.LegalName, command.CountryCode, command.CommercialName, command.SupplierId, command.ContactEmail, command.ContactPhone);
                await _repo.AddManufacturerAsync(entity, ct);
            }

            await Audit(command.TenantId, command.RequestedByUserId, entity.Id, nameof(ManufacturerProfile), AuditAction.RegulatoryManufacturerUpdated, ct);
            await _db.SaveChangesAsync(ct);
            return Result<ManufacturerDto>.Success(MapManufacturer(entity));
        }
        catch (DomainException ex)
        {
            return Result<ManufacturerDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<IReadOnlyCollection<ManufacturerDto>>> SearchManufacturersAsync(Guid tenantId, string? search, CancellationToken ct = default)
    {
        var list = await _repo.SearchManufacturersAsync(tenantId, search, ct);
        return Result<IReadOnlyCollection<ManufacturerDto>>.Success(list.Select(MapManufacturer).ToList());
    }

    public async Task<Result<ManufacturerCertificateDto>> AddCertificateAsync(AddManufacturerCertificateCommand command, CancellationToken ct = default)
    {
        try
        {
            var mfr = await _repo.GetManufacturerAsync(command.TenantId, command.ManufacturerId, ct);
            if (mfr is null)
            {
                return Result<ManufacturerCertificateDto>.Failure("Manufacturer not found.");
            }

            var cert = new ManufacturerCertificate(
                command.TenantId, command.ManufacturerId, command.Type, command.Number, command.IssuedBy,
                command.IssuedOn, command.ExpiresOn, command.Country, command.LegalFormat, command.Apostilled,
                command.Notarized, command.StoredFileId, command.Notes);
            await _repo.AddCertificateAsync(cert, ct);
            await Audit(command.TenantId, command.RequestedByUserId, cert.Id, nameof(ManufacturerCertificate), AuditAction.RegulatoryCertificateUpdated, ct);
            await _db.SaveChangesAsync(ct);
            return Result<ManufacturerCertificateDto>.Success(MapCertificate(cert));
        }
        catch (DomainException ex)
        {
            return Result<ManufacturerCertificateDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<IReadOnlyCollection<ManufacturerCertificateDto>>> ListCertificatesAsync(Guid tenantId, Guid? manufacturerId, CancellationToken ct = default)
    {
        var list = await _repo.ListCertificatesAsync(tenantId, manufacturerId, ct);
        return Result<IReadOnlyCollection<ManufacturerCertificateDto>>.Success(list.Select(MapCertificate).ToList());
    }

    public async Task<Result<ProductDto>> CreateProductAsync(CreateMedicalDeviceProductCommand command, CancellationToken ct = default)
    {
        try
        {
            if (await _repo.ProductCatalogExistsAsync(command.TenantId, command.CatalogCode, null, ct))
            {
                return Result<ProductDto>.Failure("Catalog code already exists for this tenant.");
            }

            var product = new MedicalDeviceProduct(
                command.TenantId, command.CountryCode, command.Category, command.Brand, command.RegulatoryName,
                command.CommercialName, command.Description, command.CatalogCode, command.InternalCode, command.ProductType,
                command.RiskClass, command.ManufacturerId, command.DistributorCompanyId, command.Initiative, command.Priority,
                command.SalesMarketingInput, command.OpportunityAmount, command.Currency, command.RequestedByUserId);
            product.UpdateCommercialFields(
                command.Category, command.Brand, command.RegulatoryName, command.CommercialName, command.Description,
                command.RiskClass, command.ManufacturerId, command.DistributorCompanyId, command.DistributorName,
                command.Initiative, command.Priority, command.SalesMarketingInput, command.OpportunityAmount, command.Currency,
                command.RegisteredSuppliersCount, command.TechnicalSheetReference, command.FormReference);
            product.SetSourceLine(command.SourceLineNumber);
            await _repo.AddProductAsync(product, ct);
            await Audit(command.TenantId, command.RequestedByUserId, product.Id, nameof(MedicalDeviceProduct), AuditAction.RegulatoryProductCreated, ct);
            await _db.SaveChangesAsync(ct);
            return Result<ProductDto>.Success(MapProduct(product));
        }
        catch (DomainException ex)
        {
            return Result<ProductDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<ProductDto>> UpdateProductAsync(UpdateProductCommand command, CancellationToken ct = default)
    {
        try
        {
            var product = await _repo.GetProductAsync(command.TenantId, command.ProductId, ct);
            if (product is null || product.IsDeleted)
            {
                return Result<ProductDto>.Failure("Product not found.");
            }

            product.UpdateCommercialFields(
                command.Category, command.Brand, command.RegulatoryName, command.CommercialName, command.Description,
                command.RiskClass, command.ManufacturerId, command.DistributorCompanyId, command.DistributorName,
                command.Initiative, command.Priority, command.SalesMarketingInput, command.OpportunityAmount, command.Currency,
                command.RegisteredSuppliersCount, command.TechnicalSheetReference, command.FormReference);
            await Audit(command.TenantId, command.RequestedByUserId, product.Id, nameof(MedicalDeviceProduct), AuditAction.RegulatoryProductUpdated, ct);
            await _db.SaveChangesAsync(ct);
            return Result<ProductDto>.Success(MapProduct(product));
        }
        catch (DomainException ex)
        {
            return Result<ProductDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<IReadOnlyCollection<ProductDto>>> SearchProductsAsync(ProductSearchQuery query, CancellationToken ct = default)
    {
        var list = await _repo.SearchProductsAsync(query, ct);
        return Result<IReadOnlyCollection<ProductDto>>.Success(list.Select(MapProduct).ToList());
    }

    public async Task<Result<ProductDto>> GetProductAsync(Guid tenantId, Guid productId, CancellationToken ct = default)
    {
        var product = await _repo.GetProductAsync(tenantId, productId, ct);
        return product is null || product.IsDeleted
            ? Result<ProductDto>.Failure("Product not found.")
            : Result<ProductDto>.Success(MapProduct(product));
    }

    public async Task<Result<RequirementPackDto>> EnsureDefaultRequirementPackAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var existing = await _repo.GetPublishedPackByCodeAsync(tenantId, "REGUTRACK-PA-DEFAULT", ct);
        if (existing is not null)
        {
            return Result<RequirementPackDto>.Success(MapPack(existing));
        }

        var pack = new RegulatoryRequirementPack(
            tenantId, "REGUTRACK-PA-DEFAULT", "Pack REGUTRACK Panamá (default)", "PA",
            null, null, null, null, userId);
        var order = 0;
        foreach (var item in RegutrackRequirementCatalog.Items)
        {
            pack.AddDefinition(item.Code, item.Name, item.Category, true, item.Critical, order++);
        }

        pack.Publish(_clock.UtcNow);
        await _repo.AddPackAsync(pack, ct);
        await Audit(tenantId, userId, pack.Id, nameof(RegulatoryRequirementPack), AuditAction.RegulatoryConfigured, ct);
        await _db.SaveChangesAsync(ct);
        return Result<RequirementPackDto>.Success(MapPack(pack));
    }

    public async Task<Result<IReadOnlyCollection<RequirementPackDto>>> ListRequirementPacksAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _repo.ListPacksAsync(tenantId, ct);
        return Result<IReadOnlyCollection<RequirementPackDto>>.Success(list.Select(MapPack).ToList());
    }

    public async Task<Result<DossierDetailDto>> CreateDossierAsync(CreateDossierCommand command, CancellationToken ct = default)
    {
        try
        {
            var product = await _repo.GetProductAsync(command.TenantId, command.ProductId, ct);
            if (product is null || product.IsDeleted)
            {
                return Result<DossierDetailDto>.Failure("Product not found.");
            }

            var authority = await _repo.GetAuthorityAsync(command.TenantId, command.AuthorityId, ct);
            if (authority is null)
            {
                return Result<DossierDetailDto>.Failure("Authority not found.");
            }

            var packId = command.RequirementPackId;
            RegulatoryRequirementPack? pack = null;
            if (packId is Guid pid)
            {
                pack = await _repo.GetPackAsync(command.TenantId, pid, ct);
            }
            else
            {
                var ensured = await EnsureDefaultRequirementPackAsync(command.TenantId, command.RequestedByUserId, ct);
                if (!ensured.IsSuccess)
                {
                    return Result<DossierDetailDto>.Failure(ensured.Error ?? "Pack error.");
                }

                pack = await _repo.GetPackAsync(command.TenantId, ensured.Value!.Id, ct);
            }

            if (pack is null)
            {
                return Result<DossierDetailDto>.Failure("Requirement pack not found.");
            }

            var caseNumber = await NextCaseNumberAsync(command.TenantId, ct);
            var dossier = new RegistrationDossier(
                command.TenantId, caseNumber, command.ProductId, command.AuthorityId, command.ProcessType,
                command.ExistingRegistrationId, command.Priority, command.OwnerUserId, command.SalesMarketingInput,
                command.OpportunityAmount ?? product.OpportunityAmount, command.Currency ?? product.Currency,
                command.Comments, pack.Id, pack.VersionLabel, command.RequestedByUserId);
            dossier.ApplyRequirementPack(pack);
            foreach (var req in dossier.Requirements)
            {
                if (req.Code == "TECH_SHEET" &&
                    (product.TechnicalSheetDocumentId.HasValue || product.TechnicalSheetStoredFileId.HasValue || !string.IsNullOrWhiteSpace(product.TechnicalSheetReference)))
                {
                    if (product.TechnicalSheetDocumentId.HasValue || product.TechnicalSheetStoredFileId.HasValue)
                    {
                        req.AttachFile(product.TechnicalSheetDocumentId, product.TechnicalSheetStoredFileId, _clock.UtcNow);
                    }
                }
            }

            if (!command.SaveAsDraft)
            {
                dossier.TransitionTo(RegistrationDossierStatus.Planning, _clock.UtcNow);
            }
            dossier.RecordHistory("DossierCreated", $"Dossier {caseNumber} created for product {product.RegulatoryName}", command.RequestedByUserId, _clock.UtcNow);

            await _repo.AddDossierAsync(dossier, ct);
            await Audit(command.TenantId, command.RequestedByUserId, dossier.Id, nameof(RegistrationDossier), AuditAction.RegulatoryDossierCreated, ct);
            await _db.SaveChangesAsync(ct);
            return Result<DossierDetailDto>.Success(MapDossier(dossier));
        }
        catch (DomainException ex)
        {
            return Result<DossierDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<DossierDetailDto>> GetDossierAsync(Guid tenantId, Guid dossierId, CancellationToken ct = default)
    {
        var dossier = await _repo.GetDossierAsync(tenantId, dossierId, ct);
        return dossier is null || dossier.IsDeleted
            ? Result<DossierDetailDto>.Failure("Dossier not found.")
            : Result<DossierDetailDto>.Success(MapDossier(dossier));
    }

    public async Task<Result<IReadOnlyCollection<DossierSummaryDto>>> SearchDossiersAsync(DossierSearchQuery query, CancellationToken ct = default)
    {
        var list = await _repo.SearchDossiersAsync(query, ct);
        return Result<IReadOnlyCollection<DossierSummaryDto>>.Success(list.Select(MapDossierSummary).ToList());
    }

    public async Task<Result<DossierDetailDto>> TransitionDossierAsync(TransitionDossierCommand command, CancellationToken ct = default)
    {
        try
        {
            var dossier = await RequireDossier(command.TenantId, command.DossierId, ct);

            // This legacy endpoint is restricted to dossier preparation. Review,
            // correction, submission, authority and terminal transitions must use
            // their dedicated use cases so their SoD and audit invariants cannot be bypassed.
            if (command.TargetStatus is not (
                RegistrationDossierStatus.Planning
                or RegistrationDossierStatus.WaitingManufacturerDocuments
                or RegistrationDossierStatus.DocumentsReceived
                or RegistrationDossierStatus.Assembling))
            {
                return Result<DossierDetailDto>.Failure(
                    $"Status {command.TargetStatus} cannot be set via the preparation transition endpoint. Use its dedicated governed workflow endpoint.");
            }

            if (command.TargetStatus == RegistrationDossierStatus.DocumentsReceived
                && dossier.Status == RegistrationDossierStatus.WaitingManufacturerDocuments
                && !string.IsNullOrWhiteSpace(command.WaiverReason))
            {
                dossier.MarkDocumentsReceivedWithoutEvidence(command.WaiverReason!, command.RequestedByUserId);
            }
            else
            {
                dossier.TransitionTo(command.TargetStatus, _clock.UtcNow);
            }

            dossier.IncrementRevision();
            await Audit(command.TenantId, command.RequestedByUserId, dossier.Id, nameof(RegistrationDossier), AuditAction.RegulatoryDossierTransitioned, ct);
            await _db.SaveChangesAsync(ct);
            return Result<DossierDetailDto>.Success(MapDossier(dossier));
        }
        catch (DomainException ex)
        {
            return Result<DossierDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<DossierDetailDto>> ApproveForSubmissionAsync(ApproveForSubmissionCommand command, CancellationToken ct = default)
    {
        try
        {
            var dossier = await RequireDossier(command.TenantId, command.DossierId, ct);
            var sod = await _sod.EnsureApproveForSubmissionAllowedAsync(
                command.TenantId, dossier, command.RequestedByUserId, command.EmergencyOverrideReason, ct);
            if (!sod.IsSuccess)
            {
                return Result<DossierDetailDto>.Failure(sod.Error!);
            }

            if (dossier.Status != RegistrationDossierStatus.ReadyForSubmission)
            {
                return Result<DossierDetailDto>.Failure("Dossier must be ReadyForSubmission before internal clearance.");
            }

            dossier.TransitionTo(RegistrationDossierStatus.ApprovedForSubmission, _clock.UtcNow);
            dossier.MarkInternallyApproved(command.RequestedByUserId, _clock.UtcNow);
            if (!string.IsNullOrWhiteSpace(command.Notes))
            {
                dossier.RecordHistory("InternalApprovalNotes", command.Notes.Trim(), command.RequestedByUserId, _clock.UtcNow);
            }

            dossier.IncrementRevision();
            await Audit(command.TenantId, command.RequestedByUserId, dossier.Id, nameof(RegistrationDossier), AuditAction.RegulatoryInternalApprovalGranted, ct);
            await NotifyAsync(
                command.TenantId,
                command.RequestedByUserId,
                dossier.RegulatoryOwnerUserId ?? dossier.CreatedByUserId,
                "Aprobado para sometimiento",
                $"El expediente {dossier.CaseNumber} fue autorizado internamente para sometimiento.",
                ct,
                "regulatory.dossier.status_changed",
                dossier.Id,
                dossier.CaseNumber,
                "ApprovedForSubmission");
            await _db.SaveChangesAsync(ct);
            return Result<DossierDetailDto>.Success(MapDossier(dossier));
        }
        catch (DomainException ex)
        {
            return Result<DossierDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<DossierDetailDto>> SubmitDossierAsync(SubmitDossierCommand command, CancellationToken ct = default)
    {
        try
        {
            var dossier = await RequireDossier(command.TenantId, command.DossierId, ct);
            var sod = await _sod.EnsureSubmitAllowedAsync(
                command.TenantId, dossier, command.RequestedByUserId, command.EmergencyOverrideReason, ct);
            if (!sod.IsSuccess)
            {
                return Result<DossierDetailDto>.Failure(sod.Error!);
            }

            if (string.IsNullOrWhiteSpace(command.ProcedureNumber)
                || string.IsNullOrWhiteSpace(command.ExternalNumber)
                || !command.SubmittedOn.HasValue
                || !command.ProofStoredFileId.HasValue)
            {
                return Result<DossierDetailDto>.Failure(
                    "Procedure number, external number, submission date and proof document are required.");
            }

            await EnsureDossierStoredFileAsync(command.TenantId, dossier.Id, command.ProofStoredFileId.Value, ct);
            dossier.RecordSubmission(
                command.ProcedureNumber,
                command.ExternalNumber,
                command.SubmittedOn.Value,
                command.ProofStoredFileId.Value,
                command.RequestedByUserId);
            dossier.TransitionTo(RegistrationDossierStatus.Submitted, command.SubmittedOn.Value);
            dossier.IncrementRevision();
            await Audit(command.TenantId, command.RequestedByUserId, dossier.Id, nameof(RegistrationDossier), AuditAction.RegulatoryDossierTransitioned, ct);
            await NotifyAsync(
                command.TenantId,
                command.RequestedByUserId,
                dossier.RegulatoryOwnerUserId ?? dossier.CreatedByUserId,
                "Sometimiento registrado",
                $"El expediente {dossier.CaseNumber} fue registrado como Submitted ante la autoridad.",
                ct,
                "regulatory.dossier.submitted",
                dossier.Id,
                dossier.CaseNumber,
                "Submitted");
            await _db.SaveChangesAsync(ct);
            return Result<DossierDetailDto>.Success(MapDossier(dossier));
        }
        catch (DomainException ex)
        {
            return Result<DossierDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<DossierDetailDto>> ResubmitDossierAsync(ResubmitDossierCommand command, CancellationToken ct = default)
    {
        try
        {
            var dossier = await RequireDossier(command.TenantId, command.DossierId, ct);
            var sod = await _sod.EnsureResubmitAllowedAsync(command.TenantId, dossier, command.RequestedByUserId, ct);
            if (!sod.IsSuccess)
            {
                return Result<DossierDetailDto>.Failure(sod.Error!);
            }

            if (string.IsNullOrWhiteSpace(command.ProcedureNumber)
                || string.IsNullOrWhiteSpace(command.ExternalNumber)
                || !command.SubmittedOn.HasValue
                || !command.ProofStoredFileId.HasValue)
            {
                return Result<DossierDetailDto>.Failure(
                    "Procedure number, external number, resubmission date and proof document are required.");
            }

            await EnsureDossierStoredFileAsync(command.TenantId, dossier.Id, command.ProofStoredFileId.Value, ct);
            dossier.RecordResubmission(
                command.ProcedureNumber,
                command.ExternalNumber,
                command.SubmittedOn.Value,
                command.ProofStoredFileId.Value,
                command.RequestedByUserId);
            dossier.TransitionTo(RegistrationDossierStatus.Resubmitted, command.SubmittedOn.Value);
            dossier.IncrementRevision();
            await Audit(command.TenantId, command.RequestedByUserId, dossier.Id, nameof(RegistrationDossier), AuditAction.RegulatoryDossierTransitioned, ct);
            await NotifyAsync(
                command.TenantId,
                command.RequestedByUserId,
                dossier.RegulatoryOwnerUserId ?? dossier.CreatedByUserId,
                "Resometimiento registrado",
                $"La respuesta del expediente {dossier.CaseNumber} fue resometida ante la autoridad.",
                ct,
                "regulatory.dossier.resubmitted",
                dossier.Id,
                dossier.CaseNumber,
                "Resubmitted");
            await _db.SaveChangesAsync(ct);
            return Result<DossierDetailDto>.Success(MapDossier(dossier));
        }
        catch (DomainException ex)
        {
            return Result<DossierDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<DossierDetailDto>> StartAuthorityReviewAsync(StartAuthorityReviewCommand command, CancellationToken ct = default)
    {
        try
        {
            var dossier = await RequireDossier(command.TenantId, command.DossierId, ct);
            if (dossier.Status is not (RegistrationDossierStatus.Submitted or RegistrationDossierStatus.Resubmitted))
            {
                return Result<DossierDetailDto>.Failure("Only submitted dossiers can enter authority review.");
            }

            dossier.TransitionTo(RegistrationDossierStatus.UnderAuthorityReview, _clock.UtcNow);
            dossier.IncrementRevision();
            await Audit(command.TenantId, command.RequestedByUserId, dossier.Id, nameof(RegistrationDossier), AuditAction.RegulatoryDossierTransitioned, ct);
            await _db.SaveChangesAsync(ct);
            return Result<DossierDetailDto>.Success(MapDossier(dossier));
        }
        catch (DomainException ex)
        {
            return Result<DossierDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<DossierDetailDto>> RejectDossierAsync(RejectDossierCommand command, CancellationToken ct = default)
    {
        try
        {
            var dossier = await RequireDossier(command.TenantId, command.DossierId, ct);
            var sod = await _sod.EnsureExternalDecisionAllowedAsync(
                command.TenantId, dossier, command.RequestedByUserId, command.EmergencyOverrideReason, ct);
            if (!sod.IsSuccess)
            {
                return Result<DossierDetailDto>.Failure(sod.Error!);
            }

            if (string.IsNullOrWhiteSpace(command.Reason) || command.Reason.Trim().Length < 8)
                return Result<DossierDetailDto>.Failure("A rejection reason of at least 8 characters is required.");
            if (string.IsNullOrWhiteSpace(command.ResolutionNumber))
                return Result<DossierDetailDto>.Failure("Authority resolution number is required.");
            if (!command.ResolutionStoredFileId.HasValue)
                return Result<DossierDetailDto>.Failure("Authority rejection resolution document is required.");

            await EnsureDossierStoredFileAsync(command.TenantId, dossier.Id, command.ResolutionStoredFileId.Value, ct);
            if (dossier.Status is RegistrationDossierStatus.Submitted or RegistrationDossierStatus.Resubmitted)
                dossier.TransitionTo(RegistrationDossierStatus.UnderAuthorityReview, _clock.UtcNow);
            if (dossier.Status != RegistrationDossierStatus.UnderAuthorityReview)
                return Result<DossierDetailDto>.Failure("Dossier must be under authority review before rejection.");

            dossier.RecordHistory(
                "AuthorityRejection",
                $"Resolution={command.ResolutionNumber.Trim()}; Proof={command.ResolutionStoredFileId.Value:N}; Reason={command.Reason.Trim()}",
                command.RequestedByUserId,
                command.DecidedOn);
            dossier.TransitionTo(RegistrationDossierStatus.Rejected, command.DecidedOn);
            dossier.IncrementRevision();
            await Audit(command.TenantId, command.RequestedByUserId, dossier.Id, nameof(RegistrationDossier), AuditAction.RegulatoryDossierTransitioned, ct);
            await NotifyAsync(
                command.TenantId,
                command.RequestedByUserId,
                dossier.RegulatoryOwnerUserId ?? dossier.CreatedByUserId,
                "Decisión externa rechazada",
                $"La autoridad rechazó el expediente {dossier.CaseNumber}.",
                ct,
                "regulatory.dossier.status_changed",
                dossier.Id,
                dossier.CaseNumber,
                "Rejected");
            await _db.SaveChangesAsync(ct);
            return Result<DossierDetailDto>.Success(MapDossier(dossier));
        }
        catch (DomainException ex)
        {
            return Result<DossierDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<DossierDetailDto>> UpdateDossierDatesAsync(UpdateDossierDatesCommand command, CancellationToken ct = default)
    {
        try
        {
            var dossier = await RequireDossier(command.TenantId, command.DossierId, ct);
            if (dossier.Status is not (
                RegistrationDossierStatus.Draft
                or RegistrationDossierStatus.Planning
                or RegistrationDossierStatus.WaitingManufacturerDocuments
                or RegistrationDossierStatus.DocumentsReceived
                or RegistrationDossierStatus.Assembling))
            {
                return Result<DossierDetailDto>.Failure("Dossier dates can only be edited during controlled preparation.");
            }

            dossier.UpdateKeyDates(
                command.RequestedFromFactoryOn, command.EstimatedReceptionOn, command.MaximumReceptionOn,
                command.EstimatedSubmissionOn, command.EstimatedApprovalOn, command.TargetExpirationOn);
            dossier.IncrementRevision();
            await Audit(command.TenantId, command.RequestedByUserId, dossier.Id, nameof(RegistrationDossier), AuditAction.RegulatoryDossierUpdated, ct);
            await _db.SaveChangesAsync(ct);
            return Result<DossierDetailDto>.Success(MapDossier(dossier));
        }
        catch (DomainException ex)
        {
            return Result<DossierDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<DossierDetailDto>> UpdateRequirementAsync(UpdateRequirementCommand command, CancellationToken ct = default)
    {
        try
        {
            var dossier = await RequireDossier(command.TenantId, command.DossierId, ct);
            var req = dossier.GetRequirement(command.RequirementId);
            var isReviewDecision = command.Status is DossierRequirementStatus.Accepted or DossierRequirementStatus.Rejected;
            if (isReviewDecision && dossier.Status != RegistrationDossierStatus.UnderTechnicalReview)
            {
                return Result<DossierDetailDto>.Failure("Requirement review decisions are only allowed during technical review.");
            }

            if (!isReviewDecision && dossier.Status is not (
                RegistrationDossierStatus.Draft
                or RegistrationDossierStatus.Planning
                or RegistrationDossierStatus.WaitingManufacturerDocuments
                or RegistrationDossierStatus.DocumentsReceived
                or RegistrationDossierStatus.Assembling))
            {
                return Result<DossierDetailDto>.Failure("Requirement preparation changes are not allowed in the current dossier state.");
            }

            var sod = await _sod.EnsureRequirementReviewAllowedAsync(
                command.TenantId, dossier, req, command.Status, command.RequestedByUserId, command.EmergencyOverrideReason, ct);
            if (!sod.IsSuccess)
            {
                return Result<DossierDetailDto>.Failure(sod.Error!);
            }

            if (command.Status == DossierRequirementStatus.Received && !command.StoredFileId.HasValue)
            {
                return Result<DossierDetailDto>.Failure("A dossier-owned stored file is required to mark evidence as received.");
            }

            if (command.StoredFileId.HasValue)
            {
                await EnsureDossierStoredFileAsync(command.TenantId, dossier.Id, command.StoredFileId.Value, ct);
            }

            if (command.DocumentId.HasValue || command.StoredFileId.HasValue)
            {
                req.AttachFile(command.DocumentId, command.StoredFileId, _clock.UtcNow);
            }

            req.SetStatus(command.Status, command.Notes, command.RequestedByUserId);
            dossier.IncrementRevision();
            await Audit(command.TenantId, command.RequestedByUserId, req.Id, nameof(DossierRequirement), AuditAction.RegulatoryRequirementUpdated, ct);
            await _db.SaveChangesAsync(ct);
            return Result<DossierDetailDto>.Success(MapDossier(dossier));
        }
        catch (DomainException ex)
        {
            return Result<DossierDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<DossierDetailDto>> OpenObservationAsync(OpenObservationCommand command, CancellationToken ct = default)
    {
        try
        {
            var dossier = await RequireDossier(command.TenantId, command.DossierId, ct);
            if (dossier.Status is RegistrationDossierStatus.Submitted)
            {
                dossier.TransitionTo(RegistrationDossierStatus.UnderAuthorityReview, _clock.UtcNow);
            }

            var obs = dossier.OpenObservation(command.Description, command.ReceivedOn, command.DueOn, command.ResponsibleUserId);
            if (command.RequirementIds is not null)
            {
                foreach (var rid in command.RequirementIds)
                {
                    obs.LinkRequirement(rid);
                }
            }

            dossier.IncrementRevision();
            await Audit(command.TenantId, command.RequestedByUserId, obs.Id, nameof(AuthorityObservation), AuditAction.RegulatoryObservationOpened, ct);
            await NotifyAsync(
                command.TenantId,
                command.RequestedByUserId,
                command.ResponsibleUserId ?? dossier.RegulatoryOwnerUserId ?? dossier.CreatedByUserId,
                "Observación recibida de autoridad",
                $"Se registró una observación en el expediente {dossier.CaseNumber}.",
                ct,
                "regulatory.observation.opened",
                obs.Id,
                dossier.CaseNumber,
                "Open");
            await _db.SaveChangesAsync(ct);
            return Result<DossierDetailDto>.Success(MapDossier(dossier));
        }
        catch (DomainException ex)
        {
            return Result<DossierDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<DossierDetailDto>> RespondObservationAsync(RespondObservationCommand command, CancellationToken ct = default)
    {
        try
        {
            var dossier = await RequireDossier(command.TenantId, command.DossierId, ct);
            var obs = dossier.Observations.FirstOrDefault(o => o.Id == command.ObservationId)
                ?? throw new DomainException("Observation not found.");
            obs.SubmitResponse(command.Notes, _clock.UtcNow);
            if (command.Close)
            {
                obs.Close(_clock.UtcNow);
            }

            if (dossier.Status == RegistrationDossierStatus.Observed)
            {
                dossier.TransitionTo(RegistrationDossierStatus.CorrectingObservation, _clock.UtcNow);
            }

            dossier.IncrementRevision();
            await Audit(command.TenantId, command.RequestedByUserId, obs.Id, nameof(AuthorityObservation), AuditAction.RegulatoryObservationResponded, ct);
            await _db.SaveChangesAsync(ct);
            return Result<DossierDetailDto>.Success(MapDossier(dossier));
        }
        catch (DomainException ex)
        {
            return Result<DossierDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<RegistrationDto>> ApproveDossierAsync(ApproveDossierCommand command, CancellationToken ct = default)
    {
        try
        {
            var dossier = await RequireDossier(command.TenantId, command.DossierId, ct);
            var sod = await _sod.EnsureExternalDecisionAllowedAsync(
                command.TenantId, dossier, command.RequestedByUserId, command.EmergencyOverrideReason, ct);
            if (!sod.IsSuccess)
            {
                return Result<RegistrationDto>.Failure(sod.Error!);
            }

            if (string.IsNullOrWhiteSpace(command.RegistrationNumber))
            {
                return Result<RegistrationDto>.Failure("Registration number is required to record external authority approval.");
            }

            await EnsureDossierStoredFileAsync(command.TenantId, dossier.Id, command.ResolutionStoredFileId, ct);
            if (dossier.Status is RegistrationDossierStatus.Submitted)
            {
                dossier.TransitionTo(RegistrationDossierStatus.UnderAuthorityReview, _clock.UtcNow);
            }

            if (dossier.Status is RegistrationDossierStatus.Observed or RegistrationDossierStatus.CorrectingObservation)
            {
                return Result<RegistrationDto>.Failure("Close or resolve observations before approval, or transition to UnderAuthorityReview after resubmission.");
            }

            if (dossier.Status is not (RegistrationDossierStatus.UnderAuthorityReview or RegistrationDossierStatus.Resubmitted))
            {
                if (dossier.Status is RegistrationDossierStatus.ReadyForSubmission or RegistrationDossierStatus.ApprovedForSubmission)
                {
                    return Result<RegistrationDto>.Failure("Submit the dossier and wait for authority review before recording external approval.");
                }
            }

            if (dossier.Status == RegistrationDossierStatus.Resubmitted)
            {
                dossier.TransitionTo(RegistrationDossierStatus.UnderAuthorityReview, _clock.UtcNow);
            }

            dossier.TransitionTo(RegistrationDossierStatus.Approved, _clock.UtcNow);
            dossier.RecordHistory(
                "AuthorityApproval",
                $"Registration={command.RegistrationNumber.Trim()}; Proof={command.ResolutionStoredFileId:N}",
                command.RequestedByUserId,
                command.IssuedOn);

            var previous = await _repo.GetCurrentRegistrationAsync(command.TenantId, dossier.ProductId, dossier.AuthorityId, ct);
            var registration = new SanitaryRegistration(
                command.TenantId, dossier.ProductId, dossier.AuthorityId, command.RegistrationNumber,
                command.IssuedOn, command.ExpiresOn, command.Notes, command.RequestedByUserId, activate: true);
            if (previous is not null)
            {
                previous.MarkReplaced(registration.Id);
                registration.SetReplaces(previous.Id);
            }

            await _repo.AddRegistrationAsync(registration, ct);
            dossier.SetResultingRegistration(registration.Id);
            dossier.TransitionTo(RegistrationDossierStatus.Closed, _clock.UtcNow);
            dossier.IncrementRevision();

            var product = await _repo.GetProductAsync(command.TenantId, dossier.ProductId, ct);
            product?.SetCommercializable(true);

            await Audit(command.TenantId, command.RequestedByUserId, registration.Id, nameof(SanitaryRegistration), AuditAction.RegulatoryRegistrationApproved, ct);
            await _db.SaveChangesAsync(ct);
            return Result<RegistrationDto>.Success(MapRegistration(registration, _clock.UtcNow));
        }
        catch (DomainException ex)
        {
            return Result<RegistrationDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<RegulatorySoDSettingsDto>> GetSoDSettingsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var settings = await _sod.GetOrCreateSettingsAsync(tenantId, ct);
        return Result<RegulatorySoDSettingsDto>.Success(MapSoD(settings));
    }

    public async Task<Result<RegulatorySoDSettingsDto>> UpdateSoDSettingsAsync(UpdateSoDSettingsCommand command, CancellationToken ct = default)
    {
        var settings = await _sod.GetOrCreateSettingsAsync(command.TenantId, ct);
        settings.Update(
            command.PreventSelfReview,
            command.PreventSelfApproval,
            command.SeparateApproverAndSubmitter,
            command.SeparateDocumentUploaderAndReviewer,
            command.RequireSecondApprovalForCriticalWaiver,
            command.RequireApprovalForCriticalityChange,
            command.RequireApprovalForExternalDecisionRecording,
            command.AllowEmergencyOverride,
            command.EmergencyOverrideRequiresReason,
            command.EmergencyOverrideRequiresSecondaryReview,
            command.RequireInternalApprovalBeforeSubmission);
        await Audit(command.TenantId, command.RequestedByUserId, settings.Id, nameof(RegulatorySoDSettings), AuditAction.RegulatoryConfigured, ct);
        await _db.SaveChangesAsync(ct);
        return Result<RegulatorySoDSettingsDto>.Success(MapSoD(settings));
    }

    public async Task<Result<DossierDetailDto>> StartRenewalAsync(StartRenewalCommand command, CancellationToken ct = default)
    {
        var current = await _repo.GetCurrentRegistrationAsync(command.TenantId, command.ProductId, command.AuthorityId, ct);
        return await CreateDossierAsync(new CreateDossierCommand(
            command.TenantId, command.ProductId, command.AuthorityId, RegistrationProcessType.Renewal,
            current?.Id, "Renewal", command.RequestedByUserId, null, null, null, "Auto-started renewal",
            command.RequirementPackId, command.RequestedByUserId), ct);
    }

    public async Task<Result<IReadOnlyCollection<RegistrationDto>>> SearchRegistrationsAsync(Guid tenantId, string? search, CancellationToken ct = default)
    {
        var now = _clock.UtcNow;
        var list = await _repo.ListRegistrationsAsync(tenantId, search, ct);
        foreach (var reg in list)
        {
            reg.RefreshExpirationStatus(now);
        }

        await _db.SaveChangesAsync(ct);
        return Result<IReadOnlyCollection<RegistrationDto>>.Success(list.Select(r => MapRegistration(r, now)).ToList());
    }

    public async Task<Result<OperatingLicenseDto>> CreateOperatingLicenseAsync(CreateOperatingLicenseCommand command, CancellationToken ct = default)
    {
        try
        {
            var license = new OperatingLicense(
                command.TenantId, command.CompanyName, command.CompanyId, command.LicenseType, command.AuthorityId,
                command.LicenseNumber, command.IssuedOn, command.ExpiresOn, command.Comments, command.RequestedByUserId,
                command.CompanyConstitutedOn, command.OperationsStartedOn);
            await _repo.AddOperatingLicenseAsync(license, ct);
            await Audit(command.TenantId, command.RequestedByUserId, license.Id, nameof(OperatingLicense), AuditAction.RegulatoryLicenseUpdated, ct);
            await _db.SaveChangesAsync(ct);
            return Result<OperatingLicenseDto>.Success(MapLicense(license));
        }
        catch (DomainException ex)
        {
            return Result<OperatingLicenseDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<IReadOnlyCollection<OperatingLicenseDto>>> ListOperatingLicensesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _repo.ListOperatingLicensesAsync(tenantId, ct);
        var now = _clock.UtcNow;
        foreach (var lic in list)
        {
            lic.RefreshExpirationStatus(now);
        }

        await _db.SaveChangesAsync(ct);
        return Result<IReadOnlyCollection<OperatingLicenseDto>>.Success(list.Select(MapLicense).ToList());
    }

    public async Task<Result<OperatingLicenseDto>> UpdateOperatingLicenseCompanyDatesAsync(UpdateOperatingLicenseCompanyDatesCommand command, CancellationToken ct = default)
    {
        var license = await _repo.GetOperatingLicenseAsync(command.TenantId, command.LicenseId, ct);
        if (license is null)
        {
            return Result<OperatingLicenseDto>.Failure("Operating license not found.");
        }

        if (command.ClearConstitution || command.ClearOperationsStart)
        {
            license.ClearCompanyCorporateDates(command.ClearConstitution, command.ClearOperationsStart);
        }

        license.SetCompanyCorporateDates(command.CompanyConstitutedOn, command.OperationsStartedOn);
        await Audit(command.TenantId, command.RequestedByUserId, license.Id, nameof(OperatingLicense), AuditAction.RegulatoryLicenseUpdated, ct);
        await _db.SaveChangesAsync(ct);
        return Result<OperatingLicenseDto>.Success(MapLicense(license));
    }

    public async Task<Result<ProductDto>> AttachProductArtifactAsync(AttachProductArtifactCommand command, CancellationToken ct = default)
    {
        var product = await _repo.GetProductAsync(command.TenantId, command.ProductId, ct);
        if (product is null)
        {
            return Result<ProductDto>.Failure("Product not found.");
        }

        var kind = (command.ArtifactKind ?? string.Empty).Trim().ToLowerInvariant();
        var now = _clock.UtcNow;
        if (kind is "ficha" or "ficha_tecnica" or "technicalsheet" or "technical_sheet")
        {
            product.AttachTechnicalSheet(command.Reference, command.DocumentId, command.StoredFileId, command.Status, command.RequestedByUserId, now);
        }
        else if (kind is "formulario" or "form" or "authority_form")
        {
            product.AttachAuthorityForm(command.Reference, command.DocumentId, command.StoredFileId, command.Status, command.RequestedByUserId, now);
        }
        else
        {
            return Result<ProductDto>.Failure("ArtifactKind must be ficha_tecnica or formulario.");
        }

        await Audit(command.TenantId, command.RequestedByUserId, product.Id, nameof(MedicalDeviceProduct), AuditAction.RegulatoryProductUpdated, ct);
        await _db.SaveChangesAsync(ct);
        return Result<ProductDto>.Success(MapProduct(product));
    }

    public async Task<Result<LicenseCaseDto>> StartLicenseRenewalAsync(StartLicenseRenewalCommand command, CancellationToken ct = default)
    {
        try
        {
            var license = await _repo.GetOperatingLicenseAsync(command.TenantId, command.LicenseId, ct)
                ?? throw new DomainException("License not found.");
            var caseNumber = $"LIC-{DateTime.UtcNow:yyyyMMddHHmmss}";
            var renewal = new LicenseRenewalCase(command.TenantId, license.Id, caseNumber, command.RequestedByUserId, command.Comments, command.RequestedByUserId);
            renewal.MarkManualPlatformUpdate("Pendiente actualización en plataforma gubernamental (sin integración automática).");
            foreach (var item in LicenseOpRequirementCatalog.Items)
            {
                renewal.AddRequirement(item.Code, item.Name, item.Required);
            }
            license.AttachRenewalCase(renewal.Id);
            await _repo.AddLicenseCaseAsync(renewal, ct);
            await Audit(command.TenantId, command.RequestedByUserId, renewal.Id, nameof(LicenseRenewalCase), AuditAction.RegulatoryLicenseUpdated, ct);
            await _db.SaveChangesAsync(ct);
            return Result<LicenseCaseDto>.Success(new LicenseCaseDto(renewal.Id, renewal.OperatingLicenseId, renewal.CaseNumber, renewal.Status, renewal.ManualPlatformTaskNotes));
        }
        catch (DomainException ex)
        {
            return Result<LicenseCaseDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<RegulatoryDashboardDto>> GetDashboardAsync(Guid tenantId, CancellationToken ct = default)
        => Result<RegulatoryDashboardDto>.Success(await _repo.BuildDashboardAsync(tenantId, _clock.UtcNow, ct));

    public async Task<Result<RegulatoryAlertSettingsDto>> GetAlertSettingsAsync(
        Guid tenantId,
        CancellationToken ct = default)
    {
        var settings = await _repo.GetAlertSettingsAsync(tenantId, ct);
        if (settings is null)
        {
            settings = new RegulatoryAlertSettings(tenantId, "90,60,30,15,7,1,0");
            await _repo.AddAlertSettingsAsync(settings, ct);
            await _db.SaveChangesAsync(ct);
        }

        return Result<RegulatoryAlertSettingsDto>.Success(
            new RegulatoryAlertSettingsDto(settings.ThresholdsCsv, settings.ThresholdsDays));
    }

    public async Task<Result<RegulatoryAlertSettingsDto>> UpdateAlertSettingsAsync(
        UpdateRegulatoryAlertSettingsCommand command,
        CancellationToken ct = default)
    {
        try
        {
            var settings = await _repo.GetAlertSettingsAsync(command.TenantId, ct);
            if (settings is null)
            {
                settings = new RegulatoryAlertSettings(command.TenantId, command.ThresholdsCsv);
                await _repo.AddAlertSettingsAsync(settings, ct);
            }
            else
            {
                settings.UpdateThresholds(command.ThresholdsCsv);
            }

            await Audit(
                command.TenantId,
                command.RequestedByUserId,
                settings.Id,
                nameof(RegulatoryAlertSettings),
                AuditAction.RegulatoryConfigured,
                ct);
            await _db.SaveChangesAsync(ct);
            return Result<RegulatoryAlertSettingsDto>.Success(
                new RegulatoryAlertSettingsDto(settings.ThresholdsCsv, settings.ThresholdsDays));
        }
        catch (DomainException ex)
        {
            return Result<RegulatoryAlertSettingsDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<IReadOnlyCollection<RegulatoryAlertDto>>> EvaluateAlertsAsync(Guid tenantId, Guid? userId, CancellationToken ct = default)
    {
        var settings = await _repo.GetAlertSettingsAsync(tenantId, ct);
        if (settings is null)
        {
            settings = new RegulatoryAlertSettings(tenantId, "90,60,30,15,7,1,0");
            await _repo.AddAlertSettingsAsync(settings, ct);
        }

        var now = _clock.UtcNow;
        var alerts = new List<RegulatoryAlertDto>();
        var regs = await _repo.ListRegistrationsAsync(tenantId, null, ct);
        foreach (var reg in regs.Where(r => r.IsCurrent && r.ExpiresOn.HasValue))
        {
            var days = (int)(reg.ExpiresOn!.Value.Date - now.Date).TotalDays;
            foreach (var threshold in settings.ThresholdsDays)
            {
                var match = threshold == 0
                    ? days < 0
                    : days == threshold;
                if (!match)
                {
                    continue;
                }

                var type = threshold == 0 ? "RegistrationExpired" : "RegistrationExpiring";
                if (!await _repo.AlertExistsAsync(tenantId, type, reg.Id, threshold, now.AddDays(-1), ct))
                {
                    var message = threshold == 0
                        ? $"CT/RS {reg.RegistrationNumber} EXPIRADO hace {Math.Abs(days)} días."
                        : $"CT/RS {reg.RegistrationNumber} vence en {days} días.";
                    await _repo.AddAlertLogAsync(new RegulatoryAlertLog(tenantId, type, nameof(SanitaryRegistration), reg.Id, days, userId, "InApp"), ct);
                    alerts.Add(new RegulatoryAlertDto(type, nameof(SanitaryRegistration), reg.Id, days, message));
                }
            }
        }

        var certs = await _repo.ListCertificatesAsync(tenantId, null, ct);
        foreach (var cert in certs.Where(c => c.ExpiresOn.HasValue))
        {
            cert.RefreshExpirationStatus(now);
            var days = (int)(cert.ExpiresOn!.Value.Date - now.Date).TotalDays;
            foreach (var threshold in settings.ThresholdsDays)
            {
                var match = threshold == 0 ? days < 0 : days == threshold;
                if (!match)
                {
                    continue;
                }

                var type = threshold == 0 ? "ManufacturerCertificateExpired" : "ManufacturerCertificateExpiring";
                if (!await _repo.AlertExistsAsync(tenantId, type, cert.Id, threshold, now.AddDays(-1), ct))
                {
                    await _repo.AddAlertLogAsync(new RegulatoryAlertLog(tenantId, type, nameof(ManufacturerCertificate), cert.Id, days, userId, "InApp"), ct);
                    alerts.Add(new RegulatoryAlertDto(type, nameof(ManufacturerCertificate), cert.Id, days, $"{cert.Type} {cert.Number}: {days} días."));
                }
            }
        }

        var dossiers = await _repo.SearchDossiersAsync(new DossierSearchQuery(tenantId, null, null, null, null, null), ct);
        foreach (var d in dossiers.Where(x => x.MaximumReceptionOn.HasValue && x.MaximumReceptionOn < now && x.Status == RegistrationDossierStatus.WaitingManufacturerDocuments))
        {
            var type = "MaxReceptionOverdue";
            if (!await _repo.AlertExistsAsync(tenantId, type, d.Id, 0, now.AddDays(-1), ct))
            {
                await _repo.AddAlertLogAsync(new RegulatoryAlertLog(tenantId, type, nameof(RegistrationDossier), d.Id, 0, userId, "InApp"), ct);
                alerts.Add(new RegulatoryAlertDto(type, nameof(RegistrationDossier), d.Id, 0, $"Expediente {d.CaseNumber}: fecha máxima de recepción incumplida."));
            }
        }

        var licenses = await _repo.ListOperatingLicensesAsync(tenantId, ct);
        foreach (var lic in licenses.Where(l => l.ExpiresOn.HasValue))
        {
            lic.RefreshExpirationStatus(now);
            var days = (int)(lic.ExpiresOn!.Value.Date - now.Date).TotalDays;
            foreach (var threshold in settings.ThresholdsDays)
            {
                var match = threshold == 0 ? days < 0 : days == threshold;
                if (!match)
                {
                    continue;
                }

                var type = threshold == 0 ? "OperatingLicenseExpired" : "OperatingLicenseExpiring";
                if (!await _repo.AlertExistsAsync(tenantId, type, lic.Id, threshold, now.AddDays(-1), ct))
                {
                    await _repo.AddAlertLogAsync(new RegulatoryAlertLog(tenantId, type, nameof(OperatingLicense), lic.Id, days, userId, "InApp"), ct);
                    alerts.Add(new RegulatoryAlertDto(type, nameof(OperatingLicense), lic.Id, days, $"{lic.CompanyName} · {lic.LicenseType}: {days} días."));
                }
            }
        }

        await _db.SaveChangesAsync(ct);
        return Result<IReadOnlyCollection<RegulatoryAlertDto>>.Success(alerts);
    }

    public async Task<Result<ImportJobDto>> StageImportAsync(StageImportCommand command, CancellationToken ct = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(command.RowsJson) ? "[]" : command.RowsJson);
            var job = new RegutrackImportJob(command.TenantId, command.SourceFileName, command.RequestedByUserId, command.RowsJson);
            await _repo.AddImportJobAsync(job, ct);

            var warnings = 0;
            var errors = 0;
            var i = 0;
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                i++;
                var sheet = el.TryGetProperty("sheet", out var s) ? s.GetString() ?? "CTT REGISTROS" : "CTT REGISTROS";
                var rowNumber = el.TryGetProperty("row", out var r) && r.TryGetInt32(out var rn) ? rn : i;
                var raw = el.GetRawText();
                string? err = null;
                var recordType = el.TryGetProperty("recordType", out var rt) ? rt.GetString() : null;
                if (string.Equals(recordType, "ManufacturerCertificate", StringComparison.OrdinalIgnoreCase))
                {
                    if (!el.TryGetProperty("manufacturerName", out var mfr) || string.IsNullOrWhiteSpace(mfr.GetString()))
                    {
                        err = "manufacturerName required";
                        errors++;
                    }
                    else if (!el.TryGetProperty("certificateType", out var ctype) || string.IsNullOrWhiteSpace(ctype.GetString()))
                    {
                        err = "certificateType required";
                        errors++;
                    }
                }
                else if (string.Equals(recordType, "OperatingLicense", StringComparison.OrdinalIgnoreCase))
                {
                    if (!el.TryGetProperty("companyName", out var company) || string.IsNullOrWhiteSpace(company.GetString()))
                    {
                        err = "companyName required";
                        errors++;
                    }
                    else if (!el.TryGetProperty("licenseType", out var ltype) || string.IsNullOrWhiteSpace(ltype.GetString()))
                    {
                        err = "licenseType required";
                        errors++;
                    }
                }
                else if (!el.TryGetProperty("regulatoryName", out var name) || string.IsNullOrWhiteSpace(name.GetString()))
                {
                    err = "regulatoryName required";
                    errors++;
                }
                else if (!el.TryGetProperty("catalogCode", out var code) || string.IsNullOrWhiteSpace(code.GetString()))
                {
                    warnings++;
                }

                await _repo.AddImportRowAsync(new RegutrackImportRow(command.TenantId, job.Id, sheet, rowNumber, raw, raw, err), ct);
            }

            job.SetValidation(JsonSerializer.Serialize(new { rows = i, warnings, errors, source = command.SourceFileName }), warnings, errors);
            if (errors == 0)
            {
                job.MarkSimulated();
            }

            await Audit(command.TenantId, command.RequestedByUserId, job.Id, nameof(RegutrackImportJob), AuditAction.RegulatoryImportStaged, ct);
            await _db.SaveChangesAsync(ct);
            return Result<ImportJobDto>.Success(MapImport(job));
        }
        catch (Exception ex)
        {
            return Result<ImportJobDto>.Failure(ex.Message);
        }
    }

    public Task<Result<ImportJobDto>> StageImportXlsxAsync(StageImportXlsxCommand command, CancellationToken ct = default)
    {
        try
        {
            var rowsJson = _workbookParser.ParseToRowsJson(command.FileBytes);
            return StageImportAsync(new StageImportCommand(command.TenantId, command.SourceFileName, rowsJson, command.RequestedByUserId), ct);
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<ImportJobDto>.Failure(ex.Message));
        }
    }

    public async Task<Result<ImportJobDto>> CommitImportAsync(CommitImportCommand command, CancellationToken ct = default)
    {
        try
        {
            await EnsureDefaultAuthoritiesAsync(command.TenantId, command.RequestedByUserId, ct);
            await EnsureDefaultRequirementPackAsync(command.TenantId, command.RequestedByUserId, ct);

            var job = await _repo.GetImportJobAsync(command.TenantId, command.JobId, ct)
                ?? throw new DomainException("Import job not found.");
            if (job.Status is not (RegutrackImportJobStatus.Validated or RegutrackImportJobStatus.Simulated))
            {
                return Result<ImportJobDto>.Failure("Import job is not ready to commit.");
            }

            var rows = await _repo.ListImportRowsAsync(command.TenantId, job.Id, ct);
            var validRows = rows.Where(r => r.IsValid).AsEnumerable();
            if (command.MaxRows is int max && max > 0)
            {
                validRows = validRows.Take(max);
            }

            var minsa = await _repo.GetAuthorityByCodeAsync(command.TenantId, "MINSA", ct);
            var imported = 0;
            var processed = 0;
            var usedCatalogCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in validRows)
            {
                try
                {
                using var el = JsonDocument.Parse(row.NormalizedJson ?? row.RawJson);
                var root = el.RootElement;
                var recordType = root.TryGetProperty("recordType", out var rt) ? rt.GetString() : null;

                if (string.Equals(recordType, "ManufacturerCertificate", StringComparison.OrdinalIgnoreCase))
                {
                    imported += await CommitCertificateRowAsync(command, root, row, ct) ? 1 : 0;
                    processed++;
                    if (processed % 25 == 0)
                    {
                        await _db.SaveChangesAsync(ct);
                    }
                    continue;
                }

                if (string.Equals(recordType, "OperatingLicense", StringComparison.OrdinalIgnoreCase))
                {
                    imported += await CommitLicenseRowAsync(command, root, row, ct) ? 1 : 0;
                    processed++;
                    if (processed % 25 == 0)
                    {
                        await _db.SaveChangesAsync(ct);
                    }
                    continue;
                }

                var regulatoryName = root.GetProperty("regulatoryName").GetString()!;
                var catalogCode = root.TryGetProperty("catalogCode", out var cc) && !string.IsNullOrWhiteSpace(cc.GetString())
                    ? cc.GetString()!
                    : null;
                if (catalogCode is { Length: > 100 })
                {
                    catalogCode = catalogCode[..100];
                }

                ManufacturerProfile? mfr = null;
                if (root.TryGetProperty("manufacturerName", out var mn) && !string.IsNullOrWhiteSpace(mn.GetString()))
                {
                    var rawName = mn.GetString()!.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
                    var existingMfr = (await _repo.SearchManufacturersAsync(command.TenantId, rawName, ct))
                        .FirstOrDefault(x => string.Equals(x.LegalName, rawName, StringComparison.OrdinalIgnoreCase));
                    if (existingMfr is not null)
                    {
                        mfr = existingMfr;
                    }
                    else
                    {
                        var country = NormalizeCountryCode(root.TryGetProperty("manufacturerCountry", out var mc) ? mc.GetString() : null);
                        mfr = new ManufacturerProfile(command.TenantId, rawName, country);
                        await _repo.AddManufacturerAsync(mfr, ct);
                    }
                }

                var risk = DeviceRiskClass.Unknown;
                if (root.TryGetProperty("riskClass", out var rc) && Enum.TryParse<DeviceRiskClass>(rc.GetString(), true, out var parsed))
                {
                    risk = parsed;
                }

                var brand = root.TryGetProperty("brand", out var br) ? br.GetString() : null;
                if (string.IsNullOrWhiteSpace(brand))
                {
                    brand = "N/A";
                }

                var category = root.TryGetProperty("category", out var cat) ? cat.GetString() : null;
                if (string.IsNullOrWhiteSpace(category))
                {
                    category = "Insumos Médicos";
                }

                if (category!.Length > 120)
                {
                    category = category[..120];
                }

                if (regulatoryName.Length > 320)
                {
                    regulatoryName = regulatoryName[..320];
                }

                decimal? opportunity = null;
                if (root.TryGetProperty("opportunityAmount", out var opp) && opp.TryGetDecimal(out var oppVal))
                {
                    opportunity = oppVal;
                }

                int? suppliers = null;
                if (root.TryGetProperty("registeredSuppliersCount", out var sc) && sc.TryGetInt32(out var scv))
                {
                    suppliers = scv;
                }

                int? line = null;
                if (root.TryGetProperty("sourceLineNumber", out var ln) && ln.TryGetInt32(out var lnv))
                {
                    line = lnv;
                }

                var distributorName = root.TryGetProperty("distributorName", out var dn) ? dn.GetString() : null;
                var techSheet = root.TryGetProperty("technicalSheetReference", out var ts) ? ts.GetString() : null;
                var formRef = root.TryGetProperty("formReference", out var fr) ? fr.GetString() : null;
                var countryCode = NormalizeCountryCode(root.TryGetProperty("countryCode", out var coc) ? coc.GetString() : "PA");

                // Idempotent match: catalog code first, then regulatory name within brand.
                MedicalDeviceProduct? product = null;
                if (!string.IsNullOrWhiteSpace(catalogCode))
                {
                    product = (await _repo.SearchProductsAsync(new ProductSearchQuery(command.TenantId, catalogCode, null, null, null, null), ct))
                        .FirstOrDefault(p => string.Equals(p.CatalogCode, catalogCode, StringComparison.OrdinalIgnoreCase));
                }

                if (product is null)
                {
                    product = (await _repo.SearchProductsAsync(new ProductSearchQuery(command.TenantId, regulatoryName, null, null, null, null), ct))
                        .FirstOrDefault(p =>
                            string.Equals(p.RegulatoryName, regulatoryName, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(p.Brand, brand, StringComparison.OrdinalIgnoreCase));
                }

                if (product is not null)
                {
                    product.UpdateCommercialFields(
                        category!, brand!, regulatoryName, null,
                        root.TryGetProperty("description", out var descM) ? descM.GetString() : null,
                        risk, mfr?.Id ?? product.ManufacturerId, null, distributorName ?? product.DistributorName,
                        root.TryGetProperty("initiative", out var iniM) ? iniM.GetString() : product.Initiative,
                        root.TryGetProperty("priority", out var priM) ? priM.GetString() : product.Priority,
                        root.TryGetProperty("salesMarketingInput", out var smM) ? smM.GetString() : product.SalesMarketingInput,
                        opportunity ?? product.OpportunityAmount, "USD", suppliers ?? product.RegisteredSuppliersCount,
                        techSheet ?? product.TechnicalSheetReference, formRef ?? product.FormReference);
                    if (line.HasValue)
                    {
                        product.SetSourceLine(line);
                    }

                    row.LinkCreated(product.Id, null);
                    imported++;
                    processed++;
                    if (processed % 25 == 0)
                    {
                        await _db.SaveChangesAsync(ct);
                    }

                    continue;
                }

                catalogCode ??= $"IMP-{row.RowNumber}-{Guid.NewGuid().ToString("N")[..6]}";
                var baseCode = catalogCode;
                var suffix = 0;
                while (await _repo.ProductCatalogExistsAsync(command.TenantId, catalogCode, null, ct)
                       || !usedCatalogCodes.Add(catalogCode))
                {
                    suffix++;
                    var candidate = $"{baseCode}-{suffix}";
                    catalogCode = candidate.Length > 120 ? $"{baseCode[..Math.Min(100, baseCode.Length)]}-{suffix}" : candidate;
                    if (suffix > 500)
                    {
                        catalogCode = $"IMP-{row.RowNumber}-{Guid.NewGuid().ToString("N")[..8]}";
                        usedCatalogCodes.Add(catalogCode);
                        break;
                    }
                }

                product = new MedicalDeviceProduct(
                    command.TenantId, countryCode, category!, brand!, regulatoryName, null,
                    root.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                    catalogCode, null, null,
                    risk, mfr?.Id, null, root.TryGetProperty("initiative", out var ini) ? ini.GetString() : null,
                    root.TryGetProperty("priority", out var pri) ? pri.GetString() : null,
                    root.TryGetProperty("salesMarketingInput", out var sm) ? sm.GetString() : null,
                    opportunity, "USD", command.RequestedByUserId);
                product.UpdateCommercialFields(
                    category!, brand!, regulatoryName, null,
                    root.TryGetProperty("description", out var desc2) ? desc2.GetString() : null,
                    risk, mfr?.Id, null, distributorName,
                    root.TryGetProperty("initiative", out var ini2) ? ini2.GetString() : null,
                    root.TryGetProperty("priority", out var pri2) ? pri2.GetString() : null,
                    root.TryGetProperty("salesMarketingInput", out var sm2) ? sm2.GetString() : null,
                    opportunity, "USD", suppliers, techSheet, formRef);
                product.SetSourceLine(line);
                await _repo.AddProductAsync(product, ct);

                var authorityId = minsa?.Id ?? (await _repo.ListAuthoritiesAsync(command.TenantId, ct)).First().Id;
                if (root.TryGetProperty("authorityCode", out var ac) && !string.IsNullOrWhiteSpace(ac.GetString()))
                {
                    var authCode = ac.GetString()!;
                    if (authCode.Contains("CSS", StringComparison.OrdinalIgnoreCase))
                    {
                        authCode = "CSS";
                    }
                    else if (authCode.Contains("MINSA", StringComparison.OrdinalIgnoreCase) || authCode.Contains("DIRECCION", StringComparison.OrdinalIgnoreCase))
                    {
                        authCode = "MINSA";
                    }

                    var auth = await _repo.GetAuthorityByCodeAsync(command.TenantId, authCode, ct);
                    if (auth is not null)
                    {
                        authorityId = auth.Id;
                    }
                }

                if (root.TryGetProperty("registrationNumber", out var regNum) && !string.IsNullOrWhiteSpace(regNum.GetString()))
                {
                    DateTimeOffset? issued = null, expires = null;
                    if (root.TryGetProperty("issuedOn", out var io) && DateTimeOffset.TryParse(io.GetString(), out var iod))
                    {
                        issued = iod;
                    }

                    if (root.TryGetProperty("expiresOn", out var eo) && DateTimeOffset.TryParse(eo.GetString(), out var eod))
                    {
                        expires = eod;
                    }

                    var registration = new SanitaryRegistration(
                        command.TenantId, product.Id, authorityId, regNum.GetString(), issued ?? _clock.UtcNow, expires, "Imported from REGUTRACK", command.RequestedByUserId, activate: true);
                    await _repo.AddRegistrationAsync(registration, ct);
                    product.SetCommercializable(true);
                    row.LinkCreated(product.Id, null);
                }
                else
                {
                    var create = await CreateDossierAsync(new CreateDossierCommand(
                        command.TenantId, product.Id, authorityId, RegistrationProcessType.NewRegistration,
                        null, root.TryGetProperty("priority", out var prio) ? prio.GetString() : null,
                        command.RequestedByUserId,
                        root.TryGetProperty("salesMarketingInput", out var smi) ? smi.GetString() : null,
                        opportunity, "USD",
                        root.TryGetProperty("comments", out var com) ? com.GetString() ?? $"Imported row {row.RowNumber}" : $"Imported row {row.RowNumber}",
                        null, command.RequestedByUserId), ct);
                    if (create.IsSuccess && create.Value is not null)
                    {
                        var dossier = await _repo.GetDossierAsync(command.TenantId, create.Value.Id, ct);
                        if (dossier is not null)
                        {
                            ApplyImportedDates(dossier, root, command.RequestedByUserId);
                        }
                    }

                    row.LinkCreated(product.Id, create.IsSuccess ? create.Value?.Id : null);
                }

                imported++;
                processed++;
                if (processed % 25 == 0)
                {
                    await _db.SaveChangesAsync(ct);
                }
                }
                catch (Exception ex)
                {
                    // Skip bad Excel rows without aborting the whole commit.
                    try { row.MarkFailed(ex.Message); } catch { /* ignore */ }
                }
            }

            job.MarkCommitted(imported, _clock.UtcNow);
            await Audit(command.TenantId, command.RequestedByUserId, job.Id, nameof(RegutrackImportJob), AuditAction.RegulatoryImportCommitted, ct);
            await _db.SaveChangesAsync(ct);
            return Result<ImportJobDto>.Success(MapImport(job));
        }
        catch (DomainException ex)
        {
            return Result<ImportJobDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<ImportJobDto>.Failure($"Import commit failed: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyCollection<ImportJobDto>>> ListImportJobsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _repo.ListImportJobsAsync(tenantId, ct);
        return Result<IReadOnlyCollection<ImportJobDto>>.Success(list.Select(MapImport).ToList());
    }

    public async Task<Result<ImportJobDto>> RollbackImportAsync(RollbackImportCommand command, CancellationToken ct = default)
    {
        try
        {
            var job = await _repo.GetImportJobAsync(command.TenantId, command.JobId, ct)
                ?? throw new DomainException("Import job not found.");
            job.MarkRolledBack(command.Reason ?? "Certification / operator rollback");
            await Audit(command.TenantId, command.RequestedByUserId, job.Id, nameof(RegutrackImportJob), AuditAction.RegulatoryImportCommitted, ct);
            await _db.SaveChangesAsync(ct);
            return Result<ImportJobDto>.Success(MapImport(job));
        }
        catch (DomainException ex)
        {
            return Result<ImportJobDto>.Failure(ex.Message);
        }
    }

    private async Task EnsureAuthority(Guid tenantId, string code, string name, RegulatoryAuthorityType type, CancellationToken ct)
    {
        if (await _repo.GetAuthorityByCodeAsync(tenantId, code, ct) is not null)
        {
            return;
        }

        await _repo.AddAuthorityAsync(new RegulatoryAuthority(tenantId, code, name, "PA", type), ct);
    }

    private async Task<bool> CommitCertificateRowAsync(CommitImportCommand command, JsonElement root, RegutrackImportRow row, CancellationToken ct)
    {
        var mfrName = root.TryGetProperty("manufacturerName", out var mn) ? mn.GetString() : null;
        if (string.IsNullOrWhiteSpace(mfrName))
        {
            return false;
        }

        var legal = mfrName.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
        var existing = (await _repo.SearchManufacturersAsync(command.TenantId, legal, ct))
            .FirstOrDefault(x => string.Equals(x.LegalName, legal, StringComparison.OrdinalIgnoreCase));
        ManufacturerProfile mfr;
        if (existing is not null)
        {
            mfr = existing;
        }
        else
        {
            var country = NormalizeCountryCode(root.TryGetProperty("manufacturerCountry", out var mc) ? mc.GetString() : null);
            mfr = new ManufacturerProfile(command.TenantId, legal, country);
            await _repo.AddManufacturerAsync(mfr, ct);
        }

        var typeText = root.TryGetProperty("certificateType", out var ctEl) ? ctEl.GetString() ?? "Other" : "Other";
        var certType = MapCertificateType(typeText);
        DateTimeOffset? expires = null, requested = null;
        if (root.TryGetProperty("expiresOn", out var eo) && DateTimeOffset.TryParse(eo.GetString(), out var eod))
        {
            expires = eod;
        }

        if (root.TryGetProperty("requestedOn", out var ro) && DateTimeOffset.TryParse(ro.GetString(), out var rod))
        {
            requested = rod;
        }

        var legalFormat = CertificateLegalFormat.Other;
        if (root.TryGetProperty("legalFormat", out var lf))
        {
            var text = lf.GetString() ?? string.Empty;
            if (text.Contains("apostill", StringComparison.OrdinalIgnoreCase))
            {
                legalFormat = CertificateLegalFormat.Apostilled;
            }
            else if (text.Contains("notari", StringComparison.OrdinalIgnoreCase))
            {
                legalFormat = CertificateLegalFormat.Notarized;
            }
            else if (text.Contains("original", StringComparison.OrdinalIgnoreCase))
            {
                legalFormat = CertificateLegalFormat.Original;
            }
        }

        var number = $"{certType}-{legal}".Length > 80 ? $"{certType}-{legal[..60]}" : $"{certType}-{legal}";
        var existingCerts = await _repo.ListCertificatesAsync(command.TenantId, mfr.Id, ct);
        var matched = existingCerts.FirstOrDefault(c =>
            c.Type == certType &&
            (string.Equals(c.Number, number, StringComparison.OrdinalIgnoreCase)
             || string.Equals(c.Number, $"{certType}-{row.RowNumber}", StringComparison.OrdinalIgnoreCase)));
        if (matched is not null)
        {
            row.LinkCreated(mfr.Id, matched.Id);
            return true;
        }

        var cert = new ManufacturerCertificate(
            command.TenantId, mfr.Id, certType, number, "Imported REGUTRACK", null, expires,
            mfr.CountryCode, legalFormat, legalFormat == CertificateLegalFormat.Apostilled,
            legalFormat == CertificateLegalFormat.Notarized, null,
            root.TryGetProperty("comments", out var com) ? com.GetString() : typeText);
        cert.SetRequestedOn(requested);
        await _repo.AddCertificateAsync(cert, ct);
        row.LinkCreated(mfr.Id, cert.Id);
        return true;
    }

    private async Task<bool> CommitLicenseRowAsync(CommitImportCommand command, JsonElement root, RegutrackImportRow row, CancellationToken ct)
    {
        var company = root.TryGetProperty("companyName", out var cn) ? cn.GetString() : null;
        var licenseType = root.TryGetProperty("licenseType", out var lt) ? lt.GetString() : null;
        if (string.IsNullOrWhiteSpace(company) || string.IsNullOrWhiteSpace(licenseType))
        {
            return false;
        }

        DateTimeOffset? expires = null;
        if (root.TryGetProperty("expiresOn", out var eo) && DateTimeOffset.TryParse(eo.GetString(), out var eod))
        {
            expires = eod;
        }

        DateOnly? constituted = ParseDateOnly(root, "companyConstitutedOn");
        DateOnly? opsStart = ParseDateOnly(root, "operationsStartedOn");

        // Idempotent match
        var existing = (await _repo.ListOperatingLicensesAsync(command.TenantId, ct))
            .FirstOrDefault(l =>
                string.Equals(l.CompanyName, company, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(l.LicenseType, licenseType, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            existing.SetCompanyCorporateDates(constituted, opsStart);
            row.LinkCreated(existing.Id, null);
            return true;
        }

        var license = new OperatingLicense(
            command.TenantId, company, null, licenseType, null, null, null, expires,
            root.TryGetProperty("comments", out var com) ? com.GetString() : null, command.RequestedByUserId,
            constituted, opsStart);
        await _repo.AddOperatingLicenseAsync(license, ct);
        row.LinkCreated(license.Id, null);
        return true;
    }

    private static DateOnly? ParseDateOnly(JsonElement root, string prop)
    {
        if (!root.TryGetProperty(prop, out var el))
        {
            return null;
        }

        var raw = el.ValueKind == JsonValueKind.String ? el.GetString() : el.ToString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        if (DateOnly.TryParse(raw, out var d))
        {
            return d;
        }

        if (DateTimeOffset.TryParse(raw, out var dto))
        {
            return DateOnly.FromDateTime(dto.UtcDateTime);
        }

        return null;
    }

    private void ApplyImportedDates(RegistrationDossier dossier, JsonElement root, Guid userId)
    {
        DateTimeOffset? Parse(string prop)
        {
            if (root.TryGetProperty(prop, out var el) && DateTimeOffset.TryParse(el.GetString(), out var dt))
            {
                return dt;
            }

            return null;
        }

        dossier.UpdateKeyDates(
            Parse("requestedFromFactoryOn"),
            Parse("estimatedReceptionOn"),
            Parse("maximumReceptionOn"),
            Parse("estimatedSubmissionOn"),
            Parse("estimatedApprovalOn"),
            Parse("expiresOn"));
        dossier.RecordHistory("ImportedFromRegutrack", "Fechas y metadatos importados desde REGUTRACK", userId, _clock.UtcNow);
    }

    private static ManufacturerCertificateType MapCertificateType(string text)
    {
        if (text.Contains("ISO", StringComparison.OrdinalIgnoreCase) || text.Contains("13485", StringComparison.OrdinalIgnoreCase))
        {
            return ManufacturerCertificateType.Iso13485;
        }

        if (text.Contains("CLV", StringComparison.OrdinalIgnoreCase) || text.Contains("libre venta", StringComparison.OrdinalIgnoreCase))
        {
            return ManufacturerCertificateType.Clv;
        }

        if (text.Contains("CE", StringComparison.OrdinalIgnoreCase))
        {
            return ManufacturerCertificateType.Ce;
        }

        if (text.Contains("FDA", StringComparison.OrdinalIgnoreCase))
        {
            return ManufacturerCertificateType.Fda;
        }

        if (text.Contains("GMP", StringComparison.OrdinalIgnoreCase) || text.Contains("BPF", StringComparison.OrdinalIgnoreCase))
        {
            return ManufacturerCertificateType.Gmp;
        }

        return ManufacturerCertificateType.Other;
    }

    private async Task<RegistrationDossier> RequireDossier(Guid tenantId, Guid dossierId, CancellationToken ct)
    {
        var dossier = await _repo.GetDossierAsync(tenantId, dossierId, ct);
        if (dossier is null || dossier.IsDeleted)
        {
            throw new DomainException("Dossier not found.");
        }

        return dossier;
    }

    private async Task EnsureDossierStoredFileAsync(Guid tenantId, Guid dossierId, Guid storedFileId, CancellationToken ct)
    {
        var storedFile = await _storage.GetByIdAsync(tenantId, storedFileId, ct)
            ?? throw new DomainException("Stored file not found.");
        if (!string.Equals(storedFile.OwnerEntityName, nameof(RegistrationDossier), StringComparison.Ordinal)
            || storedFile.OwnerEntityId != dossierId)
        {
            throw new DomainException("Stored file does not belong to this dossier.");
        }

        if (storedFile.Status != StoredFileStatus.Available)
        {
            throw new DomainException("Stored file is not available for regulatory evidence.");
        }
    }

    private async Task<string> NextCaseNumberAsync(Guid tenantId, CancellationToken ct)
    {
        for (var i = 0; i < 5; i++)
        {
            var candidate = $"RA-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
            if (!await _repo.CaseNumberExistsAsync(tenantId, candidate, ct))
            {
                return candidate;
            }
        }

        return $"RA-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
    }

    private async Task Audit(Guid tenantId, Guid userId, Guid entityId, string entityName, AuditAction action, CancellationToken ct)
    {
        var context = new AuditContext(tenantId, userId, null, null, null, null, null, null, null);
        var evt = new AuditEvent(entityName, entityId, action, AuditCategory.RegulatoryAffairs, context, new AuditSnapshot(null, null), new AuditMetadata(null), true, null);
        await _audit.AddAsync(AuditLog.FromEvent(evt, _clock.UtcNow), ct);
    }

    private async Task NotifyAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid? targetUserId,
        string subject,
        string body,
        CancellationToken ct,
        string? eventCode = null,
        Guid? entityId = null,
        string? caseNumber = null,
        string? status = null)
    {
        if (_alertEvents is not null && !string.IsNullOrWhiteSpace(eventCode) && entityId.HasValue)
        {
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    entityId,
                    caseNumber,
                    status,
                    ownerUserId = targetUserId,
                    responsibleUserId = targetUserId,
                    creatorUserId = actorUserId,
                    subject
                });
                await _alertEvents.IngestAsync(new IngestAlertEventCommand(
                    tenantId,
                    eventCode,
                    payload,
                    "RegulatoryAffairs",
                    eventCode.Contains("observation", StringComparison.OrdinalIgnoreCase)
                        ? nameof(AuthorityObservation)
                        : nameof(RegistrationDossier),
                    entityId,
                    $"ra:{entityId.Value:N}:{Guid.NewGuid():N}",
                    _clock.UtcNow), ct);
            }
            catch
            {
                // Progressive dual-write: configurable alerts cannot block the authoritative regulatory transaction.
            }
        }

        if (!targetUserId.HasValue || targetUserId.Value == Guid.Empty)
        {
            return;
        }

        try
        {
            await _notifications.QueueAsync(new QueueNotificationCommand(
                tenantId,
                actorUserId,
                NotificationChannel.InApp,
                $"user:{targetUserId.Value:N}",
                subject,
                body,
                null,
                new Dictionary<string, string>
                {
                    ["module"] = "RegulatoryAffairs",
                    ["subject"] = subject
                },
                NotificationPriority.High,
                targetUserId), ct);
        }
        catch
        {
            // Notifications must not block regulatory lifecycle; SoD/audit remain authoritative.
        }
    }

    private static AuthorityDto MapAuthority(RegulatoryAuthority a) => new(a.Id, a.Code, a.Name, a.CountryCode, a.AuthorityType, a.IsActive);
    private static RegulatorySoDSettingsDto MapSoD(RegulatorySoDSettings s) => new(
        s.PreventSelfReview, s.PreventSelfApproval, s.SeparateApproverAndSubmitter, s.SeparateDocumentUploaderAndReviewer,
        s.RequireSecondApprovalForCriticalWaiver, s.RequireApprovalForCriticalityChange, s.RequireApprovalForExternalDecisionRecording,
        s.AllowEmergencyOverride, s.EmergencyOverrideRequiresReason, s.EmergencyOverrideRequiresSecondaryReview,
        s.RequireInternalApprovalBeforeSubmission);
    private static ManufacturerDto MapManufacturer(ManufacturerProfile m) => new(m.Id, m.LegalName, m.CommercialName, m.CountryCode, m.SupplierId, m.ContactEmail, m.ContactPhone, m.IsActive);
    private static ManufacturerCertificateDto MapCertificate(ManufacturerCertificate c) => new(c.Id, c.ManufacturerId, c.Type, c.Number, c.IssuedBy, c.IssuedOn, c.ExpiresOn, c.RequestedOn, c.Status, c.LegalFormat, c.Apostilled, c.Notarized, c.StoredFileId);
    private static ProductDto MapProduct(MedicalDeviceProduct p) => new(p.Id, p.CountryCode, p.Category, p.Brand, p.RegulatoryName, p.CommercialName, p.CatalogCode, p.RiskClass, p.ManufacturerId, p.DistributorCompanyId, p.DistributorName, p.OpportunityAmount, p.Currency, p.IsCommercializable, p.Priority, p.Initiative, p.RegisteredSuppliersCount, p.SourceLineNumber, p.TechnicalSheetReference, p.FormReference, p.TechnicalSheetStatus, p.TechnicalSheetDocumentId, p.TechnicalSheetStoredFileId, p.FormStatus, p.FormDocumentId, p.FormStoredFileId);
    private static RequirementPackDto MapPack(RegulatoryRequirementPack p) => new(p.Id, p.Code, p.Name, p.VersionLabel, p.Status, p.Definitions.Select(d => new RequirementDefDto(d.Id, d.Code, d.Name, d.Category, d.IsRequired, d.IsCritical, d.Order)).ToList());
    private static DossierSummaryDto MapDossierSummary(RegistrationDossier d)
    {
        var anchor = d.UpdatedAtUtc ?? d.CreatedAtUtc;
        var days = Math.Max(0, (int)(DateTimeOffset.UtcNow.Date - anchor.Date).TotalDays);
        return new(d.Id, d.CaseNumber, d.ProductId, d.AuthorityId, d.ProcessType, d.Status, d.Priority, d.OpportunityAmount, d.SubmittedOn, d.ApprovedOn, d.MaximumReceptionOn, d.CreatedAtUtc, days);
    }

    private static DossierDetailDto MapDossier(RegistrationDossier d) => new(
        d.Id, d.CaseNumber, d.ProductId, d.AuthorityId, d.ProcessType, d.ExistingRegistrationId, d.Status, d.Priority, d.RegulatoryOwnerUserId, d.Comments,
        d.SalesMarketingInput, d.OpportunityAmount, d.Currency,
        d.RequirementPackId, d.RequirementPackVersionLabel, d.ResultingRegistrationId,
        d.RequestedFromFactoryOn, d.EstimatedReceptionOn, d.MaximumReceptionOn, d.ReceivedOn, d.AssembledOn,
        d.EstimatedSubmissionOn, d.SubmittedOn, d.SubmittedByUserId, d.SubmissionProcedureNumber,
        d.SubmissionExternalNumber, d.SubmissionProofStoredFileId, d.ObservationReceivedOn,
        d.EstimatedApprovalOn, d.ApprovedOn, d.TargetExpirationOn,
        d.Requirements.OrderBy(r => r.Order).Select(r => new RequirementDto(r.Id, r.Code, r.Name, r.Category, r.IsRequired, r.IsCritical, r.Status, r.StoredFileId, r.CurrentDocumentId, r.ValidationNotes, r.Order)).ToList(),
        d.Milestones.Select(m => new MilestoneDto(m.Id, m.MilestoneType, m.PlannedDate, m.ActualDate, m.Status)).ToList(),
        d.Observations.OrderBy(o => o.ObservationNumber).Select(o => new ObservationDto(o.Id, o.ObservationNumber, o.ReceivedOn, o.DueOn, o.Description, o.Status, o.ResponseSubmittedOn, o.ClosedOn, o.Notes)).ToList(),
        d.History.OrderByDescending(h => h.OccurredAtUtc).Select(h => new DossierHistoryDto(h.Id, h.EventType, h.Summary, h.ActorUserId, h.OccurredAtUtc)).ToList(),
        d.Revision);
    private static RegistrationDto MapRegistration(SanitaryRegistration r, DateTimeOffset now)
    {
        int? days = r.ExpiresOn.HasValue ? (int)(r.ExpiresOn.Value.Date - now.Date).TotalDays : null;
        return new RegistrationDto(r.Id, r.ProductId, r.AuthorityId, r.RegistrationNumber, r.IssuedOn, r.ExpiresOn, r.Status, r.IsCurrent, days);
    }

    private static OperatingLicenseDto MapLicense(OperatingLicense l) => new(l.Id, l.CompanyName, l.LicenseType, l.LicenseNumber, l.IssuedOn, l.ExpiresOn, l.Status, l.ActiveRenewalCaseId, l.CompanyConstitutedOn, l.OperationsStartedOn);
    private static ImportJobDto MapImport(RegutrackImportJob j) => new(j.Id, j.SourceFileName, j.Status, j.WarningCount, j.ErrorCount, j.ImportedRowCount, j.CreatedAtUtc, j.ValidationReportJson);

    /// <summary>Maps Excel country names/codes to ISO-like codes within the 8-char domain limit.</summary>
    private static string NormalizeCountryCode(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "XX";
        }

        var value = raw.Trim();
        if (value.Length <= 3 && value.All(char.IsLetter))
        {
            return value.ToUpperInvariant();
        }

        var upper = value.ToUpperInvariant();
        return upper switch
        {
            var v when v.Contains("PANAMA") || v.Contains("PANAMÁ") => "PA",
            var v when v.Contains("CHINA") || v.Contains("PEOPLE'S REPUBLIC") => "CN",
            var v when v.Contains("UNITED STATES") || v.Contains("ESTADOS UNIDOS") || v.Contains("USA") || v == "US" => "US",
            var v when v.Contains("GERMANY") || v.Contains("ALEMANIA") || v.Contains("DEUTSCHLAND") => "DE",
            var v when v.Contains("ITALY") || v.Contains("ITALIA") => "IT",
            var v when v.Contains("FRANCE") || v.Contains("FRANCIA") => "FR",
            var v when v.Contains("SPAIN") || v.Contains("ESPAÑA") || v.Contains("ESPANA") => "ES",
            var v when v.Contains("MEXICO") || v.Contains("MÉXICO") => "MX",
            var v when v.Contains("BRAZIL") || v.Contains("BRASIL") => "BR",
            var v when v.Contains("JAPAN") || v.Contains("JAPÓN") || v.Contains("JAPON") => "JP",
            var v when v.Contains("KOREA") || v.Contains("COREA") => "KR",
            var v when v.Contains("INDIA") => "IN",
            var v when v.Contains("UNITED KINGDOM") || v.Contains("REINO UNIDO") || v.Contains("INGLATERRA") => "GB",
            var v when v.Contains("SWITZERLAND") || v.Contains("SUIZA") => "CH",
            var v when v.Contains("NETHERLANDS") || v.Contains("PAISES BAJOS") || v.Contains("HOLANDA") => "NL",
            var v when v.Contains("IRELAND") || v.Contains("IRLANDA") => "IE",
            var v when v.Contains("TAIWAN") => "TW",
            var v when v.Contains("SINGAPORE") || v.Contains("SINGAPUR") => "SG",
            var v when v.Contains("COSTA RICA") => "CR",
            var v when v.Contains("COLOMBIA") => "CO",
            _ => value.Length <= 8 ? value.ToUpperInvariant() : "XX"
        };
    }
}
