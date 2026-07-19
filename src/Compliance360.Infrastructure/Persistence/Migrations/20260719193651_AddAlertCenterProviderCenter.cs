using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertCenterProviderCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AlertDefinitionId",
                schema: "compliance360",
                table: "notification_messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AlertDefinitionVersionId",
                schema: "compliance360",
                table: "notification_messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AlertOccurrenceId",
                schema: "compliance360",
                table: "notification_messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_alert_occurrences_TenantId_Id",
                schema: "compliance360",
                table: "alert_occurrences",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.CreateTable(
                name: "tenant_notification_providers",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Authentication = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    FromAddress = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    FromName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    ProtectedSettings = table.Column<string>(type: "text", nullable: false),
                    RateLimitPerMinute = table.Column<int>(type: "integer", nullable: false),
                    RateWindowStartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RateWindowCount = table.Column<int>(type: "integer", nullable: false),
                    CircuitFailureThreshold = table.Column<int>(type: "integer", nullable: false),
                    CircuitBreakSeconds = table.Column<int>(type: "integer", nullable: false),
                    ConsecutiveFailures = table.Column<int>(type: "integer", nullable: false),
                    CircuitOpenUntilUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSucceededAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastFailedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastFailureCode = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_notification_providers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notification_messages_TenantId_AlertDefinitionId",
                schema: "compliance360",
                table: "notification_messages",
                columns: new[] { "TenantId", "AlertDefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_messages_TenantId_AlertDefinitionVersionId",
                schema: "compliance360",
                table: "notification_messages",
                columns: new[] { "TenantId", "AlertDefinitionVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_messages_TenantId_AlertOccurrenceId",
                schema: "compliance360",
                table: "notification_messages",
                columns: new[] { "TenantId", "AlertOccurrenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_notification_providers_TenantId_Name",
                schema: "compliance360",
                table: "tenant_notification_providers",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_notification_providers_TenantId_Priority",
                schema: "compliance360",
                table: "tenant_notification_providers",
                columns: new[] { "TenantId", "Priority" });

            migrationBuilder.AddForeignKey(
                name: "FK_notification_messages_alert_definition_versions_TenantId_Al~",
                schema: "compliance360",
                table: "notification_messages",
                columns: new[] { "TenantId", "AlertDefinitionVersionId" },
                principalSchema: "compliance360",
                principalTable: "alert_definition_versions",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_notification_messages_alert_definitions_TenantId_AlertDefin~",
                schema: "compliance360",
                table: "notification_messages",
                columns: new[] { "TenantId", "AlertDefinitionId" },
                principalSchema: "compliance360",
                principalTable: "alert_definitions",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_notification_messages_alert_occurrences_TenantId_AlertOccur~",
                schema: "compliance360",
                table: "notification_messages",
                columns: new[] { "TenantId", "AlertOccurrenceId" },
                principalSchema: "compliance360",
                principalTable: "alert_occurrences",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_notification_messages_alert_definition_versions_TenantId_Al~",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropForeignKey(
                name: "FK_notification_messages_alert_definitions_TenantId_AlertDefin~",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropForeignKey(
                name: "FK_notification_messages_alert_occurrences_TenantId_AlertOccur~",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropTable(
                name: "tenant_notification_providers",
                schema: "compliance360");

            migrationBuilder.DropIndex(
                name: "IX_notification_messages_TenantId_AlertDefinitionId",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropIndex(
                name: "IX_notification_messages_TenantId_AlertDefinitionVersionId",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropIndex(
                name: "IX_notification_messages_TenantId_AlertOccurrenceId",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_alert_occurrences_TenantId_Id",
                schema: "compliance360",
                table: "alert_occurrences");

            migrationBuilder.DropColumn(
                name: "AlertDefinitionId",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropColumn(
                name: "AlertDefinitionVersionId",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.DropColumn(
                name: "AlertOccurrenceId",
                schema: "compliance360",
                table: "notification_messages");
        }
    }
}
