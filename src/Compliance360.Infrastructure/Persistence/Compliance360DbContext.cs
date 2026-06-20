using Compliance360.Application;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Identity;
using Compliance360.Domain.Storage;
using Compliance360.Domain.TenantManagement;
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
            entity.Property(auditLog => auditLog.IpAddress).HasMaxLength(80);
            entity.Property(auditLog => auditLog.UserAgent).HasMaxLength(500);
            entity.Property(auditLog => auditLog.CorrelationId).HasMaxLength(120);
            entity.HasIndex(auditLog => new { auditLog.TenantId, auditLog.OccurredAtUtc });
            entity.HasIndex(auditLog => new { auditLog.EntityName, auditLog.EntityId });
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
