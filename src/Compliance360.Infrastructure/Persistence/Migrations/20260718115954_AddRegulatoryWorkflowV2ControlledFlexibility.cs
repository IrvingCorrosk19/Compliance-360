using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRegulatoryWorkflowV2ControlledFlexibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Revision",
                schema: "compliance360",
                table: "registration_dossiers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "dossier_change_events",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DossierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<long>(type: "bigint", nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorRole = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    FromStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Field = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BeforeJson = table.Column<string>(type: "jsonb", nullable: true),
                    AfterJson = table.Column<string>(type: "jsonb", nullable: true),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dossier_change_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dossier_change_events_registration_dossiers_DossierId",
                        column: x => x.DossierId,
                        principalSchema: "compliance360",
                        principalTable: "registration_dossiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dossier_correction_requests",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DossierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dossier_correction_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dossier_correction_requests_registration_dossiers_DossierId",
                        column: x => x.DossierId,
                        principalSchema: "compliance360",
                        principalTable: "registration_dossiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dossier_override_requests",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DossierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RequesterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ConsumedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConsumedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dossier_override_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dossier_override_requests_registration_dossiers_DossierId",
                        column: x => x.DossierId,
                        principalSchema: "compliance360",
                        principalTable: "registration_dossiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dossier_reopen_requests",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DossierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RequesterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dossier_reopen_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dossier_reopen_requests_registration_dossiers_DossierId",
                        column: x => x.DossierId,
                        principalSchema: "compliance360",
                        principalTable: "registration_dossiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dossier_correction_scope_items",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrectionRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: true),
                    FieldPath = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dossier_correction_scope_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dossier_correction_scope_items_dossier_correction_requests_~",
                        column: x => x.CorrectionRequestId,
                        principalSchema: "compliance360",
                        principalTable: "dossier_correction_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dossier_evidence_revisions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DossierId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrectionRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    StoredFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FileName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dossier_evidence_revisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dossier_evidence_revisions_dossier_correction_requests_Corr~",
                        column: x => x.CorrectionRequestId,
                        principalSchema: "compliance360",
                        principalTable: "dossier_correction_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dossier_evidence_revisions_dossier_requirements_Requirement~",
                        column: x => x.RequirementId,
                        principalSchema: "compliance360",
                        principalTable: "dossier_requirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dossier_evidence_revisions_registration_dossiers_DossierId",
                        column: x => x.DossierId,
                        principalSchema: "compliance360",
                        principalTable: "registration_dossiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dossier_override_approvals",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OverrideRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Stage = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApproverUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dossier_override_approvals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dossier_override_approvals_dossier_override_requests_Overri~",
                        column: x => x.OverrideRequestId,
                        principalSchema: "compliance360",
                        principalTable: "dossier_override_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dossier_reopen_approvals",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReopenRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Stage = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApproverUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dossier_reopen_approvals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dossier_reopen_approvals_dossier_reopen_requests_ReopenRequ~",
                        column: x => x.ReopenRequestId,
                        principalSchema: "compliance360",
                        principalTable: "dossier_reopen_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dossier_change_events_DossierId",
                schema: "compliance360",
                table: "dossier_change_events",
                column: "DossierId");

            migrationBuilder.CreateIndex(
                name: "IX_dossier_change_events_TenantId_DossierId_Sequence",
                schema: "compliance360",
                table: "dossier_change_events",
                columns: new[] { "TenantId", "DossierId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dossier_correction_requests_DossierId",
                schema: "compliance360",
                table: "dossier_correction_requests",
                column: "DossierId");

            migrationBuilder.CreateIndex(
                name: "IX_dossier_correction_requests_TenantId_DossierId_Status",
                schema: "compliance360",
                table: "dossier_correction_requests",
                columns: new[] { "TenantId", "DossierId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_dossier_correction_scope_items_CorrectionRequestId",
                schema: "compliance360",
                table: "dossier_correction_scope_items",
                column: "CorrectionRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_dossier_correction_scope_items_TenantId_CorrectionRequestId~",
                schema: "compliance360",
                table: "dossier_correction_scope_items",
                columns: new[] { "TenantId", "CorrectionRequestId", "ScopeType" });

            migrationBuilder.CreateIndex(
                name: "IX_dossier_evidence_revisions_CorrectionRequestId",
                schema: "compliance360",
                table: "dossier_evidence_revisions",
                column: "CorrectionRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_dossier_evidence_revisions_DossierId",
                schema: "compliance360",
                table: "dossier_evidence_revisions",
                column: "DossierId");

            migrationBuilder.CreateIndex(
                name: "IX_dossier_evidence_revisions_RequirementId",
                schema: "compliance360",
                table: "dossier_evidence_revisions",
                column: "RequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_dossier_evidence_revisions_TenantId_DossierId_IsCurrent",
                schema: "compliance360",
                table: "dossier_evidence_revisions",
                columns: new[] { "TenantId", "DossierId", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_dossier_evidence_revisions_TenantId_RequirementId_VersionNu~",
                schema: "compliance360",
                table: "dossier_evidence_revisions",
                columns: new[] { "TenantId", "RequirementId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dossier_override_approvals_OverrideRequestId",
                schema: "compliance360",
                table: "dossier_override_approvals",
                column: "OverrideRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_dossier_override_approvals_TenantId_OverrideRequestId_Appro~",
                schema: "compliance360",
                table: "dossier_override_approvals",
                columns: new[] { "TenantId", "OverrideRequestId", "ApproverUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dossier_override_approvals_TenantId_OverrideRequestId_Stage",
                schema: "compliance360",
                table: "dossier_override_approvals",
                columns: new[] { "TenantId", "OverrideRequestId", "Stage" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dossier_override_requests_DossierId",
                schema: "compliance360",
                table: "dossier_override_requests",
                column: "DossierId");

            migrationBuilder.CreateIndex(
                name: "IX_dossier_override_requests_TenantId_DossierId_Status",
                schema: "compliance360",
                table: "dossier_override_requests",
                columns: new[] { "TenantId", "DossierId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_dossier_reopen_approvals_ReopenRequestId",
                schema: "compliance360",
                table: "dossier_reopen_approvals",
                column: "ReopenRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_dossier_reopen_approvals_TenantId_ReopenRequestId_ApproverU~",
                schema: "compliance360",
                table: "dossier_reopen_approvals",
                columns: new[] { "TenantId", "ReopenRequestId", "ApproverUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dossier_reopen_approvals_TenantId_ReopenRequestId_Stage",
                schema: "compliance360",
                table: "dossier_reopen_approvals",
                columns: new[] { "TenantId", "ReopenRequestId", "Stage" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dossier_reopen_requests_DossierId",
                schema: "compliance360",
                table: "dossier_reopen_requests",
                column: "DossierId");

            migrationBuilder.CreateIndex(
                name: "IX_dossier_reopen_requests_TenantId_DossierId_Status",
                schema: "compliance360",
                table: "dossier_reopen_requests",
                columns: new[] { "TenantId", "DossierId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("This append-only regulatory history migration cannot be reversed destructively.");
        }
    }
}
