using System.Security.Claims;
using Compliance360.Application.Audit;
using Compliance360.Application.AuditManagement;
using Compliance360.Application.CapaManagement;
using Compliance360.Application.Documents;
using Compliance360.Application.Enterprise;
using Compliance360.Application.Identity;
using Compliance360.Application.Mfa;
using Compliance360.Application.Notifications;
using Compliance360.Application.QualityIndicators;
using Compliance360.Application.Rbac;
using Compliance360.Application.Reporting;
using Compliance360.Application.RiskManagement;
using Compliance360.Application.Storage;
using Compliance360.Application.SuperAdmin;
using Compliance360.Application.Suppliers;
using Compliance360.Application.TechnicalSheets;
using Compliance360.Application.TenantManagement;
using Compliance360.Application.Workflows;
using Compliance360.Domain.Documents;
using Compliance360.Domain.Enterprise;
using Compliance360.Domain.AuditManagement;
using Compliance360.Domain.CapaManagement;
using Compliance360.Domain.Identity;
using Compliance360.Domain.QualityIndicators;
using Compliance360.Domain.Reporting;
using Compliance360.Domain.RiskManagement;
using Compliance360.Domain.Suppliers;
using Compliance360.Domain.TechnicalSheets;
using Compliance360.Domain.TenantManagement;
using Compliance360.Domain.Workflows;
using Compliance360.Web.Security;

namespace Compliance360.Web.Api;

public static class FoundationEndpoints
{
    public static RouteGroupBuilder MapFoundationApi(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api/v1")
            .WithTags("Compliance 360 API v1")
            .RequireRateLimiting("api");

        MapIdentity(api);
        MapTenants(api);
        MapRbac(api);
        MapMfa(api);
        MapAudit(api);
        MapStorage(api);
        MapNotifications(api);
        MapDocuments(api);
        MapWorkflows(api);
        MapTechnicalSheets(api);
        MapSuppliers(api);
        MapAuditManagement(api);
        MapCapaManagement(api);
        MapRiskManagement(api);
        MapQualityIndicators(api);
        MapReportingEngine(api);
        MapEnterpriseWorkspaces(api);
        MapSuperAdminPlatform(api);

        return api;
    }

    private static void MapSuperAdminPlatform(RouteGroupBuilder api)
    {
        var superAdmin = api.MapGroup("/superadmin/platform-center")
            .WithTags("SuperAdmin Platform Center");

        superAdmin.MapGet("/", async (ISuperAdminPlatformService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetCenterAsync(cancellationToken)))
            .RequireAuthorization(PermissionPolicies.SuperAdminDashboard);

        superAdmin.MapGet("/tenants", async (string? searchText, string? status, int page, int pageSize, ISuperAdminPlatformService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.SearchTenantsAsync(new SuperAdminTenantSearchQuery(searchText, status, page, pageSize), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.SuperAdminTenantsRead);

        superAdmin.MapGet("/audit-timeline", async (int page, int pageSize, ISuperAdminPlatformService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetGlobalAuditTimelineAsync(page, pageSize, cancellationToken)))
            .RequireAuthorization(PermissionPolicies.SuperAdminAudit);

        superAdmin.MapGet("/audit-timeline/export", async (int page, int pageSize, ISuperAdminPlatformService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetGlobalAuditTimelineAsync(page, pageSize, cancellationToken);
            if (!result.IsSuccess || result.Value is null)
            {
                return Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
            }

            var lines = new[] { "Id,TenantId,TenantName,OccurredAtUtc,Action,Category,EntityName,EntityId,UserId,IpAddress,CorrelationId,Success" }
                .Concat(result.Value.Select(item => $"{item.Id},{item.TenantId},{EscapeCsv(item.TenantName)},{item.OccurredAtUtc:O},{item.Action},{item.Category},{item.EntityName},{item.EntityId},{item.UserId},{item.IpAddress},{item.CorrelationId},{item.Success}"));
            return Results.Text(string.Join(Environment.NewLine, lines), "text/csv");
        })
            .RequireAuthorization(PermissionPolicies.SuperAdminAudit);
    }

    private static void MapIdentity(RouteGroupBuilder api)
    {
        var auth = api.MapGroup("/auth").WithTags("Identity");

        auth.MapPost("/login", async (LoginRequest request, HttpContext httpContext, IIdentityService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.LoginAsync(
                new LoginCommand(request.TenantId, request.Email, request.Password, ApiContext.IpAddress(httpContext), ApiContext.UserAgent(httpContext)),
                cancellationToken)));

        auth.MapPost("/mfa/complete", async (CompleteMfaChallengeRequest request, HttpContext httpContext, IIdentityService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CompleteMfaChallengeAsync(
                new CompleteMfaChallengeCommand(request.ChallengeToken, request.Method, request.VerificationCode, ApiContext.IpAddress(httpContext), ApiContext.UserAgent(httpContext)),
                cancellationToken)));

        auth.MapPost("/refresh", async (RefreshTokenRequest request, HttpContext httpContext, IIdentityService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.RefreshTokenAsync(
                new RefreshTokenCommand(request.RefreshToken, ApiContext.IpAddress(httpContext), ApiContext.UserAgent(httpContext)),
                cancellationToken)));

        auth.MapPost("/logout", async (LogoutRequest request, HttpContext httpContext, IIdentityService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.LogoutAsync(
                new LogoutCommand(ApiContext.TenantId(httpContext, request.TenantId), request.UserId, request.RefreshTokenHash, ApiContext.IpAddress(httpContext), ApiContext.UserAgent(httpContext)),
                cancellationToken)))
            .RequireAuthorization();

        auth.MapPost("/password", async (ChangePasswordRequest request, HttpContext httpContext, IIdentityService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ChangePasswordAsync(
                new ChangePasswordCommand(ApiContext.TenantId(httpContext, request.TenantId), ApiContext.UserId(httpContext), request.CurrentPassword, request.NewPassword, ApiContext.IpAddress(httpContext), ApiContext.UserAgent(httpContext)),
                cancellationToken)))
            .RequireAuthorization();
    }

    private static void MapEnterpriseWorkspaces(RouteGroupBuilder api)
    {
        var enterprise = api.MapGroup("/tenants/{tenantId:guid}/enterprise-workspaces")
            .WithTags("Enterprise Workspaces")
            .RequireAuthorization(PermissionPolicies.TenantManage);

        enterprise.MapPost("/", async (Guid tenantId, CreateEnterpriseWorkspaceItemRequest request, HttpContext httpContext, IEnterpriseWorkspaceService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateAsync(
                new CreateEnterpriseWorkspaceItemCommand(
                    ApiContext.TenantId(httpContext, tenantId),
                    request.Type,
                    request.Title,
                    request.Code,
                    request.Description,
                    ApiContext.UserId(httpContext),
                    request.OwnerUserId,
                    request.DueAtUtc,
                    request.MetadataJson ?? "{}"),
                cancellationToken)));

        enterprise.MapGet("/", async (Guid tenantId, EnterpriseWorkspaceType? type, EnterpriseWorkspaceStatus? status, string? searchText, HttpContext httpContext, IEnterpriseWorkspaceService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.SearchAsync(new EnterpriseWorkspaceSearchQuery(ApiContext.TenantId(httpContext, tenantId), type, status, searchText), cancellationToken)));

        enterprise.MapGet("/dashboard", async (Guid tenantId, HttpContext httpContext, IEnterpriseWorkspaceService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetDashboardAsync(ApiContext.TenantId(httpContext, tenantId), cancellationToken)));

        enterprise.MapPost("/{itemId:guid}/complete", async (Guid tenantId, Guid itemId, HttpContext httpContext, IEnterpriseWorkspaceService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CompleteAsync(new EnterpriseWorkspaceActionCommand(ApiContext.TenantId(httpContext, tenantId), itemId, ApiContext.UserId(httpContext)), cancellationToken)));

        enterprise.MapPost("/{itemId:guid}/reopen", async (Guid tenantId, Guid itemId, HttpContext httpContext, IEnterpriseWorkspaceService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ReopenAsync(new EnterpriseWorkspaceActionCommand(ApiContext.TenantId(httpContext, tenantId), itemId, ApiContext.UserId(httpContext)), cancellationToken)));
    }

    private static void MapTenants(RouteGroupBuilder api)
    {
        var tenants = api.MapGroup("/tenants").WithTags("Tenant Administration Center");

        tenants.MapGet("/", async (string? searchText, TenantStatus? status, int page, int pageSize, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.SearchTenantsAsync(new TenantSearchQuery(searchText, status, page, pageSize), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantRead);

        tenants.MapPost("/", async (CreateTenantRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateTenantAsync(new CreateTenantCommand(
                request.Name,
                request.Slug,
                request.LegalName,
                request.CommercialName,
                request.TaxIdentifier,
                request.CountryCode,
                request.Currency,
                ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantCreate);

        tenants.MapGet("/{tenantId:guid}", async (Guid tenantId, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetTenantAsync(ApiContext.TenantId(httpContext, tenantId), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantRead);

        tenants.MapGet("/{tenantId:guid}/administration-dashboard", async (Guid tenantId, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetAdministrationDashboardAsync(ApiContext.TenantId(httpContext, tenantId), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantRead);

        tenants.MapGet("/{tenantId:guid}/administration-center", async (Guid tenantId, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetAdministrationCenterStateAsync(ApiContext.TenantId(httpContext, tenantId), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantRead);

        tenants.MapPut("/{tenantId:guid}/general-information", async (Guid tenantId, UpdateTenantGeneralInformationRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.UpdateGeneralInformationAsync(
                new UpdateTenantGeneralInformationCommand(
                    ApiContext.TenantId(httpContext, tenantId),
                    request.Name,
                    request.LegalName,
                    request.CommercialName,
                    request.TaxIdentifier,
                    request.Industry,
                    request.Description,
                    request.AddressLine1,
                    request.City,
                    request.Province,
                    request.CountryCode,
                    request.PostalCode,
                    request.Phone,
                    request.Email,
                    request.Website,
                    request.Currency,
                    request.ChangeReason,
                    ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantUpdate);

        tenants.MapPost("/{tenantId:guid}/companies", async (Guid tenantId, AddCompanyRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddCompanyAsync(
                new AddCompanyCommand(ApiContext.TenantId(httpContext, tenantId), request.LegalName, request.TaxIdentifier, request.CountryCode, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantUpdate);

        tenants.MapPost("/{tenantId:guid}/trial", async (Guid tenantId, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.StartTrialAsync(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantStatus);

        tenants.MapPost("/{tenantId:guid}/activate", async (Guid tenantId, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ActivateTenantAsync(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantStatus);

        tenants.MapPost("/{tenantId:guid}/suspend", async (Guid tenantId, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.SuspendTenantAsync(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantStatus);

        tenants.MapPost("/{tenantId:guid}/archive", async (Guid tenantId, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ArchiveTenantAsync(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantDelete);

        tenants.MapPost("/{tenantId:guid}/restore", async (Guid tenantId, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.RestoreTenantAsync(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantRestore);

        tenants.MapPut("/{tenantId:guid}/settings", async (Guid tenantId, ConfigureTenantSettingsRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ConfigureSettingsAsync(
                new ConfigureTenantSettingsCommand(ApiContext.TenantId(httpContext, tenantId), request.Culture, request.TimeZone, request.RequireMfa, request.DocumentRetentionDays, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantUpdate);

        tenants.MapPut("/{tenantId:guid}/security", async (Guid tenantId, ConfigureTenantSecurityRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ConfigureSecurityAsync(
                new ConfigureTenantSecurityCommand(
                    ApiContext.TenantId(httpContext, tenantId),
                    request.RequireMfa,
                    request.SessionTimeoutMinutes,
                    request.PasswordExpirationDays,
                    request.LockoutMaxFailedAttempts,
                    request.LockoutMinutes,
                    request.IpWhitelist,
                    request.TrustedDevicesEnabled,
                    request.SecurityScore,
                    request.ChangeReason,
                    ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantSecurity);

        tenants.MapPut("/{tenantId:guid}/branding", async (Guid tenantId, ConfigureTenantBrandingRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ConfigureBrandingAsync(
                new ConfigureTenantBrandingCommand(
                    ApiContext.TenantId(httpContext, tenantId),
                    request.DisplayName,
                    request.LogoUri,
                    request.FaviconUri,
                    request.PrimaryColor,
                    request.SecondaryColor,
                    request.Theme,
                    request.LoginBackgroundUri,
                    request.CorporateEmail,
                    request.FooterText,
                    request.ChangeReason,
                    ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantBranding);

        tenants.MapPut("/{tenantId:guid}/subscription", async (Guid tenantId, ChangeSubscriptionRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ChangeSubscriptionAsync(
                new ChangeSubscriptionCommand(
                    ApiContext.TenantId(httpContext, tenantId),
                    request.Plan,
                    request.MaxUsers,
                    request.MaxStorageGb,
                    request.Status,
                    request.ExpiresOn,
                    request.ChangeReason,
                    ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantBilling);

        tenants.MapPatch("/{tenantId:guid}/subscription", async (Guid tenantId, ChangeSubscriptionRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ChangeSubscriptionAsync(
                new ChangeSubscriptionCommand(
                    ApiContext.TenantId(httpContext, tenantId),
                    request.Plan,
                    request.MaxUsers,
                    request.MaxStorageGb,
                    request.Status,
                    request.ExpiresOn,
                    request.ChangeReason,
                    ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantBilling);

        tenants.MapPut("/{tenantId:guid}/domains", async (Guid tenantId, UpsertTenantDomainRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.UpsertDomainAsync(new UpsertTenantDomainCommand(ApiContext.TenantId(httpContext, tenantId), null, request.HostName, request.Kind, request.IsDefault, request.RedirectToHostName, request.ChangeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantDomains);

        tenants.MapPatch("/{tenantId:guid}/domains/{domainId:guid}", async (Guid tenantId, Guid domainId, UpsertTenantDomainRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.UpsertDomainAsync(new UpsertTenantDomainCommand(ApiContext.TenantId(httpContext, tenantId), domainId, request.HostName, request.Kind, request.IsDefault, request.RedirectToHostName, request.ChangeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantDomains);

        tenants.MapDelete("/{tenantId:guid}/domains/{domainId:guid}", async (Guid tenantId, Guid domainId, string? changeReason, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.DisableDomainAsync(new TenantEntityActionCommand(ApiContext.TenantId(httpContext, tenantId), domainId, changeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantDomains);

        tenants.MapPut("/{tenantId:guid}/sso", async (Guid tenantId, UpsertTenantSsoRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.UpsertSsoAsync(new UpsertTenantSsoCommand(ApiContext.TenantId(httpContext, tenantId), null, request.Provider, request.Name, request.Authority, request.MetadataUrl, request.ClientId, request.SecretReference, request.ClaimsMappingJson, request.RoleMappingJson, request.JitProvisioningEnabled, request.ScimEnabled, request.CertificateThumbprint, request.Enabled, request.ChangeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantSso);

        tenants.MapPatch("/{tenantId:guid}/sso/{ssoId:guid}", async (Guid tenantId, Guid ssoId, UpsertTenantSsoRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.UpsertSsoAsync(new UpsertTenantSsoCommand(ApiContext.TenantId(httpContext, tenantId), ssoId, request.Provider, request.Name, request.Authority, request.MetadataUrl, request.ClientId, request.SecretReference, request.ClaimsMappingJson, request.RoleMappingJson, request.JitProvisioningEnabled, request.ScimEnabled, request.CertificateThumbprint, request.Enabled, request.ChangeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantSso);

        tenants.MapPost("/{tenantId:guid}/sso/{ssoId:guid}/test", async (Guid tenantId, Guid ssoId, TenantActionRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.TestSsoAsync(new TenantEntityActionCommand(ApiContext.TenantId(httpContext, tenantId), ssoId, request.ChangeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantSso);

        tenants.MapPost("/{tenantId:guid}/api-keys", async (Guid tenantId, CreateTenantApiCredentialRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateApiCredentialAsync(new CreateTenantApiCredentialCommand(ApiContext.TenantId(httpContext, tenantId), request.Name, request.PlainTextSecret, request.Scopes, request.ExpiresAtUtc, request.ChangeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantApiKeys);

        tenants.MapPost("/{tenantId:guid}/api-keys/{apiKeyId:guid}/rotate", async (Guid tenantId, Guid apiKeyId, RotateTenantApiCredentialRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.RotateApiCredentialAsync(new RotateTenantApiCredentialCommand(ApiContext.TenantId(httpContext, tenantId), apiKeyId, request.PlainTextSecret, request.ExpiresAtUtc, request.ChangeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantApiKeys);

        tenants.MapDelete("/{tenantId:guid}/api-keys/{apiKeyId:guid}", async (Guid tenantId, Guid apiKeyId, string? changeReason, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.RevokeApiCredentialAsync(new TenantEntityActionCommand(ApiContext.TenantId(httpContext, tenantId), apiKeyId, changeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantApiKeys);

        tenants.MapPut("/{tenantId:guid}/webhooks", async (Guid tenantId, UpsertTenantWebhookRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.UpsertWebhookAsync(new UpsertTenantWebhookCommand(ApiContext.TenantId(httpContext, tenantId), null, request.Name, request.Url, request.Events, request.PlainTextSecret, request.MaxRetries, request.Enabled, request.ChangeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantWebhooks);

        tenants.MapPatch("/{tenantId:guid}/webhooks/{webhookId:guid}", async (Guid tenantId, Guid webhookId, UpsertTenantWebhookRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.UpsertWebhookAsync(new UpsertTenantWebhookCommand(ApiContext.TenantId(httpContext, tenantId), webhookId, request.Name, request.Url, request.Events, request.PlainTextSecret, request.MaxRetries, request.Enabled, request.ChangeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantWebhooks);

        tenants.MapPost("/{tenantId:guid}/webhooks/{webhookId:guid}/test", async (Guid tenantId, Guid webhookId, TenantActionRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.TestWebhookAsync(new TenantEntityActionCommand(ApiContext.TenantId(httpContext, tenantId), webhookId, request.ChangeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantWebhooks);

        tenants.MapDelete("/{tenantId:guid}/webhooks/{webhookId:guid}", async (Guid tenantId, Guid webhookId, string? changeReason, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.DisableWebhookAsync(new TenantEntityActionCommand(ApiContext.TenantId(httpContext, tenantId), webhookId, changeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantWebhooks);

        tenants.MapPut("/{tenantId:guid}/license", async (Guid tenantId, UpsertTenantLicenseRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.UpsertLicenseAsync(new UpsertTenantLicenseCommand(ApiContext.TenantId(httpContext, tenantId), request.LicenseNumber, request.Status, request.FeaturesJson, request.ModulesJson, request.EntitlementsJson, request.PeriodStart, request.PeriodEnd, request.RenewalDate, request.SeatsPurchased, request.SeatsUsed, request.StorageGbPurchased, request.StorageBytesUsed, request.ChangeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantBilling);

        tenants.MapGet("/{tenantId:guid}/health", async (Guid tenantId, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetHealthCenterAsync(ApiContext.TenantId(httpContext, tenantId), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantHealth);

        tenants.MapPost("/{tenantId:guid}/backups", async (Guid tenantId, RecordTenantBackupRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.RecordBackupAsync(new RecordTenantBackupCommand(ApiContext.TenantId(httpContext, tenantId), request.BackupKind, request.Result, request.StartedAtUtc, request.CompletedAtUtc, request.SizeBytes, request.Message, request.RpoMinutes, request.RtoMinutes, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantBackup);

        tenants.MapGet("/{tenantId:guid}/users", async (Guid tenantId, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetUsersAsync(ApiContext.TenantId(httpContext, tenantId), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantUsers);

        tenants.MapPost("/{tenantId:guid}/users", async (Guid tenantId, CreateTenantUserRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateUserAsync(new CreateTenantUserCommand(ApiContext.TenantId(httpContext, tenantId), request.Email, request.FullName, request.InitialPassword, request.ForcePasswordChange, request.RoleId, request.ChangeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantUsers);

        tenants.MapPatch("/{tenantId:guid}/users/{userId:guid}/status", async (Guid tenantId, Guid userId, ChangeTenantUserStatusRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ChangeUserStatusAsync(new ChangeTenantUserStatusCommand(ApiContext.TenantId(httpContext, tenantId), userId, request.Status, request.ChangeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantUsers);

        tenants.MapPost("/{tenantId:guid}/users/{userId:guid}/reset-mfa", async (Guid tenantId, Guid userId, TenantActionRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ResetUserMfaAsync(new TenantUserActionCommand(ApiContext.TenantId(httpContext, tenantId), userId, request.ChangeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantSecurity);

        tenants.MapPost("/{tenantId:guid}/users/{userId:guid}/roles", async (Guid tenantId, Guid userId, AssignTenantUserRoleRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AssignUserRoleAsync(new AssignTenantUserRoleCommand(ApiContext.TenantId(httpContext, tenantId), userId, request.RoleId, request.ChangeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantRoles);

        tenants.MapDelete("/{tenantId:guid}/users/{userId:guid}/roles/{roleId:guid}", async (Guid tenantId, Guid userId, Guid roleId, string? changeReason, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.RevokeUserRoleAsync(new AssignTenantUserRoleCommand(ApiContext.TenantId(httpContext, tenantId), userId, roleId, changeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantRoles);

        tenants.MapPost("/{tenantId:guid}/users/{userId:guid}/sessions/close", async (Guid tenantId, Guid userId, TenantActionRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CloseUserSessionsAsync(new TenantUserActionCommand(ApiContext.TenantId(httpContext, tenantId), userId, request.ChangeReason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantUsers);

        tenants.MapGet("/{tenantId:guid}/audit-timeline", async (Guid tenantId, int page, int pageSize, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetAuditTimelineAsync(ApiContext.TenantId(httpContext, tenantId), page, pageSize, cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantAudit);

        tenants.MapGet("/{tenantId:guid}/audit-timeline/export", async (Guid tenantId, int page, int pageSize, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetAuditTimelineAsync(ApiContext.TenantId(httpContext, tenantId), page, pageSize, cancellationToken);
            if (!result.IsSuccess || result.Value is null)
            {
                return Results.BadRequest(result.Error);
            }

            var lines = new[] { "Id,OccurredAtUtc,Action,EntityName,EntityId,UserId,IpAddress,CorrelationId" }
                .Concat(result.Value.Select(item => $"{item.Id},{item.OccurredAtUtc:O},{item.Action},{item.EntityName},{item.EntityId},{item.UserId},{item.IpAddress},{item.CorrelationId}"));
            return Results.Text(string.Join(Environment.NewLine, lines), "text/csv");
        })
            .RequireAuthorization(PermissionPolicies.TenantAudit);
    }

    private static void MapRbac(RouteGroupBuilder api)
    {
        var rbac = api.MapGroup("/tenants/{tenantId:guid}/rbac")
            .WithTags("RBAC")
            .RequireAuthorization(PermissionPolicies.RbacManage);

        rbac.MapPost("/roles", async (Guid tenantId, CreateRoleRequest request, HttpContext httpContext, IRbacService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateRoleAsync(
                new CreateRoleCommand(ApiContext.TenantId(httpContext, tenantId), request.Name, request.IsSystemRole, ApiContext.UserId(httpContext)),
                cancellationToken)));

        rbac.MapPost("/permissions", async (Guid tenantId, CreatePermissionRequest request, HttpContext httpContext, IRbacService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreatePermissionAsync(
                new CreatePermissionCommand(request.Module, request.Action, request.Description, ApiContext.UserId(httpContext), ApiContext.TenantId(httpContext, tenantId)),
                cancellationToken)));

        rbac.MapPost("/roles/assign", async (Guid tenantId, AssignRoleRequest request, HttpContext httpContext, IRbacService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AssignRoleAsync(
                new RbacAssignRoleCommand(ApiContext.TenantId(httpContext, tenantId), request.UserId, request.RoleId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        rbac.MapPost("/permissions/grant", async (Guid tenantId, GrantPermissionRequest request, HttpContext httpContext, IRbacService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GrantPermissionAsync(
                new RbacGrantPermissionCommand(ApiContext.TenantId(httpContext, tenantId), request.RoleId, request.PermissionId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        rbac.MapGet("/users/{userId:guid}/permissions", async (Guid tenantId, Guid userId, HttpContext httpContext, IRbacService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetUserPermissionsAsync(ApiContext.TenantId(httpContext, tenantId), userId, cancellationToken)));
    }

    private static void MapMfa(RouteGroupBuilder api)
    {
        var mfa = api.MapGroup("/tenants/{tenantId:guid}/users/{userId:guid}/mfa")
            .WithTags("MFA")
            .RequireAuthorization(PermissionPolicies.IdentityManage);

        mfa.MapPost("/setup", async (Guid tenantId, Guid userId, BeginMfaSetupRequest request, HttpContext httpContext, IMfaService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.BeginSetupAsync(
                new BeginMfaSetupCommand(ApiContext.TenantId(httpContext, tenantId), userId, request.Method, ApiContext.UserId(httpContext)),
                cancellationToken)));

        mfa.MapPost("/enable", async (Guid tenantId, Guid userId, EnableMfaRequest request, HttpContext httpContext, IMfaService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.EnableAsync(
                new EnableMfaCommand(ApiContext.TenantId(httpContext, tenantId), userId, request.Method, request.VerificationCode, ApiContext.UserId(httpContext)),
                cancellationToken)));

        mfa.MapPost("/verify", async (Guid tenantId, Guid userId, VerifyMfaRequest request, HttpContext httpContext, IMfaService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.VerifyAsync(
                new VerifyMfaCommand(ApiContext.TenantId(httpContext, tenantId), userId, request.Method, request.VerificationCode, ApiContext.IpAddress(httpContext), ApiContext.UserAgent(httpContext)),
                cancellationToken)))
            .RequireAuthorization();

        mfa.MapPost("/disable", async (Guid tenantId, Guid userId, DisableMfaRequest request, HttpContext httpContext, IMfaService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.DisableAsync(
                new DisableMfaCommand(ApiContext.TenantId(httpContext, tenantId), userId, request.Method, ApiContext.UserId(httpContext)),
                cancellationToken)));
    }

    private static void MapAudit(RouteGroupBuilder api)
    {
        api.MapPost("/tenants/{tenantId:guid}/audit/search", async (Guid tenantId, AuditSearchRequest request, HttpContext httpContext, IAuditService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.SearchAsync(
                new AuditSearchQuery(
                    ApiContext.TenantId(httpContext, tenantId),
                    new AuditQueryPrincipal(ApiContext.UserId(httpContext), tenantId, Permissions(httpContext)),
                    request.Action,
                    request.Category,
                    request.EntityName,
                    request.EntityId,
                    request.SearchText,
                    request.FromUtc,
                    request.ToUtc,
                    request.Page,
                    request.PageSize),
                cancellationToken)))
            .WithTags("Audit")
            .RequireAuthorization(PermissionPolicies.AuditRead);
    }

    private static void MapStorage(RouteGroupBuilder api)
    {
        var storage = api.MapGroup("/tenants/{tenantId:guid}/storage")
            .WithTags("Storage")
            .RequireAuthorization(PermissionPolicies.StorageManage);

        storage.MapPost("/files", async (
                Guid tenantId,
                IFormFile file,
                string ownerEntityName,
                Guid ownerEntityId,
                Guid? versionEntityId,
                HttpContext httpContext,
                IStorageFoundationService service,
                CancellationToken cancellationToken) =>
            {
                await using var stream = file.OpenReadStream();
                return ApiResult.From(await service.UploadAsync(
                    new UploadFileCommand(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), file.FileName, file.ContentType, stream, ownerEntityName, ownerEntityId, versionEntityId),
                    cancellationToken));
            })
            .DisableAntiforgery();

        storage.MapGet("/files/{storedFileId:guid}", async (Guid tenantId, Guid storedFileId, HttpContext httpContext, IStorageFoundationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetAsync(
                new GetStoredFileQuery(ApiContext.TenantId(httpContext, tenantId), storedFileId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        storage.MapPost("/files/{storedFileId:guid}/download", async (Guid tenantId, Guid storedFileId, HttpContext httpContext, IStorageFoundationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.RegisterDownloadAsync(
                new RegisterFileDownloadCommand(ApiContext.TenantId(httpContext, tenantId), storedFileId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        storage.MapPost("/files/{storedFileId:guid}/available", async (Guid tenantId, Guid storedFileId, HttpContext httpContext, IStorageFoundationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.MarkAvailableAsync(
                new ChangeStoredFileStatusCommand(ApiContext.TenantId(httpContext, tenantId), storedFileId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        storage.MapPost("/files/{storedFileId:guid}/quarantine", async (Guid tenantId, Guid storedFileId, HttpContext httpContext, IStorageFoundationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.QuarantineAsync(
                new ChangeStoredFileStatusCommand(ApiContext.TenantId(httpContext, tenantId), storedFileId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        storage.MapDelete("/files/{storedFileId:guid}", async (Guid tenantId, Guid storedFileId, HttpContext httpContext, IStorageFoundationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.DeleteAsync(
                new ChangeStoredFileStatusCommand(ApiContext.TenantId(httpContext, tenantId), storedFileId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        storage.MapGet("/providers", async (Guid tenantId, HttpContext httpContext, IStorageFoundationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ListProvidersAsync(ApiContext.TenantId(httpContext, tenantId), cancellationToken)));

        storage.MapPost("/providers", async (Guid tenantId, ConfigureStorageProviderRequest request, HttpContext httpContext, IStorageFoundationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateProviderAsync(
                new CreateStorageProviderConfigurationCommand(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), request.Provider, request.Name, request.ContainerName, request.Priority, request.IsDefault, request.IsEnabled, request.SettingsJson),
                cancellationToken)));

        storage.MapPut("/providers/{providerConfigurationId:guid}", async (Guid tenantId, Guid providerConfigurationId, ConfigureStorageProviderRequest request, HttpContext httpContext, IStorageFoundationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.UpdateProviderAsync(
                new UpdateStorageProviderConfigurationCommand(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), providerConfigurationId, request.Name, request.ContainerName, request.Priority, request.IsDefault, request.IsEnabled, request.SettingsJson),
                cancellationToken)));

        storage.MapPost("/providers/{providerConfigurationId:guid}/disable", async (Guid tenantId, Guid providerConfigurationId, HttpContext httpContext, IStorageFoundationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.DisableProviderAsync(
                new ChangeStorageProviderCommand(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), providerConfigurationId),
                cancellationToken)));

        storage.MapPost("/providers/{providerConfigurationId:guid}/activate", async (Guid tenantId, Guid providerConfigurationId, HttpContext httpContext, IStorageFoundationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.SetActiveProviderAsync(
                new ChangeStorageProviderCommand(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), providerConfigurationId),
                cancellationToken)));

        storage.MapPost("/providers/{providerConfigurationId:guid}/test", async (Guid tenantId, Guid providerConfigurationId, HttpContext httpContext, IStorageFoundationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.TestProviderAsync(
                new ChangeStorageProviderCommand(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), providerConfigurationId),
                cancellationToken)));
    }

    private static void MapNotifications(RouteGroupBuilder api)
    {
        var notifications = api.MapGroup("/tenants/{tenantId:guid}/notifications")
            .WithTags("Notifications");

        notifications.MapPost("/templates", async (Guid tenantId, CreateNotificationTemplateRequest request, HttpContext httpContext, INotificationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateTemplateAsync(
                new CreateNotificationTemplateCommand(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), request.Code, request.Channel, request.Subject, request.Body, request.TextBody, request.Locale, request.BrandingJson),
                cancellationToken))).RequireAuthorization(PermissionPolicies.NotificationTemplate);

        notifications.MapPost("/templates/preview", async (Guid tenantId, PreviewNotificationTemplateRequest request, HttpContext httpContext, INotificationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.PreviewTemplateAsync(
                new PreviewNotificationTemplateCommand(ApiContext.TenantId(httpContext, tenantId), request.Subject, request.Body, request.TextBody, request.Variables, request.Branding),
                cancellationToken))).RequireAuthorization(PermissionPolicies.NotificationTemplate);

        notifications.MapPost("/messages", async (Guid tenantId, QueueNotificationRequest request, HttpContext httpContext, INotificationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.QueueAsync(
                new QueueNotificationCommand(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), request.Channel, request.Recipient, request.Subject, request.Body, request.TemplateCode, request.Variables, request.Priority, request.TargetUserId),
                cancellationToken))).RequireAuthorization(PermissionPolicies.NotificationSend);

        notifications.MapPost("/messages/{messageId:guid}/send", async (Guid tenantId, Guid messageId, HttpContext httpContext, INotificationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.SendAsync(
                new SendNotificationCommand(ApiContext.TenantId(httpContext, tenantId), messageId, ApiContext.UserId(httpContext)),
                cancellationToken))).RequireAuthorization(PermissionPolicies.NotificationSend);

        notifications.MapPost("/messages/{messageId:guid}/retry", async (Guid tenantId, Guid messageId, HttpContext httpContext, INotificationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.RetryAsync(
                new RetryNotificationCommand(ApiContext.TenantId(httpContext, tenantId), messageId, ApiContext.UserId(httpContext)),
                cancellationToken))).RequireAuthorization(PermissionPolicies.NotificationSend);

        notifications.MapPost("/messages/{messageId:guid}/cancel", async (Guid tenantId, Guid messageId, HttpContext httpContext, INotificationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CancelAsync(
                new CancelNotificationCommand(ApiContext.TenantId(httpContext, tenantId), messageId, ApiContext.UserId(httpContext)),
                cancellationToken))).RequireAuthorization(PermissionPolicies.NotificationManage);

        notifications.MapGet("/history", async (Guid tenantId, HttpContext httpContext, INotificationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetHistoryAsync(ApiContext.TenantId(httpContext, tenantId), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.NotificationRead);

        notifications.MapGet("/tracking/dead-letters", async (Guid tenantId, HttpContext httpContext, INotificationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetDeadLettersAsync(ApiContext.TenantId(httpContext, tenantId), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.NotificationRead);

        notifications.MapGet("/dashboard", async (Guid tenantId, HttpContext httpContext, INotificationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetDashboardAsync(ApiContext.TenantId(httpContext, tenantId), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.NotificationRead);

        notifications.MapPost("/providers", async (Guid tenantId, ConfigureNotificationProviderRequest request, HttpContext httpContext, INotificationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ConfigureProviderAsync(
                new ConfigureNotificationProviderCommand(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), request.Provider, request.Name, request.Priority, request.IsDefault, request.IsEnabled),
                cancellationToken))).RequireAuthorization(PermissionPolicies.NotificationAdmin);
    }

    private static void MapDocuments(RouteGroupBuilder api)
    {
        var documents = api.MapGroup("/tenants/{tenantId:guid}/documents")
            .WithTags("Document Management")
            .RequireAuthorization(PermissionPolicies.DocumentManage);

        documents.MapPost("/types", async (Guid tenantId, CreateDocumentTypeRequest request, HttpContext httpContext, IDocumentManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateTypeAsync(
                new CreateDocumentTypeCommand(ApiContext.TenantId(httpContext, tenantId), request.Name, request.Code, request.RetentionDays, ApiContext.UserId(httpContext)),
                cancellationToken)));

        documents.MapPost("/categories", async (Guid tenantId, CreateDocumentCategoryRequest request, HttpContext httpContext, IDocumentManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateCategoryAsync(
                new CreateDocumentCategoryCommand(ApiContext.TenantId(httpContext, tenantId), request.Name, request.Code, ApiContext.UserId(httpContext)),
                cancellationToken)));

        documents.MapPost("/", async (Guid tenantId, CreateDocumentRequest request, HttpContext httpContext, IDocumentManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateDocumentAsync(
                new CreateDocumentCommand(ApiContext.TenantId(httpContext, tenantId), request.DocumentTypeId, request.CategoryId, request.Title, request.Code, ApiContext.UserId(httpContext)),
                cancellationToken)));

        documents.MapPost("/{documentId:guid}/versions", async (Guid tenantId, Guid documentId, AddDocumentVersionRequest request, HttpContext httpContext, IDocumentManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddVersionAsync(
                new AddDocumentVersionCommand(ApiContext.TenantId(httpContext, tenantId), documentId, request.StoredFileId, request.ChangeSummary, ApiContext.UserId(httpContext)),
                cancellationToken)));

        documents.MapPost("/{documentId:guid}/submit", async (Guid tenantId, Guid documentId, HttpContext httpContext, IDocumentManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.SubmitForReviewAsync(
                new DocumentActionCommand(ApiContext.TenantId(httpContext, tenantId), documentId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        documents.MapPost("/{documentId:guid}/decision", async (Guid tenantId, Guid documentId, DecideDocumentRequest request, HttpContext httpContext, IDocumentManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.DecideAsync(
                new DecideDocumentCommand(ApiContext.TenantId(httpContext, tenantId), documentId, request.Decision, request.Comments, ApiContext.UserId(httpContext)),
                cancellationToken)));

        documents.MapPost("/{documentId:guid}/obsolete", async (Guid tenantId, Guid documentId, HttpContext httpContext, IDocumentManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.MarkObsoleteAsync(
                new DocumentActionCommand(ApiContext.TenantId(httpContext, tenantId), documentId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        documents.MapPost("/{documentId:guid}/permissions", async (Guid tenantId, Guid documentId, GrantDocumentPermissionRequest request, HttpContext httpContext, IDocumentManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GrantPermissionAsync(
                new GrantDocumentPermissionCommand(ApiContext.TenantId(httpContext, tenantId), documentId, request.PrincipalId, request.Level, ApiContext.UserId(httpContext)),
                cancellationToken)));

        documents.MapGet("/", async (
                Guid tenantId,
                string? searchText,
                DocumentStatus? status,
                Guid? documentTypeId,
                Guid? categoryId,
                int page,
                int pageSize,
                HttpContext httpContext,
                IDocumentManagementService service,
                CancellationToken cancellationToken) =>
            ApiResult.From(await service.SearchAsync(
                new DocumentSearchQuery(ApiContext.TenantId(httpContext, tenantId), searchText, status, documentTypeId, categoryId, page, pageSize),
                cancellationToken)));
    }

    private static void MapWorkflows(RouteGroupBuilder api)
    {
        var workflows = api.MapGroup("/tenants/{tenantId:guid}/workflows")
            .WithTags("Workflow Engine")
            .RequireAuthorization(PermissionPolicies.WorkflowManage);

        workflows.MapPost("/", async (Guid tenantId, CreateWorkflowRequest request, HttpContext httpContext, IWorkflowEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateWorkflowAsync(
                new CreateWorkflowCommand(ApiContext.TenantId(httpContext, tenantId), request.Name, request.Code, request.EntityName, ApiContext.UserId(httpContext)),
                cancellationToken)));

        workflows.MapPost("/{workflowId:guid}/steps", async (Guid tenantId, Guid workflowId, AddWorkflowStepRequest request, HttpContext httpContext, IWorkflowEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddStepAsync(
                new AddWorkflowStepCommand(ApiContext.TenantId(httpContext, tenantId), workflowId, request.Name, request.Type, request.Sequence, request.SlaHours, request.AssignedRoleId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        workflows.MapPost("/{workflowId:guid}/transitions", async (Guid tenantId, Guid workflowId, AddWorkflowTransitionRequest request, HttpContext httpContext, IWorkflowEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddTransitionAsync(
                new AddWorkflowTransitionCommand(ApiContext.TenantId(httpContext, tenantId), workflowId, request.FromStepId, request.ToStepId, request.Decision, ApiContext.UserId(httpContext)),
                cancellationToken)));

        workflows.MapPost("/{workflowId:guid}/rules", async (Guid tenantId, Guid workflowId, AddWorkflowRuleRequest request, HttpContext httpContext, IWorkflowEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddRuleAsync(
                new AddWorkflowRuleCommand(ApiContext.TenantId(httpContext, tenantId), workflowId, request.FieldName, request.Operator, request.ExpectedValue, ApiContext.UserId(httpContext)),
                cancellationToken)));

        workflows.MapPost("/{workflowId:guid}/activate", async (Guid tenantId, Guid workflowId, HttpContext httpContext, IWorkflowEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ActivateAsync(
                new WorkflowActionCommand(ApiContext.TenantId(httpContext, tenantId), workflowId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        workflows.MapPost("/{workflowId:guid}/instances", async (Guid tenantId, Guid workflowId, StartWorkflowRequest request, HttpContext httpContext, IWorkflowEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.StartAsync(
                new StartWorkflowCommand(ApiContext.TenantId(httpContext, tenantId), workflowId, request.EntityName, request.EntityId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        workflows.MapPost("/instances/{workflowInstanceId:guid}/assignments", async (Guid tenantId, Guid workflowInstanceId, AssignWorkflowRequest request, HttpContext httpContext, IWorkflowEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AssignAsync(
                new AssignWorkflowCommand(ApiContext.TenantId(httpContext, tenantId), workflowInstanceId, request.StepId, request.AssignedToUserId, request.DueAtUtc, ApiContext.UserId(httpContext)),
                cancellationToken)));

        workflows.MapPost("/instances/{workflowInstanceId:guid}/complete", async (Guid tenantId, Guid workflowInstanceId, CompleteWorkflowAssignmentRequest request, HttpContext httpContext, IWorkflowEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CompleteAssignmentAsync(
                new CompleteWorkflowAssignmentCommand(ApiContext.TenantId(httpContext, tenantId), workflowInstanceId, request.AssignmentId, request.Decision, ApiContext.UserId(httpContext)),
                cancellationToken)));

        workflows.MapPost("/instances/{workflowInstanceId:guid}/escalate", async (Guid tenantId, Guid workflowInstanceId, EscalateWorkflowRequest request, HttpContext httpContext, IWorkflowEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.EscalateAsync(
                new EscalateWorkflowCommand(ApiContext.TenantId(httpContext, tenantId), workflowInstanceId, request.AssignmentId, request.EscalatedToUserId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        workflows.MapPost("/instances/{workflowInstanceId:guid}/reminders", async (Guid tenantId, Guid workflowInstanceId, HttpContext httpContext, IWorkflowEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.QueueReminderAsync(
                new WorkflowInstanceActionCommand(ApiContext.TenantId(httpContext, tenantId), workflowInstanceId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        workflows.MapGet("/instances", async (
                Guid tenantId,
                Guid? workflowId,
                WorkflowInstanceStatus? status,
                string? entityName,
                Guid? entityId,
                int page,
                int pageSize,
                HttpContext httpContext,
                IWorkflowEngineService service,
                CancellationToken cancellationToken) =>
            ApiResult.From(await service.SearchInstancesAsync(
                new WorkflowInstanceSearchQuery(ApiContext.TenantId(httpContext, tenantId), workflowId, status, entityName, entityId, page, pageSize),
                cancellationToken)));
    }

    private static void MapTechnicalSheets(RouteGroupBuilder api)
    {
        var technicalSheets = api.MapGroup("/tenants/{tenantId:guid}/technical-sheets")
            .WithTags("Technical Sheets")
            .RequireAuthorization(PermissionPolicies.TechnicalSheetManage);

        technicalSheets.MapPost("/products", async (Guid tenantId, CreateProductRequest request, HttpContext httpContext, ITechnicalSheetService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateProductAsync(
                new CreateProductCommand(ApiContext.TenantId(httpContext, tenantId), request.Name, request.Sku, request.Description, ApiContext.UserId(httpContext)),
                cancellationToken)));

        technicalSheets.MapPost("/", async (Guid tenantId, CreateTechnicalSheetRequest request, HttpContext httpContext, ITechnicalSheetService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateSheetAsync(
                new CreateTechnicalSheetCommand(ApiContext.TenantId(httpContext, tenantId), request.ProductId, request.Title, ApiContext.UserId(httpContext)),
                cancellationToken)));

        technicalSheets.MapPost("/{technicalSheetId:guid}/versions", async (Guid tenantId, Guid technicalSheetId, CreateTechnicalSheetVersionRequest request, HttpContext httpContext, ITechnicalSheetService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateVersionAsync(
                new CreateTechnicalSheetVersionCommand(ApiContext.TenantId(httpContext, tenantId), technicalSheetId, request.ChangeSummary, ApiContext.UserId(httpContext)),
                cancellationToken)));

        technicalSheets.MapPost("/{technicalSheetId:guid}/ingredients", async (Guid tenantId, Guid technicalSheetId, AddTechnicalSheetIngredientRequest request, HttpContext httpContext, ITechnicalSheetService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddIngredientAsync(
                new AddIngredientCommand(ApiContext.TenantId(httpContext, tenantId), technicalSheetId, request.Name, request.Percentage, request.Allergen, ApiContext.UserId(httpContext)),
                cancellationToken)));

        technicalSheets.MapPost("/{technicalSheetId:guid}/nutrients", async (Guid tenantId, Guid technicalSheetId, AddTechnicalSheetNutrientRequest request, HttpContext httpContext, ITechnicalSheetService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddNutrientAsync(
                new AddNutrientCommand(ApiContext.TenantId(httpContext, tenantId), technicalSheetId, request.Name, request.Amount, request.Unit, ApiContext.UserId(httpContext)),
                cancellationToken)));

        technicalSheets.MapPost("/{technicalSheetId:guid}/certifications", async (Guid tenantId, Guid technicalSheetId, AddTechnicalSheetCertificationRequest request, HttpContext httpContext, ITechnicalSheetService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddCertificationAsync(
                new AddCertificationCommand(ApiContext.TenantId(httpContext, tenantId), technicalSheetId, request.Name, request.Issuer, request.ExpiresAtUtc, ApiContext.UserId(httpContext)),
                cancellationToken)));

        technicalSheets.MapPost("/{technicalSheetId:guid}/submit", async (Guid tenantId, Guid technicalSheetId, HttpContext httpContext, ITechnicalSheetService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.SubmitAsync(
                new TechnicalSheetActionCommand(ApiContext.TenantId(httpContext, tenantId), technicalSheetId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        technicalSheets.MapPost("/{technicalSheetId:guid}/decision", async (Guid tenantId, Guid technicalSheetId, DecideTechnicalSheetRequest request, HttpContext httpContext, ITechnicalSheetService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.DecideAsync(
                new DecideTechnicalSheetCommand(ApiContext.TenantId(httpContext, tenantId), technicalSheetId, request.Decision, request.Comments, ApiContext.UserId(httpContext)),
                cancellationToken)));

        technicalSheets.MapPost("/{technicalSheetId:guid}/pdf", async (Guid tenantId, Guid technicalSheetId, AttachTechnicalSheetPdfRequest request, HttpContext httpContext, ITechnicalSheetService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AttachPdfAsync(
                new AttachTechnicalSheetPdfCommand(ApiContext.TenantId(httpContext, tenantId), technicalSheetId, request.PdfObjectKey, ApiContext.UserId(httpContext)),
                cancellationToken)));

        technicalSheets.MapPost("/{technicalSheetId:guid}/obsolete", async (Guid tenantId, Guid technicalSheetId, HttpContext httpContext, ITechnicalSheetService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.MarkObsoleteAsync(
                new TechnicalSheetActionCommand(ApiContext.TenantId(httpContext, tenantId), technicalSheetId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        technicalSheets.MapGet("/", async (
                Guid tenantId,
                string? searchText,
                TechnicalSheetStatus? status,
                Guid? productId,
                int page,
                int pageSize,
                HttpContext httpContext,
                ITechnicalSheetService service,
                CancellationToken cancellationToken) =>
            ApiResult.From(await service.SearchAsync(
                new TechnicalSheetSearchQuery(ApiContext.TenantId(httpContext, tenantId), searchText, status, productId, page, pageSize),
                cancellationToken)));
    }

    private static void MapSuppliers(RouteGroupBuilder api)
    {
        var suppliers = api.MapGroup("/tenants/{tenantId:guid}/suppliers")
            .WithTags("Supplier Management")
            .RequireAuthorization(PermissionPolicies.SupplierManage);

        suppliers.MapPost("/", async (Guid tenantId, CreateSupplierRequest request, HttpContext httpContext, ISupplierManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateSupplierAsync(
                new CreateSupplierCommand(ApiContext.TenantId(httpContext, tenantId), request.LegalName, request.TaxIdentifier, request.CountryCode, ApiContext.UserId(httpContext)),
                cancellationToken)));

        suppliers.MapPost("/{supplierId:guid}/documents", async (Guid tenantId, Guid supplierId, AddSupplierDocumentRequest request, HttpContext httpContext, ISupplierManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddDocumentAsync(
                new AddSupplierDocumentCommand(ApiContext.TenantId(httpContext, tenantId), supplierId, request.Type, request.DocumentNumber, request.StoredFileId, request.IssuedAtUtc, request.ExpiresAtUtc, ApiContext.UserId(httpContext)),
                cancellationToken)));

        suppliers.MapPost("/{supplierId:guid}/documents/{supplierDocumentId:guid}/validate", async (Guid tenantId, Guid supplierId, Guid supplierDocumentId, HttpContext httpContext, ISupplierManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ValidateDocumentAsync(
                new ReviewSupplierDocumentCommand(ApiContext.TenantId(httpContext, tenantId), supplierId, supplierDocumentId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        suppliers.MapPost("/{supplierId:guid}/documents/{supplierDocumentId:guid}/reject", async (Guid tenantId, Guid supplierId, Guid supplierDocumentId, RejectSupplierDocumentRequest request, HttpContext httpContext, ISupplierManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.RejectDocumentAsync(
                new RejectSupplierDocumentCommand(ApiContext.TenantId(httpContext, tenantId), supplierId, supplierDocumentId, request.Reason, ApiContext.UserId(httpContext)),
                cancellationToken)));

        suppliers.MapPost("/{supplierId:guid}/evaluations", async (Guid tenantId, Guid supplierId, AddSupplierEvaluationRequest request, HttpContext httpContext, ISupplierManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddEvaluationAsync(
                new AddSupplierEvaluationCommand(ApiContext.TenantId(httpContext, tenantId), supplierId, request.Score, request.Comments, ApiContext.UserId(httpContext)),
                cancellationToken)));

        suppliers.MapPost("/{supplierId:guid}/homologate", async (Guid tenantId, Guid supplierId, HttpContext httpContext, ISupplierManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.HomologateAsync(
                new SupplierActionCommand(ApiContext.TenantId(httpContext, tenantId), supplierId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        suppliers.MapPost("/{supplierId:guid}/documents/{supplierDocumentId:guid}/alerts", async (Guid tenantId, Guid supplierId, Guid supplierDocumentId, HttpContext httpContext, ISupplierManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateExpirationAlertAsync(
                new CreateSupplierExpirationAlertCommand(ApiContext.TenantId(httpContext, tenantId), supplierId, supplierDocumentId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        suppliers.MapPost("/{supplierId:guid}/suspend", async (Guid tenantId, Guid supplierId, SuspendSupplierRequest request, HttpContext httpContext, ISupplierManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.SuspendAsync(
                new SuspendSupplierCommand(ApiContext.TenantId(httpContext, tenantId), supplierId, request.Reason, ApiContext.UserId(httpContext)),
                cancellationToken)));

        suppliers.MapGet("/", async (
                Guid tenantId,
                string? searchText,
                SupplierStatus? status,
                int page,
                int pageSize,
                HttpContext httpContext,
                ISupplierManagementService service,
                CancellationToken cancellationToken) =>
            ApiResult.From(await service.SearchAsync(
                new SupplierSearchQuery(ApiContext.TenantId(httpContext, tenantId), searchText, status, page, pageSize),
                cancellationToken)));
    }

    private static void MapAuditManagement(RouteGroupBuilder api)
    {
        var audits = api.MapGroup("/tenants/{tenantId:guid}/audit-management")
            .WithTags("Audit Management")
            .RequireAuthorization(PermissionPolicies.AuditManagementManage);

        audits.MapPost("/programs", async (Guid tenantId, CreateAuditProgramRequest request, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateProgramAsync(
                new CreateAuditProgramCommand(ApiContext.TenantId(httpContext, tenantId), request.Name, request.Code, request.Year, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/checklists", async (Guid tenantId, CreateAuditChecklistRequest request, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateChecklistAsync(
                new CreateAuditChecklistCommand(ApiContext.TenantId(httpContext, tenantId), request.Name, request.Code, request.Type, request.Version, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/checklists/{checklistId:guid}/items", async (Guid tenantId, Guid checklistId, AddAuditChecklistItemRequest request, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddChecklistItemAsync(
                new AddAuditChecklistItemCommand(ApiContext.TenantId(httpContext, tenantId), checklistId, request.Clause, request.Question, request.Weight, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/plans", async (Guid tenantId, CreateAuditPlanRequest request, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreatePlanAsync(
                new CreateAuditPlanCommand(ApiContext.TenantId(httpContext, tenantId), request.AuditProgramId, request.Scope, request.Criteria, request.PlannedStartUtc, request.PlannedEndUtc, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/", async (Guid tenantId, CreateManagedAuditRequest request, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateAuditAsync(
                new CreateManagedAuditCommand(ApiContext.TenantId(httpContext, tenantId), request.AuditProgramId, request.AuditPlanId, request.Title, request.Code, request.Type, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/{auditId:guid}/checklist", async (Guid tenantId, Guid auditId, AssignAuditChecklistRequest request, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AssignChecklistAsync(
                new AssignAuditChecklistCommand(ApiContext.TenantId(httpContext, tenantId), auditId, request.ChecklistId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/{auditId:guid}/schedule", async (Guid tenantId, Guid auditId, ScheduleAuditRequest request, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ScheduleAsync(
                new ScheduleAuditCommand(ApiContext.TenantId(httpContext, tenantId), auditId, request.StartUtc, request.EndUtc, request.Location, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/{auditId:guid}/participants", async (Guid tenantId, Guid auditId, AddAuditParticipantRequest request, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddParticipantAsync(
                new AddAuditParticipantCommand(ApiContext.TenantId(httpContext, tenantId), auditId, request.UserId, request.Role, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/{auditId:guid}/areas", async (Guid tenantId, Guid auditId, AddAuditAreaRequest request, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddAreaAsync(
                new AddAuditAreaCommand(ApiContext.TenantId(httpContext, tenantId), auditId, request.Name, request.Process, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/{auditId:guid}/start", async (Guid tenantId, Guid auditId, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.StartAsync(
                new ManagedAuditActionCommand(ApiContext.TenantId(httpContext, tenantId), auditId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/{auditId:guid}/findings", async (Guid tenantId, Guid auditId, AddAuditFindingRequest request, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddFindingAsync(
                new AddAuditFindingCommand(ApiContext.TenantId(httpContext, tenantId), auditId, request.Title, request.Description, request.Severity, request.ChecklistItemId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/{auditId:guid}/evidence", async (Guid tenantId, Guid auditId, AddAuditEvidenceRequest request, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddEvidenceAsync(
                new AddAuditEvidenceCommand(ApiContext.TenantId(httpContext, tenantId), auditId, request.FindingId, request.Type, request.StoredFileId, request.FileName, request.ContentType, request.SizeBytes, request.Sha256Hash, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/{auditId:guid}/observations", async (Guid tenantId, Guid auditId, AddAuditObservationRequest request, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddObservationAsync(
                new AddAuditObservationCommand(ApiContext.TenantId(httpContext, tenantId), auditId, request.Description, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/{auditId:guid}/non-conformities", async (Guid tenantId, Guid auditId, AddAuditNonConformityRequest request, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddNonConformityAsync(
                new AddAuditNonConformityCommand(ApiContext.TenantId(httpContext, tenantId), auditId, request.FindingId, request.Requirement, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/{auditId:guid}/recommendations", async (Guid tenantId, Guid auditId, AddAuditRecommendationRequest request, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddRecommendationAsync(
                new AddAuditRecommendationCommand(ApiContext.TenantId(httpContext, tenantId), auditId, request.FindingId, request.Recommendation, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/{auditId:guid}/corrective-actions", async (Guid tenantId, Guid auditId, LinkAuditCorrectiveActionRequest request, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.LinkCorrectiveActionAsync(
                new LinkAuditCorrectiveActionCommand(ApiContext.TenantId(httpContext, tenantId), auditId, request.FindingId, request.CorrectiveActionId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/{auditId:guid}/attachments", async (Guid tenantId, Guid auditId, AddAuditAttachmentRequest request, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddAttachmentAsync(
                new AddAuditAttachmentCommand(ApiContext.TenantId(httpContext, tenantId), auditId, request.StoredFileId, request.FileName, request.ContentType, request.SizeBytes, request.Sha256Hash, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/{auditId:guid}/complete", async (Guid tenantId, Guid auditId, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CompleteAsync(
                new ManagedAuditActionCommand(ApiContext.TenantId(httpContext, tenantId), auditId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/{auditId:guid}/close", async (Guid tenantId, Guid auditId, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CloseAsync(
                new ManagedAuditActionCommand(ApiContext.TenantId(httpContext, tenantId), auditId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapPost("/{auditId:guid}/reopen", async (Guid tenantId, Guid auditId, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ReopenAsync(
                new ManagedAuditActionCommand(ApiContext.TenantId(httpContext, tenantId), auditId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        audits.MapGet("/", async (
                Guid tenantId,
                string? searchText,
                ManagedAuditType? type,
                ManagedAuditStatus? status,
                int page,
                int pageSize,
                HttpContext httpContext,
                IAuditManagementService service,
                CancellationToken cancellationToken) =>
            ApiResult.From(await service.SearchAsync(
                new ManagedAuditSearchQuery(ApiContext.TenantId(httpContext, tenantId), searchText, type, status, page, pageSize),
                cancellationToken)));

        audits.MapGet("/dashboard", async (Guid tenantId, HttpContext httpContext, IAuditManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetDashboardAsync(ApiContext.TenantId(httpContext, tenantId), cancellationToken)));

        audits.MapPost("/export", async (
                Guid tenantId,
                ManagedAuditType? type,
                ManagedAuditStatus? status,
                string? format,
                HttpContext httpContext,
                IAuditManagementService service,
                CancellationToken cancellationToken) =>
            ApiResult.From(await service.ExportAsync(
                new ManagedAuditExportQuery(ApiContext.TenantId(httpContext, tenantId), type, status, format ?? "csv", ApiContext.UserId(httpContext)),
                cancellationToken)));
    }

    private static void MapCapaManagement(RouteGroupBuilder api)
    {
        var capas = api.MapGroup("/tenants/{tenantId:guid}/capas")
            .WithTags("CAPA Management");

        capas.MapPost("/", async (Guid tenantId, CreateCapaRequest request, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateAsync(
                new CreateCapaCommand(ApiContext.TenantId(httpContext, tenantId), request.Title, request.Code, request.Description, request.Priority, request.RiskLevel, request.SourceType, request.SourceEntityId, request.SupplierId, request.DocumentId, request.AuditId, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaManage);

        capas.MapPost("/{capaId:guid}/classify", async (Guid tenantId, Guid capaId, ClassifyCapaRequest request, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ClassifyAsync(
                new ClassifyCapaCommand(ApiContext.TenantId(httpContext, tenantId), capaId, request.Priority, request.RiskLevel, request.CommitmentDueAtUtc, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaManage);

        capas.MapPost("/{capaId:guid}/owners", async (Guid tenantId, Guid capaId, AssignCapaOwnerRequest request, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AssignOwnerAsync(
                new AssignCapaOwnerCommand(ApiContext.TenantId(httpContext, tenantId), capaId, request.OwnerUserId, request.DueAtUtc, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaManage);

        capas.MapPost("/{capaId:guid}/approvers", async (Guid tenantId, Guid capaId, AddCapaApproverRequest request, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddApproverAsync(
                new AddCapaApproverCommand(ApiContext.TenantId(httpContext, tenantId), capaId, request.ApproverUserId, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaApprove);

        capas.MapPost("/{capaId:guid}/root-cause", async (Guid tenantId, Guid capaId, DefineCapaRootCauseRequest request, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.DefineRootCauseAsync(
                new DefineCapaRootCauseCommand(ApiContext.TenantId(httpContext, tenantId), capaId, request.Description, request.Method, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaManage);

        capas.MapPost("/{capaId:guid}/analysis/5-why", async (Guid tenantId, Guid capaId, AddCapaFiveWhyRequest request, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddFiveWhyAsync(
                new AddCapaFiveWhyCommand(ApiContext.TenantId(httpContext, tenantId), capaId, request.Why1, request.Why2, request.Why3, request.Why4, request.Why5, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaManage);

        capas.MapPost("/{capaId:guid}/analysis/ishikawa", async (Guid tenantId, Guid capaId, AddCapaIshikawaRequest request, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddIshikawaAsync(
                new AddCapaIshikawaCommand(ApiContext.TenantId(httpContext, tenantId), capaId, request.People, request.Process, request.Equipment, request.Material, request.Environment, request.Measurement, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaManage);

        capas.MapPost("/{capaId:guid}/containment-actions", async (Guid tenantId, Guid capaId, AddCapaActionRequest request, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddContainmentActionAsync(
                new AddCapaActionCommand(ApiContext.TenantId(httpContext, tenantId), capaId, request.Description, request.ResponsibleUserId, request.DueAtUtc, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaManage);

        capas.MapPost("/{capaId:guid}/corrective-actions", async (Guid tenantId, Guid capaId, AddCapaActionRequest request, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddCorrectiveActionAsync(
                new AddCapaActionCommand(ApiContext.TenantId(httpContext, tenantId), capaId, request.Description, request.ResponsibleUserId, request.DueAtUtc, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaManage);

        capas.MapPost("/{capaId:guid}/preventive-actions", async (Guid tenantId, Guid capaId, AddCapaActionRequest request, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddPreventiveActionAsync(
                new AddCapaActionCommand(ApiContext.TenantId(httpContext, tenantId), capaId, request.Description, request.ResponsibleUserId, request.DueAtUtc, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaManage);

        capas.MapPost("/{capaId:guid}/evidence", async (Guid tenantId, Guid capaId, AddCapaEvidenceRequest request, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddEvidenceAsync(
                new AddCapaEvidenceCommand(ApiContext.TenantId(httpContext, tenantId), capaId, request.StoredFileId, request.FileName, request.ContentType, request.SizeBytes, request.Sha256Hash, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaManage);

        capas.MapPost("/{capaId:guid}/attachments", async (Guid tenantId, Guid capaId, AddCapaAttachmentRequest request, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddAttachmentAsync(
                new AddCapaAttachmentCommand(ApiContext.TenantId(httpContext, tenantId), capaId, request.StoredFileId, request.FileName, request.ContentType, request.SizeBytes, request.Sha256Hash, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaManage);

        capas.MapPost("/{capaId:guid}/follow-up", async (Guid tenantId, Guid capaId, RegisterCapaFollowUpRequest request, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.RegisterFollowUpAsync(
                new CapaFollowUpCommand(ApiContext.TenantId(httpContext, tenantId), capaId, request.Notes, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaManage);

        capas.MapPost("/{capaId:guid}/escalate-overdue", async (Guid tenantId, Guid capaId, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.EscalateOverdueAsync(
                new CapaActionCommand(ApiContext.TenantId(httpContext, tenantId), capaId, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaManage);

        capas.MapPost("/{capaId:guid}/effectiveness", async (Guid tenantId, Guid capaId, VerifyCapaEffectivenessRequest request, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.VerifyEffectivenessAsync(
                new VerifyCapaEffectivenessCommand(ApiContext.TenantId(httpContext, tenantId), capaId, request.IsEffective, request.VerificationSummary, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaManage);

        capas.MapPost("/{capaId:guid}/workflow", async (Guid tenantId, Guid capaId, AttachCapaWorkflowRequest request, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AttachWorkflowAsync(
                new AttachCapaWorkflowCommand(ApiContext.TenantId(httpContext, tenantId), capaId, request.WorkflowInstanceId, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.WorkflowManage);

        capas.MapPost("/{capaId:guid}/approve-closure", async (Guid tenantId, Guid capaId, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ApproveClosureAsync(
                new CapaActionCommand(ApiContext.TenantId(httpContext, tenantId), capaId, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaClose);

        capas.MapPost("/{capaId:guid}/reopen", async (Guid tenantId, Guid capaId, ReopenCapaRequest request, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ReopenAsync(
                new ReopenCapaCommand(ApiContext.TenantId(httpContext, tenantId), capaId, request.Reason, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaManage);

        capas.MapGet("/", async (
                Guid tenantId,
                string? searchText,
                CapaStatus? status,
                CapaPriority? priority,
                CapaRiskLevel? riskLevel,
                Guid? ownerUserId,
                Guid? supplierId,
                Guid? auditId,
                int page,
                int pageSize,
                HttpContext httpContext,
                ICapaManagementService service,
                CancellationToken cancellationToken) =>
            ApiResult.From(await service.SearchAsync(
                new CapaSearchQuery(ApiContext.TenantId(httpContext, tenantId), searchText, status, priority, riskLevel, ownerUserId, supplierId, auditId, page, pageSize),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaRead);

        capas.MapGet("/dashboard", async (Guid tenantId, HttpContext httpContext, ICapaManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetDashboardAsync(ApiContext.TenantId(httpContext, tenantId), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaRead);

        capas.MapPost("/export", async (
                Guid tenantId,
                CapaStatus? status,
                CapaPriority? priority,
                CapaRiskLevel? riskLevel,
                string? format,
                HttpContext httpContext,
                ICapaManagementService service,
                CancellationToken cancellationToken) =>
            ApiResult.From(await service.ExportAsync(
                new CapaExportQuery(ApiContext.TenantId(httpContext, tenantId), status, priority, riskLevel, format ?? "csv", ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.CapaRead);
    }

    private static void MapRiskManagement(RouteGroupBuilder api)
    {
        var risks = api.MapGroup("/tenants/{tenantId:guid}/risks")
            .WithTags("Risk Management");

        risks.MapPost("/categories", async (Guid tenantId, CreateRiskCategoryRequest request, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateCategoryAsync(new CreateRiskCategoryCommand(ApiContext.TenantId(httpContext, tenantId), request.Name, request.Code, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskManage);

        risks.MapPost("/matrices", async (Guid tenantId, CreateRiskMatrixRequest request, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateMatrixAsync(new CreateRiskMatrixCommand(ApiContext.TenantId(httpContext, tenantId), request.Name, request.ToleranceScore, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskManage);

        risks.MapPost("/", async (Guid tenantId, CreateRiskRequest request, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateRiskAsync(new CreateRiskCommand(ApiContext.TenantId(httpContext, tenantId), request.CategoryId, request.Title, request.Code, request.Description, request.Type, request.Area, request.Process, request.SupplierId, request.DocumentId, request.AuditId, request.CapaId, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskManage);

        risks.MapPost("/{riskId:guid}/classify", async (Guid tenantId, Guid riskId, ClassifyRiskRequest request, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ClassifyAsync(new ClassifyRiskCommand(ApiContext.TenantId(httpContext, tenantId), riskId, request.Type, request.Area, request.Process, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskManage);

        risks.MapPost("/{riskId:guid}/owners", async (Guid tenantId, Guid riskId, AssignRiskOwnerRequest request, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AssignOwnerAsync(new AssignRiskOwnerCommand(ApiContext.TenantId(httpContext, tenantId), riskId, request.OwnerUserId, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskManage);

        risks.MapPost("/{riskId:guid}/assessments", async (Guid tenantId, Guid riskId, AssessRiskRequest request, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AssessAsync(new AssessRiskCommand(ApiContext.TenantId(httpContext, tenantId), riskId, request.Probability, request.Impact, request.ResidualProbability, request.ResidualImpact, request.ToleranceScore, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskManage);

        risks.MapPost("/{riskId:guid}/treatments", async (Guid tenantId, Guid riskId, AddRiskTreatmentRequest request, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddTreatmentAsync(new AddRiskTreatmentCommand(ApiContext.TenantId(httpContext, tenantId), riskId, request.Strategy, request.Rationale, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskApprove);

        risks.MapPost("/{riskId:guid}/mitigation-plans", async (Guid tenantId, Guid riskId, AddRiskMitigationPlanRequest request, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddMitigationPlanAsync(new AddRiskMitigationPlanCommand(ApiContext.TenantId(httpContext, tenantId), riskId, request.Description, request.ResponsibleUserId, request.DueAtUtc, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskManage);

        risks.MapPost("/{riskId:guid}/controls", async (Guid tenantId, Guid riskId, AddRiskControlRequest request, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddControlAsync(new AddRiskControlCommand(ApiContext.TenantId(httpContext, tenantId), riskId, request.Name, request.Type, request.Description, request.IsEffective, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskManage);

        risks.MapPost("/{riskId:guid}/evidence", async (Guid tenantId, Guid riskId, AddRiskEvidenceRequest request, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddEvidenceAsync(new AddRiskEvidenceCommand(ApiContext.TenantId(httpContext, tenantId), riskId, request.StoredFileId, request.FileName, request.ContentType, request.SizeBytes, request.Sha256Hash, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskManage);

        risks.MapPost("/{riskId:guid}/attachments", async (Guid tenantId, Guid riskId, AddRiskAttachmentRequest request, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddAttachmentAsync(new AddRiskAttachmentCommand(ApiContext.TenantId(httpContext, tenantId), riskId, request.StoredFileId, request.FileName, request.ContentType, request.SizeBytes, request.Sha256Hash, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskManage);

        risks.MapPost("/{riskId:guid}/reviews", async (Guid tenantId, Guid riskId, ScheduleRiskReviewRequest request, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ScheduleReviewAsync(new ScheduleRiskReviewCommand(ApiContext.TenantId(httpContext, tenantId), riskId, request.DueAtUtc, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskManage);

        risks.MapPost("/{riskId:guid}/reviews/complete", async (Guid tenantId, Guid riskId, CompleteRiskReviewRequest request, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CompleteReviewAsync(new CompleteRiskReviewCommand(ApiContext.TenantId(httpContext, tenantId), riskId, request.ReviewId, request.Summary, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskManage);

        risks.MapPost("/{riskId:guid}/indicators", async (Guid tenantId, Guid riskId, AddRiskIndicatorRequest request, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddIndicatorAsync(new AddRiskIndicatorCommand(ApiContext.TenantId(httpContext, tenantId), riskId, request.Name, request.Value, request.Threshold, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskManage);

        risks.MapPost("/{riskId:guid}/escalate-critical", async (Guid tenantId, Guid riskId, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.EscalateCriticalAsync(new RiskActionCommand(ApiContext.TenantId(httpContext, tenantId), riskId, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskApprove);

        risks.MapPost("/{riskId:guid}/workflow", async (Guid tenantId, Guid riskId, AttachRiskWorkflowRequest request, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AttachWorkflowAsync(new AttachRiskWorkflowCommand(ApiContext.TenantId(httpContext, tenantId), riskId, request.WorkflowInstanceId, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.WorkflowManage);

        risks.MapPost("/{riskId:guid}/close", async (Guid tenantId, Guid riskId, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CloseAsync(new RiskActionCommand(ApiContext.TenantId(httpContext, tenantId), riskId, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskClose);

        risks.MapPost("/{riskId:guid}/reopen", async (Guid tenantId, Guid riskId, ReopenRiskRequest request, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ReopenAsync(new ReopenRiskCommand(ApiContext.TenantId(httpContext, tenantId), riskId, request.Reason, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskManage);

        risks.MapGet("/", async (Guid tenantId, string? searchText, RiskStatus? status, RiskType? type, RiskLevel? level, string? area, Guid? supplierId, Guid? auditId, Guid? capaId, int page, int pageSize, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.SearchAsync(new RiskSearchQuery(ApiContext.TenantId(httpContext, tenantId), searchText, status, type, level, area, supplierId, auditId, capaId, page, pageSize), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskRead);

        risks.MapGet("/dashboard", async (Guid tenantId, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetDashboardAsync(ApiContext.TenantId(httpContext, tenantId), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskRead);

        risks.MapGet("/heat-map", async (Guid tenantId, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetHeatMapAsync(ApiContext.TenantId(httpContext, tenantId), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskRead);

        risks.MapPost("/export", async (Guid tenantId, RiskStatus? status, RiskType? type, RiskLevel? level, string? format, HttpContext httpContext, IRiskManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ExportAsync(new RiskExportQuery(ApiContext.TenantId(httpContext, tenantId), status, type, level, format ?? "csv", ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.RiskRead);
    }

    private static void MapQualityIndicators(RouteGroupBuilder api)
    {
        var indicators = api.MapGroup("/tenants/{tenantId:guid}/indicators")
            .WithTags("Quality Indicators");

        indicators.MapPost("/categories", async (Guid tenantId, CreateIndicatorCategoryRequest request, HttpContext httpContext, IQualityIndicatorService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateCategoryAsync(new CreateIndicatorCategoryCommand(ApiContext.TenantId(httpContext, tenantId), request.Name, request.Code, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.IndicatorManage);

        indicators.MapPost("/", async (Guid tenantId, CreateQualityIndicatorRequest request, HttpContext httpContext, IQualityIndicatorService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateIndicatorAsync(new CreateQualityIndicatorCommand(ApiContext.TenantId(httpContext, tenantId), request.CategoryId, request.Name, request.Code, request.Description, request.Type, request.Frequency, request.CalculationType, request.Unit, request.SupplierId, request.AuditId, request.CapaId, request.RiskId, request.DocumentId, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.IndicatorManage);

        indicators.MapPost("/{indicatorId:guid}/activate", async (Guid tenantId, Guid indicatorId, HttpContext httpContext, IQualityIndicatorService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ActivateAsync(new IndicatorActionCommand(ApiContext.TenantId(httpContext, tenantId), indicatorId, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.IndicatorManage);

        indicators.MapPost("/{indicatorId:guid}/formula", async (Guid tenantId, Guid indicatorId, DefineIndicatorFormulaRequest request, HttpContext httpContext, IQualityIndicatorService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.DefineFormulaAsync(new DefineIndicatorFormulaCommand(ApiContext.TenantId(httpContext, tenantId), indicatorId, request.Expression, request.CalculationType, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.IndicatorManage);

        indicators.MapPost("/{indicatorId:guid}/target", async (Guid tenantId, Guid indicatorId, DefineIndicatorTargetRequest request, HttpContext httpContext, IQualityIndicatorService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.DefineTargetAsync(new DefineIndicatorTargetCommand(ApiContext.TenantId(httpContext, tenantId), indicatorId, request.TargetValue, request.EffectiveFromUtc, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.IndicatorManage);

        indicators.MapPost("/{indicatorId:guid}/threshold", async (Guid tenantId, Guid indicatorId, DefineIndicatorThresholdRequest request, HttpContext httpContext, IQualityIndicatorService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.DefineThresholdAsync(new DefineIndicatorThresholdCommand(ApiContext.TenantId(httpContext, tenantId), indicatorId, request.WarningMinimum, request.CriticalMinimum, request.ExcellentMinimum, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.IndicatorManage);

        indicators.MapPost("/{indicatorId:guid}/periods", async (Guid tenantId, Guid indicatorId, AddIndicatorPeriodRequest request, HttpContext httpContext, IQualityIndicatorService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddPeriodAsync(new AddIndicatorPeriodCommand(ApiContext.TenantId(httpContext, tenantId), indicatorId, request.Year, request.PeriodNumber, request.StartUtc, request.EndUtc, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.IndicatorManage);

        indicators.MapPost("/{indicatorId:guid}/processes", async (Guid tenantId, Guid indicatorId, AssociateIndicatorProcessRequest request, HttpContext httpContext, IQualityIndicatorService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AssociateProcessAsync(new AssociateIndicatorProcessCommand(ApiContext.TenantId(httpContext, tenantId), indicatorId, request.ProcessName, request.Area, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.IndicatorManage);

        indicators.MapPost("/{indicatorId:guid}/measurements", async (Guid tenantId, Guid indicatorId, CaptureIndicatorMeasurementRequest request, HttpContext httpContext, IQualityIndicatorService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CaptureMeasurementAsync(new CaptureIndicatorMeasurementCommand(ApiContext.TenantId(httpContext, tenantId), indicatorId, request.PeriodId, request.Numerator, request.Denominator, request.IsAutomatic, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.IndicatorManage);

        indicators.MapPost("/{indicatorId:guid}/results", async (Guid tenantId, Guid indicatorId, CalculateIndicatorResultRequest request, HttpContext httpContext, IQualityIndicatorService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CalculateResultAsync(new CalculateIndicatorResultCommand(ApiContext.TenantId(httpContext, tenantId), indicatorId, request.PeriodId, request.MeasurementId, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.IndicatorManage);

        indicators.MapPost("/{indicatorId:guid}/attachments", async (Guid tenantId, Guid indicatorId, AddIndicatorAttachmentRequest request, HttpContext httpContext, IQualityIndicatorService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddAttachmentAsync(new AddIndicatorAttachmentCommand(ApiContext.TenantId(httpContext, tenantId), indicatorId, request.StoredFileId, request.FileName, request.ContentType, request.SizeBytes, request.Sha256Hash, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.IndicatorManage);

        indicators.MapPost("/{indicatorId:guid}/workflow", async (Guid tenantId, Guid indicatorId, AttachIndicatorWorkflowRequest request, HttpContext httpContext, IQualityIndicatorService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AttachWorkflowAsync(new AttachIndicatorWorkflowCommand(ApiContext.TenantId(httpContext, tenantId), indicatorId, request.WorkflowInstanceId, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.WorkflowManage);

        indicators.MapPost("/{indicatorId:guid}/approve", async (Guid tenantId, Guid indicatorId, HttpContext httpContext, IQualityIndicatorService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ApproveAsync(new IndicatorActionCommand(ApiContext.TenantId(httpContext, tenantId), indicatorId, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.IndicatorApprove);

        indicators.MapGet("/", async (Guid tenantId, string? searchText, IndicatorStatus? status, IndicatorType? type, IndicatorFrequency? frequency, Guid? supplierId, Guid? auditId, Guid? capaId, Guid? riskId, int page, int pageSize, HttpContext httpContext, IQualityIndicatorService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.SearchAsync(new IndicatorSearchQuery(ApiContext.TenantId(httpContext, tenantId), searchText, status, type, frequency, supplierId, auditId, capaId, riskId, page, pageSize), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.IndicatorRead);

        indicators.MapGet("/dashboard", async (Guid tenantId, HttpContext httpContext, IQualityIndicatorService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetDashboardAsync(ApiContext.TenantId(httpContext, tenantId), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.IndicatorRead);

        indicators.MapGet("/trends", async (Guid tenantId, Guid? indicatorId, HttpContext httpContext, IQualityIndicatorService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetTrendsAsync(ApiContext.TenantId(httpContext, tenantId), indicatorId, cancellationToken)))
            .RequireAuthorization(PermissionPolicies.IndicatorRead);

        indicators.MapPost("/export", async (Guid tenantId, IndicatorStatus? status, IndicatorType? type, string? format, HttpContext httpContext, IQualityIndicatorService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ExportAsync(new IndicatorExportQuery(ApiContext.TenantId(httpContext, tenantId), status, type, format ?? "csv", ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.IndicatorExport);
    }

    private static void MapReportingEngine(RouteGroupBuilder api)
    {
        var reports = api.MapGroup("/tenants/{tenantId:guid}/reports")
            .WithTags("Reporting Engine");

        reports.MapGet("/standard", async (IReportingEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetStandardReportsAsync(cancellationToken)))
            .RequireAuthorization(PermissionPolicies.ReportRead);

        reports.MapPost("/standard/seed", async (Guid tenantId, HttpContext httpContext, IReportingEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.SeedStandardReportsAsync(new SeedStandardReportsCommand(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.ReportManage);

        reports.MapPost("/categories", async (Guid tenantId, CreateReportCategoryRequest request, HttpContext httpContext, IReportingEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateCategoryAsync(new CreateReportCategoryCommand(ApiContext.TenantId(httpContext, tenantId), request.Name, request.Code, request.Module, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.ReportManage);

        reports.MapPost("/", async (Guid tenantId, CreateReportDefinitionRequest request, HttpContext httpContext, IReportingEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateDefinitionAsync(new CreateReportDefinitionCommand(ApiContext.TenantId(httpContext, tenantId), request.CategoryId, request.Name, request.Code, request.Description, request.Module, request.DatasetKey, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.ReportManage);

        reports.MapPost("/{reportDefinitionId:guid}/templates", async (Guid tenantId, Guid reportDefinitionId, AddReportTemplateRequest request, HttpContext httpContext, IReportingEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddTemplateAsync(new AddReportTemplateCommand(ApiContext.TenantId(httpContext, tenantId), reportDefinitionId, request.Name, request.Format, request.Content, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.ReportManage);

        reports.MapPost("/{reportDefinitionId:guid}/parameters", async (Guid tenantId, Guid reportDefinitionId, AddReportParameterRequest request, HttpContext httpContext, IReportingEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddParameterAsync(new AddReportParameterCommand(ApiContext.TenantId(httpContext, tenantId), reportDefinitionId, request.Name, request.Label, request.Type, request.IsRequired, request.DefaultValue, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.ReportManage);

        reports.MapPost("/{reportDefinitionId:guid}/permissions", async (Guid tenantId, Guid reportDefinitionId, GrantReportPermissionRequest request, HttpContext httpContext, IReportingEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GrantPermissionAsync(new GrantReportPermissionCommand(ApiContext.TenantId(httpContext, tenantId), reportDefinitionId, request.Scope, request.Subject, request.CanExecute, request.CanExport, request.CanSchedule, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.ReportManage);

        reports.MapPost("/{reportDefinitionId:guid}/activate", async (Guid tenantId, Guid reportDefinitionId, HttpContext httpContext, IReportingEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ActivateAsync(new ReportActionCommand(ApiContext.TenantId(httpContext, tenantId), reportDefinitionId, ApiContext.UserId(httpContext), Permissions(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.ReportManage);

        reports.MapPost("/{reportDefinitionId:guid}/execute", async (Guid tenantId, Guid reportDefinitionId, ExecuteReportRequest request, HttpContext httpContext, IReportingEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ExecuteAsync(new ExecuteReportCommand(ApiContext.TenantId(httpContext, tenantId), reportDefinitionId, request.ParametersJson, ApiContext.UserId(httpContext), Permissions(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.ReportExecute);

        reports.MapPost("/{reportDefinitionId:guid}/complete", async (Guid tenantId, Guid reportDefinitionId, CompleteReportExecutionRequest request, HttpContext httpContext, IReportingEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CompleteExecutionAsync(new CompleteReportExecutionCommand(ApiContext.TenantId(httpContext, tenantId), reportDefinitionId, request.ExecutionId, request.RowCount, request.DatasetDescriptorJson, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.ReportExecute);

        reports.MapPost("/{reportDefinitionId:guid}/export", async (Guid tenantId, Guid reportDefinitionId, ExportReportRequest request, HttpContext httpContext, IReportingEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ExportAsync(new ExportReportCommand(ApiContext.TenantId(httpContext, tenantId), reportDefinitionId, request.ExecutionId, request.Format, ApiContext.UserId(httpContext), Permissions(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.ReportExport);

        reports.MapPost("/{reportDefinitionId:guid}/schedules", async (Guid tenantId, Guid reportDefinitionId, ScheduleReportRequest request, HttpContext httpContext, IReportingEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ScheduleAsync(new ScheduleReportCommand(ApiContext.TenantId(httpContext, tenantId), reportDefinitionId, request.Frequency, request.NextRunUtc, ApiContext.UserId(httpContext), Permissions(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.ReportSchedule);

        reports.MapPost("/{reportDefinitionId:guid}/subscriptions", async (Guid tenantId, Guid reportDefinitionId, SubscribeReportRequest request, HttpContext httpContext, IReportingEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.SubscribeAsync(new SubscribeReportCommand(ApiContext.TenantId(httpContext, tenantId), reportDefinitionId, request.Recipient, request.Format, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.ReportSchedule);

        reports.MapPost("/{reportDefinitionId:guid}/dashboard-bindings", async (Guid tenantId, Guid reportDefinitionId, BindReportDashboardRequest request, HttpContext httpContext, IReportingEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.BindDashboardAsync(new BindReportDashboardCommand(ApiContext.TenantId(httpContext, tenantId), reportDefinitionId, request.DashboardKey, request.DatasetKey, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.ReportManage);

        reports.MapGet("/", async (Guid tenantId, string? searchText, ReportModule? module, ReportDefinitionStatus? status, int page, int pageSize, HttpContext httpContext, IReportingEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.SearchAsync(new ReportSearchQuery(ApiContext.TenantId(httpContext, tenantId), searchText, module, status, page, pageSize), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.ReportRead);

        reports.MapGet("/dashboard-datasets", async (Guid tenantId, HttpContext httpContext, IReportingEngineService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.GetDashboardDatasetsAsync(ApiContext.TenantId(httpContext, tenantId), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.ReportRead);
    }

    private static IReadOnlyCollection<string> Permissions(HttpContext httpContext)
    {
        return httpContext.User.Claims
            .Where(claim => string.Equals(claim.Type, "permission", StringComparison.OrdinalIgnoreCase))
            .Select(claim => claim.Value)
            .ToArray();
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var escaped = value.Replace("\"", "\"\"", StringComparison.Ordinal);
        return escaped.Contains(',') || escaped.Contains('"') || escaped.Contains('\n')
            ? $"\"{escaped}\""
            : escaped;
    }
}
