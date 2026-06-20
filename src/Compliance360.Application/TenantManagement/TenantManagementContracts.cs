using Compliance360.Domain.Audit;
using Compliance360.Domain.TenantManagement;
using Compliance360.Shared;

namespace Compliance360.Application.TenantManagement;

public interface ITenantManagementService
{
    Task<Result<TenantSummary>> CreateTenantAsync(CreateTenantCommand command, CancellationToken cancellationToken = default);

    Task<Result<CompanySummary>> AddCompanyAsync(AddCompanyCommand command, CancellationToken cancellationToken = default);

    Task<Result> ActivateTenantAsync(Guid tenantId, Guid? requestedByUserId, CancellationToken cancellationToken = default);

    Task<Result> SuspendTenantAsync(Guid tenantId, Guid? requestedByUserId, CancellationToken cancellationToken = default);

    Task<Result> ConfigureSettingsAsync(ConfigureTenantSettingsCommand command, CancellationToken cancellationToken = default);

    Task<Result> ConfigureBrandingAsync(ConfigureTenantBrandingCommand command, CancellationToken cancellationToken = default);

    Task<Result> ChangeSubscriptionAsync(ChangeSubscriptionCommand command, CancellationToken cancellationToken = default);
}

public interface ITenantManagementRepository
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);

    Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);

    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

public sealed record CreateTenantCommand(
    string Name,
    string Slug,
    Guid? RequestedByUserId);

public sealed record AddCompanyCommand(
    Guid TenantId,
    string LegalName,
    string TaxIdentifier,
    string CountryCode,
    Guid? RequestedByUserId);

public sealed record ConfigureTenantSettingsCommand(
    Guid TenantId,
    string Culture,
    string TimeZone,
    bool RequireMfa,
    int DocumentRetentionDays,
    Guid? RequestedByUserId);

public sealed record ConfigureTenantBrandingCommand(
    Guid TenantId,
    string DisplayName,
    string? LogoUri,
    string PrimaryColor,
    string SecondaryColor,
    Guid? RequestedByUserId);

public sealed record ChangeSubscriptionCommand(
    Guid TenantId,
    SubscriptionPlan Plan,
    int MaxUsers,
    int MaxStorageGb,
    Guid? RequestedByUserId);

public sealed record TenantSummary(
    Guid Id,
    string Name,
    string Slug,
    TenantStatus Status,
    string Culture,
    string TimeZone,
    bool RequireMfa,
    string BrandingDisplayName,
    SubscriptionPlan Plan,
    SubscriptionStatus SubscriptionStatus);

public sealed record CompanySummary(
    Guid Id,
    Guid TenantId,
    string LegalName,
    string TaxIdentifier,
    string CountryCode,
    bool IsActive);
