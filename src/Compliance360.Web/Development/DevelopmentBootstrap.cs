using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using Compliance360.Application;
using Compliance360.Application.Identity;
using Compliance360.Application.Rbac;
using Compliance360.Application.Notifications;
using Compliance360.Application.Storage;
using Compliance360.Domain.Common;
using Compliance360.Domain.Identity;
using Compliance360.Domain.TenantManagement;
using Compliance360.Infrastructure.Persistence;
using Compliance360.Web.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Formatting.Display;

namespace Compliance360.Web.Development;

public static class DevelopmentBootstrapServiceCollectionExtensions
{
    public static IServiceCollection AddDevelopmentBootstrap(this IServiceCollection services)
    {
        services.AddSingleton<DevelopmentBootstrapRunner>();
        return services;
    }
}

public static class DevelopmentBootstrapRuntime
{
    public static bool IsTestHost =>
        Assembly.GetEntryAssembly()?.GetName().Name?.Contains("testhost", StringComparison.OrdinalIgnoreCase) == true;
}

public sealed record DevelopmentBootstrapPrecheckResult(bool CanContinue);

public static class DevelopmentBootstrapPrecheck
{
    public static DevelopmentBootstrapPrecheckResult Run(IConfiguration configuration)
    {
        var ports = GetConfiguredPorts(configuration).ToArray();
        foreach (var port in ports)
        {
            var listener = PortListenerInspector.GetListeners(port).FirstOrDefault();
            if (listener is null)
            {
                continue;
            }

            if (listener.IsCompliance360Process)
            {
                DevelopmentBootstrapConsole.WriteAlreadyRunning(listener, BuildLocalUrl(port));
                return new DevelopmentBootstrapPrecheckResult(false);
            }

            DevelopmentBootstrapConsole.WriteFatal($"""
                Port {port} is already in use by another process.

                PID: {listener.ProcessId}
                Process: {listener.ProcessName}
                Executable: {listener.ExecutablePath ?? "unknown"}
                URL: {BuildLocalUrl(port)}

                Stop that process or choose an explicit development URL.
                """);
            return new DevelopmentBootstrapPrecheckResult(false);
        }

        return new DevelopmentBootstrapPrecheckResult(true);
    }

    private static IEnumerable<int> GetConfiguredPorts(IConfiguration configuration)
    {
        var explicitCandidates = new[]
        {
            configuration["urls"],
            Environment.GetEnvironmentVariable("ASPNETCORE_URLS"),
            Environment.GetEnvironmentVariable("DOTNET_URLS")
        }
        .Concat(GetCommandLineUrls())
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .ToArray();

        var candidates = explicitCandidates.Length > 0
            ? explicitCandidates
            : ["http://localhost:5272"];

        return candidates
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .SelectMany(value => value!.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Select(ParsePort)
            .Where(port => port > 0)
            .Distinct();
    }

    private static IEnumerable<string> GetCommandLineUrls()
    {
        var args = Environment.GetCommandLineArgs();
        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (arg.Equals("--urls", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
            {
                yield return args[index + 1];
            }
            else if (arg.StartsWith("--urls=", StringComparison.OrdinalIgnoreCase))
            {
                yield return arg["--urls=".Length..];
            }
        }
    }

    private static int ParsePort(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            ? uri.Port
            : 0;
    }

    private static string BuildLocalUrl(int port)
    {
        return $"http://localhost:{port.ToString(CultureInfo.InvariantCulture)}";
    }
}

public sealed record PortListener(int Port, int ProcessId, string ProcessName, string? ExecutablePath)
{
    public bool IsCompliance360Process =>
        ProcessName.Contains("Compliance360", StringComparison.OrdinalIgnoreCase)
        || (ExecutablePath?.Contains("Compliance 360", StringComparison.OrdinalIgnoreCase) ?? false);
}

public static class PortListenerInspector
{
    public static IReadOnlyCollection<PortListener> GetListeners(int port)
    {
        if (!OperatingSystem.IsWindows())
        {
            return IsPortOpen(port)
                ? [new PortListener(port, 0, "unknown", null)]
                : [];
        }

        return GetWindowsTcpListeners(port);
    }

    private static bool IsPortOpen(int port)
    {
        return IPGlobalProperties.GetIPGlobalProperties()
            .GetActiveTcpListeners()
            .Any(endpoint => endpoint.Port == port);
    }

    private static IReadOnlyCollection<PortListener> GetWindowsTcpListeners(int port)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "netstat",
            Arguments = "-ano -p tcp",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        });

        if (process is null)
        {
            return [];
        }

        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit(5_000);

        var listeners = new Dictionary<int, PortListener>();
        foreach (var line in output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!line.StartsWith("TCP", StringComparison.OrdinalIgnoreCase) || !line.Contains("LISTENING", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length < 5 || !LocalAddressMatchesPort(parts[1], port) || !int.TryParse(parts[^1], out var processId))
            {
                continue;
            }

            listeners[processId] = BuildListener(port, processId);
        }

        return listeners.Values.ToArray();
    }

    private static bool LocalAddressMatchesPort(string localAddress, int port)
    {
        var portText = port.ToString(CultureInfo.InvariantCulture);
        return localAddress.EndsWith($":{portText}", StringComparison.Ordinal);
    }

    private static PortListener BuildListener(int port, int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            return new PortListener(port, processId, process.ProcessName, TryGetPath(process));
        }
        catch
        {
            return new PortListener(port, processId, "unknown", null);
        }
    }

    private static string? TryGetPath(Process process)
    {
        try
        {
            return process.MainModule?.FileName;
        }
        catch
        {
            return null;
        }
    }
}

public sealed class DevelopmentBootstrapRunner
{
    private static readonly string[] CriticalTables =
    [
        "tenants",
        "tenant_settings",
        "tenant_branding",
        "subscriptions",
        "users",
        "roles",
        "permissions",
        "user_roles",
        "role_permissions",
        "audit_logs"
    ];

    private static readonly string[] CriticalIndexes =
    [
        "IX_permissions_Code",
        "IX_roles_TenantId_NormalizedName",
        "IX_users_TenantId_NormalizedEmail",
        "IX_user_roles_TenantId_UserId_RoleId",
        "IX_role_permissions_TenantId_RoleId_PermissionId"
    ];

    private static readonly string[] CriticalConstraints =
    [
        "PK_tenants",
        "PK_users",
        "PK_roles",
        "PK_permissions",
        "PK_user_roles",
        "PK_role_permissions",
        "FK_user_roles_users_UserId",
        "FK_role_permissions_roles_RoleId"
    ];

    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public DevelopmentBootstrapRunner(IServiceProvider serviceProvider, IConfiguration configuration, IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<DevelopmentBootstrapResult> RunAsync(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return DevelopmentBootstrapResult.Start();
        }

        var options = DevelopmentBootstrapOptions.From(_configuration);
        var checks = new List<DevelopmentBootstrapCheck>();
        DevelopmentBootstrapConsole.WriteHeader(_environment.EnvironmentName, options.ApplicationVersion);
        DevelopmentBootstrapConsole.WriteEnvironmentDiagnostics(DevelopmentEnvironmentDiagnostics.Capture(_environment.EnvironmentName, options.ApplicationVersion));

        if (!options.Enabled)
        {
            DevelopmentBootstrapConsole.WriteFatal("Development bootstrap is disabled.");
            return DevelopmentBootstrapResult.Stop();
        }

        if (string.IsNullOrWhiteSpace(options.SuperAdminPassword))
        {
            DevelopmentBootstrapConsole.WriteFatal("BootstrapSuperAdmin:Password is required in Development. The value is never written to console or logs.");
            return DevelopmentBootstrapResult.Stop();
        }

        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Compliance360DbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var rbacProvisioning = scope.ServiceProvider.GetRequiredService<IRbacProvisioningService>();

        try
        {
            AddCheck(checks, "Application", DevelopmentBootstrapStatus.Ok, "Application services are built and bootstrap can run.");

            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            AddCheck(checks, "Database", canConnect ? DevelopmentBootstrapStatus.Ok : DevelopmentBootstrapStatus.Error, canConnect ? "PostgreSQL connection is available." : "PostgreSQL is not reachable.");
            if (!canConnect)
            {
                DevelopmentBootstrapConsole.WriteFatal("PostgreSQL is not reachable with the configured connection string.");
                return DevelopmentBootstrapResult.Stop();
            }

            var postgresVersion = await GetPostgreSqlVersionAsync(dbContext, cancellationToken);
            AddCheck(checks, "PostgreSQL Version", string.IsNullOrWhiteSpace(postgresVersion) ? DevelopmentBootstrapStatus.Warning : DevelopmentBootstrapStatus.Ok, string.IsNullOrWhiteSpace(postgresVersion) ? "PostgreSQL version could not be resolved." : postgresVersion);

            if (options.ApplyMigrations)
            {
                await dbContext.Database.MigrateAsync(cancellationToken);
            }

            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
            AddCheck(checks, "Migrations", pendingMigrations.Length == 0 ? DevelopmentBootstrapStatus.Ok : DevelopmentBootstrapStatus.Error, pendingMigrations.Length == 0 ? "Current EF migration is applied." : $"Pending migrations: {string.Join(", ", pendingMigrations)}");
            if (pendingMigrations.Length > 0)
            {
                DevelopmentBootstrapConsole.WriteFatal("There are pending EF migrations after bootstrap. Application startup stopped.");
                return DevelopmentBootstrapResult.Stop();
            }

            var schemaValidation = await ValidateSchemaAsync(dbContext, cancellationToken);
            AddCheck(checks, "Schema", schemaValidation.IsValid ? DevelopmentBootstrapStatus.Ok : DevelopmentBootstrapStatus.Error, schemaValidation.Message);
            AddCheck(checks, "Schema Version", schemaValidation.IsValid ? DevelopmentBootstrapStatus.Ok : DevelopmentBootstrapStatus.Error, $"Current={schemaValidation.CurrentMigration ?? "none"}; Expected={schemaValidation.ExpectedMigration ?? "none"}");
            if (!schemaValidation.IsValid)
            {
                DevelopmentBootstrapConsole.WriteFatal(schemaValidation.Message);
                return DevelopmentBootstrapResult.Stop();
            }

            await EnsureTenantAsync(dbContext, options, cancellationToken);
            AddCheck(checks, "Tenant Bootstrap", DevelopmentBootstrapStatus.Ok, "Technical tenant is present and active.");

            await rbacProvisioning.EnsurePermissionCatalogAsync(cancellationToken);
            AddCheck(checks, "Permissions", DevelopmentBootstrapStatus.Ok, $"Official permission catalog is present ({PermissionCatalog.AllCodes.Count} permissions).");

            await rbacProvisioning.EnsurePlatformRolesAsync(options.TenantId, cancellationToken);
            var role = await EnsurePlatformAdministratorRoleAsync(dbContext, options, cancellationToken);

            // Reconcile every existing business tenant with the official tenant
            // role catalog so the whole environment is RBAC-consistent, not just
            // newly created tenants.
            var businessTenantIds = await dbContext.Tenants
                .Where(tenant => tenant.Id != options.TenantId)
                .Select(tenant => tenant.Id)
                .ToListAsync(cancellationToken);
            foreach (var businessTenantId in businessTenantIds)
            {
                await rbacProvisioning.EnsureTenantRolesAsync(businessTenantId, cancellationToken);
            }

            AddCheck(checks, "Roles", DevelopmentBootstrapStatus.Ok,
                $"Role catalog present: {RoleCatalog.PlatformRoles.Count} platform + {RoleCatalog.TenantRoles.Count} tenant roles across {businessTenantIds.Count + 1} tenant(s).");

            await EnsureSuperAdminAsync(dbContext, passwordHasher, role, options, cancellationToken);
            AddCheck(checks, "Platform Administrator", DevelopmentBootstrapStatus.Ok, "Platform Administrator user is present, active, assigned, and password hash is synchronized.");

            foreach (var configurationCheck in await EnsureConfigurationAsync(scope.ServiceProvider, _configuration, options, cancellationToken))
            {
                checks.Add(configurationCheck);
                DevelopmentBootstrapConsole.WriteCheck(configurationCheck);
                if (configurationCheck.Status == DevelopmentBootstrapStatus.Error)
                {
                    DevelopmentBootstrapConsole.WriteFatal(configurationCheck.Message);
                    return DevelopmentBootstrapResult.Stop();
                }
            }

            var health = await scope.ServiceProvider.GetRequiredService<HealthCheckService>().CheckHealthAsync(cancellationToken);
            AddCheck(checks, "Health", health.Status == HealthStatus.Healthy ? DevelopmentBootstrapStatus.Healthy : health.Status == HealthStatus.Degraded ? DevelopmentBootstrapStatus.Warning : DevelopmentBootstrapStatus.Error, $"Health checks completed with status {health.Status}.");
            if (health.Status == HealthStatus.Unhealthy)
            {
                DevelopmentBootstrapConsole.WriteFatal("One or more required health checks are unhealthy.");
                return DevelopmentBootstrapResult.Stop();
            }

            DevelopmentBootstrapConsole.WriteFinalSummary(checks, _environment.EnvironmentName);
            return DevelopmentBootstrapResult.Start();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            DevelopmentBootstrapConsole.WriteFatal(ex.Message);
            return DevelopmentBootstrapResult.Stop();
        }
    }

    private static void AddCheck(ICollection<DevelopmentBootstrapCheck> checks, string name, DevelopmentBootstrapStatus status, string message)
    {
        var check = new DevelopmentBootstrapCheck(name, status, message);
        checks.Add(check);
        DevelopmentBootstrapConsole.WriteCheck(check);
    }

    private static async Task<string?> GetPostgreSqlVersionAsync(Compliance360DbContext dbContext, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "select version()";
        return (await command.ExecuteScalarAsync(cancellationToken))?.ToString();
    }

    private static async Task<SchemaValidationResult> ValidateSchemaAsync(Compliance360DbContext dbContext, CancellationToken cancellationToken)
    {
        var expectedMigration = dbContext.Database.GetMigrations().LastOrDefault();
        var currentMigration = (await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken)).LastOrDefault();
        if (!string.Equals(expectedMigration, currentMigration, StringComparison.Ordinal))
        {
            return SchemaValidationResult.Invalid(currentMigration, expectedMigration, "Database schema version does not match the expected EF migration.");
        }

        var missingTables = await MissingCatalogObjectsAsync(
            dbContext,
            CriticalTables,
            "select exists (select 1 from information_schema.tables where table_schema = 'compliance360' and table_name = @name)",
            cancellationToken);
        if (missingTables.Count > 0)
        {
            return SchemaValidationResult.Invalid(currentMigration, expectedMigration, $"Missing critical tables: {string.Join(", ", missingTables)}.");
        }

        var missingIndexes = await MissingCatalogObjectsAsync(
            dbContext,
            CriticalIndexes,
            "select exists (select 1 from pg_indexes where schemaname = 'compliance360' and indexname = @name)",
            cancellationToken);
        if (missingIndexes.Count > 0)
        {
            return SchemaValidationResult.Invalid(currentMigration, expectedMigration, $"Missing critical indexes: {string.Join(", ", missingIndexes)}.");
        }

        var missingConstraints = await MissingCatalogObjectsAsync(
            dbContext,
            CriticalConstraints,
            "select exists (select 1 from information_schema.table_constraints where constraint_schema = 'compliance360' and constraint_name = @name)",
            cancellationToken);
        if (missingConstraints.Count > 0)
        {
            return SchemaValidationResult.Invalid(currentMigration, expectedMigration, $"Missing critical constraints: {string.Join(", ", missingConstraints)}.");
        }

        return SchemaValidationResult.Valid(currentMigration, expectedMigration, "Critical tables, indexes, constraints, and migration version are consistent.");
    }

    private static async Task<IReadOnlyCollection<string>> MissingCatalogObjectsAsync(Compliance360DbContext dbContext, IReadOnlyCollection<string> names, string sql, CancellationToken cancellationToken)
    {
        var missing = new List<string>();
        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        foreach (var name in names)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            var parameter = command.CreateParameter();
            parameter.ParameterName = "name";
            parameter.Value = name;
            command.Parameters.Add(parameter);

            var exists = await command.ExecuteScalarAsync(cancellationToken);
            if (exists is not bool typedExists || !typedExists)
            {
                missing.Add(name);
            }
        }

        return missing;
    }

    private static async Task EnsureTenantAsync(Compliance360DbContext dbContext, DevelopmentBootstrapOptions options, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants
            .Include(item => item.Settings)
            .Include(item => item.Branding)
            .Include(item => item.Subscription)
            .FirstOrDefaultAsync(item => item.Id == options.TenantId || item.Slug == options.TenantSlug, cancellationToken);

        if (tenant is null)
        {
            tenant = new Tenant(
                options.TenantName,
                options.TenantSlug,
                options.TenantLegalName,
                options.TenantCommercialName,
                options.TenantTaxIdentifier,
                options.TenantCountryCode,
                options.TenantCurrency,
                createdByUserId: null);
            SetEntityId(tenant, options.TenantId);
            SetTenantId(tenant.Settings, options.TenantId);
            SetTenantId(tenant.Branding, options.TenantId);
            SetTenantId(tenant.Subscription, options.TenantId);
            tenant.Activate();
            await dbContext.Tenants.AddAsync(tenant, cancellationToken);
        }
        else if (tenant.Status != TenantStatus.Active)
        {
            tenant.Activate();
        }

        if (tenant.Settings is null)
        {
            await dbContext.TenantSettings.AddAsync(TenantSettings.CreateDefault(tenant.Id), cancellationToken);
        }

        if (tenant.Branding is null)
        {
            await dbContext.TenantBranding.AddAsync(TenantBranding.CreateDefault(tenant.Id, tenant.CommercialName), cancellationToken);
        }

        if (tenant.Subscription is null)
        {
            await dbContext.Subscriptions.AddAsync(Subscription.CreateTrial(tenant.Id), cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Role> EnsurePlatformAdministratorRoleAsync(Compliance360DbContext dbContext, DevelopmentBootstrapOptions options, CancellationToken cancellationToken)
    {
        var normalizedName = options.SuperAdminRoleName.ToUpperInvariant();
        var role = await dbContext.Roles
            .FirstOrDefaultAsync(item => item.TenantId == options.TenantId && item.NormalizedName == normalizedName, cancellationToken);

        if (role is null)
        {
            // Provisioning normally creates this role; keep the bootstrap resilient
            // if the role name was overridden through configuration.
            role = new Role(options.TenantId, options.SuperAdminRoleName, isSystemRole: true);
            await dbContext.Roles.AddAsync(role, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return role;
    }

    private static async Task EnsureSuperAdminAsync(Compliance360DbContext dbContext, IPasswordHasher passwordHasher, Role role, DevelopmentBootstrapOptions options, CancellationToken cancellationToken)
    {
        var normalizedEmail = options.SuperAdminEmail.ToUpperInvariant();
        var user = await dbContext.Users
            .Include(item => item.Roles)
            .FirstOrDefaultAsync(item => item.TenantId == options.TenantId && item.NormalizedEmail == normalizedEmail, cancellationToken);

        var passwordHash = passwordHasher.HashPassword(options.SuperAdminPassword);
        if (user is null)
        {
            user = new User(options.TenantId, options.SuperAdminEmail, options.SuperAdminFullName);
            user.SetPasswordHash(passwordHash);
            await dbContext.Users.AddAsync(user, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var mutated = false;
            var passwordIsValid = passwordHasher.Verify(options.SuperAdminPassword, user.PasswordHash) == PasswordVerificationResult.Success;
            if (!passwordIsValid && options.ResetPasswordWhenOutOfSync)
            {
                user.ChangePassword(passwordHash, DateTimeOffset.UtcNow);
                mutated = true;
            }

            if (user.IsLocked(DateTimeOffset.UtcNow) || user.Status == UserStatus.Locked)
            {
                user.Unlock();
                mutated = true;
            }

            if (mutated)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        // Assign the Platform Administrator role idempotently with a set-based
        // statement. This avoids aggregate change-tracking side effects while the
        // shared bootstrap DbContext is holding a large provisioning graph.
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            insert into compliance360.user_roles ("Id", "TenantId", "UserId", "RoleId", "CreatedAtUtc")
            select {Guid.NewGuid()}, {options.TenantId}, {user.Id}, {role.Id}, {DateTimeOffset.UtcNow}
            where not exists (
                select 1 from compliance360.user_roles
                where "TenantId" = {options.TenantId} and "UserId" = {user.Id} and "RoleId" = {role.Id})
            """, cancellationToken);
    }

    private static Task<IReadOnlyCollection<DevelopmentBootstrapCheck>> EnsureConfigurationAsync(IServiceProvider serviceProvider, IConfiguration configuration, DevelopmentBootstrapOptions options, CancellationToken cancellationToken)
    {
        var checks = new List<DevelopmentBootstrapCheck>();

        _ = serviceProvider.GetRequiredService<IFileStorageService>();
        _ = serviceProvider.GetRequiredService<INotificationDispatcher>();
        _ = serviceProvider.GetRequiredService<IObservabilityTelemetry>();
        _ = serviceProvider.GetRequiredService<IPasswordHasher>();
        _ = serviceProvider.GetRequiredService<IJwtTokenService>();

        var storageOptions = serviceProvider.GetRequiredService<IOptions<StorageOptions>>().Value;
        if (string.IsNullOrWhiteSpace(storageOptions.Provider) || string.IsNullOrWhiteSpace(storageOptions.ContainerName))
        {
            checks.Add(new DevelopmentBootstrapCheck("Storage", DevelopmentBootstrapStatus.Error, "Storage provider and container name must be configured for Development."));
        }
        else
        {
            checks.Add(new DevelopmentBootstrapCheck("Storage", DevelopmentBootstrapStatus.Ok, $"Provider={storageOptions.Provider}; Container={storageOptions.ContainerName}."));
        }

        var notificationOptions = serviceProvider.GetRequiredService<IOptions<NotificationProviderOptions>>().Value;
        if (!notificationOptions.Providers.TryGetValue(notificationOptions.DefaultProvider, out var endpoint)
            || string.IsNullOrWhiteSpace(endpoint.Host)
            || !endpoint.Port.HasValue
            || string.IsNullOrWhiteSpace(endpoint.FromAddress))
        {
            checks.Add(new DevelopmentBootstrapCheck("SMTP", DevelopmentBootstrapStatus.Error, "Development SMTP provider must define Host, Port, and FromAddress."));
        }
        else
        {
            checks.Add(new DevelopmentBootstrapCheck("SMTP", DevelopmentBootstrapStatus.Ok, $"Default provider {notificationOptions.DefaultProvider} is configured."));
        }

        checks.Add(notificationOptions.Providers.Count > 0
            ? new DevelopmentBootstrapCheck("Providers", DevelopmentBootstrapStatus.Ok, $"{notificationOptions.Providers.Count} notification provider configuration(s) loaded.")
            : new DevelopmentBootstrapCheck("Providers", DevelopmentBootstrapStatus.Warning, "No notification provider configurations were loaded."));

        var jwtSigningKey = configuration["Jwt:SigningKey"];
        checks.Add(string.IsNullOrWhiteSpace(jwtSigningKey)
            ? new DevelopmentBootstrapCheck("JWT", DevelopmentBootstrapStatus.Error, "Jwt:SigningKey is required.")
            : jwtSigningKey.Length < 32
                ? new DevelopmentBootstrapCheck("JWT", DevelopmentBootstrapStatus.Warning, "Jwt:SigningKey is configured but shorter than the recommended 32 characters for local development.")
                : new DevelopmentBootstrapCheck("JWT", DevelopmentBootstrapStatus.Ok, "JWT issuer, audience, and signing key are configured."));

        checks.Add(new DevelopmentBootstrapCheck("Identity", DevelopmentBootstrapStatus.Ok, "Password hasher and JWT token services are registered."));
        checks.Add(new DevelopmentBootstrapCheck("Observability", DevelopmentBootstrapStatus.Ok, "OpenTelemetry and observability services are registered."));

        return Task.FromResult<IReadOnlyCollection<DevelopmentBootstrapCheck>>(checks);
    }

    private static void SetEntityId(Entity entity, Guid id)
    {
        typeof(Entity).GetProperty(nameof(Entity.Id))?.SetValue(entity, id);
    }

    private static void SetTenantId(TenantEntity entity, Guid tenantId)
    {
        typeof(TenantEntity).GetProperty(nameof(TenantEntity.TenantId))?.SetValue(entity, tenantId);
    }
}

public sealed record DevelopmentBootstrapResult(bool CanStart)
{
    public static DevelopmentBootstrapResult Start() => new(true);

    public static DevelopmentBootstrapResult Stop() => new(false);
}

public enum DevelopmentBootstrapStatus
{
    Ok,
    Warning,
    Error,
    Healthy
}

public sealed record DevelopmentBootstrapCheck(string Name, DevelopmentBootstrapStatus Status, string Message);

public sealed record SchemaValidationResult(bool IsValid, string? CurrentMigration, string? ExpectedMigration, string Message)
{
    public static SchemaValidationResult Valid(string? currentMigration, string? expectedMigration, string message) =>
        new(true, currentMigration, expectedMigration, message);

    public static SchemaValidationResult Invalid(string? currentMigration, string? expectedMigration, string message) =>
        new(false, currentMigration, expectedMigration, message);
}

public sealed record DevelopmentBootstrapOptions
{
    public bool Enabled { get; set; } = true;

    public bool ApplyMigrations { get; set; } = true;

    public bool ResetPasswordWhenOutOfSync { get; set; } = true;

    public string ApplicationVersion { get; set; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "development";

    public Guid TenantId { get; set; } = Guid.Parse("dc7c46ee-cb25-4ed5-b0b4-800788f7f626");

    public string TenantName { get; set; } = "Compliance 360 Technical Tenant";

    public string TenantSlug { get; set; } = "compliance360-local";

    public string TenantLegalName { get; set; } = "Compliance 360 Local Development";

    public string TenantCommercialName { get; set; } = "Compliance 360 Local";

    public string TenantTaxIdentifier { get; set; } = "DEV-360-LOCAL";

    public string TenantCountryCode { get; set; } = "PA";

    public string TenantCurrency { get; set; } = "USD";

    public string SuperAdminEmail { get; set; } = "admin@compliance360.local";

    public string SuperAdminFullName { get; set; } = "Compliance 360 Platform Administrator";

    public string SuperAdminPassword { get; set; } = string.Empty;

    public string SuperAdminRoleName { get; set; } = RoleCatalog.PlatformAdministrator;

    public IReadOnlyCollection<string> RequiredPermissions { get; set; } = DefaultPermissions;

    public static DevelopmentBootstrapOptions From(IConfiguration configuration)
    {
        var options = new DevelopmentBootstrapOptions();
        configuration.GetSection("DevelopmentBootstrap").Bind(options);
        var superAdmin = configuration.GetSection("BootstrapSuperAdmin");
        return options with
        {
            TenantId = Guid.TryParse(superAdmin["TenantId"], out var tenantId) ? tenantId : options.TenantId,
            SuperAdminEmail = superAdmin["Email"] ?? options.SuperAdminEmail,
            SuperAdminFullName = superAdmin["FullName"] ?? options.SuperAdminFullName,
            SuperAdminPassword = superAdmin["Password"] ?? options.SuperAdminPassword,
            SuperAdminRoleName = superAdmin["RoleName"] ?? options.SuperAdminRoleName,
            RequiredPermissions = (configuration.GetSection("DevelopmentBootstrap:RequiredPermissions").Get<string[]>() ?? DefaultPermissions)
                .Select(item => item.Trim().ToUpperInvariant())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct()
                .ToArray()
        };
    }

    private static readonly string[] DefaultPermissions =
    [
        "TENANT.READ",
        "TENANT.CREATE",
        "TENANT.UPDATE",
        "TENANT.STATUS",
        "TENANT.BRANDING",
        "TENANT.SECURITY",
        "TENANT.STORAGE",
        "TENANT.NOTIFICATIONS",
        "TENANT.INTEGRATIONS",
        "TENANT.BILLING",
        "TENANT.USERS",
        "TENANT.ROLES",
        "TENANT.AUDIT",
        "TENANT.DELETE",
        "TENANT.RESTORE",
        "TENANT.DOMAINS",
        "TENANT.SSO",
        "TENANT.WEBHOOKS",
        "TENANT.API_KEYS",
        "TENANT.HEALTH",
        "TENANT.BACKUP",
        "IDENTITY.MANAGE",
        "RBAC.MANAGE",
        "AUDIT.READ",
        "AUDIT.MANAGE",
        "AUDITMANAGEMENT.MANAGE",
        "CAPA.MANAGE",
        "CAPA.READ",
        "CAPA.APPROVE",
        "CAPA.CLOSE",
        "RISK.MANAGE",
        "RISK.READ",
        "RISK.APPROVE",
        "RISK.CLOSE",
        "INDICATOR.MANAGE",
        "INDICATOR.READ",
        "INDICATOR.APPROVE",
        "INDICATOR.EXPORT",
        "REPORT.MANAGE",
        "REPORT.READ",
        "REPORT.EXECUTE",
        "REPORT.EXPORT",
        "REPORT.SCHEDULE",
        "STORAGE.MANAGE",
        "NOTIFICATION.MANAGE",
        "NOTIFICATION.SEND",
        "NOTIFICATION.READ",
        "NOTIFICATION.TEMPLATE",
        "NOTIFICATION.ADMIN",
        "DOCUMENT.MANAGE",
        "WORKFLOW.MANAGE",
        "TECHNICALSHEET.MANAGE",
        "SUPPLIER.MANAGE",
        "OBSERVABILITY.READ",
        "OBSERVABILITY.MANAGE",
        "OBSERVABILITY.ADMIN",
        "SUPERADMIN.DASHBOARD.READ",
        "SUPERADMIN.TENANTS.READ",
        "SUPERADMIN.TENANTS.CREATE",
        "SUPERADMIN.TENANTS.UPDATE",
        "SUPERADMIN.TENANTS.STATUS",
        "SUPERADMIN.TENANTS.DELETE",
        "SUPERADMIN.LICENSES.MANAGE",
        "SUPERADMIN.MODULES.MANAGE",
        "SUPERADMIN.PROVIDERS.MANAGE",
        "SUPERADMIN.PROVIDERS.HEALTH",
        "SUPERADMIN.SECURITY.MANAGE",
        "SUPERADMIN.OBSERVABILITY.READ",
        "SUPERADMIN.AUDIT.READ",
        "SUPERADMIN.AUDIT.EXPORT",
        "SUPERADMIN.DATABASE.READ",
        "SUPERADMIN.AI.MANAGE",
        "SUPERADMIN.CONFIGURATION.MANAGE",
        "SUPERADMIN.BACKUPS.READ",
        "SUPERADMIN.DEVOPS.READ",
        "SUPERADMIN.SEARCH"
    ];
}

public sealed record DevelopmentEnvironmentDiagnostics(
    string DotNetSdk,
    string DotNetRuntime,
    string ApplicationVersion,
    string GitBranch,
    string GitCommit,
    string Environment,
    string OperatingSystem,
    string Architecture,
    string User,
    DateTimeOffset Timestamp)
{
    public static DevelopmentEnvironmentDiagnostics Capture(string environment, string applicationVersion)
    {
        return new DevelopmentEnvironmentDiagnostics(
            RunProcess("dotnet", "--version") ?? "not available",
            RuntimeInformation.FrameworkDescription,
            applicationVersion,
            RunProcess("git", "rev-parse --abbrev-ref HEAD") ?? "not available",
            RunProcess("git", "rev-parse --short HEAD") ?? "not available",
            environment,
            RuntimeInformation.OSDescription,
            RuntimeInformation.ProcessArchitecture.ToString(),
            System.Environment.UserName,
            DateTimeOffset.Now);
    }

    private static string? RunProcess(string fileName, string arguments)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (process is null || !process.WaitForExit(2_000) || process.ExitCode != 0)
            {
                return null;
            }

            return process.StandardOutput.ReadToEnd().Trim();
        }
        catch
        {
            return null;
        }
    }
}

public static class DevelopmentBootstrapLogging
{
    public static void ConfigureBootstrapLogger(string environment)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.WithProperty("ServiceName", "Compliance360.Enterprise")
            .Enrich.WithProperty("Environment", environment)
            .WriteTo.Console(new MessageTemplateTextFormatter("{Message:lj}{NewLine}", null))
            .CreateLogger();
    }
}

public static class DevelopmentBootstrapConsole
{
    private const int BorderWidth = 50;

    public static void WriteHeader(string environment, string applicationVersion)
    {
        Log.Information($"""
            {Border()}
            Compliance 360 Enterprise
            Development Bootstrap
            {Border()}

            Environment ............ {environment}
            Application Version .... {applicationVersion}
            """);
    }

    public static void WriteEnvironmentDiagnostics(DevelopmentEnvironmentDiagnostics diagnostics)
    {
        Log.Information($"""
            Development Environment Diagnostics

            .NET SDK ............... {diagnostics.DotNetSdk}
            .NET Runtime ........... {diagnostics.DotNetRuntime}
            Application Version .... {diagnostics.ApplicationVersion}
            Git Branch ............. {diagnostics.GitBranch}
            Commit ................. {diagnostics.GitCommit}
            Environment ............ {diagnostics.Environment}
            Operating System ....... {diagnostics.OperatingSystem}
            Architecture ........... {diagnostics.Architecture}
            User ................... {diagnostics.User}
            Date ................... {diagnostics.Timestamp:yyyy-MM-dd}
            Time ................... {diagnostics.Timestamp:HH:mm:ss zzz}
            """);
    }

    public static void WriteCheck(DevelopmentBootstrapCheck check)
    {
        var status = StatusText(check.Status);
        var line = $"{check.Name.PadRight(24, '.')} {status} - {check.Message}";
        switch (check.Status)
        {
            case DevelopmentBootstrapStatus.Ok:
            case DevelopmentBootstrapStatus.Healthy:
                Log.Information(line);
                break;
            case DevelopmentBootstrapStatus.Warning:
                Log.Warning(line);
                break;
            case DevelopmentBootstrapStatus.Error:
                Log.Error(line);
                break;
        }
    }

    public static void WriteAlreadyRunning(PortListener listener, string url)
    {
        Log.Warning($"""
            {Border()}
            Compliance 360 Enterprise

            Ya existe una instancia ejecutandose.

            PID .................... {listener.ProcessId}
            Proceso ................ {listener.ProcessName}
            Puerto ................. {listener.Port}
            Ruta del ejecutable .... {listener.ExecutablePath ?? "unknown"}
            URL .................... {url}

            No se iniciara una segunda instancia.
            Utilice la instancia existente.
            {Border()}
            """);
    }

    public static void WriteFatal(string message)
    {
        Log.Error($"""
            Development Bootstrap ERROR

            {message}

            Startup stopped without stacktrace because this is a controlled Development Bootstrap failure.
            """);
    }

    public static void WriteFinalSummary(IReadOnlyCollection<DevelopmentBootstrapCheck> checks, string environment)
    {
        Log.Information($"""
            {Border()}
            Compliance 360 Enterprise
            Development Bootstrap

            Environment ............ {environment}
            Application ............ {SummaryStatus(checks, "Application")}
            Database ............... {SummaryStatus(checks, "Database")}
            Schema ................. {SummaryStatus(checks, "Schema")}
            Migrations ............. {SummaryStatus(checks, "Migrations")}
            Identity ............... {SummaryStatus(checks, "Identity")}
            Platform Administrator . {SummaryStatus(checks, "Platform Administrator")}
            Permissions ............ {SummaryStatus(checks, "Permissions")}
            SMTP ................... {SummaryStatus(checks, "SMTP")}
            Storage ................ {SummaryStatus(checks, "Storage")}
            Providers .............. {SummaryStatus(checks, "Providers")}
            Observability .......... {SummaryStatus(checks, "Observability")}
            Health ................. {SummaryStatus(checks, "Health")}

            Ready for Functional Testing
            {Border()}
            """);
    }

    private static string SummaryStatus(IEnumerable<DevelopmentBootstrapCheck> checks, string name)
    {
        var check = checks.LastOrDefault(item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return check is null ? "WARNING" : StatusText(check.Status);
    }

    private static string StatusText(DevelopmentBootstrapStatus status)
    {
        return status switch
        {
            DevelopmentBootstrapStatus.Ok => "OK",
            DevelopmentBootstrapStatus.Warning => "WARNING",
            DevelopmentBootstrapStatus.Error => "ERROR",
            DevelopmentBootstrapStatus.Healthy => "HEALTHY",
            _ => "WARNING"
        };
    }

    private static string Border()
    {
        return new string('=', BorderWidth);
    }
}
