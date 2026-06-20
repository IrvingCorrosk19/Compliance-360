using System.Text.Json;
using Compliance360.Application;
using Compliance360.Application.Audit;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Compliance360.Infrastructure.Audit;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IAuditContextAccessor _auditContextAccessor;
    private readonly IClock _clock;

    public AuditSaveChangesInterceptor(IAuditContextAccessor auditContextAccessor, IClock clock)
    {
        _auditContextAccessor = auditContextAccessor;
        _clock = clock;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddAuditEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddAuditEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditEntries(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var auditEntries = dbContext.ChangeTracker
            .Entries()
            .Where(entry => entry.Entity is not AuditLog && entry.Entity is Entity)
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(CreateAuditLog)
            .ToList();

        if (auditEntries.Count == 0)
        {
            return;
        }

        dbContext.Set<AuditLog>().AddRange(auditEntries);
    }

    private AuditLog CreateAuditLog(EntityEntry entry)
    {
        var entity = (Entity)entry.Entity;
        var context = _auditContextAccessor.Current;
        var tenantId = entry.Entity is ITenantScoped tenantScoped ? tenantScoped.TenantId : context.TenantId;
        var action = entry.State switch
        {
            EntityState.Added => AuditAction.Created,
            EntityState.Modified => AuditAction.Updated,
            EntityState.Deleted => AuditAction.Deleted,
            _ => AuditAction.Updated
        };

        var auditEvent = new AuditEvent(
            entry.Metadata.ClrType.Name,
            entity.Id,
            action,
            AuditLog.InferCategory(action),
            context with { TenantId = tenantId },
            new AuditSnapshot(
                entry.State == EntityState.Added ? null : SerializeValues(entry, originalValues: true),
                entry.State == EntityState.Deleted ? null : SerializeValues(entry, originalValues: false)),
            new AuditMetadata("{\"source\":\"ef-interceptor\"}"),
            Success: true,
            ErrorMessage: null);

        return AuditLog.FromEvent(auditEvent, _clock.UtcNow);
    }

    private static string SerializeValues(EntityEntry entry, bool originalValues)
    {
        var values = entry.Properties
            .Where(property => !property.Metadata.IsPrimaryKey())
            .ToDictionary(
                property => property.Metadata.Name,
                property => originalValues && entry.State != EntityState.Added
                    ? property.OriginalValue
                    : property.CurrentValue);

        return JsonSerializer.Serialize(values, JsonOptions);
    }
}
