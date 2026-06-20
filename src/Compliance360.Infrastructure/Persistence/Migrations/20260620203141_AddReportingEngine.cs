using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReportingEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "report_categories",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Module = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "report_definitions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Module = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DatasetKey = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "report_dashboard_bindings",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DashboardKey = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    DatasetKey = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_dashboard_bindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_report_dashboard_bindings_report_definitions_ReportDefiniti~",
                        column: x => x.ReportDefinitionId,
                        principalSchema: "compliance360",
                        principalTable: "report_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_executions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParametersJson = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    QueuedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RowCount = table.Column<int>(type: "integer", nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_executions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_report_executions_report_definitions_ReportDefinitionId",
                        column: x => x.ReportDefinitionId,
                        principalSchema: "compliance360",
                        principalTable: "report_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_history",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_report_history_report_definitions_ReportDefinitionId",
                        column: x => x.ReportDefinitionId,
                        principalSchema: "compliance360",
                        principalTable: "report_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_parameters",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Label = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultValue = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_parameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_report_parameters_report_definitions_ReportDefinitionId",
                        column: x => x.ReportDefinitionId,
                        principalSchema: "compliance360",
                        principalTable: "report_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_permissions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Subject = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    CanExecute = table.Column<bool>(type: "boolean", nullable: false),
                    CanExport = table.Column<bool>(type: "boolean", nullable: false),
                    CanSchedule = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_report_permissions_report_definitions_ReportDefinitionId",
                        column: x => x.ReportDefinitionId,
                        principalSchema: "compliance360",
                        principalTable: "report_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_schedules",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Frequency = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    NextRunUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_report_schedules_report_definitions_ReportDefinitionId",
                        column: x => x.ReportDefinitionId,
                        principalSchema: "compliance360",
                        principalTable: "report_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_subscriptions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Recipient = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    Format = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_report_subscriptions_report_definitions_ReportDefinitionId",
                        column: x => x.ReportDefinitionId,
                        principalSchema: "compliance360",
                        principalTable: "report_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_templates",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Format = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Content = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_report_templates_report_definitions_ReportDefinitionId",
                        column: x => x.ReportDefinitionId,
                        principalSchema: "compliance360",
                        principalTable: "report_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_exports",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Format = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    ExportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExportedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_exports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_report_exports_report_executions_ReportExecutionId",
                        column: x => x.ReportExecutionId,
                        principalSchema: "compliance360",
                        principalTable: "report_executions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_outputs",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowCount = table.Column<int>(type: "integer", nullable: false),
                    DatasetDescriptorJson = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_outputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_report_outputs_report_executions_ReportExecutionId",
                        column: x => x.ReportExecutionId,
                        principalSchema: "compliance360",
                        principalTable: "report_executions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_report_categories_TenantId_Code",
                schema: "compliance360",
                table: "report_categories",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_categories_TenantId_Module",
                schema: "compliance360",
                table: "report_categories",
                columns: new[] { "TenantId", "Module" });

            migrationBuilder.CreateIndex(
                name: "IX_report_dashboard_bindings_ReportDefinitionId",
                schema: "compliance360",
                table: "report_dashboard_bindings",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_report_dashboard_bindings_TenantId_DashboardKey_DatasetKey",
                schema: "compliance360",
                table: "report_dashboard_bindings",
                columns: new[] { "TenantId", "DashboardKey", "DatasetKey" });

            migrationBuilder.CreateIndex(
                name: "IX_report_definitions_TenantId_Code",
                schema: "compliance360",
                table: "report_definitions",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_definitions_TenantId_Module_Status",
                schema: "compliance360",
                table: "report_definitions",
                columns: new[] { "TenantId", "Module", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_report_executions_ReportDefinitionId",
                schema: "compliance360",
                table: "report_executions",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_report_executions_TenantId_ReportDefinitionId_Status_Queued~",
                schema: "compliance360",
                table: "report_executions",
                columns: new[] { "TenantId", "ReportDefinitionId", "Status", "QueuedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_report_exports_ReportExecutionId",
                schema: "compliance360",
                table: "report_exports",
                column: "ReportExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_report_exports_TenantId_ReportDefinitionId_Format_ExportedA~",
                schema: "compliance360",
                table: "report_exports",
                columns: new[] { "TenantId", "ReportDefinitionId", "Format", "ExportedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_report_history_ReportDefinitionId",
                schema: "compliance360",
                table: "report_history",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_report_history_TenantId_ReportDefinitionId_OccurredAtUtc",
                schema: "compliance360",
                table: "report_history",
                columns: new[] { "TenantId", "ReportDefinitionId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_report_outputs_ReportExecutionId",
                schema: "compliance360",
                table: "report_outputs",
                column: "ReportExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_report_outputs_TenantId_ReportDefinitionId_ReportExecutionId",
                schema: "compliance360",
                table: "report_outputs",
                columns: new[] { "TenantId", "ReportDefinitionId", "ReportExecutionId" });

            migrationBuilder.CreateIndex(
                name: "IX_report_parameters_ReportDefinitionId",
                schema: "compliance360",
                table: "report_parameters",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_report_parameters_TenantId_ReportDefinitionId_Name",
                schema: "compliance360",
                table: "report_parameters",
                columns: new[] { "TenantId", "ReportDefinitionId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_permissions_ReportDefinitionId",
                schema: "compliance360",
                table: "report_permissions",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_report_permissions_TenantId_ReportDefinitionId_Scope_Subject",
                schema: "compliance360",
                table: "report_permissions",
                columns: new[] { "TenantId", "ReportDefinitionId", "Scope", "Subject" });

            migrationBuilder.CreateIndex(
                name: "IX_report_schedules_ReportDefinitionId",
                schema: "compliance360",
                table: "report_schedules",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_report_schedules_TenantId_ReportDefinitionId_IsActive_NextR~",
                schema: "compliance360",
                table: "report_schedules",
                columns: new[] { "TenantId", "ReportDefinitionId", "IsActive", "NextRunUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_report_subscriptions_ReportDefinitionId",
                schema: "compliance360",
                table: "report_subscriptions",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_report_subscriptions_TenantId_ReportDefinitionId_IsActive",
                schema: "compliance360",
                table: "report_subscriptions",
                columns: new[] { "TenantId", "ReportDefinitionId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_report_templates_ReportDefinitionId",
                schema: "compliance360",
                table: "report_templates",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_report_templates_TenantId_ReportDefinitionId_Format_Version",
                schema: "compliance360",
                table: "report_templates",
                columns: new[] { "TenantId", "ReportDefinitionId", "Format", "Version" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "report_categories",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "report_dashboard_bindings",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "report_exports",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "report_history",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "report_outputs",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "report_parameters",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "report_permissions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "report_schedules",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "report_subscriptions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "report_templates",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "report_executions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "report_definitions",
                schema: "compliance360");
        }
    }
}
