using System.Security.Claims;
using Compliance360.Application.Audit;
using Compliance360.Application.AuditManagement;
using Compliance360.Application.Documents;
using Compliance360.Application.Identity;
using Compliance360.Application.Mfa;
using Compliance360.Application.Notifications;
using Compliance360.Application.Rbac;
using Compliance360.Application.Storage;
using Compliance360.Application.Suppliers;
using Compliance360.Application.TechnicalSheets;
using Compliance360.Application.TenantManagement;
using Compliance360.Application.Workflows;
using Compliance360.Domain.Documents;
using Compliance360.Domain.AuditManagement;
using Compliance360.Domain.Identity;
using Compliance360.Domain.Suppliers;
using Compliance360.Domain.TechnicalSheets;
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

        return api;
    }

    private static void MapIdentity(RouteGroupBuilder api)
    {
        var auth = api.MapGroup("/auth").WithTags("Identity");

        auth.MapPost("/login", async (LoginRequest request, HttpContext httpContext, IIdentityService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.LoginAsync(
                new LoginCommand(request.TenantId, request.Email, request.Password, ApiContext.IpAddress(httpContext), ApiContext.UserAgent(httpContext)),
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

    private static void MapTenants(RouteGroupBuilder api)
    {
        var tenants = api.MapGroup("/tenants").WithTags("Tenant Management");

        tenants.MapPost("/", async (CreateTenantRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateTenantAsync(new CreateTenantCommand(request.Name, request.Slug, ApiContext.UserId(httpContext)), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantManage);

        tenants.MapPost("/{tenantId:guid}/companies", async (Guid tenantId, AddCompanyRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.AddCompanyAsync(
                new AddCompanyCommand(ApiContext.TenantId(httpContext, tenantId), request.LegalName, request.TaxIdentifier, request.CountryCode, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantManage);

        tenants.MapPost("/{tenantId:guid}/activate", async (Guid tenantId, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ActivateTenantAsync(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantManage);

        tenants.MapPost("/{tenantId:guid}/suspend", async (Guid tenantId, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.SuspendTenantAsync(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantManage);

        tenants.MapPut("/{tenantId:guid}/settings", async (Guid tenantId, ConfigureTenantSettingsRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ConfigureSettingsAsync(
                new ConfigureTenantSettingsCommand(ApiContext.TenantId(httpContext, tenantId), request.Culture, request.TimeZone, request.RequireMfa, request.DocumentRetentionDays, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantManage);

        tenants.MapPut("/{tenantId:guid}/branding", async (Guid tenantId, ConfigureTenantBrandingRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ConfigureBrandingAsync(
                new ConfigureTenantBrandingCommand(ApiContext.TenantId(httpContext, tenantId), request.DisplayName, request.LogoUri, request.PrimaryColor, request.SecondaryColor, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantManage);

        tenants.MapPut("/{tenantId:guid}/subscription", async (Guid tenantId, ChangeSubscriptionRequest request, HttpContext httpContext, ITenantManagementService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.ChangeSubscriptionAsync(
                new ChangeSubscriptionCommand(ApiContext.TenantId(httpContext, tenantId), request.Plan, request.MaxUsers, request.MaxStorageGb, ApiContext.UserId(httpContext)),
                cancellationToken)))
            .RequireAuthorization(PermissionPolicies.TenantManage);
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
    }

    private static void MapNotifications(RouteGroupBuilder api)
    {
        var notifications = api.MapGroup("/tenants/{tenantId:guid}/notifications")
            .WithTags("Notifications")
            .RequireAuthorization(PermissionPolicies.NotificationManage);

        notifications.MapPost("/templates", async (Guid tenantId, CreateNotificationTemplateRequest request, HttpContext httpContext, INotificationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CreateTemplateAsync(
                new CreateNotificationTemplateCommand(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), request.Code, request.Channel, request.Subject, request.Body),
                cancellationToken)));

        notifications.MapPost("/messages", async (Guid tenantId, QueueNotificationRequest request, HttpContext httpContext, INotificationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.QueueAsync(
                new QueueNotificationCommand(ApiContext.TenantId(httpContext, tenantId), ApiContext.UserId(httpContext), request.Channel, request.Recipient, request.Subject, request.Body, request.TemplateCode, request.Variables, request.Priority, request.TargetUserId),
                cancellationToken)));

        notifications.MapPost("/messages/{messageId:guid}/send", async (Guid tenantId, Guid messageId, HttpContext httpContext, INotificationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.SendAsync(
                new SendNotificationCommand(ApiContext.TenantId(httpContext, tenantId), messageId, ApiContext.UserId(httpContext)),
                cancellationToken)));

        notifications.MapPost("/messages/{messageId:guid}/cancel", async (Guid tenantId, Guid messageId, HttpContext httpContext, INotificationService service, CancellationToken cancellationToken) =>
            ApiResult.From(await service.CancelAsync(
                new CancelNotificationCommand(ApiContext.TenantId(httpContext, tenantId), messageId, ApiContext.UserId(httpContext)),
                cancellationToken)));
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

    private static IReadOnlyCollection<string> Permissions(HttpContext httpContext)
    {
        return httpContext.User.Claims
            .Where(claim => string.Equals(claim.Type, "permission", StringComparison.OrdinalIgnoreCase))
            .Select(claim => claim.Value)
            .ToArray();
    }
}
