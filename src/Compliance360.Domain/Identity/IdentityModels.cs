using Compliance360.Domain.Common;

namespace Compliance360.Domain.Identity;

public enum UserStatus
{
    Invited = 0,
    Active = 1,
    Locked = 2,
    Disabled = 3
}

public enum PermissionAction
{
    Read = 0,
    Create = 1,
    Update = 2,
    Approve = 3,
    Reject = 4,
    Delete = 5,
    Export = 6,
    Manage = 7
}

public sealed class User : TenantEntity
{
    private readonly List<UserRole> _roles = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    private User()
    {
        Email = string.Empty;
        NormalizedEmail = string.Empty;
        FullName = string.Empty;
        PasswordHash = string.Empty;
    }

    public User(Guid tenantId, string email, string fullName)
        : base(tenantId)
    {
        Email = Guard.AgainstNullOrWhiteSpace(email, nameof(email), 320);
        NormalizedEmail = Email.ToUpperInvariant();
        FullName = Guard.AgainstNullOrWhiteSpace(fullName, nameof(fullName), 180);
        Status = UserStatus.Invited;
        PasswordHash = string.Empty;
    }

    public Guid? CompanyId { get; private set; }

    public string Email { get; private set; }

    public string NormalizedEmail { get; private set; }

    public string FullName { get; private set; }

    public string PasswordHash { get; private set; }

    public UserStatus Status { get; private set; }

    public bool MfaEnabled { get; private set; }

    public string? MfaSecretEncrypted { get; private set; }

    public DateTimeOffset? LastLoginAtUtc { get; private set; }

    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public void AssignToCompany(Guid companyId)
    {
        CompanyId = Guard.AgainstEmpty(companyId, nameof(companyId));
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash), 1_000);
        Status = UserStatus.Active;
    }

    public void EnableMfa(string encryptedSecret)
    {
        MfaSecretEncrypted = Guard.AgainstNullOrWhiteSpace(encryptedSecret, nameof(encryptedSecret), 2_000);
        MfaEnabled = true;
    }

    public void DisableMfa()
    {
        MfaSecretEncrypted = null;
        MfaEnabled = false;
    }

    public void RegisterLogin(DateTimeOffset occurredAtUtc)
    {
        LastLoginAtUtc = occurredAtUtc;
    }

    public void AssignRole(Guid roleId)
    {
        Guard.AgainstEmpty(roleId, nameof(roleId));

        if (_roles.Any(role => role.RoleId == roleId))
        {
            return;
        }

        _roles.Add(new UserRole(TenantId, Id, roleId));
    }

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        if (refreshToken.TenantId != TenantId || refreshToken.UserId != Id)
        {
            throw new DomainException("Refresh token must belong to the same tenant and user.");
        }

        _refreshTokens.Add(refreshToken);
    }
}

public sealed class Role : TenantEntity
{
    private readonly List<RolePermission> _permissions = [];

    private Role()
    {
        Name = string.Empty;
        NormalizedName = string.Empty;
    }

    public Role(Guid tenantId, string name, bool isSystemRole = false)
        : base(tenantId)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 120);
        NormalizedName = Name.ToUpperInvariant();
        IsSystemRole = isSystemRole;
    }

    public string Name { get; private set; }

    public string NormalizedName { get; private set; }

    public bool IsSystemRole { get; private set; }

    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

    public void GrantPermission(Guid permissionId)
    {
        Guard.AgainstEmpty(permissionId, nameof(permissionId));

        if (_permissions.Any(permission => permission.PermissionId == permissionId))
        {
            return;
        }

        _permissions.Add(new RolePermission(TenantId, Id, permissionId));
    }
}

public sealed class Permission : Entity
{
    private Permission()
    {
        Module = string.Empty;
        Code = string.Empty;
        Description = string.Empty;
    }

    public Permission(string module, PermissionAction action, string description)
    {
        Module = Guard.AgainstNullOrWhiteSpace(module, nameof(module), 80);
        Action = action;
        Code = $"{Module}.{Action}".ToUpperInvariant();
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), 250);
    }

    public string Module { get; private set; }

    public PermissionAction Action { get; private set; }

    public string Code { get; private set; }

    public string Description { get; private set; }
}

public sealed class UserRole : TenantEntity
{
    private UserRole()
    {
    }

    public UserRole(Guid tenantId, Guid userId, Guid roleId)
        : base(tenantId)
    {
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        RoleId = Guard.AgainstEmpty(roleId, nameof(roleId));
    }

    public Guid UserId { get; private set; }

    public Guid RoleId { get; private set; }
}

public sealed class RolePermission : TenantEntity
{
    private RolePermission()
    {
    }

    public RolePermission(Guid tenantId, Guid roleId, Guid permissionId)
        : base(tenantId)
    {
        RoleId = Guard.AgainstEmpty(roleId, nameof(roleId));
        PermissionId = Guard.AgainstEmpty(permissionId, nameof(permissionId));
    }

    public Guid RoleId { get; private set; }

    public Guid PermissionId { get; private set; }
}

public sealed class RefreshToken : TenantEntity
{
    private RefreshToken()
    {
        TokenHash = string.Empty;
    }

    public RefreshToken(Guid tenantId, Guid userId, string tokenHash, DateTimeOffset expiresAtUtc)
        : base(tenantId)
    {
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        TokenHash = Guard.AgainstNullOrWhiteSpace(tokenHash, nameof(tokenHash), 512);
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    public bool IsActive(DateTimeOffset nowUtc)
    {
        return RevokedAtUtc is null && ExpiresAtUtc > nowUtc;
    }

    public void Revoke(DateTimeOffset revokedAtUtc, string? replacedByTokenHash = null)
    {
        RevokedAtUtc = revokedAtUtc;
        ReplacedByTokenHash = string.IsNullOrWhiteSpace(replacedByTokenHash) ? null : replacedByTokenHash.Trim();
    }
}
