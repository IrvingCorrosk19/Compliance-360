using System.Diagnostics.CodeAnalysis;
using Compliance360.Domain.Common;

namespace Compliance360.Domain.Reporting;

public enum ReportModule
{
    TenantManagement = 0,
    Identity = 1,
    DocumentManagement = 2,
    Workflow = 3,
    TechnicalSheets = 4,
    SupplierManagement = 5,
    AuditManagement = 6,
    Capa = 7,
    RiskManagement = 8,
    QualityIndicators = 9,
    AuditLog = 10,
    Notifications = 11
}

public enum ReportFormat
{
    Pdf = 0,
    Excel = 1,
    Word = 2,
    Csv = 3,
    Json = 4
}

public enum ReportDefinitionStatus
{
    Draft = 0,
    Active = 1,
    Archived = 2
}

public enum ReportParameterType
{
    Text = 0,
    Number = 1,
    Date = 2,
    Boolean = 3,
    Guid = 4,
    Select = 5
}

public enum ReportExecutionStatus
{
    Queued = 0,
    Running = 1,
    Completed = 2,
    Failed = 3
}

public enum ReportScheduleFrequency
{
    Daily = 0,
    Weekly = 1,
    Monthly = 2,
    Quarterly = 3
}

public enum ReportPermissionScope
{
    User = 0,
    Role = 1,
    Permission = 2
}

public sealed class ReportCategory : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private ReportCategory()
    {
        Name = string.Empty;
        Code = string.Empty;
    }

    public ReportCategory(Guid tenantId, string name, string code, ReportModule module, Guid createdByUserId)
        : base(tenantId)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 80).ToUpperInvariant();
        Module = module;
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        IsActive = true;
    }

    public string Name { get; private set; }
    public string Code { get; private set; }
    public ReportModule Module { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public bool IsActive { get; private set; }
}

public sealed class ReportDefinition : TenantEntity
{
    private readonly List<ReportTemplate> _templates = [];
    private readonly List<ReportParameter> _parameters = [];
    private readonly List<ReportExecution> _executions = [];
    private readonly List<ReportSchedule> _schedules = [];
    private readonly List<ReportSubscription> _subscriptions = [];
    private readonly List<ReportHistory> _history = [];
    private readonly List<ReportPermission> _permissions = [];
    private readonly List<ReportDashboardBinding> _dashboardBindings = [];

    [ExcludeFromCodeCoverage]
    private ReportDefinition()
    {
        Name = string.Empty;
        Code = string.Empty;
        Description = string.Empty;
        DatasetKey = string.Empty;
    }

    public ReportDefinition(Guid tenantId, Guid categoryId, string name, string code, string description, ReportModule module, string datasetKey, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        CategoryId = Guard.AgainstEmpty(categoryId, nameof(categoryId));
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 220);
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 120).ToUpperInvariant();
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), 2_000);
        Module = module;
        DatasetKey = Guard.AgainstNullOrWhiteSpace(datasetKey, nameof(datasetKey), 160);
        Version = 1;
        Status = ReportDefinitionStatus.Draft;
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        CreatedAtUtc = createdAtUtc;
        AddHistory("Report definition created.", createdByUserId, createdAtUtc);
    }

    public Guid CategoryId { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string Description { get; private set; }
    public ReportModule Module { get; private set; }
    public string DatasetKey { get; private set; }
    public int Version { get; private set; }
    public ReportDefinitionStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public IReadOnlyCollection<ReportTemplate> Templates => _templates.AsReadOnly();
    public IReadOnlyCollection<ReportParameter> Parameters => _parameters.AsReadOnly();
    public IReadOnlyCollection<ReportExecution> Executions => _executions.AsReadOnly();
    public IReadOnlyCollection<ReportSchedule> Schedules => _schedules.AsReadOnly();
    public IReadOnlyCollection<ReportSubscription> Subscriptions => _subscriptions.AsReadOnly();
    public IReadOnlyCollection<ReportHistory> History => _history.AsReadOnly();
    public IReadOnlyCollection<ReportPermission> Permissions => _permissions.AsReadOnly();
    public IReadOnlyCollection<ReportDashboardBinding> DashboardBindings => _dashboardBindings.AsReadOnly();

    public void Activate(Guid userId, DateTimeOffset occurredAtUtc)
    {
        if (_templates.Count == 0)
        {
            throw new DomainException("A report template is required before activation.");
        }

        Status = ReportDefinitionStatus.Active;
        Version++;
        AddHistory("Report definition activated.", userId, occurredAtUtc);
    }

    public ReportTemplate AddTemplate(string name, ReportFormat format, string content, Guid userId, DateTimeOffset occurredAtUtc)
    {
        var template = new ReportTemplate(TenantId, Id, name, format, content, Version + 1, userId, occurredAtUtc);
        _templates.Add(template);
        Version++;
        AddHistory("Report template added.", userId, occurredAtUtc);
        return template;
    }

    public ReportParameter AddParameter(string name, string label, ReportParameterType type, bool isRequired, string? defaultValue, Guid userId, DateTimeOffset occurredAtUtc)
    {
        if (_parameters.Any(parameter => string.Equals(parameter.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException("Report parameter name already exists.");
        }

        var parameter = new ReportParameter(TenantId, Id, name, label, type, isRequired, defaultValue);
        _parameters.Add(parameter);
        AddHistory("Report parameter added.", userId, occurredAtUtc);
        return parameter;
    }

    public ReportPermission GrantPermission(ReportPermissionScope scope, string subject, bool canExecute, bool canExport, bool canSchedule, Guid userId, DateTimeOffset occurredAtUtc)
    {
        var permission = new ReportPermission(TenantId, Id, scope, subject, canExecute, canExport, canSchedule);
        _permissions.Add(permission);
        AddHistory("Report permission granted.", userId, occurredAtUtc);
        return permission;
    }

    public ReportExecution StartExecution(string parametersJson, Guid requestedByUserId, DateTimeOffset startedAtUtc)
    {
        EnsureActive();
        EnsureRequiredParameters(parametersJson);
        var execution = new ReportExecution(TenantId, Id, requestedByUserId, parametersJson, startedAtUtc);
        execution.MarkRunning(startedAtUtc);
        _executions.Add(execution);
        AddHistory("Report execution started.", requestedByUserId, startedAtUtc);
        return execution;
    }

    public ReportOutput CompleteExecution(Guid executionId, int rowCount, string datasetDescriptorJson, Guid userId, DateTimeOffset completedAtUtc)
    {
        var execution = FindExecution(executionId);
        execution.MarkCompleted(rowCount, completedAtUtc);
        var output = new ReportOutput(TenantId, Id, execution.Id, rowCount, datasetDescriptorJson, completedAtUtc);
        execution.AddOutput(output);
        AddHistory("Report execution completed.", userId, completedAtUtc);
        return output;
    }

    public ReportExport Export(Guid executionId, ReportFormat format, Guid userId, DateTimeOffset exportedAtUtc)
    {
        var execution = FindExecution(executionId);
        if (execution.Status != ReportExecutionStatus.Completed)
        {
            throw new DomainException("Only completed report executions can be exported.");
        }

        var export = new ReportExport(TenantId, Id, execution.Id, format, BuildFileName(format), ContentTypeFor(format), userId, exportedAtUtc);
        execution.AddExport(export);
        AddHistory("Report export generated.", userId, exportedAtUtc);
        return export;
    }

    public ReportSchedule Schedule(ReportScheduleFrequency frequency, DateTimeOffset nextRunUtc, Guid userId, DateTimeOffset scheduledAtUtc)
    {
        EnsureActive();
        if (nextRunUtc <= scheduledAtUtc)
        {
            throw new DomainException("Report schedule next run must be in the future.");
        }

        var schedule = new ReportSchedule(TenantId, Id, frequency, nextRunUtc, userId, scheduledAtUtc);
        _schedules.Add(schedule);
        AddHistory("Report schedule created.", userId, scheduledAtUtc);
        return schedule;
    }

    public ReportSubscription Subscribe(string recipient, ReportFormat format, Guid userId, DateTimeOffset subscribedAtUtc)
    {
        EnsureActive();
        var subscription = new ReportSubscription(TenantId, Id, recipient, format, userId, subscribedAtUtc);
        _subscriptions.Add(subscription);
        AddHistory("Report subscription created.", userId, subscribedAtUtc);
        return subscription;
    }

    public ReportDashboardBinding BindDashboard(string dashboardKey, string datasetKey, Guid userId, DateTimeOffset occurredAtUtc)
    {
        var binding = new ReportDashboardBinding(TenantId, Id, dashboardKey, datasetKey, userId, occurredAtUtc);
        _dashboardBindings.Add(binding);
        AddHistory("Report dashboard binding created.", userId, occurredAtUtc);
        return binding;
    }

    public bool CanExecute(IEnumerable<string> permissionClaims, Guid userId)
    {
        var claims = permissionClaims.ToArray();
        return _permissions.Count == 0
            || _permissions.Any(permission => permission.CanExecute && permission.Matches(claims, userId));
    }

    public bool CanExport(IEnumerable<string> permissionClaims, Guid userId)
    {
        var claims = permissionClaims.ToArray();
        return _permissions.Count == 0
            || _permissions.Any(permission => permission.CanExport && permission.Matches(claims, userId));
    }

    public static IReadOnlyCollection<StandardReportDescriptor> StandardReports()
    {
        return
        [
            new(ReportModule.DocumentManagement, "DOC-ACTIVE", "Documentos vigentes", "documents.active"),
            new(ReportModule.DocumentManagement, "DOC-EXPIRED", "Documentos vencidos", "documents.expired"),
            new(ReportModule.DocumentManagement, "DOC-BY-AREA", "Documentos por area", "documents.by-area"),
            new(ReportModule.DocumentManagement, "DOC-HISTORY", "Historial documental", "documents.history"),
            new(ReportModule.SupplierManagement, "SUP-ACTIVE", "Proveedores activos", "suppliers.active"),
            new(ReportModule.SupplierManagement, "SUP-SUSPENDED", "Proveedores suspendidos", "suppliers.suspended"),
            new(ReportModule.SupplierManagement, "SUP-CERT-EXPIRED", "Certificados vencidos", "suppliers.certificates.expired"),
            new(ReportModule.SupplierManagement, "SUP-HOMOLOGATION", "Homologaciones", "suppliers.homologations"),
            new(ReportModule.AuditManagement, "AUD-OPEN", "Auditorias abiertas", "audits.open"),
            new(ReportModule.AuditManagement, "AUD-CLOSED", "Auditorias cerradas", "audits.closed"),
            new(ReportModule.AuditManagement, "AUD-FINDINGS", "Hallazgos", "audits.findings"),
            new(ReportModule.AuditManagement, "AUD-NC", "No conformidades", "audits.nonconformities"),
            new(ReportModule.Capa, "CAPA-OPEN", "CAPAs abiertas", "capas.open"),
            new(ReportModule.Capa, "CAPA-OVERDUE", "CAPAs vencidas", "capas.overdue"),
            new(ReportModule.Capa, "CAPA-BY-OWNER", "CAPAs por responsable", "capas.by-owner"),
            new(ReportModule.Capa, "CAPA-EFFECTIVENESS", "CAPAs por efectividad", "capas.effectiveness"),
            new(ReportModule.RiskManagement, "RISK-MAP", "Mapa de riesgos", "risks.map"),
            new(ReportModule.RiskManagement, "RISK-CRITICAL", "Riesgos criticos", "risks.critical"),
            new(ReportModule.RiskManagement, "RISK-BY-AREA", "Riesgos por area", "risks.by-area"),
            new(ReportModule.RiskManagement, "RISK-BY-SUPPLIER", "Riesgos por proveedor", "risks.by-supplier"),
            new(ReportModule.QualityIndicators, "KPI-SUMMARY", "KPIs", "indicators.kpis"),
            new(ReportModule.QualityIndicators, "KPI-TRENDS", "Tendencias", "indicators.trends"),
            new(ReportModule.QualityIndicators, "KPI-COMPLIANCE", "Cumplimiento", "indicators.compliance"),
            new(ReportModule.QualityIndicators, "KPI-DEVIATIONS", "Desviaciones", "indicators.deviations")
        ];
    }

    private void EnsureActive()
    {
        if (Status != ReportDefinitionStatus.Active)
        {
            throw new DomainException("Only active report definitions can be used.");
        }
    }

    private void EnsureRequiredParameters(string parametersJson)
    {
        if (_parameters.Any(parameter => parameter.IsRequired) && string.IsNullOrWhiteSpace(parametersJson))
        {
            throw new DomainException("Required report parameters must be provided.");
        }
    }

    private ReportExecution FindExecution(Guid executionId) =>
        _executions.FirstOrDefault(execution => execution.Id == executionId) ?? throw new DomainException("Report execution not found.");

    private string BuildFileName(ReportFormat format) => $"{Code.ToLowerInvariant()}-v{Version}.{ExtensionFor(format)}";

    public static string ContentTypeFor(ReportFormat format) => format switch
    {
        ReportFormat.Pdf => "application/pdf",
        ReportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ReportFormat.Word => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ReportFormat.Json => "application/json",
        _ => "text/csv"
    };

    private static string ExtensionFor(ReportFormat format) => format switch
    {
        ReportFormat.Pdf => "pdf",
        ReportFormat.Excel => "xlsx",
        ReportFormat.Word => "docx",
        ReportFormat.Json => "json",
        _ => "csv"
    };

    private void AddHistory(string action, Guid userId, DateTimeOffset occurredAtUtc)
    {
        _history.Add(new ReportHistory(TenantId, Id, action, userId, occurredAtUtc));
    }
}

public sealed record StandardReportDescriptor(ReportModule Module, string Code, string Name, string DatasetKey);

public sealed class ReportTemplate : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private ReportTemplate()
    {
        Name = string.Empty;
        Content = string.Empty;
    }

    public ReportTemplate(Guid tenantId, Guid reportDefinitionId, string name, ReportFormat format, string content, int version, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        ReportDefinitionId = Guard.AgainstEmpty(reportDefinitionId, nameof(reportDefinitionId));
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        Format = format;
        Content = Guard.AgainstNullOrWhiteSpace(content, nameof(content), 10_000);
        Version = Guard.AgainstOutOfRange(version, nameof(version), 1, 10_000);
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        CreatedAtUtc = createdAtUtc;
    }

    public Guid ReportDefinitionId { get; private set; }
    public string Name { get; private set; }
    public ReportFormat Format { get; private set; }
    public string Content { get; private set; }
    public int Version { get; private set; }
    public Guid CreatedByUserId { get; private set; }
}

public sealed class ReportParameter : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private ReportParameter()
    {
        Name = string.Empty;
        Label = string.Empty;
    }

    public ReportParameter(Guid tenantId, Guid reportDefinitionId, string name, string label, ReportParameterType type, bool isRequired, string? defaultValue)
        : base(tenantId)
    {
        ReportDefinitionId = Guard.AgainstEmpty(reportDefinitionId, nameof(reportDefinitionId));
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 120);
        Label = Guard.AgainstNullOrWhiteSpace(label, nameof(label), 180);
        Type = type;
        IsRequired = isRequired;
        DefaultValue = string.IsNullOrWhiteSpace(defaultValue) ? null : defaultValue.Trim();
    }

    public Guid ReportDefinitionId { get; private set; }
    public string Name { get; private set; }
    public string Label { get; private set; }
    public ReportParameterType Type { get; private set; }
    public bool IsRequired { get; private set; }
    public string? DefaultValue { get; private set; }
}

public sealed class ReportExecution : TenantEntity
{
    private readonly List<ReportOutput> _outputs = [];
    private readonly List<ReportExport> _exports = [];

    [ExcludeFromCodeCoverage]
    private ReportExecution()
    {
        ParametersJson = string.Empty;
    }

    public ReportExecution(Guid tenantId, Guid reportDefinitionId, Guid requestedByUserId, string parametersJson, DateTimeOffset queuedAtUtc)
        : base(tenantId)
    {
        ReportDefinitionId = Guard.AgainstEmpty(reportDefinitionId, nameof(reportDefinitionId));
        RequestedByUserId = Guard.AgainstEmpty(requestedByUserId, nameof(requestedByUserId));
        ParametersJson = string.IsNullOrWhiteSpace(parametersJson) ? "{}" : parametersJson.Trim();
        QueuedAtUtc = queuedAtUtc;
        Status = ReportExecutionStatus.Queued;
    }

    public Guid ReportDefinitionId { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public string ParametersJson { get; private set; }
    public ReportExecutionStatus Status { get; private set; }
    public DateTimeOffset QueuedAtUtc { get; private set; }
    public DateTimeOffset? StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public int RowCount { get; private set; }
    public string? FailureReason { get; private set; }
    public IReadOnlyCollection<ReportOutput> Outputs => _outputs.AsReadOnly();
    public IReadOnlyCollection<ReportExport> Exports => _exports.AsReadOnly();

    public void MarkRunning(DateTimeOffset startedAtUtc)
    {
        Status = ReportExecutionStatus.Running;
        StartedAtUtc = startedAtUtc;
    }

    public void MarkCompleted(int rowCount, DateTimeOffset completedAtUtc)
    {
        if (Status != ReportExecutionStatus.Running)
        {
            throw new DomainException("Only running report executions can be completed.");
        }

        RowCount = Guard.AgainstOutOfRange(rowCount, nameof(rowCount), 0, int.MaxValue);
        Status = ReportExecutionStatus.Completed;
        CompletedAtUtc = completedAtUtc;
    }

    public void MarkFailed(string reason, DateTimeOffset completedAtUtc)
    {
        Status = ReportExecutionStatus.Failed;
        FailureReason = Guard.AgainstNullOrWhiteSpace(reason, nameof(reason), 1_000);
        CompletedAtUtc = completedAtUtc;
    }

    public void AddOutput(ReportOutput output) => _outputs.Add(output);
    public void AddExport(ReportExport export) => _exports.Add(export);
}

public sealed class ReportOutput : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private ReportOutput()
    {
        DatasetDescriptorJson = string.Empty;
    }

    public ReportOutput(Guid tenantId, Guid reportDefinitionId, Guid reportExecutionId, int rowCount, string datasetDescriptorJson, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        ReportDefinitionId = Guard.AgainstEmpty(reportDefinitionId, nameof(reportDefinitionId));
        ReportExecutionId = Guard.AgainstEmpty(reportExecutionId, nameof(reportExecutionId));
        RowCount = Guard.AgainstOutOfRange(rowCount, nameof(rowCount), 0, int.MaxValue);
        DatasetDescriptorJson = Guard.AgainstNullOrWhiteSpace(datasetDescriptorJson, nameof(datasetDescriptorJson), 10_000);
        CreatedAtUtc = createdAtUtc;
    }

    public Guid ReportDefinitionId { get; private set; }
    public Guid ReportExecutionId { get; private set; }
    public int RowCount { get; private set; }
    public string DatasetDescriptorJson { get; private set; }
}

public sealed class ReportExport : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private ReportExport()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
    }

    public ReportExport(Guid tenantId, Guid reportDefinitionId, Guid reportExecutionId, ReportFormat format, string fileName, string contentType, Guid exportedByUserId, DateTimeOffset exportedAtUtc)
        : base(tenantId)
    {
        ReportDefinitionId = Guard.AgainstEmpty(reportDefinitionId, nameof(reportDefinitionId));
        ReportExecutionId = Guard.AgainstEmpty(reportExecutionId, nameof(reportExecutionId));
        Format = format;
        FileName = Guard.AgainstNullOrWhiteSpace(fileName, nameof(fileName), 260);
        ContentType = Guard.AgainstNullOrWhiteSpace(contentType, nameof(contentType), 160);
        ExportedByUserId = Guard.AgainstEmpty(exportedByUserId, nameof(exportedByUserId));
        ExportedAtUtc = exportedAtUtc;
    }

    public Guid ReportDefinitionId { get; private set; }
    public Guid ReportExecutionId { get; private set; }
    public ReportFormat Format { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public Guid ExportedByUserId { get; private set; }
    public DateTimeOffset ExportedAtUtc { get; private set; }
}

public sealed class ReportSchedule : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private ReportSchedule()
    {
    }

    public ReportSchedule(Guid tenantId, Guid reportDefinitionId, ReportScheduleFrequency frequency, DateTimeOffset nextRunUtc, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        ReportDefinitionId = Guard.AgainstEmpty(reportDefinitionId, nameof(reportDefinitionId));
        Frequency = frequency;
        NextRunUtc = nextRunUtc;
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        CreatedAtUtc = createdAtUtc;
        IsActive = true;
    }

    public Guid ReportDefinitionId { get; private set; }
    public ReportScheduleFrequency Frequency { get; private set; }
    public DateTimeOffset NextRunUtc { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public bool IsActive { get; private set; }
}

public sealed class ReportSubscription : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private ReportSubscription()
    {
        Recipient = string.Empty;
    }

    public ReportSubscription(Guid tenantId, Guid reportDefinitionId, string recipient, ReportFormat format, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        ReportDefinitionId = Guard.AgainstEmpty(reportDefinitionId, nameof(reportDefinitionId));
        Recipient = Guard.AgainstNullOrWhiteSpace(recipient, nameof(recipient), 260);
        Format = format;
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        CreatedAtUtc = createdAtUtc;
        IsActive = true;
    }

    public Guid ReportDefinitionId { get; private set; }
    public string Recipient { get; private set; }
    public ReportFormat Format { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public bool IsActive { get; private set; }
}

public sealed class ReportPermission : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private ReportPermission()
    {
        Subject = string.Empty;
    }

    public ReportPermission(Guid tenantId, Guid reportDefinitionId, ReportPermissionScope scope, string subject, bool canExecute, bool canExport, bool canSchedule)
        : base(tenantId)
    {
        ReportDefinitionId = Guard.AgainstEmpty(reportDefinitionId, nameof(reportDefinitionId));
        Scope = scope;
        Subject = Guard.AgainstNullOrWhiteSpace(subject, nameof(subject), 180);
        CanExecute = canExecute;
        CanExport = canExport;
        CanSchedule = canSchedule;
    }

    public Guid ReportDefinitionId { get; private set; }
    public ReportPermissionScope Scope { get; private set; }
    public string Subject { get; private set; }
    public bool CanExecute { get; private set; }
    public bool CanExport { get; private set; }
    public bool CanSchedule { get; private set; }

    public bool Matches(IReadOnlyCollection<string> permissionClaims, Guid userId)
    {
        return Scope switch
        {
            ReportPermissionScope.User => string.Equals(Subject, userId.ToString(), StringComparison.OrdinalIgnoreCase),
            ReportPermissionScope.Permission => permissionClaims.Contains(Subject, StringComparer.OrdinalIgnoreCase),
            _ => permissionClaims.Contains($"ROLE:{Subject}", StringComparer.OrdinalIgnoreCase)
        };
    }
}

public sealed class ReportDashboardBinding : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private ReportDashboardBinding()
    {
        DashboardKey = string.Empty;
        DatasetKey = string.Empty;
    }

    public ReportDashboardBinding(Guid tenantId, Guid reportDefinitionId, string dashboardKey, string datasetKey, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        ReportDefinitionId = Guard.AgainstEmpty(reportDefinitionId, nameof(reportDefinitionId));
        DashboardKey = Guard.AgainstNullOrWhiteSpace(dashboardKey, nameof(dashboardKey), 160);
        DatasetKey = Guard.AgainstNullOrWhiteSpace(datasetKey, nameof(datasetKey), 160);
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        CreatedAtUtc = createdAtUtc;
    }

    public Guid ReportDefinitionId { get; private set; }
    public string DashboardKey { get; private set; }
    public string DatasetKey { get; private set; }
    public Guid CreatedByUserId { get; private set; }
}

public sealed class ReportHistory : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private ReportHistory()
    {
        Action = string.Empty;
    }

    public ReportHistory(Guid tenantId, Guid reportDefinitionId, string action, Guid userId, DateTimeOffset occurredAtUtc)
        : base(tenantId)
    {
        ReportDefinitionId = Guard.AgainstEmpty(reportDefinitionId, nameof(reportDefinitionId));
        Action = Guard.AgainstNullOrWhiteSpace(action, nameof(action), 1_200);
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid ReportDefinitionId { get; private set; }
    public string Action { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
