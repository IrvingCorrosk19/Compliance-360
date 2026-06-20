using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityIndicators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "indicator_categories",
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
                    table.PrimaryKey("PK_indicator_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "quality_indicators",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Frequency = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CalculationType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Unit = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: true),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: true),
                    RiskId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quality_indicators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "indicator_alerts",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResultId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAcknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indicator_alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_indicator_alerts_quality_indicators_IndicatorId",
                        column: x => x.IndicatorId,
                        principalSchema: "compliance360",
                        principalTable: "quality_indicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "indicator_attachments",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicatorId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_indicator_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_indicator_attachments_quality_indicators_IndicatorId",
                        column: x => x.IndicatorId,
                        principalSchema: "compliance360",
                        principalTable: "quality_indicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "indicator_formulas",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Expression = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CalculationType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indicator_formulas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_indicator_formulas_quality_indicators_IndicatorId",
                        column: x => x.IndicatorId,
                        principalSchema: "compliance360",
                        principalTable: "quality_indicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "indicator_history",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indicator_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_indicator_history_quality_indicators_IndicatorId",
                        column: x => x.IndicatorId,
                        principalSchema: "compliance360",
                        principalTable: "quality_indicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "indicator_measurements",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    Numerator = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Denominator = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    IsAutomatic = table.Column<bool>(type: "boolean", nullable: false),
                    CapturedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CapturedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indicator_measurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_indicator_measurements_quality_indicators_IndicatorId",
                        column: x => x.IndicatorId,
                        principalSchema: "compliance360",
                        principalTable: "quality_indicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "indicator_periods",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    PeriodNumber = table.Column<int>(type: "integer", nullable: false),
                    StartUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indicator_periods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_indicator_periods_quality_indicators_IndicatorId",
                        column: x => x.IndicatorId,
                        principalSchema: "compliance360",
                        principalTable: "quality_indicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "indicator_processes",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Area = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indicator_processes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_indicator_processes_quality_indicators_IndicatorId",
                        column: x => x.IndicatorId,
                        principalSchema: "compliance360",
                        principalTable: "quality_indicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "indicator_results",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    MeasurementId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TargetValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indicator_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_indicator_results_quality_indicators_IndicatorId",
                        column: x => x.IndicatorId,
                        principalSchema: "compliance360",
                        principalTable: "quality_indicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "indicator_targets",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    EffectiveFromUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indicator_targets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_indicator_targets_quality_indicators_IndicatorId",
                        column: x => x.IndicatorId,
                        principalSchema: "compliance360",
                        principalTable: "quality_indicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "indicator_thresholds",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarningMinimum = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CriticalMinimum = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ExcellentMinimum = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indicator_thresholds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_indicator_thresholds_quality_indicators_IndicatorId",
                        column: x => x.IndicatorId,
                        principalSchema: "compliance360",
                        principalTable: "quality_indicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "indicator_trends",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    Direction = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PreviousValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indicator_trends", x => x.Id);
                    table.ForeignKey(
                        name: "FK_indicator_trends_quality_indicators_IndicatorId",
                        column: x => x.IndicatorId,
                        principalSchema: "compliance360",
                        principalTable: "quality_indicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_indicator_alerts_IndicatorId",
                schema: "compliance360",
                table: "indicator_alerts",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_indicator_alerts_TenantId_IndicatorId_Type_IsAcknowledged",
                schema: "compliance360",
                table: "indicator_alerts",
                columns: new[] { "TenantId", "IndicatorId", "Type", "IsAcknowledged" });

            migrationBuilder.CreateIndex(
                name: "IX_indicator_attachments_IndicatorId",
                schema: "compliance360",
                table: "indicator_attachments",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_indicator_categories_TenantId_Code",
                schema: "compliance360",
                table: "indicator_categories",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_indicator_formulas_IndicatorId",
                schema: "compliance360",
                table: "indicator_formulas",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_indicator_history_IndicatorId",
                schema: "compliance360",
                table: "indicator_history",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_indicator_history_TenantId_IndicatorId_OccurredAtUtc",
                schema: "compliance360",
                table: "indicator_history",
                columns: new[] { "TenantId", "IndicatorId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_indicator_measurements_IndicatorId",
                schema: "compliance360",
                table: "indicator_measurements",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_indicator_measurements_TenantId_IndicatorId_PeriodId",
                schema: "compliance360",
                table: "indicator_measurements",
                columns: new[] { "TenantId", "IndicatorId", "PeriodId" });

            migrationBuilder.CreateIndex(
                name: "IX_indicator_periods_IndicatorId",
                schema: "compliance360",
                table: "indicator_periods",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_indicator_periods_TenantId_IndicatorId_Year_PeriodNumber",
                schema: "compliance360",
                table: "indicator_periods",
                columns: new[] { "TenantId", "IndicatorId", "Year", "PeriodNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_indicator_processes_IndicatorId",
                schema: "compliance360",
                table: "indicator_processes",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_indicator_processes_TenantId_Area_ProcessName",
                schema: "compliance360",
                table: "indicator_processes",
                columns: new[] { "TenantId", "Area", "ProcessName" });

            migrationBuilder.CreateIndex(
                name: "IX_indicator_results_IndicatorId",
                schema: "compliance360",
                table: "indicator_results",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_indicator_results_TenantId_IndicatorId_PeriodId_Status",
                schema: "compliance360",
                table: "indicator_results",
                columns: new[] { "TenantId", "IndicatorId", "PeriodId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_indicator_targets_IndicatorId",
                schema: "compliance360",
                table: "indicator_targets",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_indicator_thresholds_IndicatorId",
                schema: "compliance360",
                table: "indicator_thresholds",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_indicator_trends_IndicatorId",
                schema: "compliance360",
                table: "indicator_trends",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_indicator_trends_TenantId_IndicatorId_Direction",
                schema: "compliance360",
                table: "indicator_trends",
                columns: new[] { "TenantId", "IndicatorId", "Direction" });

            migrationBuilder.CreateIndex(
                name: "IX_quality_indicators_TenantId_AuditId",
                schema: "compliance360",
                table: "quality_indicators",
                columns: new[] { "TenantId", "AuditId" });

            migrationBuilder.CreateIndex(
                name: "IX_quality_indicators_TenantId_CapaId",
                schema: "compliance360",
                table: "quality_indicators",
                columns: new[] { "TenantId", "CapaId" });

            migrationBuilder.CreateIndex(
                name: "IX_quality_indicators_TenantId_Code",
                schema: "compliance360",
                table: "quality_indicators",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quality_indicators_TenantId_RiskId",
                schema: "compliance360",
                table: "quality_indicators",
                columns: new[] { "TenantId", "RiskId" });

            migrationBuilder.CreateIndex(
                name: "IX_quality_indicators_TenantId_Status_Type_Frequency",
                schema: "compliance360",
                table: "quality_indicators",
                columns: new[] { "TenantId", "Status", "Type", "Frequency" });

            migrationBuilder.CreateIndex(
                name: "IX_quality_indicators_TenantId_SupplierId",
                schema: "compliance360",
                table: "quality_indicators",
                columns: new[] { "TenantId", "SupplierId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "indicator_alerts",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "indicator_attachments",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "indicator_categories",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "indicator_formulas",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "indicator_history",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "indicator_measurements",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "indicator_periods",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "indicator_processes",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "indicator_results",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "indicator_targets",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "indicator_thresholds",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "indicator_trends",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "quality_indicators",
                schema: "compliance360");
        }
    }
}
