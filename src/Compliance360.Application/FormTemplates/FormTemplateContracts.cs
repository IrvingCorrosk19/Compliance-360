using Compliance360.Domain.FormTemplates;
using Compliance360.Shared;

namespace Compliance360.Application.FormTemplates;

public interface IFormTemplateService
{
    Task<Result<FormTemplateDetailDto>> CreateAsync(CreateFormTemplateCommand command, CancellationToken cancellationToken = default);
    Task<Result<FormTemplateDetailDto>> UpdateHeaderAsync(UpdateFormTemplateHeaderCommand command, CancellationToken cancellationToken = default);
    Task<Result<FormTemplateDetailDto>> SaveDraftAsync(SaveFormTemplateDraftCommand command, CancellationToken cancellationToken = default);
    Task<Result<FormTemplateDetailDto>> PublishAsync(FormTemplateActionCommand command, CancellationToken cancellationToken = default);
    Task<Result<FormTemplateDetailDto>> DuplicateAsync(DuplicateFormTemplateCommand command, CancellationToken cancellationToken = default);
    Task<Result<FormTemplateDetailDto>> RestoreVersionAsync(RestoreFormTemplateVersionCommand command, CancellationToken cancellationToken = default);
    Task<Result> ArchiveAsync(FormTemplateActionCommand command, CancellationToken cancellationToken = default);
    Task<Result> SoftDeleteAsync(FormTemplateActionCommand command, CancellationToken cancellationToken = default);
    Task<Result<FormTemplateDetailDto>> GetAsync(Guid tenantId, Guid templateId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyCollection<FormTemplateSummaryDto>>> SearchAsync(FormTemplateSearchQuery query, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyCollection<FormTemplateSummaryDto>>> SearchPublishedAsync(Guid tenantId, FormTemplateKind? kind, CancellationToken cancellationToken = default);
}

public interface IFormTemplateRepository
{
    Task AddAsync(FormTemplate template, CancellationToken cancellationToken = default);
    Task<FormTemplate?> GetAsync(Guid tenantId, Guid templateId, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(Guid tenantId, string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<FormTemplateSummaryDto>> SearchAsync(FormTemplateSearchQuery query, CancellationToken cancellationToken = default);
}

public sealed record CreateFormTemplateCommand(
    Guid TenantId,
    string Name,
    string Code,
    string Category,
    FormTemplateKind Kind,
    string Description,
    string? InitialSchemaJson,
    Guid RequestedByUserId);

public sealed record UpdateFormTemplateHeaderCommand(
    Guid TenantId,
    Guid TemplateId,
    string Name,
    string Category,
    FormTemplateKind Kind,
    string Description,
    Guid RequestedByUserId);

public sealed record SaveFormTemplateDraftCommand(
    Guid TenantId,
    Guid TemplateId,
    Guid? VersionId,
    string SchemaJson,
    string ChangeLog,
    Guid RequestedByUserId);

public sealed record FormTemplateActionCommand(Guid TenantId, Guid TemplateId, Guid? VersionId, Guid RequestedByUserId);

public sealed record DuplicateFormTemplateCommand(
    Guid TenantId,
    Guid TemplateId,
    string NewName,
    string NewCode,
    Guid RequestedByUserId);

public sealed record RestoreFormTemplateVersionCommand(
    Guid TenantId,
    Guid TemplateId,
    Guid VersionId,
    Guid RequestedByUserId);

public sealed record FormTemplateSearchQuery(
    Guid TenantId,
    string? SearchText,
    FormTemplateLifecycleStatus? Status,
    FormTemplateKind? Kind,
    bool IncludeDeleted = false);

public sealed record FormTemplateSummaryDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Code,
    string Category,
    FormTemplateKind Kind,
    FormTemplateLifecycleStatus Status,
    string? PublishedVersionNumber,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

public sealed record FormTemplateVersionDto(
    Guid Id,
    string VersionNumber,
    string SchemaJson,
    string ChangeLog,
    bool IsPublished,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? PublishedAtUtc,
    Guid CreatedByUserId);

public sealed record FormTemplateDetailDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Code,
    string Category,
    FormTemplateKind Kind,
    string Description,
    FormTemplateLifecycleStatus Status,
    string? PublishedVersionNumber,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    FormTemplateVersionDto? WorkingVersion,
    IReadOnlyCollection<FormTemplateVersionDto> Versions);
