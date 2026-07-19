using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertCenterOperationalFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AvailableAtUtc",
                schema: "compliance360",
                table: "notification_messages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAtUtc",
                schema: "compliance360",
                table: "notification_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                schema: "compliance360",
                table: "notification_messages",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValueSql: "('message:' || gen_random_uuid()::text)");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastAttemptAtUtc",
                schema: "compliance360",
                table: "notification_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeaseOwner",
                schema: "compliance360",
                table: "notification_messages",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeaseToken",
                schema: "compliance360",
                table: "notification_messages",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LeaseUntilUtc",
                schema: "compliance360",
                table: "notification_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxAttempts",
                schema: "compliance360",
                table: "notification_messages",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ProcessingStartedAtUtc",
                schema: "compliance360",
                table: "notification_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE compliance360.notification_messages
                SET "AvailableAtUtc" = "QueuedAtUtc",
                    "IdempotencyKey" = 'legacy:' || "Id"::text,
                    "MaxAttempts" = GREATEST(3, "RetryCount" + 1),
                    "CompletedAtUtc" = CASE
                        WHEN "Status" = 'Delivered' THEN "DeliveredAtUtc"
                        WHEN "Status" = 'Sent' THEN "SentAtUtc"
                        WHEN "Status" IN ('Failed', 'DeadLetter', 'Cancelled') THEN COALESCE("FailedAtUtc", "UpdatedAtUtc", "QueuedAtUtc")
                        ELSE NULL
                    END;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_notification_messages_MaxAttempts",
                schema: "compliance360",
                table: "notification_messages",
                sql: "\"MaxAttempts\" BETWEEN 1 AND 20");

            migrationBuilder.CreateTable(
                name: "notification_outbox",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    AggregateType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    AggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AvailableAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    LeaseToken = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    LeaseOwner = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    LeaseUntilUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_outbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notification_worker_heartbeats",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerId = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    InstanceName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ActiveLeases = table.Column<int>(type: "integer", nullable: false),
                    ProcessedCount = table.Column<long>(type: "bigint", nullable: false),
                    FailureCount = table.Column<long>(type: "bigint", nullable: false),
                    LastError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_worker_heartbeats", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notification_messages_Status_AvailableAtUtc_NextRetryAtUtc_~",
                schema: "compliance360",
                table: "notification_messages",
                columns: new[] { "Status", "AvailableAtUtc", "NextRetryAtUtc", "LeaseUntilUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_messages_TenantId_IdempotencyKey",
                schema: "compliance360",
                table: "notification_messages",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_outbox_Status_AvailableAtUtc_LeaseUntilUtc",
                schema: "compliance360",
                table: "notification_outbox",
                columns: new[] { "Status", "AvailableAtUtc", "LeaseUntilUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_outbox_TenantId_CorrelationId",
                schema: "compliance360",
                table: "notification_outbox",
                columns: new[] { "TenantId", "CorrelationId" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_outbox_TenantId_EventType_AggregateId",
                schema: "compliance360",
                table: "notification_outbox",
                columns: new[] { "TenantId", "EventType", "AggregateId" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_worker_heartbeats_Status_LastSeenAtUtc",
                schema: "compliance360",
                table: "notification_worker_heartbeats",
                columns: new[] { "Status", "LastSeenAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_worker_heartbeats_WorkerId",
                schema: "compliance360",
                table: "notification_worker_heartbeats",
                column: "WorkerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_outbox",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "notification_worker_heartbeats",
                schema: "compliance360");

            migrationBuilder.DropIndex(
                name: "IX_notification_messages_Status_AvailableAtUtc_NextRetryAtUtc_~",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_notification_messages_MaxAttempts",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropIndex(
                name: "IX_notification_messages_TenantId_IdempotencyKey",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropColumn(
                name: "AvailableAtUtc",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropColumn(
                name: "LastAttemptAtUtc",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropColumn(
                name: "LeaseOwner",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropColumn(
                name: "LeaseToken",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropColumn(
                name: "LeaseUntilUtc",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropColumn(
                name: "MaxAttempts",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropColumn(
                name: "ProcessingStartedAtUtc",
                schema: "compliance360",
                table: "notification_messages");
        }
    }
}
