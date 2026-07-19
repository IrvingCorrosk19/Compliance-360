using Compliance360.Application;
using Compliance360.Application.Audit;
using Compliance360.Application.AuditManagement;
using Compliance360.Application.CapaManagement;
using Compliance360.Application.Documents;
using Compliance360.Application.Enterprise;
using Compliance360.Application.FormTemplates;
using Compliance360.Application.Identity;
using Compliance360.Application.Mfa;
using Compliance360.Application.Notifications;
using Compliance360.Application.Rbac;
using Compliance360.Application.RiskManagement;
using Compliance360.Application.QualityIndicators;
using Compliance360.Application.Reporting;
using Compliance360.Application.RegulatoryAffairs;
using Compliance360.Application.Storage;
using Compliance360.Application.SuperAdmin;
using Compliance360.Application.Suppliers;
using Compliance360.Application.TechnicalSheets;
using Compliance360.Application.TenantManagement;
using Compliance360.Application.Workflows;
using Compliance360.Domain.Storage;
using Compliance360.Infrastructure.Audit;
using Compliance360.Infrastructure.AuditManagement;
using Compliance360.Infrastructure.CapaManagement;
using Compliance360.Infrastructure.Documents;
using Compliance360.Infrastructure.Enterprise;
using Compliance360.Infrastructure.FormTemplates;
using Compliance360.Infrastructure.Identity;
using Compliance360.Infrastructure.Mfa;
using Compliance360.Infrastructure.Notifications;
using Compliance360.Infrastructure.Persistence;
using Compliance360.Infrastructure.Rbac;
using Compliance360.Infrastructure.RiskManagement;
using Compliance360.Infrastructure.QualityIndicators;
using Compliance360.Infrastructure.Reporting;
using Compliance360.Infrastructure.RegulatoryAffairs;
using Compliance360.Infrastructure.Security;
using Compliance360.Infrastructure.Storage;
using Compliance360.Infrastructure.SuperAdmin;
using Compliance360.Infrastructure.Suppliers;
using Compliance360.Infrastructure.TechnicalSheets;
using Compliance360.Infrastructure.TenantManagement;
using Compliance360.Infrastructure.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Compliance360.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<MfaChallengeOptions>(configuration.GetSection(MfaChallengeOptions.SectionName));
        services.Configure<AuthResolverOptions>(configuration.GetSection(AuthResolverOptions.SectionName));
        services.Configure<RefreshTokenOptions>(configuration.GetSection(RefreshTokenOptions.SectionName));
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.Configure<PasswordPolicyOptions>(configuration.GetSection(PasswordPolicyOptions.SectionName));
        services.Configure<LockoutOptions>(configuration.GetSection(LockoutOptions.SectionName));
        services.Configure<AuditOptions>(configuration.GetSection(AuditOptions.SectionName));
        services.Configure<DocumentManagementOptions>(configuration.GetSection(DocumentManagementOptions.SectionName));
        services.Configure<WorkflowEngineOptions>(configuration.GetSection(WorkflowEngineOptions.SectionName));
        services.Configure<TechnicalSheetOptions>(configuration.GetSection(TechnicalSheetOptions.SectionName));
        services.Configure<SupplierManagementOptions>(configuration.GetSection(SupplierManagementOptions.SectionName));
        services.Configure<NotificationProviderOptions>(configuration.GetSection(NotificationProviderOptions.SectionName));

        services.AddDataProtection();
        services.AddMemoryCache();
        services.AddHttpClient();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IAuditContextAccessor, AuditContextAccessor>();
        services.AddScoped<IAuditPermissionEvaluator, AuditPermissionEvaluator>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IRegulatoryAffairsService, RegulatoryAffairsService>();
        services.AddScoped<IRegulatoryWorkflowV2Service, RegulatoryWorkflowV2Service>();
        services.AddScoped<IRegulatorySoDGate, RegulatorySoDGate>();
        services.AddScoped<ICurrentUserPermissions, AllowAllUserPermissions>();
        services.AddScoped<IRegutrackWorkbookParser, ClosedXmlRegutrackWorkbookParser>();
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<IMfaSecretProtector, DataProtectionMfaSecretProtector>();
        services.AddScoped<ITotpService, TotpService>();
        services.AddScoped<IMfaService, MfaService>();
        services.AddScoped<IPasswordPolicyValidator, PasswordPolicyValidator>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<IMfaChallengeTokenService, MfaChallengeTokenService>();
        services.AddScoped<IAuthResolverTokenService, AuthResolverTokenService>();
        services.AddScoped<IStorageProvider, LocalStorageProvider>();
        services.AddScoped<IStorageProvider>(_ => new ConfiguredObjectStorageProvider(StorageProviderKind.AzureBlob));
        services.AddScoped<IStorageProvider>(_ => new ConfiguredObjectStorageProvider(StorageProviderKind.AwsS3));
        services.AddScoped<IStorageProvider>(_ => new ConfiguredObjectStorageProvider(StorageProviderKind.MinIO));
        services.AddScoped<IStorageProvider>(_ => new ConfiguredObjectStorageProvider(StorageProviderKind.GoogleCloudStorage));
        services.AddScoped<IStorageProvider>(_ => new ConfiguredObjectStorageProvider(StorageProviderKind.Sftp));
        services.AddScoped<IStorageProviderFactory, StorageProviderFactory>();
        services.AddScoped<IFileStorageService, EnterpriseFileStorageService>();
        services.AddScoped<IStorageFoundationService, StorageFoundationService>();
        services.AddScoped<ISuperAdminPlatformService, SuperAdminPlatformService>();
        services.AddScoped<INotificationTemplateEngine, NotificationTemplateEngine>();
        services.AddScoped<INotificationRetryService, NotificationRetryService>();
        services.AddScoped<INotificationTrackingService, NotificationTrackingService>();
        services.AddScoped<INotificationAuditService, NotificationAuditService>();
        services.AddScoped<INotificationProvider, SmtpNotificationProvider>();
        services.AddScoped<INotificationProvider, SendGridNotificationProvider>();
        services.AddScoped<INotificationProvider, MailgunNotificationProvider>();
        services.AddScoped<INotificationProvider, ResendNotificationProvider>();
        services.AddScoped<INotificationProvider, GmailSmtpNotificationProvider>();
        services.AddScoped<INotificationProvider, Microsoft365NotificationProvider>();
        services.AddScoped<INotificationProvider, ExchangeOnlineNotificationProvider>();
        services.AddScoped<INotificationProvider, AmazonSesNotificationProvider>();
        services.AddScoped<INotificationProviderFactory, NotificationProviderFactory>();
        services.AddScoped<INotificationDispatcher, EnterpriseNotificationDispatcher>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ITenantManagementService, TenantManagementService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IRbacService, RbacService>();
        services.AddScoped<IEnterpriseWorkspaceService, EnterpriseWorkspaceService>();
        services.AddScoped<IFormTemplateService, FormTemplateService>();
        services.AddScoped<IDocumentManagementService, DocumentManagementService>();
        services.AddScoped<IWorkflowEngineService, WorkflowEngineService>();
        services.AddScoped<ITechnicalSheetService, TechnicalSheetService>();
        services.AddScoped<ISupplierManagementService, SupplierManagementService>();
        services.AddScoped<IAuditManagementService, AuditManagementService>();
        services.AddScoped<ICapaManagementService, CapaManagementService>();
        services.AddScoped<IRiskManagementService, RiskManagementService>();
        services.AddScoped<IQualityIndicatorService, QualityIndicatorService>();
        services.AddScoped<IReportingEngineService, ReportingEngineService>();

        var connectionString = configuration.GetConnectionString("Compliance360");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<Compliance360DbContext>((provider, options) =>
            {
                options
                    .UseNpgsql(connectionString)
                    .AddInterceptors(provider.GetRequiredService<AuditSaveChangesInterceptor>());
            });
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<Compliance360DbContext>());
            services.AddScoped<IAuditRepository, EfAuditRepository>();
            services.AddScoped<IRegulatoryAffairsRepository, EfRegulatoryAffairsRepository>();
            services.AddScoped<IRegulatoryWorkflowV2Repository, EfRegulatoryWorkflowV2Repository>();
            services.AddScoped<ITenantManagementRepository, EfTenantManagementRepository>();
            services.AddScoped<IIdentityRepository, EfIdentityRepository>();
            services.AddScoped<IRbacRepository, EfRbacRepository>();
            services.AddScoped<IRbacProvisioningService, EfRbacProvisioningService>();
            services.AddScoped<IMfaRepository, EfMfaRepository>();
            services.AddScoped<IStorageRepository, EfStorageRepository>();
            services.AddScoped<ISuperAdminPlatformRepository, EfSuperAdminPlatformRepository>();
            services.AddScoped<INotificationRepository, EfNotificationRepository>();
            services.AddScoped<IEnterpriseWorkspaceRepository, EfEnterpriseWorkspaceRepository>();
            services.AddScoped<IFormTemplateRepository, EfFormTemplateRepository>();
            services.AddScoped<IDocumentRepository, EfDocumentRepository>();
            services.AddScoped<IWorkflowRepository, EfWorkflowRepository>();
            services.AddScoped<ITechnicalSheetRepository, EfTechnicalSheetRepository>();
            services.AddScoped<ISupplierRepository, EfSupplierRepository>();
            services.AddScoped<IAuditManagementRepository, EfAuditManagementRepository>();
            services.AddScoped<ICapaManagementRepository, EfCapaManagementRepository>();
            services.AddScoped<IRiskManagementRepository, EfRiskManagementRepository>();
            services.AddScoped<IQualityIndicatorRepository, EfQualityIndicatorRepository>();
            services.AddScoped<IReportingEngineRepository, EfReportingEngineRepository>();
        }

        return services;
    }
}
