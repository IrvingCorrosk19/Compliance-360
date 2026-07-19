using Compliance360.Domain.Common;

namespace Compliance360.Domain.FormTemplates;

public enum FormTemplateLifecycleStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}

public enum FormTemplateKind
{
    Generic = 0,
    Audit = 1,
    Inspection = 2,
    Evaluation = 3,
    Capa = 4,
    Risk = 5,
    Control = 6,
    Checklist = 7,
    Regulatory = 8,
    InternalProcess = 9,
    Investigation = 10,
    ActionPlan = 11,
    Document = 12
}

/// <summary>Reusable form template definition (header). Schema lives in versions.</summary>
public sealed class FormTemplate : TenantEntity
{
    private FormTemplate()
    {
        Name = string.Empty;
        Code = string.Empty;
        Category = string.Empty;
        Description = string.Empty;
        Versions = [];
    }

    public FormTemplate(
        Guid tenantId,
        string name,
        string code,
        string category,
        FormTemplateKind kind,
        string description,
        Guid createdByUserId)
        : base(tenantId)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 220);
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 100).ToUpperInvariant();
        Category = Guard.AgainstNullOrWhiteSpace(category, nameof(category), 120);
        Kind = kind;
        Description = string.IsNullOrWhiteSpace(description) ? string.Empty : description.Trim();
        if (Description.Length > 2_000)
        {
            throw new DomainException("Description cannot exceed 2000 characters.");
        }

        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        Status = FormTemplateLifecycleStatus.Draft;
        PublishedVersionNumber = null;
        IsDeleted = false;
        Versions = [];
    }

    public string Name { get; private set; }

    public string Code { get; private set; }

    public string Category { get; private set; }

    public FormTemplateKind Kind { get; private set; }

    public string Description { get; private set; }

    public FormTemplateLifecycleStatus Status { get; private set; }

    public string? PublishedVersionNumber { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public bool IsDeleted { get; private set; }

    public List<FormTemplateVersion> Versions { get; private set; }

    public void UpdateHeader(string name, string category, FormTemplateKind kind, string description)
    {
        EnsureNotDeleted();
        if (Status == FormTemplateLifecycleStatus.Archived)
        {
            throw new DomainException("Archived templates cannot be edited.");
        }

        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 220);
        Category = Guard.AgainstNullOrWhiteSpace(category, nameof(category), 120);
        Kind = kind;
        Description = string.IsNullOrWhiteSpace(description) ? string.Empty : description.Trim();
        if (Description.Length > 2_000)
        {
            throw new DomainException("Description cannot exceed 2000 characters.");
        }
    }

    public FormTemplateVersion CreateDraftVersion(string schemaJson, string changeLog, Guid userId, string? baseVersionNumber = null)
    {
        EnsureNotDeleted();
        if (Status == FormTemplateLifecycleStatus.Archived)
        {
            throw new DomainException("Archived templates cannot receive new drafts.");
        }

        var next = NextVersionNumber(baseVersionNumber ?? PublishedVersionNumber);
        var version = new FormTemplateVersion(Id, TenantId, next, schemaJson, changeLog, userId);
        Versions.Add(version);
        Status = FormTemplateLifecycleStatus.Draft;
        return version;
    }

    public FormTemplateVersion SaveDraftSchema(Guid versionId, string schemaJson, string changeLog, Guid userId)
    {
        EnsureNotDeleted();
        var version = Versions.SingleOrDefault(v => v.Id == versionId)
            ?? throw new DomainException("Template version was not found.");
        if (version.IsPublished)
        {
            throw new DomainException("Published versions are immutable. Create a new draft version.");
        }

        version.UpdateDraft(schemaJson, changeLog, userId);
        Status = FormTemplateLifecycleStatus.Draft;
        return version;
    }

    public FormTemplateVersion Publish(Guid versionId, Guid userId, DateTimeOffset publishedAtUtc)
    {
        EnsureNotDeleted();
        var version = Versions.SingleOrDefault(v => v.Id == versionId)
            ?? throw new DomainException("Template version was not found.");
        version.Publish(userId, publishedAtUtc);
        PublishedVersionNumber = version.VersionNumber;
        Status = FormTemplateLifecycleStatus.Published;
        return version;
    }

    public void Archive()
    {
        EnsureNotDeleted();
        Status = FormTemplateLifecycleStatus.Archived;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        Status = FormTemplateLifecycleStatus.Archived;
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new DomainException("Template is deleted.");
        }
    }

    private static string NextVersionNumber(string? current)
    {
        if (string.IsNullOrWhiteSpace(current))
        {
            return "1.0";
        }

        var parts = current.Split('.', 2, StringSplitOptions.TrimEntries);
        if (parts.Length == 2 && int.TryParse(parts[0], out var major) && int.TryParse(parts[1], out var minor))
        {
            return $"{major}.{minor + 1}";
        }

        return $"{current}.1";
    }
}

public sealed class FormTemplateVersion : TenantEntity
{
    private FormTemplateVersion()
    {
        VersionNumber = "1.0";
        SchemaJson = "{}";
        ChangeLog = string.Empty;
    }

    public FormTemplateVersion(
        Guid templateId,
        Guid tenantId,
        string versionNumber,
        string schemaJson,
        string changeLog,
        Guid createdByUserId)
        : base(tenantId)
    {
        TemplateId = Guard.AgainstEmpty(templateId, nameof(templateId));
        VersionNumber = Guard.AgainstNullOrWhiteSpace(versionNumber, nameof(versionNumber), 20);
        SchemaJson = NormalizeSchema(schemaJson);
        ChangeLog = string.IsNullOrWhiteSpace(changeLog) ? "Borrador" : changeLog.Trim();
        if (ChangeLog.Length > 1_000)
        {
            throw new DomainException("Change log cannot exceed 1000 characters.");
        }

        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        IsPublished = false;
    }

    public Guid TemplateId { get; private set; }

    public string VersionNumber { get; private set; }

    public string SchemaJson { get; private set; }

    public string ChangeLog { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset? PublishedAtUtc { get; private set; }

    public Guid? PublishedByUserId { get; private set; }

    public void UpdateDraft(string schemaJson, string changeLog, Guid userId)
    {
        if (IsPublished)
        {
            throw new DomainException("Published versions cannot be modified.");
        }

        SchemaJson = NormalizeSchema(schemaJson);
        ChangeLog = string.IsNullOrWhiteSpace(changeLog) ? ChangeLog : changeLog.Trim();
        _ = Guard.AgainstEmpty(userId, nameof(userId));
    }

    public void Publish(Guid userId, DateTimeOffset publishedAtUtc)
    {
        if (IsPublished)
        {
            throw new DomainException("Version is already published.");
        }

        IsPublished = true;
        PublishedAtUtc = publishedAtUtc;
        PublishedByUserId = Guard.AgainstEmpty(userId, nameof(userId));
    }

    private static string NormalizeSchema(string schemaJson)
    {
        var value = string.IsNullOrWhiteSpace(schemaJson) ? "{\"fields\":[],\"rules\":[]}" : schemaJson.Trim();
        if (value.Length > 500_000)
        {
            throw new DomainException("Schema JSON is too large.");
        }

        return value;
    }
}
