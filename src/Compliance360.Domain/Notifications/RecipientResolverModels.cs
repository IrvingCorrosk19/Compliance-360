using System.Text.RegularExpressions;
using Compliance360.Domain.Common;

namespace Compliance360.Domain.Notifications;

public enum RecipientKind
{
    Owner,
    Creator,
    Responsible,
    Reviewer,
    Approver,
    Submitter,
    Supervisor,
    Role,
    Group,
    Department,
    ExplicitUser,
    ExternalEmail,
    DistributionList,
    Subscription
}

public enum RecipientRouting
{
    To,
    Cc,
    Bcc
}

public enum RecipientFallbackMode
{
    None,
    User,
    Role,
    Group,
    Department,
    DistributionList
}

public sealed class RecipientGroup : TenantEntity
{
    private RecipientGroup() { Name = string.Empty; }

    public RecipientGroup(Guid tenantId, string name) : base(tenantId)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 160);
    }

    public string Name { get; private set; }
}

public sealed class RecipientGroupMember : TenantEntity
{
    private RecipientGroupMember() { }

    public RecipientGroupMember(Guid tenantId, Guid groupId, Guid userId) : base(tenantId)
    {
        GroupId = Guard.AgainstEmpty(groupId, nameof(groupId));
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
    }

    public Guid GroupId { get; private set; }
    public Guid UserId { get; private set; }
}

public sealed class RecipientDepartment : TenantEntity
{
    private RecipientDepartment() { Name = string.Empty; }

    public RecipientDepartment(Guid tenantId, string name) : base(tenantId)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 160);
    }

    public string Name { get; private set; }
}

public sealed class RecipientDirectoryProfile : TenantEntity
{
    private RecipientDirectoryProfile() { }

    public RecipientDirectoryProfile(Guid tenantId, Guid userId, Guid? departmentId, Guid? supervisorUserId) : base(tenantId)
    {
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        DepartmentId = departmentId;
        SupervisorUserId = supervisorUserId;
    }

    public Guid UserId { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public Guid? SupervisorUserId { get; private set; }

    public void Update(Guid? departmentId, Guid? supervisorUserId, DateTimeOffset nowUtc)
    {
        DepartmentId = departmentId;
        SupervisorUserId = supervisorUserId;
        MarkUpdated(nowUtc);
    }
}

public sealed class AuthorizedExternalRecipient : TenantEntity
{
    private static readonly Regex EmailRegex = new("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private AuthorizedExternalRecipient() { Email = string.Empty; DisplayName = string.Empty; }

    public AuthorizedExternalRecipient(Guid tenantId, string email, string displayName) : base(tenantId)
    {
        Email = NormalizeEmail(email);
        DisplayName = Guard.AgainstNullOrWhiteSpace(displayName, nameof(displayName), 180);
        IsActive = true;
    }

    public string Email { get; private set; }
    public string DisplayName { get; private set; }
    public bool IsActive { get; private set; }

    private static string NormalizeEmail(string value)
    {
        var email = Guard.AgainstNullOrWhiteSpace(value, nameof(value), 320).ToLowerInvariant();
        if (!EmailRegex.IsMatch(email))
        {
            throw new DomainException("External recipient email is invalid.");
        }

        return email;
    }
}

public sealed class RecipientDistributionList : TenantEntity
{
    private RecipientDistributionList() { Name = string.Empty; }

    public RecipientDistributionList(Guid tenantId, string name) : base(tenantId)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 160);
    }

    public string Name { get; private set; }
}

public sealed class RecipientDistributionListMember : TenantEntity
{
    private RecipientDistributionListMember() { }

    public RecipientDistributionListMember(Guid tenantId, Guid distributionListId, Guid? userId, Guid? externalRecipientId) : base(tenantId)
    {
        DistributionListId = Guard.AgainstEmpty(distributionListId, nameof(distributionListId));
        if (userId.HasValue == externalRecipientId.HasValue)
        {
            throw new DomainException("Distribution list member must reference exactly one user or authorized external recipient.");
        }

        UserId = userId;
        ExternalRecipientId = externalRecipientId;
    }

    public Guid DistributionListId { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? ExternalRecipientId { get; private set; }
}

public sealed class RecipientFallbackConfiguration : TenantEntity
{
    private RecipientFallbackConfiguration() { }

    public RecipientFallbackConfiguration(Guid tenantId, RecipientFallbackMode mode, Guid? targetId, RecipientRouting routing) : base(tenantId)
    {
        Set(mode, targetId, routing, DateTimeOffset.UtcNow);
    }

    public RecipientFallbackMode Mode { get; private set; }
    public Guid? TargetId { get; private set; }
    public RecipientRouting Routing { get; private set; }

    public void Set(RecipientFallbackMode mode, Guid? targetId, RecipientRouting routing, DateTimeOffset nowUtc)
    {
        if (mode != RecipientFallbackMode.None && !targetId.HasValue)
        {
            throw new DomainException("Fallback target is required.");
        }

        Mode = mode;
        TargetId = mode == RecipientFallbackMode.None ? null : targetId;
        Routing = routing;
        MarkUpdated(nowUtc);
    }
}
