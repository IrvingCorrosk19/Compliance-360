using Compliance360.Application;
using Compliance360.Application.Audit;
using Compliance360.Application.Identity;
using Compliance360.Application.Mfa;
using Compliance360.Application.Rbac;
using Compliance360.Application.TenantManagement;
using Compliance360.Infrastructure.Audit;
using Compliance360.Infrastructure.Identity;
using Compliance360.Infrastructure.Mfa;
using Compliance360.Infrastructure.Persistence;
using Compliance360.Infrastructure.Rbac;
using Compliance360.Infrastructure.Security;
using Compliance360.Infrastructure.Storage;
using Compliance360.Infrastructure.TenantManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Compliance360.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<RefreshTokenOptions>(configuration.GetSection(RefreshTokenOptions.SectionName));
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.Configure<PasswordPolicyOptions>(configuration.GetSection(PasswordPolicyOptions.SectionName));
        services.Configure<LockoutOptions>(configuration.GetSection(LockoutOptions.SectionName));
        services.Configure<AuditOptions>(configuration.GetSection(AuditOptions.SectionName));

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IAuditContextAccessor, AuditContextAccessor>();
        services.AddScoped<IAuditPermissionEvaluator, AuditPermissionEvaluator>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<IMfaSecretProtector, Base64MfaSecretProtector>();
        services.AddScoped<ITotpService, TotpService>();
        services.AddScoped<IMfaService, MfaService>();
        services.AddScoped<IPasswordPolicyValidator, PasswordPolicyValidator>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<ITenantManagementService, TenantManagementService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IRbacService, RbacService>();

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
            services.AddScoped<ITenantManagementRepository, EfTenantManagementRepository>();
            services.AddScoped<IIdentityRepository, EfIdentityRepository>();
            services.AddScoped<IRbacRepository, EfRbacRepository>();
            services.AddScoped<IMfaRepository, EfMfaRepository>();
        }

        return services;
    }
}
