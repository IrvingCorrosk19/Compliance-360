using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HardenRegulatoryWorkflowV2Governance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_dossier_reopen_requests_TenantId_DossierId",
                schema: "compliance360",
                table: "dossier_reopen_requests",
                columns: new[] { "TenantId", "DossierId" },
                unique: true,
                filter: "\"Status\" IN ('Pending', 'Approved')");

            migrationBuilder.CreateIndex(
                name: "IX_dossier_override_requests_TenantId_DossierId",
                schema: "compliance360",
                table: "dossier_override_requests",
                columns: new[] { "TenantId", "DossierId" },
                unique: true,
                filter: "\"Status\" IN ('Pending', 'Approved')");

            migrationBuilder.CreateIndex(
                name: "IX_dossier_evidence_revisions_TenantId_RequirementId",
                schema: "compliance360",
                table: "dossier_evidence_revisions",
                columns: new[] { "TenantId", "RequirementId" },
                unique: true,
                filter: "\"IsCurrent\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_dossier_correction_requests_TenantId_DossierId",
                schema: "compliance360",
                table: "dossier_correction_requests",
                columns: new[] { "TenantId", "DossierId" },
                unique: true,
                filter: "\"Status\" IN ('Open', 'ResponseSubmitted')");

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION compliance360.prevent_regulatory_history_mutation()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    RAISE EXCEPTION 'Regulatory history is append-only';
                END;
                $$;

                CREATE TRIGGER trg_dossier_change_events_append_only
                BEFORE UPDATE OR DELETE ON compliance360.dossier_change_events
                FOR EACH ROW EXECUTE FUNCTION compliance360.prevent_regulatory_history_mutation();

                CREATE TRIGGER trg_dossier_history_events_append_only
                BEFORE UPDATE OR DELETE ON compliance360.dossier_history_events
                FOR EACH ROW EXECUTE FUNCTION compliance360.prevent_regulatory_history_mutation();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS trg_dossier_change_events_append_only ON compliance360.dossier_change_events;
                DROP TRIGGER IF EXISTS trg_dossier_history_events_append_only ON compliance360.dossier_history_events;
                DROP FUNCTION IF EXISTS compliance360.prevent_regulatory_history_mutation();
                """);

            migrationBuilder.DropIndex(
                name: "IX_dossier_reopen_requests_TenantId_DossierId",
                schema: "compliance360",
                table: "dossier_reopen_requests");

            migrationBuilder.DropIndex(
                name: "IX_dossier_override_requests_TenantId_DossierId",
                schema: "compliance360",
                table: "dossier_override_requests");

            migrationBuilder.DropIndex(
                name: "IX_dossier_evidence_revisions_TenantId_RequirementId",
                schema: "compliance360",
                table: "dossier_evidence_revisions");

            migrationBuilder.DropIndex(
                name: "IX_dossier_correction_requests_TenantId_DossierId",
                schema: "compliance360",
                table: "dossier_correction_requests");
        }
    }
}
