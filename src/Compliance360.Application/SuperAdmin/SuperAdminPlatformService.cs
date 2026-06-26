using Compliance360.Shared;

namespace Compliance360.Application.SuperAdmin;

public sealed class SuperAdminPlatformService : ISuperAdminPlatformService
{
    private readonly ISuperAdminPlatformRepository _repository;

    public SuperAdminPlatformService(ISuperAdminPlatformRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<SuperAdminPlatformCenter>> GetCenterAsync(CancellationToken cancellationToken = default)
    {
        var metrics = await _repository.GetDashboardMetricsAsync(cancellationToken);
        var tenants = await _repository.SearchTenantsAsync(null, null, 1, 25, cancellationToken);
        var providers = await _repository.GetProvidersAsync(cancellationToken);
        var licenses = await _repository.GetLicensesAsync(cancellationToken);
        var modules = await _repository.GetModulesAsync(cancellationToken);
        var health = await _repository.GetHealthSignalsAsync(cancellationToken);
        var backups = await _repository.GetBackupsAsync(cancellationToken);
        var database = await _repository.GetDatabaseMetricsAsync(cancellationToken);
        var timeline = await _repository.GetGlobalAuditTimelineAsync(1, 25, cancellationToken);
        var alerts = BuildAlerts(metrics, providers, health, backups, database);

        return Result<SuperAdminPlatformCenter>.Success(new SuperAdminPlatformCenter(
            metrics with { Alerts = alerts.Count },
            tenants,
            providers,
            licenses,
            modules,
            health,
            backups,
            database,
            timeline,
            alerts,
            BuildQuickActions()));
    }

    public async Task<Result<SuperAdminTenantSearchResult>> SearchTenantsAsync(SuperAdminTenantSearchQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var items = await _repository.SearchTenantsAsync(query.SearchText, query.Status, page, pageSize, cancellationToken);
        var total = await _repository.CountTenantsAsync(query.SearchText, query.Status, cancellationToken);
        return Result<SuperAdminTenantSearchResult>.Success(new SuperAdminTenantSearchResult(items, total, page, pageSize));
    }

    public async Task<Result<IReadOnlyCollection<SuperAdminAuditTimelineItem>>> GetGlobalAuditTimelineAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetGlobalAuditTimelineAsync(Math.Max(1, page), Math.Clamp(pageSize, 1, 250), cancellationToken);
        return Result<IReadOnlyCollection<SuperAdminAuditTimelineItem>>.Success(items);
    }

    private static IReadOnlyCollection<SuperAdminAlert> BuildAlerts(
        SuperAdminDashboardMetrics metrics,
        IReadOnlyCollection<SuperAdminProviderSummary> providers,
        IReadOnlyCollection<SuperAdminHealthSignal> health,
        IReadOnlyCollection<SuperAdminBackupSummary> backups,
        IReadOnlyCollection<SuperAdminDatabaseMetric> database)
    {
        var alerts = new List<SuperAdminAlert>();
        if (metrics.SuspendedTenants > 0)
        {
            alerts.Add(new SuperAdminAlert("warning", "Tenants suspendidos requieren revision", $"{metrics.SuspendedTenants} tenant(s) estan suspendidos.", "Tenants"));
        }

        if (providers.Count(provider => provider.Type == "Storage" && provider.IsEnabled) == 0)
        {
            alerts.Add(new SuperAdminAlert("critical", "Sin proveedores de storage activos", "Debe existir al menos un proveedor de storage activo.", "Providers"));
        }

        if (providers.Count(provider => provider.Type == "Notification" && provider.IsEnabled) == 0)
        {
            alerts.Add(new SuperAdminAlert("warning", "Sin proveedores SMTP activos", "No hay proveedores de notificaciones activos.", "Providers"));
        }

        if (health.Any(signal => signal.Status is "Unhealthy" or "Degraded"))
        {
            alerts.Add(new SuperAdminAlert("warning", "Health Center reporta componentes degradados", "Revisar senales globales y tenant-scoped.", "Observability"));
        }

        if (backups.Count == 0)
        {
            alerts.Add(new SuperAdminAlert("warning", "Sin evidencia de backups", "El historial de backups esta vacio. Registrar o conectar ejecuciones antes de produccion.", "Backups"));
        }

        if (database.Any(metric => metric.Status is "Warning" or "Critical"))
        {
            alerts.Add(new SuperAdminAlert("warning", "Monitoreo de base de datos requiere atencion", "Una o mas metricas de base de datos estan degradadas o no disponibles.", "Database"));
        }

        return alerts;
    }

    private static IReadOnlyCollection<SuperAdminQuickAction> BuildQuickActions()
    {
        return
        [
            new("global-search", "Busqueda Global", "Buscar tenants, usuarios, providers y eventos de auditoria.", "SUPERADMIN.SEARCH", "#global-search"),
            new("tenant-create", "Crear Tenant", "Abrir el flujo de creacion de tenant.", "SUPERADMIN.TENANTS.CREATE", "#/tenant-administration"),
            new("provider-health", "Salud de Providers", "Revisar salud de SMTP y storage providers.", "SUPERADMIN.PROVIDERS.HEALTH", "#providers"),
            new("audit-export", "Exportar Auditoria Global", "Exportar la auditoria global en CSV.", "SUPERADMIN.AUDIT.EXPORT", "#audit"),
            new("db-health", "Salud de Base de Datos", "Revisar monitoreo PostgreSQL disponible.", "SUPERADMIN.DATABASE.READ", "#database")
        ];
    }
}
