using Compliance360.Application;
using Compliance360.Application.TenantManagement;
using Compliance360.Infrastructure.Persistence;
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

        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<ITenantManagementService, TenantManagementService>();

        var connectionString = configuration.GetConnectionString("Compliance360");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<Compliance360DbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<Compliance360DbContext>());
            services.AddScoped<ITenantManagementRepository, EfTenantManagementRepository>();
        }

        return services;
    }
}
