using Compliance360.Application;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Documents;
using Compliance360.Domain.Identity;
using Compliance360.Domain.Notifications;
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
