using Compliance360.Application.Audit;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.FormTemplates;
using Compliance360.Shared;

namespace Compliance360.Application.FormTemplates;

public sealed class FormTemplateService : IFormTemplateService
{
    private readonly IFormTemplateRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditRepository _auditRepository;
    private readonly IClock _clock;

    public FormTemplateService(
        IFormTemplateRepository repository,
        IApplicationDbContext dbContext,
        IAuditRepository auditRepository,
        IClock clock)
    {
        _repository = repository;
        _dbContext = dbContext;
        _auditRepository = auditRepository;
        _clock = clock;
    }

    public async Task<Result<FormTemplateDetailDto>> CreateAsync(CreateFormTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _repository.CodeExistsAsync(command.TenantId, command.Code, null, cancellationToken))
            {
                return Result<FormTemplateDetailDto>.Failure("Ya existe una plantilla con ese código en el tenant.");
            }

            var template = new FormTemplate(
                command.TenantId,
                command.Name,
                command.Code,
                command.Category,
                command.Kind,
                command.Description,
                command.RequestedByUserId);
            template.CreateDraftVersion(
                string.IsNullOrWhiteSpace(command.InitialSchemaJson) ? DefaultSchema() : command.InitialSchemaJson!,
                "Versión inicial",
                command.RequestedByUserId);

            await _repository.AddAsync(template, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, template.Id, AuditAction.FormTemplateCreated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<FormTemplateDetailDto>.Success(ToDetail(template));
        }
        catch (DomainException ex)
        {
            return Result<FormTemplateDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<FormTemplateDetailDto>> UpdateHeaderAsync(UpdateFormTemplateHeaderCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _repository.GetAsync(command.TenantId, command.TemplateId, cancellationToken);
            if (template is null || template.IsDeleted)
            {
                return Result<FormTemplateDetailDto>.Failure("Plantilla no encontrada.");
            }

            template.UpdateHeader(command.Name, command.Category, command.Kind, command.Description);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, template.Id, AuditAction.FormTemplateUpdated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<FormTemplateDetailDto>.Success(ToDetail(template));
        }
        catch (DomainException ex)
        {
            return Result<FormTemplateDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<FormTemplateDetailDto>> SaveDraftAsync(SaveFormTemplateDraftCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _repository.GetAsync(command.TenantId, command.TemplateId, cancellationToken);
            if (template is null || template.IsDeleted)
            {
                return Result<FormTemplateDetailDto>.Failure("Plantilla no encontrada.");
            }

            if (command.VersionId is Guid versionId)
            {
                var existing = template.Versions.FirstOrDefault(v => v.Id == versionId);
                if (existing is null)
                {
                    return Result<FormTemplateDetailDto>.Failure("Versión no encontrada.");
                }

                if (existing.IsPublished)
                {
                    template.CreateDraftVersion(command.SchemaJson, command.ChangeLog, command.RequestedByUserId, existing.VersionNumber);
                }
                else
                {
                    template.SaveDraftSchema(versionId, command.SchemaJson, command.ChangeLog, command.RequestedByUserId);
                }
            }
            else
            {
                template.CreateDraftVersion(command.SchemaJson, command.ChangeLog, command.RequestedByUserId, template.PublishedVersionNumber);
            }

            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, template.Id, AuditAction.FormTemplateUpdated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<FormTemplateDetailDto>.Success(ToDetail(template));
        }
        catch (DomainException ex)
        {
            return Result<FormTemplateDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<FormTemplateDetailDto>> PublishAsync(FormTemplateActionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _repository.GetAsync(command.TenantId, command.TemplateId, cancellationToken);
            if (template is null || template.IsDeleted)
            {
                return Result<FormTemplateDetailDto>.Failure("Plantilla no encontrada.");
            }

            var versionId = command.VersionId
                ?? template.Versions.Where(v => !v.IsPublished).OrderByDescending(v => v.CreatedAtUtc).Select(v => (Guid?)v.Id).FirstOrDefault();
            if (versionId is null)
            {
                return Result<FormTemplateDetailDto>.Failure("No hay borrador para publicar.");
            }

            template.Publish(versionId.Value, command.RequestedByUserId, _clock.UtcNow);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, template.Id, AuditAction.FormTemplatePublished, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<FormTemplateDetailDto>.Success(ToDetail(template));
        }
        catch (DomainException ex)
        {
            return Result<FormTemplateDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<FormTemplateDetailDto>> DuplicateAsync(DuplicateFormTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var source = await _repository.GetAsync(command.TenantId, command.TemplateId, cancellationToken);
            if (source is null || source.IsDeleted)
            {
                return Result<FormTemplateDetailDto>.Failure("Plantilla no encontrada.");
            }

            if (await _repository.CodeExistsAsync(command.TenantId, command.NewCode, null, cancellationToken))
            {
                return Result<FormTemplateDetailDto>.Failure("Ya existe una plantilla con ese código en el tenant.");
            }

            var schema = source.Versions
                .OrderByDescending(v => v.IsPublished)
                .ThenByDescending(v => v.CreatedAtUtc)
                .Select(v => v.SchemaJson)
                .FirstOrDefault() ?? DefaultSchema();

            var clone = new FormTemplate(
                command.TenantId,
                command.NewName,
                command.NewCode,
                source.Category,
                source.Kind,
                source.Description,
                command.RequestedByUserId);
            clone.CreateDraftVersion(schema, $"Duplicado desde {source.Code}", command.RequestedByUserId);
            await _repository.AddAsync(clone, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, clone.Id, AuditAction.FormTemplateCreated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<FormTemplateDetailDto>.Success(ToDetail(clone));
        }
        catch (DomainException ex)
        {
            return Result<FormTemplateDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<FormTemplateDetailDto>> RestoreVersionAsync(RestoreFormTemplateVersionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _repository.GetAsync(command.TenantId, command.TemplateId, cancellationToken);
            if (template is null || template.IsDeleted)
            {
                return Result<FormTemplateDetailDto>.Failure("Plantilla no encontrada.");
            }

            var version = template.Versions.FirstOrDefault(v => v.Id == command.VersionId);
            if (version is null)
            {
                return Result<FormTemplateDetailDto>.Failure("Versión no encontrada.");
            }

            template.CreateDraftVersion(version.SchemaJson, $"Restaurado desde {version.VersionNumber}", command.RequestedByUserId, template.PublishedVersionNumber);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, template.Id, AuditAction.FormTemplateUpdated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<FormTemplateDetailDto>.Success(ToDetail(template));
        }
        catch (DomainException ex)
        {
            return Result<FormTemplateDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result> ArchiveAsync(FormTemplateActionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _repository.GetAsync(command.TenantId, command.TemplateId, cancellationToken);
            if (template is null || template.IsDeleted)
            {
                return Result.Failure("Plantilla no encontrada.");
            }

            template.Archive();
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, template.Id, AuditAction.FormTemplateUpdated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> SoftDeleteAsync(FormTemplateActionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _repository.GetAsync(command.TenantId, command.TemplateId, cancellationToken);
            if (template is null)
            {
                return Result.Failure("Plantilla no encontrada.");
            }

            template.SoftDelete();
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, template.Id, AuditAction.Deleted, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<FormTemplateDetailDto>> GetAsync(Guid tenantId, Guid templateId, CancellationToken cancellationToken = default)
    {
        var template = await _repository.GetAsync(tenantId, templateId, cancellationToken);
        if (template is null || template.IsDeleted)
        {
            return Result<FormTemplateDetailDto>.Failure("Plantilla no encontrada.");
        }

        return Result<FormTemplateDetailDto>.Success(ToDetail(template));
    }

    public async Task<Result<IReadOnlyCollection<FormTemplateSummaryDto>>> SearchAsync(FormTemplateSearchQuery query, CancellationToken cancellationToken = default)
        => Result<IReadOnlyCollection<FormTemplateSummaryDto>>.Success(await _repository.SearchAsync(query, cancellationToken));

    public async Task<Result<IReadOnlyCollection<FormTemplateSummaryDto>>> SearchPublishedAsync(Guid tenantId, FormTemplateKind? kind, CancellationToken cancellationToken = default)
        => Result<IReadOnlyCollection<FormTemplateSummaryDto>>.Success(
            await _repository.SearchAsync(new FormTemplateSearchQuery(tenantId, null, FormTemplateLifecycleStatus.Published, kind), cancellationToken));

    private async Task AppendAuditAsync(Guid tenantId, Guid userId, Guid templateId, AuditAction action, CancellationToken cancellationToken)
    {
        var auditLog = AuditLog.FromEvent(
            new AuditEvent(
                nameof(FormTemplate),
                templateId,
                action,
                AuditCategory.Configuration,
                new AuditContext(tenantId, userId, null, null, null, null, null, null, null),
                new AuditSnapshot(null, null),
                new AuditMetadata("{\"source\":\"form-template-builder\"}"),
                true,
                null),
            _clock.UtcNow);
        await _auditRepository.AddAsync(auditLog, cancellationToken);
    }

    private static FormTemplateDetailDto ToDetail(FormTemplate template)
    {
        var versions = template.Versions
            .OrderByDescending(v => v.CreatedAtUtc)
            .Select(v => new FormTemplateVersionDto(
                v.Id,
                v.VersionNumber,
                v.SchemaJson,
                v.ChangeLog,
                v.IsPublished,
                v.CreatedAtUtc,
                v.PublishedAtUtc,
                v.CreatedByUserId))
            .ToList();

        var working = versions.FirstOrDefault(v => !v.IsPublished) ?? versions.FirstOrDefault();
        return new FormTemplateDetailDto(
            template.Id,
            template.TenantId,
            template.Name,
            template.Code,
            template.Category,
            template.Kind,
            template.Description,
            template.Status,
            template.PublishedVersionNumber,
            template.CreatedByUserId,
            template.CreatedAtUtc,
            template.UpdatedAtUtc,
            working,
            versions);
    }

    private static string DefaultSchema() =>
        """{"fields":[],"rules":[],"meta":{"builder":"Compliance360 Form Template Builder","schemaVersion":1}}""";
}
