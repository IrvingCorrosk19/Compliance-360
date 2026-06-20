using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.TenantManagement;
using Compliance360.Shared;

namespace Compliance360.Application.TenantManagement;

public sealed class TenantManagementService : ITenantManagementService
{
    private readonly ITenantManagementRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;

    public TenantManagementService(
        ITenantManagementRepository repository,
        IApplicationDbContext dbContext,
        IClock clock)
    {
        _repository = repository;
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<Result<TenantSummary>> CreateTenantAsync(CreateTenantCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedSlug = NormalizeSlug(command.Slug);
            if (await _repository.SlugExistsAsync(normalizedSlug, cancellationToken))
            {
                return Result<TenantSummary>.Failure("Tenant slug already exists.");
            }

            var tenant = new Tenant(command.Name, normalizedSlug);
            await _repository.AddAsync(tenant, cancellationToken);
            await AppendAuditAsync(tenant.Id, command.RequestedByUserId, nameof(Tenant), tenant.Id, AuditAction.Created, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result<TenantSummary>.Success(ToSummary(tenant));
        }
        catch (DomainException exception)
        {
            return Result<TenantSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<CompanySummary>> AddCompanyAsync(AddCompanyCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await GetTenantOrNullAsync(command.TenantId, cancellationToken);
            if (tenant is null)
            {
                return Result<CompanySummary>.Failure("Tenant not found.");
            }

            var company = tenant.AddCompany(command.LegalName, command.TaxIdentifier, command.CountryCode);
            await AppendAuditAsync(tenant.Id, command.RequestedByUserId, nameof(Company), company.Id, AuditAction.Created, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result<CompanySummary>.Success(ToSummary(company));
        }
        catch (DomainException exception)
        {
            return Result<CompanySummary>.Failure(exception.Message);
        }
    }

    public async Task<Result> ActivateTenantAsync(Guid tenantId, Guid? requestedByUserId, CancellationToken cancellationToken = default)
    {
        return await ChangeTenantStateAsync(
            tenantId,
            requestedByUserId,
            tenant => tenant.Activate(),
            AuditAction.Updated,
            cancellationToken);
    }

    public async Task<Result> SuspendTenantAsync(Guid tenantId, Guid? requestedByUserId, CancellationToken cancellationToken = default)
    {
        return await ChangeTenantStateAsync(
            tenantId,
            requestedByUserId,
            tenant => tenant.Suspend(),
            AuditAction.Updated,
            cancellationToken);
    }

    public async Task<Result> ConfigureSettingsAsync(ConfigureTenantSettingsCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await GetTenantOrNullAsync(command.TenantId, cancellationToken);
            if (tenant is null)
            {
                return Result.Failure("Tenant not found.");
            }

            tenant.Settings.Configure(command.Culture, command.TimeZone, command.RequireMfa, command.DocumentRetentionDays);
            await AppendAuditAsync(tenant.Id, command.RequestedByUserId, nameof(TenantSettings), tenant.Settings.Id, AuditAction.Updated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    public async Task<Result> ConfigureBrandingAsync(ConfigureTenantBrandingCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await GetTenantOrNullAsync(command.TenantId, cancellationToken);
            if (tenant is null)
            {
                return Result.Failure("Tenant not found.");
            }

            tenant.Branding.Configure(command.DisplayName, command.LogoUri, command.PrimaryColor, command.SecondaryColor);
            await AppendAuditAsync(tenant.Id, command.RequestedByUserId, nameof(TenantBranding), tenant.Branding.Id, AuditAction.Updated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    public async Task<Result> ChangeSubscriptionAsync(ChangeSubscriptionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await GetTenantOrNullAsync(command.TenantId, cancellationToken);
            if (tenant is null)
            {
                return Result.Failure("Tenant not found.");
            }

            tenant.Subscription.ChangePlan(command.Plan, command.MaxUsers, command.MaxStorageGb);
            await AppendAuditAsync(tenant.Id, command.RequestedByUserId, nameof(Subscription), tenant.Subscription.Id, AuditAction.Updated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    private async Task<Result> ChangeTenantStateAsync(
        Guid tenantId,
        Guid? requestedByUserId,
        Action<Tenant> changeState,
        AuditAction auditAction,
        CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await GetTenantOrNullAsync(tenantId, cancellationToken);
            if (tenant is null)
            {
                return Result.Failure("Tenant not found.");
            }

            changeState(tenant);
            await AppendAuditAsync(tenant.Id, requestedByUserId, nameof(Tenant), tenant.Id, auditAction, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    private Task<Tenant?> GetTenantOrNullAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        if (tenantId == Guid.Empty)
        {
            return Task.FromResult<Tenant?>(null);
        }

        return _repository.GetByIdAsync(tenantId, cancellationToken);
    }

    private async Task AppendAuditAsync(
        Guid tenantId,
        Guid? requestedByUserId,
        string entityName,
        Guid entityId,
        AuditAction action,
        CancellationToken cancellationToken)
    {
        var auditLog = AuditLog.Create(tenantId, requestedByUserId, entityName, entityId, action, _clock.UtcNow);
        await _repository.AddAuditLogAsync(auditLog, cancellationToken);
    }

    private static TenantSummary ToSummary(Tenant tenant)
    {
        return new TenantSummary(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.Status,
            tenant.Settings.Culture,
            tenant.Settings.TimeZone,
            tenant.Settings.RequireMfa,
            tenant.Branding.DisplayName,
            tenant.Subscription.Plan,
            tenant.Subscription.Status);
    }

    private static CompanySummary ToSummary(Company company)
    {
        return new CompanySummary(
            company.Id,
            company.TenantId,
            company.LegalName,
            company.TaxIdentifier,
            company.CountryCode,
            company.IsActive);
    }

    private static string NormalizeSlug(string slug)
    {
        return Guard.AgainstNullOrWhiteSpace(slug, nameof(slug), 80).ToLowerInvariant();
    }
}
