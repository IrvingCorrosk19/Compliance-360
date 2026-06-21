using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationProviderReal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrandingJson",
                schema: "compliance360",
                table: "notification_templates",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Locale",
                schema: "compliance360",
                table: "notification_templates",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextBody",
                schema: "compliance360",
                table: "notification_templates",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                schema: "compliance360",
                table: "notification_templates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeliveredAtUtc",
                schema: "compliance360",
                table: "notification_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastProvider",
                schema: "compliance360",
                table: "notification_messages",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextRetryAtUtc",
                schema: "compliance360",
                table: "notification_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                schema: "compliance360",
                table: "notification_messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TextBody",
                schema: "compliance360",
                table: "notification_messages",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "notification_attachments",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    StorageObjectKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_attachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notification_dead_letters",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    DeadLetteredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_dead_letters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notification_deliveries",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProviderMessageId = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_deliveries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notification_history",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EventName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_history", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notification_preferences",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_preferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notification_provider_configurations",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_provider_configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notification_retries",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Attempt = table.Column<int>(type: "integer", nullable: false),
                    ScheduledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExecutedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_retries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notification_subscriptions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Topic = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Channel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Recipient = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_subscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notification_attachments_TenantId_NotificationMessageId",
                schema: "compliance360",
                table: "notification_attachments",
                columns: new[] { "TenantId", "NotificationMessageId" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_dead_letters_TenantId_DeadLetteredAtUtc",
                schema: "compliance360",
                table: "notification_dead_letters",
                columns: new[] { "TenantId", "DeadLetteredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_deliveries_TenantId_NotificationMessageId_Occu~",
                schema: "compliance360",
                table: "notification_deliveries",
                columns: new[] { "TenantId", "NotificationMessageId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_history_TenantId_NotificationMessageId_Occurre~",
                schema: "compliance360",
                table: "notification_history",
                columns: new[] { "TenantId", "NotificationMessageId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_preferences_TenantId_UserId_Channel",
                schema: "compliance360",
                table: "notification_preferences",
                columns: new[] { "TenantId", "UserId", "Channel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_provider_configurations_TenantId_Provider_Name",
                schema: "compliance360",
                table: "notification_provider_configurations",
                columns: new[] { "TenantId", "Provider", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_retries_TenantId_NotificationMessageId_Attempt",
                schema: "compliance360",
                table: "notification_retries",
                columns: new[] { "TenantId", "NotificationMessageId", "Attempt" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_subscriptions_TenantId_Topic_Channel_Recipient",
                schema: "compliance360",
                table: "notification_subscriptions",
                columns: new[] { "TenantId", "Topic", "Channel", "Recipient" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_attachments",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "notification_dead_letters",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "notification_deliveries",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "notification_history",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "notification_preferences",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "notification_provider_configurations",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "notification_retries",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "notification_subscriptions",
                schema: "compliance360");

            migrationBuilder.DropColumn(
                name: "BrandingJson",
                schema: "compliance360",
                table: "notification_templates");

            migrationBuilder.DropColumn(
                name: "Locale",
                schema: "compliance360",
                table: "notification_templates");

            migrationBuilder.DropColumn(
                name: "TextBody",
                schema: "compliance360",
                table: "notification_templates");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "compliance360",
                table: "notification_templates");

            migrationBuilder.DropColumn(
                name: "DeliveredAtUtc",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropColumn(
                name: "LastProvider",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropColumn(
                name: "NextRetryAtUtc",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropColumn(
                name: "TextBody",
                schema: "compliance360",
                table: "notification_messages");
        }
    }
}
