using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Identity;
using Compliance360.Domain.Storage;
using Compliance360.Domain.TenantManagement;

namespace Compliance360.Tests;

public sealed class FoundationDomainTests
{
    [Fact]
    public void Tenant_Creates_Default_Settings_Branding_And_Subscription()
    {
        var tenant = new Tenant("Acme Quality", "acme-quality");

        Assert.Equal(TenantStatus.Draft, tenant.Status);
        Assert.Equal(tenant.Id, tenant.Settings.TenantId);
        Assert.Equal(tenant.Id, tenant.Branding.TenantId);
        Assert.Equal(tenant.Id, tenant.Subscription.TenantId);
        Assert.False(tenant.Settings.RequireMfa);
    }

    [Fact]
    public void User_AssignRole_Does_Not_Duplicate_Role()
    {
        var user = new User(Guid.NewGuid(), "qa@example.com", "Quality Manager");
        var roleId = Guid.NewGuid();

        user.AssignRole(roleId);
        user.AssignRole(roleId);

        Assert.Single(user.Roles);
    }

    [Fact]
    public void RefreshToken_Is_Inactive_After_Revoke()
    {
        var refreshToken = new RefreshToken(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TOKEN_HASH",
            DateTimeOffset.UtcNow.AddDays(30));

        refreshToken.Revoke(DateTimeOffset.UtcNow);

        Assert.False(refreshToken.IsActive(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void TenantSettings_Rejects_Retention_Lower_Than_Minimum()
    {
        var settings = TenantSettings.CreateDefault(Guid.NewGuid());

        Assert.Throws<DomainException>(() => settings.Configure("es-PA", "America/Panama", true, 10));
    }

    [Fact]
    public void AuditLog_Captures_Request_Context()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        var auditLog = AuditLog
            .Create(tenantId, userId, "Tenant", entityId, AuditAction.Created, DateTimeOffset.UtcNow)
            .WithRequestContext("127.0.0.1", "unit-test", "correlation-id");

        Assert.Equal(tenantId, auditLog.TenantId);
        Assert.Equal(userId, auditLog.UserId);
        Assert.Equal("correlation-id", auditLog.CorrelationId);
    }

    [Fact]
    public void StoredFile_Starts_As_PendingScan()
    {
        var storedFile = new StoredFile(
            Guid.NewGuid(),
            "Local",
            "documents",
            "tenant/document.pdf",
            "document.pdf",
            "application/pdf",
            100,
            "HASH",
            "DocumentVersion",
            Guid.NewGuid());

        Assert.Equal(StoredFileStatus.PendingScan, storedFile.Status);
    }
}
