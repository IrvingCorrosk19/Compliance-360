using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCapaManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "capas",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Priority = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    SourceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CommitmentDueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "capa_approvers",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capa_approvers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_capa_approvers_capas_CapaId",
                        column: x => x.CapaId,
                        principalSchema: "compliance360",
                        principalTable: "capas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "capa_attachments",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoredFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256Hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capa_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_capa_attachments_capas_CapaId",
                        column: x => x.CapaId,
                        principalSchema: "compliance360",
                        principalTable: "capas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "capa_cause_analyses",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Why1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Why2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Why3 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Why4 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Why5 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    People = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Process = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Equipment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Material = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Environment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Measurement = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capa_cause_analyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_capa_cause_analyses_capas_CapaId",
                        column: x => x.CapaId,
                        principalSchema: "compliance360",
                        principalTable: "capas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "capa_containment_actions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capa_containment_actions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_capa_containment_actions_capas_CapaId",
                        column: x => x.CapaId,
                        principalSchema: "compliance360",
                        principalTable: "capas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "capa_corrective_actions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capa_corrective_actions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_capa_corrective_actions_capas_CapaId",
                        column: x => x.CapaId,
                        principalSchema: "compliance360",
                        principalTable: "capas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "capa_effectiveness_checks",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEffective = table.Column<bool>(type: "boolean", nullable: false),
                    VerificationSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capa_effectiveness_checks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_capa_effectiveness_checks_capas_CapaId",
                        column: x => x.CapaId,
                        principalSchema: "compliance360",
                        principalTable: "capas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "capa_evidence",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoredFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256Hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capa_evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_capa_evidence_capas_CapaId",
                        column: x => x.CapaId,
                        principalSchema: "compliance360",
                        principalTable: "capas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "capa_history",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capa_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_capa_history_capas_CapaId",
                        column: x => x.CapaId,
                        principalSchema: "compliance360",
                        principalTable: "capas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "capa_owners",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capa_owners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_capa_owners_capas_CapaId",
                        column: x => x.CapaId,
                        principalSchema: "compliance360",
                        principalTable: "capas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "capa_preventive_actions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capa_preventive_actions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_capa_preventive_actions_capas_CapaId",
                        column: x => x.CapaId,
                        principalSchema: "compliance360",
                        principalTable: "capas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "capa_root_causes",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Method = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capa_root_causes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_capa_root_causes_capas_CapaId",
                        column: x => x.CapaId,
                        principalSchema: "compliance360",
                        principalTable: "capas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_capa_approvers_CapaId",
                schema: "compliance360",
                table: "capa_approvers",
                column: "CapaId");

            migrationBuilder.CreateIndex(
                name: "IX_capa_approvers_TenantId_CapaId_UserId",
                schema: "compliance360",
                table: "capa_approvers",
                columns: new[] { "TenantId", "CapaId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_capa_attachments_CapaId",
                schema: "compliance360",
                table: "capa_attachments",
                column: "CapaId");

            migrationBuilder.CreateIndex(
                name: "IX_capa_attachments_TenantId_CapaId",
                schema: "compliance360",
                table: "capa_attachments",
                columns: new[] { "TenantId", "CapaId" });

            migrationBuilder.CreateIndex(
                name: "IX_capa_cause_analyses_CapaId",
                schema: "compliance360",
                table: "capa_cause_analyses",
                column: "CapaId");

            migrationBuilder.CreateIndex(
                name: "IX_capa_cause_analyses_TenantId_CapaId_Method",
                schema: "compliance360",
                table: "capa_cause_analyses",
                columns: new[] { "TenantId", "CapaId", "Method" });

            migrationBuilder.CreateIndex(
                name: "IX_capa_containment_actions_CapaId",
                schema: "compliance360",
                table: "capa_containment_actions",
                column: "CapaId");

            migrationBuilder.CreateIndex(
                name: "IX_capa_containment_actions_TenantId_CapaId_Status_DueAtUtc",
                schema: "compliance360",
                table: "capa_containment_actions",
                columns: new[] { "TenantId", "CapaId", "Status", "DueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_capa_corrective_actions_CapaId",
                schema: "compliance360",
                table: "capa_corrective_actions",
                column: "CapaId");

            migrationBuilder.CreateIndex(
                name: "IX_capa_corrective_actions_TenantId_CapaId_Status_DueAtUtc",
                schema: "compliance360",
                table: "capa_corrective_actions",
                columns: new[] { "TenantId", "CapaId", "Status", "DueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_capa_effectiveness_checks_CapaId",
                schema: "compliance360",
                table: "capa_effectiveness_checks",
                column: "CapaId");

            migrationBuilder.CreateIndex(
                name: "IX_capa_effectiveness_checks_TenantId_CapaId_VerifiedAtUtc",
                schema: "compliance360",
                table: "capa_effectiveness_checks",
                columns: new[] { "TenantId", "CapaId", "VerifiedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_capa_effectiveness_checks_TenantId_IsEffective",
                schema: "compliance360",
                table: "capa_effectiveness_checks",
                columns: new[] { "TenantId", "IsEffective" });

            migrationBuilder.CreateIndex(
                name: "IX_capa_evidence_CapaId",
                schema: "compliance360",
                table: "capa_evidence",
                column: "CapaId");

            migrationBuilder.CreateIndex(
                name: "IX_capa_evidence_TenantId_CapaId",
                schema: "compliance360",
                table: "capa_evidence",
                columns: new[] { "TenantId", "CapaId" });

            migrationBuilder.CreateIndex(
                name: "IX_capa_evidence_TenantId_Sha256Hash",
                schema: "compliance360",
                table: "capa_evidence",
                columns: new[] { "TenantId", "Sha256Hash" });

            migrationBuilder.CreateIndex(
                name: "IX_capa_history_CapaId",
                schema: "compliance360",
                table: "capa_history",
                column: "CapaId");

            migrationBuilder.CreateIndex(
                name: "IX_capa_history_TenantId_CapaId_OccurredAtUtc",
                schema: "compliance360",
                table: "capa_history",
                columns: new[] { "TenantId", "CapaId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_capa_owners_CapaId",
                schema: "compliance360",
                table: "capa_owners",
                column: "CapaId");

            migrationBuilder.CreateIndex(
                name: "IX_capa_owners_TenantId_CapaId_UserId",
                schema: "compliance360",
                table: "capa_owners",
                columns: new[] { "TenantId", "CapaId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_capa_owners_TenantId_UserId_IsActive_DueAtUtc",
                schema: "compliance360",
                table: "capa_owners",
                columns: new[] { "TenantId", "UserId", "IsActive", "DueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_capa_preventive_actions_CapaId",
                schema: "compliance360",
                table: "capa_preventive_actions",
                column: "CapaId");

            migrationBuilder.CreateIndex(
                name: "IX_capa_preventive_actions_TenantId_CapaId_Status_DueAtUtc",
                schema: "compliance360",
                table: "capa_preventive_actions",
                columns: new[] { "TenantId", "CapaId", "Status", "DueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_capa_root_causes_CapaId",
                schema: "compliance360",
                table: "capa_root_causes",
                column: "CapaId");

            migrationBuilder.CreateIndex(
                name: "IX_capa_root_causes_TenantId_CapaId_Method",
                schema: "compliance360",
                table: "capa_root_causes",
                columns: new[] { "TenantId", "CapaId", "Method" });

            migrationBuilder.CreateIndex(
                name: "IX_capas_TenantId_AuditId",
                schema: "compliance360",
                table: "capas",
                columns: new[] { "TenantId", "AuditId" });

            migrationBuilder.CreateIndex(
                name: "IX_capas_TenantId_Code",
                schema: "compliance360",
                table: "capas",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_capas_TenantId_CommitmentDueAtUtc",
                schema: "compliance360",
                table: "capas",
                columns: new[] { "TenantId", "CommitmentDueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_capas_TenantId_Status_Priority_RiskLevel",
                schema: "compliance360",
                table: "capas",
                columns: new[] { "TenantId", "Status", "Priority", "RiskLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_capas_TenantId_SupplierId",
                schema: "compliance360",
                table: "capas",
                columns: new[] { "TenantId", "SupplierId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "capa_approvers",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "capa_attachments",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "capa_cause_analyses",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "capa_containment_actions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "capa_corrective_actions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "capa_effectiveness_checks",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "capa_evidence",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "capa_history",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "capa_owners",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "capa_preventive_actions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "capa_root_causes",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "capas",
                schema: "compliance360");
        }
    }
}
