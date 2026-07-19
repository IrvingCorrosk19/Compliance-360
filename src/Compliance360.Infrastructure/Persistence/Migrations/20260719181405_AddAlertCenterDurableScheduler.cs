using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertCenterDurableScheduler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "alert_schedules",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CronExpression = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    BusinessCalendarJson = table.Column<string>(type: "jsonb", nullable: false),
                    QuietHoursJson = table.Column<string>(type: "jsonb", nullable: false),
                    CatchUpPolicy = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    MaxCatchUpExecutions = table.Column<int>(type: "integer", nullable: false),
                    Digest = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastExecutionAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextScheduledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    NextExecutionAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LeaseUntilUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LeaseOwner = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_schedules", x => x.Id);
                    table.UniqueConstraint("AK_alert_schedules_TenantId_Id", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_alert_schedules_alert_definitions_TenantId_DefinitionId",
                        columns: x => new { x.TenantId, x.DefinitionId },
                        principalSchema: "compliance360",
                        principalTable: "alert_definitions",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "alert_schedule_executions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledForUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    WorkerId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OccurrenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_schedule_executions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_alert_schedule_executions_alert_schedules_TenantId_Schedule~",
                        columns: x => new { x.TenantId, x.ScheduleId },
                        principalSchema: "compliance360",
                        principalTable: "alert_schedules",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alert_schedule_executions_TenantId_ScheduleId_ScheduledForU~",
                schema: "compliance360",
                table: "alert_schedule_executions",
                columns: new[] { "TenantId", "ScheduleId", "ScheduledForUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alert_schedule_executions_TenantId_Status_StartedAtUtc",
                schema: "compliance360",
                table: "alert_schedule_executions",
                columns: new[] { "TenantId", "Status", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_alert_schedules_IsActive_NextExecutionAtUtc_LeaseUntilUtc",
                schema: "compliance360",
                table: "alert_schedules",
                columns: new[] { "IsActive", "NextExecutionAtUtc", "LeaseUntilUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_alert_schedules_TenantId_Code",
                schema: "compliance360",
                table: "alert_schedules",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alert_schedules_TenantId_DefinitionId",
                schema: "compliance360",
                table: "alert_schedules",
                columns: new[] { "TenantId", "DefinitionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alert_schedule_executions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "alert_schedules",
                schema: "compliance360");
        }
    }
}
