using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "risk_categories",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "risk_matrices",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    ToleranceScore = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_matrices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "risks",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Area = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Process = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: true),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewDueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    InherentScore = table.Column<int>(type: "integer", nullable: false),
                    InherentLevel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ResidualScore = table.Column<int>(type: "integer", nullable: false),
                    ResidualLevel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IsAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    IsWithinTolerance = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "risk_assessments",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Probability = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Impact = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    InherentScore = table.Column<int>(type: "integer", nullable: false),
                    InherentLevel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ResidualProbability = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ResidualImpact = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ResidualScore = table.Column<int>(type: "integer", nullable: false),
                    ResidualLevel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ToleranceScore = table.Column<int>(type: "integer", nullable: false),
                    AssessedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_risk_assessments_risks_RiskId",
                        column: x => x.RiskId,
                        principalSchema: "compliance360",
                        principalTable: "risks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "risk_attachments",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_risk_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_risk_attachments_risks_RiskId",
                        column: x => x.RiskId,
                        principalSchema: "compliance360",
                        principalTable: "risks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "risk_controls",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsEffective = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_controls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_risk_controls_risks_RiskId",
                        column: x => x.RiskId,
                        principalSchema: "compliance360",
                        principalTable: "risks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "risk_evidence",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_risk_evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_risk_evidence_risks_RiskId",
                        column: x => x.RiskId,
                        principalSchema: "compliance360",
                        principalTable: "risks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "risk_history",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_risk_history_risks_RiskId",
                        column: x => x.RiskId,
                        principalSchema: "compliance360",
                        principalTable: "risks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "risk_indicators",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Threshold = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    IsBreached = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_indicators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_risk_indicators_risks_RiskId",
                        column: x => x.RiskId,
                        principalSchema: "compliance360",
                        principalTable: "risks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "risk_mitigation_plans",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_mitigation_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_risk_mitigation_plans_risks_RiskId",
                        column: x => x.RiskId,
                        principalSchema: "compliance360",
                        principalTable: "risks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "risk_owners",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_owners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_risk_owners_risks_RiskId",
                        column: x => x.RiskId,
                        principalSchema: "compliance360",
                        principalTable: "risks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "risk_reviews",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_risk_reviews_risks_RiskId",
                        column: x => x.RiskId,
                        principalSchema: "compliance360",
                        principalTable: "risks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "risk_treatments",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Strategy = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Rationale = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_treatments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_risk_treatments_risks_RiskId",
                        column: x => x.RiskId,
                        principalSchema: "compliance360",
                        principalTable: "risks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_risk_assessments_RiskId",
                schema: "compliance360",
                table: "risk_assessments",
                column: "RiskId");

            migrationBuilder.CreateIndex(
                name: "IX_risk_assessments_TenantId_RiskId_AssessedAtUtc",
                schema: "compliance360",
                table: "risk_assessments",
                columns: new[] { "TenantId", "RiskId", "AssessedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_risk_attachments_RiskId",
                schema: "compliance360",
                table: "risk_attachments",
                column: "RiskId");

            migrationBuilder.CreateIndex(
                name: "IX_risk_categories_TenantId_Code",
                schema: "compliance360",
                table: "risk_categories",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_risk_controls_RiskId",
                schema: "compliance360",
                table: "risk_controls",
                column: "RiskId");

            migrationBuilder.CreateIndex(
                name: "IX_risk_controls_TenantId_RiskId_Type",
                schema: "compliance360",
                table: "risk_controls",
                columns: new[] { "TenantId", "RiskId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_risk_evidence_RiskId",
                schema: "compliance360",
                table: "risk_evidence",
                column: "RiskId");

            migrationBuilder.CreateIndex(
                name: "IX_risk_evidence_TenantId_RiskId",
                schema: "compliance360",
                table: "risk_evidence",
                columns: new[] { "TenantId", "RiskId" });

            migrationBuilder.CreateIndex(
                name: "IX_risk_history_RiskId",
                schema: "compliance360",
                table: "risk_history",
                column: "RiskId");

            migrationBuilder.CreateIndex(
                name: "IX_risk_history_TenantId_RiskId_OccurredAtUtc",
                schema: "compliance360",
                table: "risk_history",
                columns: new[] { "TenantId", "RiskId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_risk_indicators_RiskId",
                schema: "compliance360",
                table: "risk_indicators",
                column: "RiskId");

            migrationBuilder.CreateIndex(
                name: "IX_risk_indicators_TenantId_RiskId_IsBreached",
                schema: "compliance360",
                table: "risk_indicators",
                columns: new[] { "TenantId", "RiskId", "IsBreached" });

            migrationBuilder.CreateIndex(
                name: "IX_risk_matrices_TenantId_IsDefault",
                schema: "compliance360",
                table: "risk_matrices",
                columns: new[] { "TenantId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_risk_mitigation_plans_RiskId",
                schema: "compliance360",
                table: "risk_mitigation_plans",
                column: "RiskId");

            migrationBuilder.CreateIndex(
                name: "IX_risk_mitigation_plans_TenantId_ResponsibleUserId_DueAtUtc_I~",
                schema: "compliance360",
                table: "risk_mitigation_plans",
                columns: new[] { "TenantId", "ResponsibleUserId", "DueAtUtc", "IsCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_risk_owners_RiskId",
                schema: "compliance360",
                table: "risk_owners",
                column: "RiskId");

            migrationBuilder.CreateIndex(
                name: "IX_risk_owners_TenantId_RiskId_UserId",
                schema: "compliance360",
                table: "risk_owners",
                columns: new[] { "TenantId", "RiskId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_risk_reviews_RiskId",
                schema: "compliance360",
                table: "risk_reviews",
                column: "RiskId");

            migrationBuilder.CreateIndex(
                name: "IX_risk_reviews_TenantId_RiskId_DueAtUtc_Status",
                schema: "compliance360",
                table: "risk_reviews",
                columns: new[] { "TenantId", "RiskId", "DueAtUtc", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_risk_treatments_RiskId",
                schema: "compliance360",
                table: "risk_treatments",
                column: "RiskId");

            migrationBuilder.CreateIndex(
                name: "IX_risks_TenantId_Area_Process",
                schema: "compliance360",
                table: "risks",
                columns: new[] { "TenantId", "Area", "Process" });

            migrationBuilder.CreateIndex(
                name: "IX_risks_TenantId_AuditId",
                schema: "compliance360",
                table: "risks",
                columns: new[] { "TenantId", "AuditId" });

            migrationBuilder.CreateIndex(
                name: "IX_risks_TenantId_CapaId",
                schema: "compliance360",
                table: "risks",
                columns: new[] { "TenantId", "CapaId" });

            migrationBuilder.CreateIndex(
                name: "IX_risks_TenantId_Code",
                schema: "compliance360",
                table: "risks",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_risks_TenantId_Status_Type_ResidualLevel",
                schema: "compliance360",
                table: "risks",
                columns: new[] { "TenantId", "Status", "Type", "ResidualLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_risks_TenantId_SupplierId",
                schema: "compliance360",
                table: "risks",
                columns: new[] { "TenantId", "SupplierId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "risk_assessments",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "risk_attachments",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "risk_categories",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "risk_controls",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "risk_evidence",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "risk_history",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "risk_indicators",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "risk_matrices",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "risk_mitigation_plans",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "risk_owners",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "risk_reviews",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "risk_treatments",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "risks",
                schema: "compliance360");
        }
    }
}
