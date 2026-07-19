using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertCenterInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notification_inbox_items",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IsFavorite = table.Column<bool>(type: "boolean", nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SortAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReadAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArchivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_inbox_items", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO compliance360.notification_inbox_items
                    ("Id", "NotificationMessageId", "UserId", "State", "IsFavorite",
                     "ReceivedAtUtc", "SortAtUtc", "ReadAtUtc", "ArchivedAtUtc", "DeletedAtUtc",
                     "CreatedAtUtc", "UpdatedAtUtc", "TenantId")
                SELECT
                    gen_random_uuid(),
                    message."Id",
                    message."TargetUserId",
                    'Unread',
                    FALSE,
                    message."QueuedAtUtc",
                    message."QueuedAtUtc",
                    NULL,
                    NULL,
                    NULL,
                    message."QueuedAtUtc",
                    NULL,
                    message."TenantId"
                FROM compliance360.notification_messages AS message
                WHERE message."Channel" = 'InApp'
                  AND message."TargetUserId" IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_notification_inbox_items_TenantId_NotificationMessageId_Use~",
                schema: "compliance360",
                table: "notification_inbox_items",
                columns: new[] { "TenantId", "NotificationMessageId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_inbox_items_TenantId_UserId_IsFavorite_SortAtU~",
                schema: "compliance360",
                table: "notification_inbox_items",
                columns: new[] { "TenantId", "UserId", "IsFavorite", "SortAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_inbox_items_TenantId_UserId_State_SortAtUtc",
                schema: "compliance360",
                table: "notification_inbox_items",
                columns: new[] { "TenantId", "UserId", "State", "SortAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_inbox_items",
                schema: "compliance360");
        }
    }
}
