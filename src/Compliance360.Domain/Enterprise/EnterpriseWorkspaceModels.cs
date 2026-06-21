using Compliance360.Domain.Common;

namespace Compliance360.Domain.Enterprise;

public enum EnterpriseWorkspaceType
{
    TemplateBuilder = 0,
    Regulatory = 1,
    Training = 2,
    SupplierPortal = 3,
    CustomerPortal = 4,
    Security = 5,
    Configuration = 6
}

public enum EnterpriseWorkspaceStatus
{
    Draft = 0,
    Active = 1,
    Completed = 2,
    Archived = 3
}

public sealed class EnterpriseWorkspaceItem : TenantEntity
{
    private EnterpriseWorkspaceItem()
    {
        Title = string.Empty;
        Code = string.Empty;
        Description = string.Empty;
        MetadataJson = "{}";
    }

    public EnterpriseWorkspaceItem(
        Guid tenantId,
        EnterpriseWorkspaceType type,
        string title,
        string code,
        string description,
        Guid createdByUserId,
        string metadataJson = "{}")
        : base(tenantId)
    {
        Type = type;
        Title = Guard.AgainstNullOrWhiteSpace(title, nameof(title), 220);
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 100).ToUpperInvariant();
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), 2_000);
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? "{}" : metadataJson.Trim();
        Status = EnterpriseWorkspaceStatus.Active;
    }

    public EnterpriseWorkspaceType Type { get; private set; }

    public string Title { get; private set; }

    public string Code { get; private set; }

    public string Description { get; private set; }

    public EnterpriseWorkspaceStatus Status { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public Guid? OwnerUserId { get; private set; }

    public DateTimeOffset? DueAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public string MetadataJson { get; private set; }

    public void Assign(Guid ownerUserId, DateTimeOffset? dueAtUtc)
    {
        OwnerUserId = Guard.AgainstEmpty(ownerUserId, nameof(ownerUserId));
        DueAtUtc = dueAtUtc;
    }

    public void Complete(DateTimeOffset completedAtUtc)
    {
        if (Status == EnterpriseWorkspaceStatus.Archived)
        {
            throw new DomainException("Archived enterprise workspace items cannot be completed.");
        }

        Status = EnterpriseWorkspaceStatus.Completed;
        CompletedAtUtc = completedAtUtc;
    }

    public void Reopen()
    {
        if (Status == EnterpriseWorkspaceStatus.Archived)
        {
            throw new DomainException("Archived enterprise workspace items cannot be reopened.");
        }

        Status = EnterpriseWorkspaceStatus.Active;
        CompletedAtUtc = null;
    }
}
