using Compliance360.Application;
using Compliance360.Domain.Notifications;
using Compliance360.Infrastructure.Notifications;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Tests;

public sealed class AlertCenterPostgresIntegrationTests
{
    [Fact]
    [Trait("Category", "PostgreSQL")]
    public async Task Persists_Message_And_Transactional_Outbox_In_Real_PostgreSql()
    {
        var connectionString = Environment.GetEnvironmentVariable("COMPLIANCE360_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var options = new DbContextOptionsBuilder<Compliance360DbContext>()
            .UseNpgsql(connectionString)
            .Options;
        await using var db = new Compliance360DbContext(options, new FixedClock());
        Assert.True(await db.Database.CanConnectAsync());
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());

        var tenantId = await db.Tenants.AsNoTracking().Select(item => item.Id).FirstAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var now = new DateTimeOffset(2026, 7, 19, 20, 30, 0, TimeSpan.Zero);
        var message = new NotificationMessage(
            tenantId,
            NotificationChannel.Email,
            "postgres-integration@example.invalid",
            "PostgreSQL transactional outbox test",
            "This row is rolled back after verification.",
            NotificationPriority.Normal,
            now);
        message.ConfigureDurability($"integration:{Guid.NewGuid():N}", 3);
        var outbox = new NotificationOutboxEvent(
            tenantId,
            "notification.message.queued",
            nameof(NotificationMessage),
            message.Id,
            $$"""{"messageId":"{{message.Id}}"}""",
            message.Id.ToString("N"),
            now);
        db.NotificationMessages.Add(message);
        db.NotificationOutbox.Add(outbox);
        await db.SaveChangesAsync();

        Assert.True(await db.NotificationMessages.AnyAsync(item => item.TenantId == tenantId && item.Id == message.Id));
        Assert.True(await db.NotificationOutbox.AnyAsync(item => item.TenantId == tenantId && item.AggregateId == message.Id));
        await transaction.RollbackAsync();
    }

    [Fact]
    [Trait("Category", "PostgreSQL")]
    public async Task Multiple_Workers_Claim_Disjoint_Leases_And_Recover_Expired_Work()
    {
        var connectionString = Environment.GetEnvironmentVariable("COMPLIANCE360_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var options = new DbContextOptionsBuilder<Compliance360DbContext>().UseNpgsql(connectionString).Options;
        var now = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var ids = new List<Guid>();
        await using (var seed = new Compliance360DbContext(options, new FixedClock()))
        {
            var tenantId = await seed.Tenants.AsNoTracking().Select(item => item.Id).FirstAsync();
            for (var index = 0; index < 20; index++)
            {
                var message = new NotificationMessage(
                    tenantId,
                    NotificationChannel.Email,
                    $"worker-{index}@example.invalid",
                    "Concurrent worker lease test",
                    "Test row",
                    NotificationPriority.Normal,
                    now);
                message.ConfigureDurability($"worker-integration:{Guid.NewGuid():N}", 3);
                ids.Add(message.Id);
                seed.NotificationMessages.Add(message);
            }
            await seed.SaveChangesAsync();
        }

        try
        {
            await using var firstDb = new Compliance360DbContext(options, new FixedClock());
            await using var secondDb = new Compliance360DbContext(options, new FixedClock());
            var firstRepo = new EfNotificationQueueRepository(firstDb, new FixedClock());
            var secondRepo = new EfNotificationQueueRepository(secondDb, new FixedClock());
            var claims = await Task.WhenAll(
                firstRepo.ClaimMessagesAsync("worker-a", 10, now, TimeSpan.FromSeconds(10)),
                secondRepo.ClaimMessagesAsync("worker-b", 10, now, TimeSpan.FromSeconds(10)));
            var firstIds = claims[0].Select(item => item.Id).ToHashSet();
            var secondIds = claims[1].Select(item => item.Id).ToHashSet();

            Assert.Equal(10, firstIds.Count);
            Assert.Equal(10, secondIds.Count);
            Assert.Empty(firstIds.Intersect(secondIds));
            Assert.Equal(ids.Order(), firstIds.Concat(secondIds).Order());

            await using var recoveryDb = new Compliance360DbContext(options, new FixedClock());
            var recoveryRepo = new EfNotificationQueueRepository(recoveryDb, new FixedClock());
            var recovered = await recoveryRepo.ClaimMessagesAsync(
                "worker-restarted",
                20,
                now.AddSeconds(11),
                TimeSpan.FromSeconds(10));
            Assert.Equal(20, recovered.Count);
            Assert.All(recovered, item => Assert.Equal("worker-restarted", item.LeaseOwner));
        }
        finally
        {
            await using var cleanup = new Compliance360DbContext(options, new FixedClock());
            await cleanup.NotificationMessages.Where(item => ids.Contains(item.Id)).ExecuteDeleteAsync();
        }
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 7, 19, 20, 30, 0, TimeSpan.Zero);
    }
}
