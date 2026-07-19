using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UnifyAlertEventProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_alert_occurrences_TenantId_DefinitionVersionId_DedupeKey",
                schema: "compliance360",
                table: "alert_occurrences");

            migrationBuilder.AddColumn<string>(
                name: "Routing",
                schema: "compliance360",
                table: "notification_messages",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "To");

            migrationBuilder.CreateIndex(
                name: "IX_alert_occurrences_TenantId_DefinitionVersionId_DedupeKey_Oc~",
                schema: "compliance360",
                table: "alert_occurrences",
                columns: new[] { "TenantId", "DefinitionVersionId", "DedupeKey", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_alert_occurrences_TenantId_DefinitionVersionId_DedupeKey_Oc~",
                schema: "compliance360",
                table: "alert_occurrences");

            migrationBuilder.DropColumn(
                name: "Routing",
                schema: "compliance360",
                table: "notification_messages");

            migrationBuilder.CreateIndex(
                name: "IX_alert_occurrences_TenantId_DefinitionVersionId_DedupeKey",
                schema: "compliance360",
                table: "alert_occurrences",
                columns: new[] { "TenantId", "DefinitionVersionId", "DedupeKey" },
                unique: true);
        }
    }
}
