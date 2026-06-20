using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workflow_instances",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    StartedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_instances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workflows",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_assignments",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Decision = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_assignments_workflow_instances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalSchema: "compliance360",
                        principalTable: "workflow_instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_escalations",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EscalatedToUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EscalatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_escalations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_escalations_workflow_instances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalSchema: "compliance360",
                        principalTable: "workflow_instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_history",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_history_workflow_instances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalSchema: "compliance360",
                        principalTable: "workflow_instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_notifications",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    QueuedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_notifications_workflow_instances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalSchema: "compliance360",
                        principalTable: "workflow_instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_rules",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Operator = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ExpectedValue = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_rules_workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalSchema: "compliance360",
                        principalTable: "workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_steps",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    SlaHours = table.Column<int>(type: "integer", nullable: false),
                    AssignedRoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_steps_workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalSchema: "compliance360",
                        principalTable: "workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_transitions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_transitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_transitions_workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalSchema: "compliance360",
                        principalTable: "workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_assignments_TenantId_AssignedToUserId_Status_DueAt~",
                schema: "compliance360",
                table: "workflow_assignments",
                columns: new[] { "TenantId", "AssignedToUserId", "Status", "DueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_assignments_WorkflowInstanceId",
                schema: "compliance360",
                table: "workflow_assignments",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_escalations_TenantId_WorkflowInstanceId_EscalatedA~",
                schema: "compliance360",
                table: "workflow_escalations",
                columns: new[] { "TenantId", "WorkflowInstanceId", "EscalatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_escalations_WorkflowInstanceId",
                schema: "compliance360",
                table: "workflow_escalations",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_history_TenantId_WorkflowInstanceId_OccurredAtUtc",
                schema: "compliance360",
                table: "workflow_history",
                columns: new[] { "TenantId", "WorkflowInstanceId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_history_WorkflowInstanceId",
                schema: "compliance360",
                table: "workflow_history",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instances_TenantId_EntityName_EntityId",
                schema: "compliance360",
                table: "workflow_instances",
                columns: new[] { "TenantId", "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instances_TenantId_WorkflowId_Status",
                schema: "compliance360",
                table: "workflow_instances",
                columns: new[] { "TenantId", "WorkflowId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_notifications_TenantId_WorkflowInstanceId_Kind_Que~",
                schema: "compliance360",
                table: "workflow_notifications",
                columns: new[] { "TenantId", "WorkflowInstanceId", "Kind", "QueuedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_notifications_WorkflowInstanceId",
                schema: "compliance360",
                table: "workflow_notifications",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_rules_TenantId_WorkflowId_FieldName",
                schema: "compliance360",
                table: "workflow_rules",
                columns: new[] { "TenantId", "WorkflowId", "FieldName" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_rules_WorkflowId",
                schema: "compliance360",
                table: "workflow_rules",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_steps_TenantId_WorkflowId_Sequence",
                schema: "compliance360",
                table: "workflow_steps",
                columns: new[] { "TenantId", "WorkflowId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_steps_WorkflowId",
                schema: "compliance360",
                table: "workflow_steps",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_transitions_TenantId_WorkflowId_FromStepId_Decision",
                schema: "compliance360",
                table: "workflow_transitions",
                columns: new[] { "TenantId", "WorkflowId", "FromStepId", "Decision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_transitions_WorkflowId",
                schema: "compliance360",
                table: "workflow_transitions",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_TenantId_Code",
                schema: "compliance360",
                table: "workflows",
                columns: new[] { "TenantId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workflow_assignments",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "workflow_escalations",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "workflow_history",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "workflow_notifications",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "workflow_rules",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "workflow_steps",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "workflow_transitions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "workflow_instances",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "workflows",
                schema: "compliance360");
        }
    }
}
