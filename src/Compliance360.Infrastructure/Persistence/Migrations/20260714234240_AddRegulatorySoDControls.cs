using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRegulatorySoDControls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "InternallyApprovedAtUtc",
                schema: "compliance360",
                table: "registration_dossiers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InternallyApprovedByUserId",
                schema: "compliance360",
                table: "registration_dossiers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastStatusChangedByUserId",
                schema: "compliance360",
                table: "dossier_requirements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "regulatory_sod_settings",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreventSelfReview = table.Column<bool>(type: "boolean", nullable: false),
                    PreventSelfApproval = table.Column<bool>(type: "boolean", nullable: false),
                    SeparateApproverAndSubmitter = table.Column<bool>(type: "boolean", nullable: false),
                    SeparateDocumentUploaderAndReviewer = table.Column<bool>(type: "boolean", nullable: false),
                    RequireSecondApprovalForCriticalWaiver = table.Column<bool>(type: "boolean", nullable: false),
                    RequireApprovalForCriticalityChange = table.Column<bool>(type: "boolean", nullable: false),
                    RequireApprovalForExternalDecisionRecording = table.Column<bool>(type: "boolean", nullable: false),
                    AllowEmergencyOverride = table.Column<bool>(type: "boolean", nullable: false),
                    EmergencyOverrideRequiresReason = table.Column<bool>(type: "boolean", nullable: false),
                    EmergencyOverrideRequiresSecondaryReview = table.Column<bool>(type: "boolean", nullable: false),
                    RequireInternalApprovalBeforeSubmission = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regulatory_sod_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_regulatory_sod_settings_TenantId",
                schema: "compliance360",
                table: "regulatory_sod_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "regulatory_sod_settings",
                schema: "compliance360");

            migrationBuilder.DropColumn(
                name: "InternallyApprovedAtUtc",
                schema: "compliance360",
                table: "registration_dossiers");

            migrationBuilder.DropColumn(
                name: "InternallyApprovedByUserId",
                schema: "compliance360",
                table: "registration_dossiers");

            migrationBuilder.DropColumn(
                name: "LastStatusChangedByUserId",
                schema: "compliance360",
                table: "dossier_requirements");
        }
    }
}
