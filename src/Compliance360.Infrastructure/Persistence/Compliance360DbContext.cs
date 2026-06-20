using Compliance360.Application;
using Compliance360.Domain.Audit;
using Compliance360.Domain.AuditManagement;
using Compliance360.Domain.CapaManagement;
using Compliance360.Domain.Common;
using Compliance360.Domain.Documents;
using Compliance360.Domain.Identity;
using Compliance360.Domain.Notifications;
using Compliance360.Domain.QualityIndicators;
using Compliance360.Domain.RiskManagement;
using Compliance360.Domain.Storage;
using Compliance360.Domain.Suppliers;
using Compliance360.Domain.TechnicalSheets;
using Compliance360.Domain.TenantManagement;
using Compliance360.Domain.Workflows;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Persistence;

public sealed class Compliance360DbContext : DbContext, IApplicationDbContext
{
    private readonly IClock _clock;

    public Compliance360DbContext(DbContextOptions<Compliance360DbContext> options, IClock clock)
        : base(options)
    {
        _clock = clock;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();

    public DbSet<TenantBranding> TenantBranding => Set<TenantBranding>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<PasswordHistory> PasswordHistory => Set<PasswordHistory>();

    public DbSet<UserSession> UserSessions => Set<UserSession>();

    public DbSet<MfaConfiguration> MfaConfigurations => Set<MfaConfiguration>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();

    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

    public DbSet<NotificationMessage> NotificationMessages => Set<NotificationMessage>();

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();

    public DbSet<DocumentCategory> DocumentCategories => Set<DocumentCategory>();

    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();

    public DbSet<DocumentApproval> DocumentApprovals => Set<DocumentApproval>();

    public DbSet<DocumentHistory> DocumentHistory => Set<DocumentHistory>();

    public DbSet<DocumentPermission> DocumentPermissions => Set<DocumentPermission>();

    public DbSet<Workflow> Workflows => Set<Workflow>();

    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();

    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();

    public DbSet<WorkflowRule> WorkflowRules => Set<WorkflowRule>();

    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();

    public DbSet<WorkflowAssignment> WorkflowAssignments => Set<WorkflowAssignment>();

    public DbSet<WorkflowHistory> WorkflowHistory => Set<WorkflowHistory>();

    public DbSet<WorkflowEscalation> WorkflowEscalations => Set<WorkflowEscalation>();

    public DbSet<WorkflowNotification> WorkflowNotifications => Set<WorkflowNotification>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<TechnicalSheet> TechnicalSheets => Set<TechnicalSheet>();

    public DbSet<TechnicalSheetIngredient> TechnicalSheetIngredients => Set<TechnicalSheetIngredient>();

    public DbSet<TechnicalSheetNutrient> TechnicalSheetNutrients => Set<TechnicalSheetNutrient>();

    public DbSet<TechnicalSheetCertification> TechnicalSheetCertifications => Set<TechnicalSheetCertification>();

    public DbSet<TechnicalSheetVersion> TechnicalSheetVersions => Set<TechnicalSheetVersion>();

    public DbSet<TechnicalSheetApproval> TechnicalSheetApprovals => Set<TechnicalSheetApproval>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<SupplierDocument> SupplierDocuments => Set<SupplierDocument>();

    public DbSet<SupplierEvaluation> SupplierEvaluations => Set<SupplierEvaluation>();

    public DbSet<SupplierExpirationAlert> SupplierExpirationAlerts => Set<SupplierExpirationAlert>();

    public DbSet<AuditProgram> AuditPrograms => Set<AuditProgram>();

    public DbSet<AuditPlan> AuditPlans => Set<AuditPlan>();

    public DbSet<AuditChecklist> AuditChecklists => Set<AuditChecklist>();

    public DbSet<AuditChecklistItem> AuditChecklistItems => Set<AuditChecklistItem>();

    public DbSet<ManagedAudit> ManagedAudits => Set<ManagedAudit>();

    public DbSet<AuditSchedule> AuditSchedules => Set<AuditSchedule>();

    public DbSet<AuditParticipant> AuditParticipants => Set<AuditParticipant>();

    public DbSet<AuditAuditor> AuditAuditors => Set<AuditAuditor>();

    public DbSet<AuditArea> AuditAreas => Set<AuditArea>();

    public DbSet<AuditFinding> AuditFindings => Set<AuditFinding>();

    public DbSet<AuditEvidence> AuditEvidence => Set<AuditEvidence>();

    public DbSet<AuditObservation> AuditObservations => Set<AuditObservation>();

    public DbSet<AuditNonConformity> AuditNonConformities => Set<AuditNonConformity>();

    public DbSet<AuditRecommendation> AuditRecommendations => Set<AuditRecommendation>();

    public DbSet<AuditCorrectiveActionLink> AuditCorrectiveActionLinks => Set<AuditCorrectiveActionLink>();

    public DbSet<AuditHistory> ManagedAuditHistory => Set<AuditHistory>();

    public DbSet<AuditAttachment> AuditAttachments => Set<AuditAttachment>();

    public DbSet<Capa> Capas => Set<Capa>();

    public DbSet<CapaOwner> CapaOwners => Set<CapaOwner>();

    public DbSet<CapaApprover> CapaApprovers => Set<CapaApprover>();

    public DbSet<CapaRootCause> CapaRootCauses => Set<CapaRootCause>();

    public DbSet<CapaCauseAnalysis> CapaCauseAnalyses => Set<CapaCauseAnalysis>();

    public DbSet<CapaContainmentAction> CapaContainmentActions => Set<CapaContainmentAction>();

    public DbSet<CapaCorrectiveAction> CapaCorrectiveActions => Set<CapaCorrectiveAction>();

    public DbSet<CapaPreventiveAction> CapaPreventiveActions => Set<CapaPreventiveAction>();

    public DbSet<CapaEffectivenessCheck> CapaEffectivenessChecks => Set<CapaEffectivenessCheck>();

    public DbSet<CapaEvidence> CapaEvidence => Set<CapaEvidence>();

    public DbSet<CapaAttachment> CapaAttachments => Set<CapaAttachment>();

    public DbSet<CapaHistory> CapaHistory => Set<CapaHistory>();

    public DbSet<RiskCategory> RiskCategories => Set<RiskCategory>();

    public DbSet<RiskMatrix> RiskMatrices => Set<RiskMatrix>();

    public DbSet<Risk> Risks => Set<Risk>();

    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();

    public DbSet<RiskTreatment> RiskTreatments => Set<RiskTreatment>();

    public DbSet<RiskMitigationPlan> RiskMitigationPlans => Set<RiskMitigationPlan>();

    public DbSet<RiskControl> RiskControls => Set<RiskControl>();

    public DbSet<RiskOwner> RiskOwners => Set<RiskOwner>();

    public DbSet<RiskReview> RiskReviews => Set<RiskReview>();

    public DbSet<RiskEvidence> RiskEvidence => Set<RiskEvidence>();

    public DbSet<RiskIndicator> RiskIndicators => Set<RiskIndicator>();

    public DbSet<RiskAttachment> RiskAttachments => Set<RiskAttachment>();

    public DbSet<RiskHistory> RiskHistory => Set<RiskHistory>();

    public DbSet<IndicatorCategory> IndicatorCategories => Set<IndicatorCategory>();

    public DbSet<QualityIndicator> QualityIndicators => Set<QualityIndicator>();

    public DbSet<IndicatorFormula> IndicatorFormulas => Set<IndicatorFormula>();

    public DbSet<IndicatorMeasurement> IndicatorMeasurements => Set<IndicatorMeasurement>();

    public DbSet<IndicatorTarget> IndicatorTargets => Set<IndicatorTarget>();

    public DbSet<IndicatorThreshold> IndicatorThresholds => Set<IndicatorThreshold>();

    public DbSet<IndicatorResult> IndicatorResults => Set<IndicatorResult>();

    public DbSet<IndicatorPeriod> IndicatorPeriods => Set<IndicatorPeriod>();

    public DbSet<IndicatorProcess> IndicatorProcesses => Set<IndicatorProcess>();

    public DbSet<IndicatorAlert> IndicatorAlerts => Set<IndicatorAlert>();

    public DbSet<IndicatorTrend> IndicatorTrends => Set<IndicatorTrend>();

    public DbSet<IndicatorHistory> IndicatorHistory => Set<IndicatorHistory>();

    public DbSet<IndicatorAttachment> IndicatorAttachments => Set<IndicatorAttachment>();

    public override int SaveChanges()
    {
        ApplyFoundationRules();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyFoundationRules();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("compliance360");

        ConfigureTenantManagement(modelBuilder);
        ConfigureIdentity(modelBuilder);
        ConfigureAudit(modelBuilder);
        ConfigureStorage(modelBuilder);
        ConfigureNotifications(modelBuilder);
        ConfigureDocuments(modelBuilder);
        ConfigureWorkflows(modelBuilder);
        ConfigureTechnicalSheets(modelBuilder);
        ConfigureSuppliers(modelBuilder);
        ConfigureAuditManagement(modelBuilder);
        ConfigureCapaManagement(modelBuilder);
        ConfigureRiskManagement(modelBuilder);
        ConfigureQualityIndicators(modelBuilder);
    }

    private static void ConfigureTenantManagement(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(tenant => tenant.Id);
            entity.Property(tenant => tenant.Name).HasMaxLength(180).IsRequired();
            entity.Property(tenant => tenant.Slug).HasMaxLength(80).IsRequired();
            entity.Property(tenant => tenant.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(tenant => tenant.Slug).IsUnique();
            entity.HasMany(tenant => tenant.Companies).WithOne().HasForeignKey(company => company.TenantId);
            entity.Navigation(tenant => tenant.Companies).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.HasOne(tenant => tenant.Settings).WithOne().HasForeignKey<TenantSettings>(settings => settings.TenantId);
            entity.HasOne(tenant => tenant.Branding).WithOne().HasForeignKey<TenantBranding>(branding => branding.TenantId);
            entity.HasOne(tenant => tenant.Subscription).WithOne().HasForeignKey<Subscription>(subscription => subscription.TenantId);
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("companies");
            entity.HasKey(company => company.Id);
            entity.Property(company => company.LegalName).HasMaxLength(220).IsRequired();
            entity.Property(company => company.TaxIdentifier).HasMaxLength(80).IsRequired();
            entity.Property(company => company.CountryCode).HasMaxLength(2).IsRequired();
            entity.HasIndex(company => new { company.TenantId, company.TaxIdentifier }).IsUnique();
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.ToTable("subscriptions");
            entity.HasKey(subscription => subscription.Id);
            entity.Property(subscription => subscription.Plan).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(subscription => subscription.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(subscription => subscription.TenantId).IsUnique();
        });

        modelBuilder.Entity<TenantSettings>(entity =>
        {
            entity.ToTable("tenant_settings");
            entity.HasKey(settings => settings.Id);
            entity.Property(settings => settings.Culture).HasMaxLength(12).IsRequired();
            entity.Property(settings => settings.TimeZone).HasMaxLength(80).IsRequired();
            entity.HasIndex(settings => settings.TenantId).IsUnique();
        });

        modelBuilder.Entity<TenantBranding>(entity =>
        {
            entity.ToTable("tenant_branding");
            entity.HasKey(branding => branding.Id);
            entity.Property(branding => branding.DisplayName).HasMaxLength(180).IsRequired();
            entity.Property(branding => branding.LogoUri).HasMaxLength(500);
            entity.Property(branding => branding.PrimaryColor).HasMaxLength(20).IsRequired();
            entity.Property(branding => branding.SecondaryColor).HasMaxLength(20).IsRequired();
            entity.HasIndex(branding => branding.TenantId).IsUnique();
        });
    }

    private static void ConfigureIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Email).HasMaxLength(320).IsRequired();
            entity.Property(user => user.NormalizedEmail).HasMaxLength(320).IsRequired();
            entity.Property(user => user.FullName).HasMaxLength(180).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(1_000).IsRequired();
            entity.Property(user => user.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(user => user.MfaSecretEncrypted).HasMaxLength(2_000);
            entity.HasIndex(user => new { user.TenantId, user.NormalizedEmail }).IsUnique();
            entity.HasMany(user => user.Roles).WithOne().HasForeignKey(userRole => userRole.UserId);
            entity.HasMany(user => user.RefreshTokens).WithOne().HasForeignKey(refreshToken => refreshToken.UserId);
            entity.HasMany(user => user.PasswordHistory).WithOne().HasForeignKey(passwordHistory => passwordHistory.UserId);
            entity.HasMany(user => user.Sessions).WithOne().HasForeignKey(session => session.UserId);
            entity.Navigation(user => user.Roles).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(user => user.RefreshTokens).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(user => user.PasswordHistory).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(user => user.Sessions).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(role => role.Id);
            entity.Property(role => role.Name).HasMaxLength(120).IsRequired();
            entity.Property(role => role.NormalizedName).HasMaxLength(120).IsRequired();
            entity.HasIndex(role => new { role.TenantId, role.NormalizedName }).IsUnique();
            entity.HasMany(role => role.Permissions).WithOne().HasForeignKey(rolePermission => rolePermission.RoleId);
            entity.Navigation(role => role.Permissions).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(permission => permission.Id);
            entity.Property(permission => permission.Module).HasMaxLength(80).IsRequired();
            entity.Property(permission => permission.Action).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(permission => permission.Code).HasMaxLength(140).IsRequired();
            entity.Property(permission => permission.Description).HasMaxLength(250).IsRequired();
            entity.HasIndex(permission => permission.Code).IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(userRole => userRole.Id);
            entity.HasIndex(userRole => new { userRole.TenantId, userRole.UserId, userRole.RoleId }).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions");
            entity.HasKey(rolePermission => rolePermission.Id);
            entity.HasIndex(rolePermission => new { rolePermission.TenantId, rolePermission.RoleId, rolePermission.PermissionId }).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(refreshToken => refreshToken.Id);
            entity.Property(refreshToken => refreshToken.TokenHash).HasMaxLength(512).IsRequired();
            entity.Property(refreshToken => refreshToken.ReplacedByTokenHash).HasMaxLength(512);
            entity.HasIndex(refreshToken => new { refreshToken.TenantId, refreshToken.UserId });
            entity.HasIndex(refreshToken => refreshToken.TokenHash).IsUnique();
        });

        modelBuilder.Entity<PasswordHistory>(entity =>
        {
            entity.ToTable("password_history");
            entity.HasKey(passwordHistory => passwordHistory.Id);
            entity.Property(passwordHistory => passwordHistory.PasswordHash).HasMaxLength(1_000).IsRequired();
            entity.HasIndex(passwordHistory => new { passwordHistory.TenantId, passwordHistory.UserId, passwordHistory.ChangedAtUtc });
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.ToTable("user_sessions");
            entity.HasKey(session => session.Id);
            entity.HasIndex(session => new { session.TenantId, session.UserId, session.ExpiresAtUtc });
        });

        modelBuilder.Entity<MfaConfiguration>(entity =>
        {
            entity.ToTable("mfa_configurations");
            entity.HasKey(configuration => configuration.Id);
            entity.Property(configuration => configuration.Method).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(configuration => configuration.SecretEncrypted).HasMaxLength(2_000).IsRequired();
            entity.HasIndex(configuration => new { configuration.TenantId, configuration.UserId, configuration.Method }).IsUnique();
            entity.HasIndex(configuration => new { configuration.TenantId, configuration.IsEnabled });
        });
    }

    private static void ConfigureAudit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(auditLog => auditLog.Id);
            entity.Property(auditLog => auditLog.EntityName).HasMaxLength(160).IsRequired();
            entity.Property(auditLog => auditLog.Action).HasConversion<string>().HasMaxLength(60).IsRequired();
            entity.Property(auditLog => auditLog.Category).HasConversion<string>().HasMaxLength(60).IsRequired();
            entity.Property(auditLog => auditLog.UserName).HasMaxLength(180);
            entity.Property(auditLog => auditLog.Role).HasMaxLength(120);
            entity.Property(auditLog => auditLog.IpAddress).HasMaxLength(80);
            entity.Property(auditLog => auditLog.UserAgent).HasMaxLength(500);
            entity.Property(auditLog => auditLog.CorrelationId).HasMaxLength(120);
            entity.Property(auditLog => auditLog.RequestId).HasMaxLength(120);
            entity.Property(auditLog => auditLog.ErrorMessage).HasMaxLength(1_000);
            entity.HasIndex(auditLog => new { auditLog.TenantId, auditLog.OccurredAtUtc });
            entity.HasIndex(auditLog => new { auditLog.EntityName, auditLog.EntityId });
            entity.HasIndex(auditLog => new { auditLog.TenantId, auditLog.Category, auditLog.OccurredAtUtc });
            entity.HasIndex(auditLog => new { auditLog.TenantId, auditLog.Action, auditLog.OccurredAtUtc });
            entity.HasIndex(auditLog => new { auditLog.TenantId, auditLog.UserId, auditLog.OccurredAtUtc });
        });
    }

    private static void ConfigureStorage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoredFile>(entity =>
        {
            entity.ToTable("stored_files");
            entity.HasKey(storedFile => storedFile.Id);
            entity.Property(storedFile => storedFile.StorageProvider).HasMaxLength(80).IsRequired();
            entity.Property(storedFile => storedFile.ContainerName).HasMaxLength(120).IsRequired();
            entity.Property(storedFile => storedFile.ObjectKey).HasMaxLength(500).IsRequired();
            entity.Property(storedFile => storedFile.OriginalFileName).HasMaxLength(260).IsRequired();
            entity.Property(storedFile => storedFile.ContentType).HasMaxLength(120).IsRequired();
            entity.Property(storedFile => storedFile.Sha256Hash).HasMaxLength(128).IsRequired();
            entity.Property(storedFile => storedFile.OwnerEntityName).HasMaxLength(160).IsRequired();
            entity.Property(storedFile => storedFile.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(storedFile => new { storedFile.TenantId, storedFile.OwnerEntityName, storedFile.OwnerEntityId });
            entity.HasIndex(storedFile => new { storedFile.ContainerName, storedFile.ObjectKey }).IsUnique();
        });
    }

    private static void ConfigureNotifications(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.ToTable("notification_templates");
            entity.HasKey(template => template.Id);
            entity.Property(template => template.Code).HasMaxLength(120).IsRequired();
            entity.Property(template => template.Channel).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(template => template.Subject).HasMaxLength(250).IsRequired();
            entity.Property(template => template.Body).HasMaxLength(4_000).IsRequired();
            entity.HasIndex(template => new { template.TenantId, template.Code, template.Channel }).IsUnique();
        });

        modelBuilder.Entity<NotificationMessage>(entity =>
        {
            entity.ToTable("notification_messages");
            entity.HasKey(message => message.Id);
            entity.Property(message => message.Channel).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(message => message.Recipient).HasMaxLength(320).IsRequired();
            entity.Property(message => message.Subject).HasMaxLength(250).IsRequired();
            entity.Property(message => message.Body).HasMaxLength(4_000).IsRequired();
            entity.Property(message => message.Priority).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(message => message.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(message => message.FailureReason).HasMaxLength(1_000);
            entity.HasIndex(message => new { message.TenantId, message.Status, message.Priority, message.QueuedAtUtc });
            entity.HasIndex(message => new { message.TenantId, message.TargetUserId, message.QueuedAtUtc });
        });
    }

    private static void ConfigureDocuments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.ToTable("document_types");
            entity.HasKey(documentType => documentType.Id);
            entity.Property(documentType => documentType.Name).HasMaxLength(160).IsRequired();
            entity.Property(documentType => documentType.Code).HasMaxLength(80).IsRequired();
            entity.HasIndex(documentType => new { documentType.TenantId, documentType.Code }).IsUnique();
        });

        modelBuilder.Entity<DocumentCategory>(entity =>
        {
            entity.ToTable("document_categories");
            entity.HasKey(category => category.Id);
            entity.Property(category => category.Name).HasMaxLength(160).IsRequired();
            entity.Property(category => category.Code).HasMaxLength(80).IsRequired();
            entity.HasIndex(category => new { category.TenantId, category.Code }).IsUnique();
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("documents");
            entity.HasKey(document => document.Id);
            entity.Property(document => document.Title).HasMaxLength(220).IsRequired();
            entity.Property(document => document.Code).HasMaxLength(120).IsRequired();
            entity.Property(document => document.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(document => new { document.TenantId, document.Code }).IsUnique();
            entity.HasIndex(document => new { document.TenantId, document.Status, document.ExpiresAtUtc });
            entity.HasMany(document => document.Versions).WithOne().HasForeignKey(version => version.DocumentId);
            entity.HasMany(document => document.Approvals).WithOne().HasForeignKey(approval => approval.DocumentId);
            entity.HasMany(document => document.History).WithOne().HasForeignKey(history => history.DocumentId);
            entity.HasMany(document => document.Permissions).WithOne().HasForeignKey(permission => permission.DocumentId);
            entity.Navigation(document => document.Versions).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(document => document.Approvals).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(document => document.History).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(document => document.Permissions).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<DocumentVersion>(entity =>
        {
            entity.ToTable("document_versions");
            entity.HasKey(version => version.Id);
            entity.Property(version => version.ChangeSummary).HasMaxLength(1_000).IsRequired();
            entity.HasIndex(version => new { version.TenantId, version.DocumentId, version.VersionNumber }).IsUnique();
        });

        modelBuilder.Entity<DocumentApproval>(entity =>
        {
            entity.ToTable("document_approvals");
            entity.HasKey(approval => approval.Id);
            entity.Property(approval => approval.Decision).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(approval => approval.Comments).HasMaxLength(1_000).IsRequired();
            entity.HasIndex(approval => new { approval.TenantId, approval.DocumentId, approval.DecidedAtUtc });
        });

        modelBuilder.Entity<DocumentHistory>(entity =>
        {
            entity.ToTable("document_history");
            entity.HasKey(history => history.Id);
            entity.Property(history => history.Action).HasMaxLength(500).IsRequired();
            entity.HasIndex(history => new { history.TenantId, history.DocumentId, history.OccurredAtUtc });
        });

        modelBuilder.Entity<DocumentPermission>(entity =>
        {
            entity.ToTable("document_permissions");
            entity.HasKey(permission => permission.Id);
            entity.Property(permission => permission.Level).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(permission => new { permission.TenantId, permission.DocumentId, permission.PrincipalId, permission.Level }).IsUnique();
        });
    }

    private static void ConfigureWorkflows(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.ToTable("workflows");
            entity.HasKey(workflow => workflow.Id);
            entity.Property(workflow => workflow.Name).HasMaxLength(180).IsRequired();
            entity.Property(workflow => workflow.Code).HasMaxLength(100).IsRequired();
            entity.Property(workflow => workflow.EntityName).HasMaxLength(160).IsRequired();
            entity.Property(workflow => workflow.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(workflow => new { workflow.TenantId, workflow.Code }).IsUnique();
            entity.HasMany(workflow => workflow.Steps).WithOne().HasForeignKey(step => step.WorkflowId);
            entity.HasMany(workflow => workflow.Transitions).WithOne().HasForeignKey(transition => transition.WorkflowId);
            entity.HasMany(workflow => workflow.Rules).WithOne().HasForeignKey(rule => rule.WorkflowId);
            entity.Navigation(workflow => workflow.Steps).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(workflow => workflow.Transitions).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(workflow => workflow.Rules).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<WorkflowStep>(entity =>
        {
            entity.ToTable("workflow_steps");
            entity.HasKey(step => step.Id);
            entity.Property(step => step.Name).HasMaxLength(160).IsRequired();
            entity.Property(step => step.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(step => new { step.TenantId, step.WorkflowId, step.Sequence }).IsUnique();
        });

        modelBuilder.Entity<WorkflowTransition>(entity =>
        {
            entity.ToTable("workflow_transitions");
            entity.HasKey(transition => transition.Id);
            entity.Property(transition => transition.Decision).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(transition => new { transition.TenantId, transition.WorkflowId, transition.FromStepId, transition.Decision }).IsUnique();
        });

        modelBuilder.Entity<WorkflowRule>(entity =>
        {
            entity.ToTable("workflow_rules");
            entity.HasKey(rule => rule.Id);
            entity.Property(rule => rule.FieldName).HasMaxLength(120).IsRequired();
            entity.Property(rule => rule.Operator).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(rule => rule.ExpectedValue).HasMaxLength(250).IsRequired();
            entity.HasIndex(rule => new { rule.TenantId, rule.WorkflowId, rule.FieldName });
        });

        modelBuilder.Entity<WorkflowInstance>(entity =>
        {
            entity.ToTable("workflow_instances");
            entity.HasKey(instance => instance.Id);
            entity.Property(instance => instance.EntityName).HasMaxLength(160).IsRequired();
            entity.Property(instance => instance.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(instance => new { instance.TenantId, instance.WorkflowId, instance.Status });
            entity.HasIndex(instance => new { instance.TenantId, instance.EntityName, instance.EntityId });
            entity.HasMany(instance => instance.Assignments).WithOne().HasForeignKey(assignment => assignment.WorkflowInstanceId);
            entity.HasMany(instance => instance.History).WithOne().HasForeignKey(history => history.WorkflowInstanceId);
            entity.HasMany(instance => instance.Escalations).WithOne().HasForeignKey(escalation => escalation.WorkflowInstanceId);
            entity.HasMany(instance => instance.Notifications).WithOne().HasForeignKey(notification => notification.WorkflowInstanceId);
            entity.Navigation(instance => instance.Assignments).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(instance => instance.History).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(instance => instance.Escalations).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(instance => instance.Notifications).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<WorkflowAssignment>(entity =>
        {
            entity.ToTable("workflow_assignments");
            entity.HasKey(assignment => assignment.Id);
            entity.Property(assignment => assignment.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(assignment => assignment.Decision).HasConversion<string>().HasMaxLength(40);
            entity.HasIndex(assignment => new { assignment.TenantId, assignment.AssignedToUserId, assignment.Status, assignment.DueAtUtc });
        });

        modelBuilder.Entity<WorkflowHistory>(entity =>
        {
            entity.ToTable("workflow_history");
            entity.HasKey(history => history.Id);
            entity.Property(history => history.Action).HasMaxLength(500).IsRequired();
            entity.HasIndex(history => new { history.TenantId, history.WorkflowInstanceId, history.OccurredAtUtc });
        });

        modelBuilder.Entity<WorkflowEscalation>(entity =>
        {
            entity.ToTable("workflow_escalations");
            entity.HasKey(escalation => escalation.Id);
            entity.HasIndex(escalation => new { escalation.TenantId, escalation.WorkflowInstanceId, escalation.EscalatedAtUtc });
        });

        modelBuilder.Entity<WorkflowNotification>(entity =>
        {
            entity.ToTable("workflow_notifications");
            entity.HasKey(notification => notification.Id);
            entity.Property(notification => notification.Kind).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(notification => notification.Message).HasMaxLength(500).IsRequired();
            entity.HasIndex(notification => new { notification.TenantId, notification.WorkflowInstanceId, notification.Kind, notification.QueuedAtUtc });
        });
    }

    private static void ConfigureTechnicalSheets(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Name).HasMaxLength(180).IsRequired();
            entity.Property(product => product.Sku).HasMaxLength(80).IsRequired();
            entity.Property(product => product.Description).HasMaxLength(1_000);
            entity.HasIndex(product => new { product.TenantId, product.Sku }).IsUnique();
        });

        modelBuilder.Entity<TechnicalSheet>(entity =>
        {
            entity.ToTable("technical_sheets");
            entity.HasKey(sheet => sheet.Id);
            entity.Property(sheet => sheet.Title).HasMaxLength(220).IsRequired();
            entity.Property(sheet => sheet.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(sheet => sheet.PdfObjectKey).HasMaxLength(500);
            entity.HasIndex(sheet => new { sheet.TenantId, sheet.ProductId, sheet.Status });
            entity.HasMany(sheet => sheet.Ingredients).WithOne().HasForeignKey(ingredient => ingredient.TechnicalSheetId);
            entity.HasMany(sheet => sheet.Nutrients).WithOne().HasForeignKey(nutrient => nutrient.TechnicalSheetId);
            entity.HasMany(sheet => sheet.Certifications).WithOne().HasForeignKey(certification => certification.TechnicalSheetId);
            entity.HasMany(sheet => sheet.Versions).WithOne().HasForeignKey(version => version.TechnicalSheetId);
            entity.HasMany(sheet => sheet.Approvals).WithOne().HasForeignKey(approval => approval.TechnicalSheetId);
            entity.Navigation(sheet => sheet.Ingredients).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(sheet => sheet.Nutrients).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(sheet => sheet.Certifications).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(sheet => sheet.Versions).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(sheet => sheet.Approvals).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<TechnicalSheetIngredient>(entity =>
        {
            entity.ToTable("technical_sheet_ingredients");
            entity.HasKey(ingredient => ingredient.Id);
            entity.Property(ingredient => ingredient.Name).HasMaxLength(180).IsRequired();
            entity.Property(ingredient => ingredient.Allergen).HasMaxLength(180);
            entity.Property(ingredient => ingredient.Percentage).HasPrecision(7, 4);
        });

        modelBuilder.Entity<TechnicalSheetNutrient>(entity =>
        {
            entity.ToTable("technical_sheet_nutrients");
            entity.HasKey(nutrient => nutrient.Id);
            entity.Property(nutrient => nutrient.Name).HasMaxLength(180).IsRequired();
            entity.Property(nutrient => nutrient.Amount).HasPrecision(12, 4);
            entity.Property(nutrient => nutrient.Unit).HasMaxLength(40).IsRequired();
        });

        modelBuilder.Entity<TechnicalSheetCertification>(entity =>
        {
            entity.ToTable("technical_sheet_certifications");
            entity.HasKey(certification => certification.Id);
            entity.Property(certification => certification.Name).HasMaxLength(180).IsRequired();
            entity.Property(certification => certification.Issuer).HasMaxLength(180).IsRequired();
            entity.HasIndex(certification => new { certification.TenantId, certification.ExpiresAtUtc });
        });

        modelBuilder.Entity<TechnicalSheetVersion>(entity =>
        {
            entity.ToTable("technical_sheet_versions");
            entity.HasKey(version => version.Id);
            entity.Property(version => version.ChangeSummary).HasMaxLength(1_000).IsRequired();
            entity.HasIndex(version => new { version.TenantId, version.TechnicalSheetId, version.VersionNumber }).IsUnique();
        });

        modelBuilder.Entity<TechnicalSheetApproval>(entity =>
        {
            entity.ToTable("technical_sheet_approvals");
            entity.HasKey(approval => approval.Id);
            entity.Property(approval => approval.Decision).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(approval => approval.Comments).HasMaxLength(1_000).IsRequired();
            entity.HasIndex(approval => new { approval.TenantId, approval.TechnicalSheetId, approval.DecidedAtUtc });
        });
    }

    private static void ConfigureSuppliers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("suppliers");
            entity.HasKey(supplier => supplier.Id);
            entity.Property(supplier => supplier.LegalName).HasMaxLength(220).IsRequired();
            entity.Property(supplier => supplier.TaxIdentifier).HasMaxLength(80).IsRequired();
            entity.Property(supplier => supplier.CountryCode).HasMaxLength(2).IsRequired();
            entity.Property(supplier => supplier.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(supplier => new { supplier.TenantId, supplier.TaxIdentifier }).IsUnique();
            entity.HasIndex(supplier => new { supplier.TenantId, supplier.Status });
            entity.HasMany(supplier => supplier.Documents).WithOne().HasForeignKey(document => document.SupplierId);
            entity.HasMany(supplier => supplier.Evaluations).WithOne().HasForeignKey(evaluation => evaluation.SupplierId);
            entity.HasMany(supplier => supplier.Alerts).WithOne().HasForeignKey(alert => alert.SupplierId);
            entity.Navigation(supplier => supplier.Documents).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(supplier => supplier.Evaluations).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(supplier => supplier.Alerts).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<SupplierDocument>(entity =>
        {
            entity.ToTable("supplier_documents");
            entity.HasKey(document => document.Id);
            entity.Property(document => document.Type).HasConversion<string>().HasMaxLength(60).IsRequired();
            entity.Property(document => document.DocumentNumber).HasMaxLength(120).IsRequired();
            entity.Property(document => document.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(document => document.RejectionReason).HasMaxLength(500);
            entity.HasIndex(document => new { document.TenantId, document.SupplierId, document.Type });
            entity.HasIndex(document => new { document.TenantId, document.ExpiresAtUtc });
        });

        modelBuilder.Entity<SupplierEvaluation>(entity =>
        {
            entity.ToTable("supplier_evaluations");
            entity.HasKey(evaluation => evaluation.Id);
            entity.Property(evaluation => evaluation.Comments).HasMaxLength(1_000).IsRequired();
            entity.HasIndex(evaluation => new { evaluation.TenantId, evaluation.SupplierId, evaluation.EvaluatedAtUtc });
        });

        modelBuilder.Entity<SupplierExpirationAlert>(entity =>
        {
            entity.ToTable("supplier_expiration_alerts");
            entity.HasKey(alert => alert.Id);
            entity.Property(alert => alert.DocumentType).HasConversion<string>().HasMaxLength(60).IsRequired();
            entity.Property(alert => alert.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(alert => new { alert.TenantId, alert.Status, alert.ExpiresAtUtc });
        });
    }

    private static void ConfigureAuditManagement(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditProgram>(entity =>
        {
            entity.ToTable("audit_programs");
            entity.HasKey(program => program.Id);
            entity.Property(program => program.Name).HasMaxLength(180).IsRequired();
            entity.Property(program => program.Code).HasMaxLength(80).IsRequired();
            entity.HasIndex(program => new { program.TenantId, program.Code }).IsUnique();
            entity.HasIndex(program => new { program.TenantId, program.Year, program.IsActive });
        });

        modelBuilder.Entity<AuditChecklist>(entity =>
        {
            entity.ToTable("audit_checklists");
            entity.HasKey(checklist => checklist.Id);
            entity.Property(checklist => checklist.Name).HasMaxLength(180).IsRequired();
            entity.Property(checklist => checklist.Code).HasMaxLength(80).IsRequired();
            entity.Property(checklist => checklist.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(checklist => new { checklist.TenantId, checklist.Code, checklist.Version }).IsUnique();
            entity.HasMany(checklist => checklist.Items).WithOne().HasForeignKey(item => item.ChecklistId);
            entity.Navigation(checklist => checklist.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<AuditChecklistItem>(entity =>
        {
            entity.ToTable("audit_checklist_items");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Clause).HasMaxLength(120).IsRequired();
            entity.Property(item => item.Question).HasMaxLength(1_000).IsRequired();
            entity.HasIndex(item => new { item.TenantId, item.ChecklistId, item.Clause });
        });

        modelBuilder.Entity<AuditPlan>(entity =>
        {
            entity.ToTable("audit_plans");
            entity.HasKey(plan => plan.Id);
            entity.Property(plan => plan.Scope).HasMaxLength(2_000).IsRequired();
            entity.Property(plan => plan.Criteria).HasMaxLength(2_000).IsRequired();
            entity.HasIndex(plan => new { plan.TenantId, plan.AuditProgramId, plan.PlannedStartUtc });
        });

        modelBuilder.Entity<ManagedAudit>(entity =>
        {
            entity.ToTable("managed_audits");
            entity.HasKey(audit => audit.Id);
            entity.Property(audit => audit.Title).HasMaxLength(220).IsRequired();
            entity.Property(audit => audit.Code).HasMaxLength(100).IsRequired();
            entity.Property(audit => audit.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(audit => audit.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(audit => new { audit.TenantId, audit.Code }).IsUnique();
            entity.HasIndex(audit => new { audit.TenantId, audit.Type, audit.Status });
            entity.HasIndex(audit => new { audit.TenantId, audit.ClosedAtUtc });
            entity.HasMany(audit => audit.Schedules).WithOne().HasForeignKey(schedule => schedule.AuditId);
            entity.HasMany(audit => audit.Participants).WithOne().HasForeignKey(participant => participant.AuditId);
            entity.HasMany(audit => audit.Areas).WithOne().HasForeignKey(area => area.AuditId);
            entity.HasMany(audit => audit.Findings).WithOne().HasForeignKey(finding => finding.AuditId);
            entity.HasMany(audit => audit.Evidence).WithOne().HasForeignKey(evidence => evidence.AuditId);
            entity.HasMany(audit => audit.Observations).WithOne().HasForeignKey(observation => observation.AuditId);
            entity.HasMany(audit => audit.NonConformities).WithOne().HasForeignKey(nonConformity => nonConformity.AuditId);
            entity.HasMany(audit => audit.Recommendations).WithOne().HasForeignKey(recommendation => recommendation.AuditId);
            entity.HasMany(audit => audit.CorrectiveActionLinks).WithOne().HasForeignKey(link => link.AuditId);
            entity.HasMany(audit => audit.History).WithOne().HasForeignKey(history => history.AuditId);
            entity.HasMany(audit => audit.Attachments).WithOne().HasForeignKey(attachment => attachment.AuditId);
            entity.Navigation(audit => audit.Schedules).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(audit => audit.Participants).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(audit => audit.Areas).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(audit => audit.Findings).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(audit => audit.Evidence).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(audit => audit.Observations).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(audit => audit.NonConformities).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(audit => audit.Recommendations).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(audit => audit.CorrectiveActionLinks).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(audit => audit.History).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(audit => audit.Attachments).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<AuditSchedule>(entity =>
        {
            entity.ToTable("audit_schedules");
            entity.HasKey(schedule => schedule.Id);
            entity.Property(schedule => schedule.Location).HasMaxLength(250).IsRequired();
            entity.HasIndex(schedule => new { schedule.TenantId, schedule.AuditId, schedule.StartUtc });
        });

        modelBuilder.Entity<AuditParticipant>(entity =>
        {
            entity.ToTable("audit_participants");
            entity.HasKey(participant => participant.Id);
            entity.Property(participant => participant.Role).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(participant => new { participant.TenantId, participant.AuditId, participant.UserId, participant.Role }).IsUnique();
        });

        modelBuilder.Entity<AuditAuditor>(entity =>
        {
            entity.ToTable("audit_auditors");
            entity.HasKey(auditor => auditor.Id);
            entity.HasIndex(auditor => new { auditor.TenantId, auditor.AuditId, auditor.UserId }).IsUnique();
        });

        modelBuilder.Entity<AuditArea>(entity =>
        {
            entity.ToTable("audit_areas");
            entity.HasKey(area => area.Id);
            entity.Property(area => area.Name).HasMaxLength(180).IsRequired();
            entity.Property(area => area.Process).HasMaxLength(180).IsRequired();
            entity.HasIndex(area => new { area.TenantId, area.AuditId, area.Name });
        });

        modelBuilder.Entity<AuditFinding>(entity =>
        {
            entity.ToTable("audit_findings");
            entity.HasKey(finding => finding.Id);
            entity.Property(finding => finding.Title).HasMaxLength(220).IsRequired();
            entity.Property(finding => finding.Description).HasMaxLength(2_000).IsRequired();
            entity.Property(finding => finding.Severity).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(finding => new { finding.TenantId, finding.AuditId, finding.Severity });
            entity.HasIndex(finding => new { finding.TenantId, finding.ReportedAtUtc });
        });

        modelBuilder.Entity<AuditEvidence>(entity =>
        {
            entity.ToTable("audit_evidence");
            entity.HasKey(evidence => evidence.Id);
            entity.Property(evidence => evidence.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(evidence => evidence.FileName).HasMaxLength(260).IsRequired();
            entity.Property(evidence => evidence.ContentType).HasMaxLength(120).IsRequired();
            entity.Property(evidence => evidence.Sha256Hash).HasMaxLength(128).IsRequired();
            entity.HasIndex(evidence => new { evidence.TenantId, evidence.FindingId });
            entity.HasIndex(evidence => new { evidence.TenantId, evidence.Sha256Hash });
        });

        modelBuilder.Entity<AuditObservation>(entity =>
        {
            entity.ToTable("audit_observations");
            entity.HasKey(observation => observation.Id);
            entity.Property(observation => observation.Description).HasMaxLength(2_000).IsRequired();
            entity.HasIndex(observation => new { observation.TenantId, observation.AuditId, observation.ReportedAtUtc });
        });

        modelBuilder.Entity<AuditNonConformity>(entity =>
        {
            entity.ToTable("audit_non_conformities");
            entity.HasKey(nonConformity => nonConformity.Id);
            entity.Property(nonConformity => nonConformity.Requirement).HasMaxLength(1_000).IsRequired();
            entity.HasIndex(nonConformity => new { nonConformity.TenantId, nonConformity.FindingId });
        });

        modelBuilder.Entity<AuditRecommendation>(entity =>
        {
            entity.ToTable("audit_recommendations");
            entity.HasKey(recommendation => recommendation.Id);
            entity.Property(recommendation => recommendation.Recommendation).HasMaxLength(1_000).IsRequired();
            entity.HasIndex(recommendation => new { recommendation.TenantId, recommendation.FindingId });
        });

        modelBuilder.Entity<AuditCorrectiveActionLink>(entity =>
        {
            entity.ToTable("audit_corrective_action_links");
            entity.HasKey(link => link.Id);
            entity.HasIndex(link => new { link.TenantId, link.FindingId, link.CorrectiveActionId }).IsUnique();
        });

        modelBuilder.Entity<AuditHistory>(entity =>
        {
            entity.ToTable("managed_audit_history");
            entity.HasKey(history => history.Id);
            entity.Property(history => history.Action).HasMaxLength(500).IsRequired();
            entity.HasIndex(history => new { history.TenantId, history.AuditId, history.OccurredAtUtc });
        });

        modelBuilder.Entity<AuditAttachment>(entity =>
        {
            entity.ToTable("audit_attachments");
            entity.HasKey(attachment => attachment.Id);
            entity.Property(attachment => attachment.FileName).HasMaxLength(260).IsRequired();
            entity.Property(attachment => attachment.ContentType).HasMaxLength(120).IsRequired();
            entity.Property(attachment => attachment.Sha256Hash).HasMaxLength(128).IsRequired();
            entity.HasIndex(attachment => new { attachment.TenantId, attachment.AuditId });
        });
    }

    private static void ConfigureCapaManagement(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Capa>(entity =>
        {
            entity.ToTable("capas");
            entity.HasKey(capa => capa.Id);
            entity.Property(capa => capa.Title).HasMaxLength(220).IsRequired();
            entity.Property(capa => capa.Code).HasMaxLength(100).IsRequired();
            entity.Property(capa => capa.Description).HasMaxLength(2_000).IsRequired();
            entity.Property(capa => capa.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(capa => capa.Priority).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(capa => capa.RiskLevel).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(capa => capa.SourceType).HasConversion<string>().HasMaxLength(60).IsRequired();
            entity.HasIndex(capa => new { capa.TenantId, capa.Code }).IsUnique();
            entity.HasIndex(capa => new { capa.TenantId, capa.Status, capa.Priority, capa.RiskLevel });
            entity.HasIndex(capa => new { capa.TenantId, capa.SupplierId });
            entity.HasIndex(capa => new { capa.TenantId, capa.AuditId });
            entity.HasIndex(capa => new { capa.TenantId, capa.CommitmentDueAtUtc });
            entity.HasMany(capa => capa.Owners).WithOne().HasForeignKey(owner => owner.CapaId);
            entity.HasMany(capa => capa.Approvers).WithOne().HasForeignKey(approver => approver.CapaId);
            entity.HasMany(capa => capa.RootCauses).WithOne().HasForeignKey(rootCause => rootCause.CapaId);
            entity.HasMany(capa => capa.CauseAnalyses).WithOne().HasForeignKey(analysis => analysis.CapaId);
            entity.HasMany(capa => capa.ContainmentActions).WithOne().HasForeignKey(action => action.CapaId);
            entity.HasMany(capa => capa.CorrectiveActions).WithOne().HasForeignKey(action => action.CapaId);
            entity.HasMany(capa => capa.PreventiveActions).WithOne().HasForeignKey(action => action.CapaId);
            entity.HasMany(capa => capa.EffectivenessChecks).WithOne().HasForeignKey(check => check.CapaId);
            entity.HasMany(capa => capa.Evidence).WithOne().HasForeignKey(evidence => evidence.CapaId);
            entity.HasMany(capa => capa.Attachments).WithOne().HasForeignKey(attachment => attachment.CapaId);
            entity.HasMany(capa => capa.History).WithOne().HasForeignKey(history => history.CapaId);
            entity.Navigation(capa => capa.Owners).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(capa => capa.Approvers).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(capa => capa.RootCauses).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(capa => capa.CauseAnalyses).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(capa => capa.ContainmentActions).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(capa => capa.CorrectiveActions).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(capa => capa.PreventiveActions).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(capa => capa.EffectivenessChecks).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(capa => capa.Evidence).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(capa => capa.Attachments).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(capa => capa.History).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<CapaOwner>(entity =>
        {
            entity.ToTable("capa_owners");
            entity.HasKey(owner => owner.Id);
            entity.HasIndex(owner => new { owner.TenantId, owner.UserId, owner.IsActive, owner.DueAtUtc });
            entity.HasIndex(owner => new { owner.TenantId, owner.CapaId, owner.UserId }).IsUnique();
        });

        modelBuilder.Entity<CapaApprover>(entity =>
        {
            entity.ToTable("capa_approvers");
            entity.HasKey(approver => approver.Id);
            entity.HasIndex(approver => new { approver.TenantId, approver.CapaId, approver.UserId }).IsUnique();
        });

        modelBuilder.Entity<CapaRootCause>(entity =>
        {
            entity.ToTable("capa_root_causes");
            entity.HasKey(rootCause => rootCause.Id);
            entity.Property(rootCause => rootCause.Description).HasMaxLength(2_000).IsRequired();
            entity.Property(rootCause => rootCause.Method).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(rootCause => new { rootCause.TenantId, rootCause.CapaId, rootCause.Method });
        });

        modelBuilder.Entity<CapaCauseAnalysis>(entity =>
        {
            entity.ToTable("capa_cause_analyses");
            entity.HasKey(analysis => analysis.Id);
            entity.Property(analysis => analysis.Method).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(analysis => analysis.Why1).HasMaxLength(500);
            entity.Property(analysis => analysis.Why2).HasMaxLength(500);
            entity.Property(analysis => analysis.Why3).HasMaxLength(500);
            entity.Property(analysis => analysis.Why4).HasMaxLength(500);
            entity.Property(analysis => analysis.Why5).HasMaxLength(500);
            entity.Property(analysis => analysis.People).HasMaxLength(500);
            entity.Property(analysis => analysis.Process).HasMaxLength(500);
            entity.Property(analysis => analysis.Equipment).HasMaxLength(500);
            entity.Property(analysis => analysis.Material).HasMaxLength(500);
            entity.Property(analysis => analysis.Environment).HasMaxLength(500);
            entity.Property(analysis => analysis.Measurement).HasMaxLength(500);
            entity.HasIndex(analysis => new { analysis.TenantId, analysis.CapaId, analysis.Method });
        });

        ConfigureCapaActions(modelBuilder);

        modelBuilder.Entity<CapaEffectivenessCheck>(entity =>
        {
            entity.ToTable("capa_effectiveness_checks");
            entity.HasKey(check => check.Id);
            entity.Property(check => check.VerificationSummary).HasMaxLength(1_000).IsRequired();
            entity.HasIndex(check => new { check.TenantId, check.CapaId, check.VerifiedAtUtc });
            entity.HasIndex(check => new { check.TenantId, check.IsEffective });
        });

        modelBuilder.Entity<CapaEvidence>(entity =>
        {
            entity.ToTable("capa_evidence");
            entity.HasKey(evidence => evidence.Id);
            entity.Property(evidence => evidence.FileName).HasMaxLength(260).IsRequired();
            entity.Property(evidence => evidence.ContentType).HasMaxLength(120).IsRequired();
            entity.Property(evidence => evidence.Sha256Hash).HasMaxLength(128).IsRequired();
            entity.HasIndex(evidence => new { evidence.TenantId, evidence.CapaId });
            entity.HasIndex(evidence => new { evidence.TenantId, evidence.Sha256Hash });
        });

        modelBuilder.Entity<CapaAttachment>(entity =>
        {
            entity.ToTable("capa_attachments");
            entity.HasKey(attachment => attachment.Id);
            entity.Property(attachment => attachment.FileName).HasMaxLength(260).IsRequired();
            entity.Property(attachment => attachment.ContentType).HasMaxLength(120).IsRequired();
            entity.Property(attachment => attachment.Sha256Hash).HasMaxLength(128).IsRequired();
            entity.HasIndex(attachment => new { attachment.TenantId, attachment.CapaId });
        });

        modelBuilder.Entity<CapaHistory>(entity =>
        {
            entity.ToTable("capa_history");
            entity.HasKey(history => history.Id);
            entity.Property(history => history.Action).HasMaxLength(1_200).IsRequired();
            entity.HasIndex(history => new { history.TenantId, history.CapaId, history.OccurredAtUtc });
        });
    }

    private static void ConfigureCapaActions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CapaContainmentAction>(entity =>
        {
            entity.ToTable("capa_containment_actions");
            entity.HasKey(action => action.Id);
            entity.Property(action => action.Description).HasMaxLength(1_000).IsRequired();
            entity.Property(action => action.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(action => action.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(action => new { action.TenantId, action.CapaId, action.Status, action.DueAtUtc });
        });

        modelBuilder.Entity<CapaCorrectiveAction>(entity =>
        {
            entity.ToTable("capa_corrective_actions");
            entity.HasKey(action => action.Id);
            entity.Property(action => action.Description).HasMaxLength(1_000).IsRequired();
            entity.Property(action => action.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(action => action.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(action => new { action.TenantId, action.CapaId, action.Status, action.DueAtUtc });
        });

        modelBuilder.Entity<CapaPreventiveAction>(entity =>
        {
            entity.ToTable("capa_preventive_actions");
            entity.HasKey(action => action.Id);
            entity.Property(action => action.Description).HasMaxLength(1_000).IsRequired();
            entity.Property(action => action.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(action => action.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(action => new { action.TenantId, action.CapaId, action.Status, action.DueAtUtc });
        });
    }

    private static void ConfigureRiskManagement(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RiskCategory>(entity =>
        {
            entity.ToTable("risk_categories");
            entity.HasKey(category => category.Id);
            entity.Property(category => category.Name).HasMaxLength(180).IsRequired();
            entity.Property(category => category.Code).HasMaxLength(80).IsRequired();
            entity.HasIndex(category => new { category.TenantId, category.Code }).IsUnique();
        });

        modelBuilder.Entity<RiskMatrix>(entity =>
        {
            entity.ToTable("risk_matrices");
            entity.HasKey(matrix => matrix.Id);
            entity.Property(matrix => matrix.Name).HasMaxLength(180).IsRequired();
            entity.HasIndex(matrix => new { matrix.TenantId, matrix.IsDefault });
        });

        modelBuilder.Entity<Risk>(entity =>
        {
            entity.ToTable("risks");
            entity.HasKey(risk => risk.Id);
            entity.Property(risk => risk.Title).HasMaxLength(220).IsRequired();
            entity.Property(risk => risk.Code).HasMaxLength(100).IsRequired();
            entity.Property(risk => risk.Description).HasMaxLength(2_000).IsRequired();
            entity.Property(risk => risk.Type).HasConversion<string>().HasMaxLength(60).IsRequired();
            entity.Property(risk => risk.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(risk => risk.InherentLevel).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(risk => risk.ResidualLevel).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(risk => risk.Area).HasMaxLength(160).IsRequired();
            entity.Property(risk => risk.Process).HasMaxLength(160).IsRequired();
            entity.HasIndex(risk => new { risk.TenantId, risk.Code }).IsUnique();
            entity.HasIndex(risk => new { risk.TenantId, risk.Status, risk.Type, risk.ResidualLevel });
            entity.HasIndex(risk => new { risk.TenantId, risk.Area, risk.Process });
            entity.HasIndex(risk => new { risk.TenantId, risk.SupplierId });
            entity.HasIndex(risk => new { risk.TenantId, risk.AuditId });
            entity.HasIndex(risk => new { risk.TenantId, risk.CapaId });
            entity.HasMany(risk => risk.Assessments).WithOne().HasForeignKey(assessment => assessment.RiskId);
            entity.HasMany(risk => risk.Treatments).WithOne().HasForeignKey(treatment => treatment.RiskId);
            entity.HasMany(risk => risk.MitigationPlans).WithOne().HasForeignKey(plan => plan.RiskId);
            entity.HasMany(risk => risk.Controls).WithOne().HasForeignKey(control => control.RiskId);
            entity.HasMany(risk => risk.Owners).WithOne().HasForeignKey(owner => owner.RiskId);
            entity.HasMany(risk => risk.Reviews).WithOne().HasForeignKey(review => review.RiskId);
            entity.HasMany(risk => risk.Evidence).WithOne().HasForeignKey(evidence => evidence.RiskId);
            entity.HasMany(risk => risk.Indicators).WithOne().HasForeignKey(indicator => indicator.RiskId);
            entity.HasMany(risk => risk.Attachments).WithOne().HasForeignKey(attachment => attachment.RiskId);
            entity.HasMany(risk => risk.History).WithOne().HasForeignKey(history => history.RiskId);
            entity.Navigation(risk => risk.Assessments).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(risk => risk.Treatments).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(risk => risk.MitigationPlans).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(risk => risk.Controls).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(risk => risk.Owners).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(risk => risk.Reviews).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(risk => risk.Evidence).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(risk => risk.Indicators).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(risk => risk.Attachments).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(risk => risk.History).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<RiskAssessment>(entity =>
        {
            entity.ToTable("risk_assessments");
            entity.HasKey(assessment => assessment.Id);
            entity.Property(assessment => assessment.Probability).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(assessment => assessment.Impact).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(assessment => assessment.InherentLevel).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(assessment => assessment.ResidualProbability).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(assessment => assessment.ResidualImpact).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(assessment => assessment.ResidualLevel).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(assessment => new { assessment.TenantId, assessment.RiskId, assessment.AssessedAtUtc });
        });

        modelBuilder.Entity<RiskTreatment>(entity =>
        {
            entity.ToTable("risk_treatments");
            entity.HasKey(treatment => treatment.Id);
            entity.Property(treatment => treatment.Strategy).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(treatment => treatment.Rationale).HasMaxLength(1_000).IsRequired();
        });

        modelBuilder.Entity<RiskMitigationPlan>(entity =>
        {
            entity.ToTable("risk_mitigation_plans");
            entity.HasKey(plan => plan.Id);
            entity.Property(plan => plan.Description).HasMaxLength(1_000).IsRequired();
            entity.HasIndex(plan => new { plan.TenantId, plan.ResponsibleUserId, plan.DueAtUtc, plan.IsCompleted });
        });

        modelBuilder.Entity<RiskControl>(entity =>
        {
            entity.ToTable("risk_controls");
            entity.HasKey(control => control.Id);
            entity.Property(control => control.Name).HasMaxLength(180).IsRequired();
            entity.Property(control => control.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(control => control.Description).HasMaxLength(1_000).IsRequired();
            entity.HasIndex(control => new { control.TenantId, control.RiskId, control.Type });
        });

        modelBuilder.Entity<RiskOwner>(entity =>
        {
            entity.ToTable("risk_owners");
            entity.HasKey(owner => owner.Id);
            entity.HasIndex(owner => new { owner.TenantId, owner.RiskId, owner.UserId }).IsUnique();
        });

        modelBuilder.Entity<RiskReview>(entity =>
        {
            entity.ToTable("risk_reviews");
            entity.HasKey(review => review.Id);
            entity.Property(review => review.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(review => review.Summary).HasMaxLength(1_000);
            entity.HasIndex(review => new { review.TenantId, review.RiskId, review.DueAtUtc, review.Status });
        });

        modelBuilder.Entity<RiskEvidence>(entity =>
        {
            entity.ToTable("risk_evidence");
            entity.HasKey(evidence => evidence.Id);
            entity.Property(evidence => evidence.FileName).HasMaxLength(260).IsRequired();
            entity.Property(evidence => evidence.ContentType).HasMaxLength(120).IsRequired();
            entity.Property(evidence => evidence.Sha256Hash).HasMaxLength(128).IsRequired();
            entity.HasIndex(evidence => new { evidence.TenantId, evidence.RiskId });
        });

        modelBuilder.Entity<RiskIndicator>(entity =>
        {
            entity.ToTable("risk_indicators");
            entity.HasKey(indicator => indicator.Id);
            entity.Property(indicator => indicator.Name).HasMaxLength(180).IsRequired();
            entity.Property(indicator => indicator.Value).HasPrecision(18, 4);
            entity.Property(indicator => indicator.Threshold).HasPrecision(18, 4);
            entity.HasIndex(indicator => new { indicator.TenantId, indicator.RiskId, indicator.IsBreached });
        });

        modelBuilder.Entity<RiskAttachment>(entity =>
        {
            entity.ToTable("risk_attachments");
            entity.HasKey(attachment => attachment.Id);
            entity.Property(attachment => attachment.FileName).HasMaxLength(260).IsRequired();
            entity.Property(attachment => attachment.ContentType).HasMaxLength(120).IsRequired();
            entity.Property(attachment => attachment.Sha256Hash).HasMaxLength(128).IsRequired();
        });

        modelBuilder.Entity<RiskHistory>(entity =>
        {
            entity.ToTable("risk_history");
            entity.HasKey(history => history.Id);
            entity.Property(history => history.Action).HasMaxLength(1_200).IsRequired();
            entity.HasIndex(history => new { history.TenantId, history.RiskId, history.OccurredAtUtc });
        });
    }

    private static void ConfigureQualityIndicators(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IndicatorCategory>(entity =>
        {
            entity.ToTable("indicator_categories");
            entity.HasKey(category => category.Id);
            entity.Property(category => category.Name).HasMaxLength(180).IsRequired();
            entity.Property(category => category.Code).HasMaxLength(80).IsRequired();
            entity.HasIndex(category => new { category.TenantId, category.Code }).IsUnique();
        });

        modelBuilder.Entity<QualityIndicator>(entity =>
        {
            entity.ToTable("quality_indicators");
            entity.HasKey(indicator => indicator.Id);
            entity.Property(indicator => indicator.Name).HasMaxLength(220).IsRequired();
            entity.Property(indicator => indicator.Code).HasMaxLength(100).IsRequired();
            entity.Property(indicator => indicator.Description).HasMaxLength(2_000).IsRequired();
            entity.Property(indicator => indicator.Type).HasConversion<string>().HasMaxLength(60).IsRequired();
            entity.Property(indicator => indicator.Frequency).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(indicator => indicator.CalculationType).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(indicator => indicator.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(indicator => indicator.Unit).HasMaxLength(40).IsRequired();
            entity.HasIndex(indicator => new { indicator.TenantId, indicator.Code }).IsUnique();
            entity.HasIndex(indicator => new { indicator.TenantId, indicator.Status, indicator.Type, indicator.Frequency });
            entity.HasIndex(indicator => new { indicator.TenantId, indicator.SupplierId });
            entity.HasIndex(indicator => new { indicator.TenantId, indicator.AuditId });
            entity.HasIndex(indicator => new { indicator.TenantId, indicator.CapaId });
            entity.HasIndex(indicator => new { indicator.TenantId, indicator.RiskId });
            entity.HasMany(indicator => indicator.Formulas).WithOne().HasForeignKey(formula => formula.IndicatorId);
            entity.HasMany(indicator => indicator.Targets).WithOne().HasForeignKey(target => target.IndicatorId);
            entity.HasMany(indicator => indicator.Thresholds).WithOne().HasForeignKey(threshold => threshold.IndicatorId);
            entity.HasMany(indicator => indicator.Measurements).WithOne().HasForeignKey(measurement => measurement.IndicatorId);
            entity.HasMany(indicator => indicator.Results).WithOne().HasForeignKey(result => result.IndicatorId);
            entity.HasMany(indicator => indicator.Periods).WithOne().HasForeignKey(period => period.IndicatorId);
            entity.HasMany(indicator => indicator.Processes).WithOne().HasForeignKey(process => process.IndicatorId);
            entity.HasMany(indicator => indicator.Alerts).WithOne().HasForeignKey(alert => alert.IndicatorId);
            entity.HasMany(indicator => indicator.Trends).WithOne().HasForeignKey(trend => trend.IndicatorId);
            entity.HasMany(indicator => indicator.History).WithOne().HasForeignKey(history => history.IndicatorId);
            entity.HasMany(indicator => indicator.Attachments).WithOne().HasForeignKey(attachment => attachment.IndicatorId);
            entity.Navigation(indicator => indicator.Formulas).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(indicator => indicator.Targets).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(indicator => indicator.Thresholds).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(indicator => indicator.Measurements).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(indicator => indicator.Results).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(indicator => indicator.Periods).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(indicator => indicator.Processes).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(indicator => indicator.Alerts).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(indicator => indicator.Trends).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(indicator => indicator.History).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(indicator => indicator.Attachments).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<IndicatorFormula>(entity =>
        {
            entity.ToTable("indicator_formulas");
            entity.HasKey(formula => formula.Id);
            entity.Property(formula => formula.Expression).HasMaxLength(1_000).IsRequired();
            entity.Property(formula => formula.CalculationType).HasConversion<string>().HasMaxLength(40).IsRequired();
        });

        modelBuilder.Entity<IndicatorTarget>(entity =>
        {
            entity.ToTable("indicator_targets");
            entity.HasKey(target => target.Id);
            entity.Property(target => target.TargetValue).HasPrecision(18, 4);
        });

        modelBuilder.Entity<IndicatorThreshold>(entity =>
        {
            entity.ToTable("indicator_thresholds");
            entity.HasKey(threshold => threshold.Id);
            entity.Property(threshold => threshold.WarningMinimum).HasPrecision(18, 4);
            entity.Property(threshold => threshold.CriticalMinimum).HasPrecision(18, 4);
            entity.Property(threshold => threshold.ExcellentMinimum).HasPrecision(18, 4);
        });

        modelBuilder.Entity<IndicatorPeriod>(entity =>
        {
            entity.ToTable("indicator_periods");
            entity.HasKey(period => period.Id);
            entity.HasIndex(period => new { period.TenantId, period.IndicatorId, period.Year, period.PeriodNumber }).IsUnique();
        });

        modelBuilder.Entity<IndicatorProcess>(entity =>
        {
            entity.ToTable("indicator_processes");
            entity.HasKey(process => process.Id);
            entity.Property(process => process.ProcessName).HasMaxLength(180).IsRequired();
            entity.Property(process => process.Area).HasMaxLength(160).IsRequired();
            entity.HasIndex(process => new { process.TenantId, process.Area, process.ProcessName });
        });

        modelBuilder.Entity<IndicatorMeasurement>(entity =>
        {
            entity.ToTable("indicator_measurements");
            entity.HasKey(measurement => measurement.Id);
            entity.Property(measurement => measurement.Numerator).HasPrecision(18, 4);
            entity.Property(measurement => measurement.Denominator).HasPrecision(18, 4);
            entity.HasIndex(measurement => new { measurement.TenantId, measurement.IndicatorId, measurement.PeriodId });
        });

        modelBuilder.Entity<IndicatorResult>(entity =>
        {
            entity.ToTable("indicator_results");
            entity.HasKey(result => result.Id);
            entity.Property(result => result.Value).HasPrecision(18, 4);
            entity.Property(result => result.TargetValue).HasPrecision(18, 4);
            entity.Property(result => result.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasIndex(result => new { result.TenantId, result.IndicatorId, result.PeriodId, result.Status });
        });

        modelBuilder.Entity<IndicatorAlert>(entity =>
        {
            entity.ToTable("indicator_alerts");
            entity.HasKey(alert => alert.Id);
            entity.Property(alert => alert.Type).HasConversion<string>().HasMaxLength(60).IsRequired();
            entity.HasIndex(alert => new { alert.TenantId, alert.IndicatorId, alert.Type, alert.IsAcknowledged });
        });

        modelBuilder.Entity<IndicatorTrend>(entity =>
        {
            entity.ToTable("indicator_trends");
            entity.HasKey(trend => trend.Id);
            entity.Property(trend => trend.Direction).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(trend => trend.Value).HasPrecision(18, 4);
            entity.Property(trend => trend.PreviousValue).HasPrecision(18, 4);
            entity.HasIndex(trend => new { trend.TenantId, trend.IndicatorId, trend.Direction });
        });

        modelBuilder.Entity<IndicatorHistory>(entity =>
        {
            entity.ToTable("indicator_history");
            entity.HasKey(history => history.Id);
            entity.Property(history => history.Action).HasMaxLength(1_200).IsRequired();
            entity.HasIndex(history => new { history.TenantId, history.IndicatorId, history.OccurredAtUtc });
        });

        modelBuilder.Entity<IndicatorAttachment>(entity =>
        {
            entity.ToTable("indicator_attachments");
            entity.HasKey(attachment => attachment.Id);
            entity.Property(attachment => attachment.FileName).HasMaxLength(260).IsRequired();
            entity.Property(attachment => attachment.ContentType).HasMaxLength(120).IsRequired();
            entity.Property(attachment => attachment.Sha256Hash).HasMaxLength(128).IsRequired();
        });
    }

    private void ApplyFoundationRules()
    {
        foreach (var entry in ChangeTracker.Entries<AuditLog>())
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                throw new DomainException("Audit logs are append-only.");
            }
        }

        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.MarkUpdated(_clock.UtcNow);
            }
        }
    }
}
