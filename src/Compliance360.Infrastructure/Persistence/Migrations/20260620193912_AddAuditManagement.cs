using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_auditors",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsLead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_auditors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_checklists",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsTemplate = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_checklists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_plans",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Criteria = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PlannedStartUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PlannedEndUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_programs",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_programs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "managed_audits",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChecklistId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClosedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_managed_audits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_checklist_items",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChecklistId = table.Column<Guid>(type: "uuid", nullable: false),
                    Clause = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Question = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_checklist_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_checklist_items_audit_checklists_ChecklistId",
                        column: x => x.ChecklistId,
                        principalSchema: "compliance360",
                        principalTable: "audit_checklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_areas",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Process = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_areas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_areas_managed_audits_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "compliance360",
                        principalTable: "managed_audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_attachments",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_audit_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_attachments_managed_audits_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "compliance360",
                        principalTable: "managed_audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_corrective_action_links",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    FindingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrectiveActionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_corrective_action_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_corrective_action_links_managed_audits_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "compliance360",
                        principalTable: "managed_audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_evidence",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    FindingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
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
                    table.PrimaryKey("PK_audit_evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_evidence_managed_audits_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "compliance360",
                        principalTable: "managed_audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_findings",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ChecklistItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_findings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_findings_managed_audits_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "compliance360",
                        principalTable: "managed_audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_non_conformities",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    FindingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Requirement = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_non_conformities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_non_conformities_managed_audits_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "compliance360",
                        principalTable: "managed_audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_observations",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_observations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_observations_managed_audits_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "compliance360",
                        principalTable: "managed_audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_participants",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_participants_managed_audits_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "compliance360",
                        principalTable: "managed_audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_recommendations",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    FindingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Recommendation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_recommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_recommendations_managed_audits_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "compliance360",
                        principalTable: "managed_audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_schedules",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_schedules_managed_audits_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "compliance360",
                        principalTable: "managed_audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "managed_audit_history",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_managed_audit_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_managed_audit_history_managed_audits_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "compliance360",
                        principalTable: "managed_audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_areas_AuditId",
                schema: "compliance360",
                table: "audit_areas",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_areas_TenantId_AuditId_Name",
                schema: "compliance360",
                table: "audit_areas",
                columns: new[] { "TenantId", "AuditId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_attachments_AuditId",
                schema: "compliance360",
                table: "audit_attachments",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_attachments_TenantId_AuditId",
                schema: "compliance360",
                table: "audit_attachments",
                columns: new[] { "TenantId", "AuditId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_auditors_TenantId_AuditId_UserId",
                schema: "compliance360",
                table: "audit_auditors",
                columns: new[] { "TenantId", "AuditId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_checklist_items_ChecklistId",
                schema: "compliance360",
                table: "audit_checklist_items",
                column: "ChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_checklist_items_TenantId_ChecklistId_Clause",
                schema: "compliance360",
                table: "audit_checklist_items",
                columns: new[] { "TenantId", "ChecklistId", "Clause" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_checklists_TenantId_Code_Version",
                schema: "compliance360",
                table: "audit_checklists",
                columns: new[] { "TenantId", "Code", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_corrective_action_links_AuditId",
                schema: "compliance360",
                table: "audit_corrective_action_links",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_corrective_action_links_TenantId_FindingId_Corrective~",
                schema: "compliance360",
                table: "audit_corrective_action_links",
                columns: new[] { "TenantId", "FindingId", "CorrectiveActionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_evidence_AuditId",
                schema: "compliance360",
                table: "audit_evidence",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_evidence_TenantId_FindingId",
                schema: "compliance360",
                table: "audit_evidence",
                columns: new[] { "TenantId", "FindingId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_evidence_TenantId_Sha256Hash",
                schema: "compliance360",
                table: "audit_evidence",
                columns: new[] { "TenantId", "Sha256Hash" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_findings_AuditId",
                schema: "compliance360",
                table: "audit_findings",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_findings_TenantId_AuditId_Severity",
                schema: "compliance360",
                table: "audit_findings",
                columns: new[] { "TenantId", "AuditId", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_findings_TenantId_ReportedAtUtc",
                schema: "compliance360",
                table: "audit_findings",
                columns: new[] { "TenantId", "ReportedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_non_conformities_AuditId",
                schema: "compliance360",
                table: "audit_non_conformities",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_non_conformities_TenantId_FindingId",
                schema: "compliance360",
                table: "audit_non_conformities",
                columns: new[] { "TenantId", "FindingId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_observations_AuditId",
                schema: "compliance360",
                table: "audit_observations",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_observations_TenantId_AuditId_ReportedAtUtc",
                schema: "compliance360",
                table: "audit_observations",
                columns: new[] { "TenantId", "AuditId", "ReportedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_participants_AuditId",
                schema: "compliance360",
                table: "audit_participants",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_participants_TenantId_AuditId_UserId_Role",
                schema: "compliance360",
                table: "audit_participants",
                columns: new[] { "TenantId", "AuditId", "UserId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_plans_TenantId_AuditProgramId_PlannedStartUtc",
                schema: "compliance360",
                table: "audit_plans",
                columns: new[] { "TenantId", "AuditProgramId", "PlannedStartUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_programs_TenantId_Code",
                schema: "compliance360",
                table: "audit_programs",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_programs_TenantId_Year_IsActive",
                schema: "compliance360",
                table: "audit_programs",
                columns: new[] { "TenantId", "Year", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_recommendations_AuditId",
                schema: "compliance360",
                table: "audit_recommendations",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_recommendations_TenantId_FindingId",
                schema: "compliance360",
                table: "audit_recommendations",
                columns: new[] { "TenantId", "FindingId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_schedules_AuditId",
                schema: "compliance360",
                table: "audit_schedules",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_schedules_TenantId_AuditId_StartUtc",
                schema: "compliance360",
                table: "audit_schedules",
                columns: new[] { "TenantId", "AuditId", "StartUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_managed_audit_history_AuditId",
                schema: "compliance360",
                table: "managed_audit_history",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_managed_audit_history_TenantId_AuditId_OccurredAtUtc",
                schema: "compliance360",
                table: "managed_audit_history",
                columns: new[] { "TenantId", "AuditId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_managed_audits_TenantId_ClosedAtUtc",
                schema: "compliance360",
                table: "managed_audits",
                columns: new[] { "TenantId", "ClosedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_managed_audits_TenantId_Code",
                schema: "compliance360",
                table: "managed_audits",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_managed_audits_TenantId_Type_Status",
                schema: "compliance360",
                table: "managed_audits",
                columns: new[] { "TenantId", "Type", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_areas",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "audit_attachments",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "audit_auditors",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "audit_checklist_items",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "audit_corrective_action_links",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "audit_evidence",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "audit_findings",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "audit_non_conformities",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "audit_observations",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "audit_participants",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "audit_plans",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "audit_programs",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "audit_recommendations",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "audit_schedules",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "managed_audit_history",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "audit_checklists",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "managed_audits",
                schema: "compliance360");
        }
    }
}
