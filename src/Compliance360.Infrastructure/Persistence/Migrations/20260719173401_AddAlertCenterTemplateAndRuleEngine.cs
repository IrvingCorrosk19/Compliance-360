using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertCenterTemplateAndRuleEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_notification_templates_TenantId_Id",
                schema: "compliance360",
                table: "notification_templates",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.CreateTable(
                name: "alert_event_types",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Module = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SchemaJson = table.Column<string>(type: "jsonb", nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_event_types", x => x.Id);
                    table.UniqueConstraint("AK_alert_event_types_TenantId_Id", x => new { x.TenantId, x.Id });
                });

            migrationBuilder.Sql(
                """
                INSERT INTO compliance360.alert_event_types
                    ("Id", "Code", "Name", "Module", "SchemaJson", "SchemaVersion", "IsActive", "CreatedAtUtc", "UpdatedAtUtc", "TenantId")
                SELECT
                    md5(tenant."Id"::text || catalog.code)::uuid,
                    catalog.code,
                    catalog.name,
                    catalog.module,
                    '{"type":"object","additionalProperties":true}'::jsonb,
                    1,
                    TRUE,
                    CURRENT_TIMESTAMP,
                    NULL,
                    tenant."Id"
                FROM compliance360.tenants AS tenant
                CROSS JOIN (VALUES
                    ('regulatory.dossier.created', 'Dossier creado', 'RegulatoryAffairs'),
                    ('regulatory.dossier.status_changed', 'Estado de dossier cambiado', 'RegulatoryAffairs'),
                    ('regulatory.dossier.correction_requested', 'Corrección solicitada', 'RegulatoryAffairs'),
                    ('regulatory.dossier.submitted', 'Dossier sometido', 'RegulatoryAffairs'),
                    ('regulatory.dossier.resubmitted', 'Dossier resometido', 'RegulatoryAffairs'),
                    ('regulatory.observation.opened', 'Observación abierta', 'RegulatoryAffairs'),
                    ('regulatory.observation.overdue', 'Observación vencida', 'RegulatoryAffairs'),
                    ('regulatory.registration.expiring', 'Registro sanitario próximo a vencer', 'RegulatoryAffairs'),
                    ('document.created', 'Documento creado', 'Documents'),
                    ('document.approved', 'Documento aprobado', 'Documents'),
                    ('document.expiring', 'Documento próximo a vencer', 'Documents'),
                    ('workflow.step.assigned', 'Paso de workflow asignado', 'Workflow'),
                    ('workflow.step.overdue', 'Paso de workflow vencido', 'Workflow'),
                    ('capa.overdue', 'CAPA vencida', 'CAPA'),
                    ('supplier.document.expiring', 'Documento de proveedor próximo a vencer', 'Suppliers'),
                    ('indicator.threshold_crossed', 'Indicador fuera de umbral', 'QualityIndicators'),
                    ('security.sod_conflict', 'Conflicto de segregación de funciones', 'Security'),
                    ('alert_center.provider.degraded', 'Proveedor degradado', 'AlertCenter'),
                    ('alert_center.backlog.threshold_crossed', 'Backlog fuera de objetivo', 'AlertCenter'),
                    ('alert_center.dead_letter.created', 'Dead letter creada', 'AlertCenter')
                ) AS catalog(code, name, module);
                """);

            migrationBuilder.CreateTable(
                name: "notification_template_versions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Locale = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Subject = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    HtmlBody = table.Column<string>(type: "character varying(64000)", maxLength: 64000, nullable: false),
                    TextBody = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
                    VariablesJson = table.Column<string>(type: "jsonb", nullable: false),
                    BrandingJson = table.Column<string>(type: "jsonb", nullable: true),
                    Lifecycle = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PublishedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedForReviewAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RetiredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArchivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_template_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notification_template_versions_notification_templates_Tenan~",
                        columns: x => new { x.TenantId, x.NotificationTemplateId },
                        principalSchema: "compliance360",
                        principalTable: "notification_templates",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO compliance360.notification_template_versions
                    ("Id", "NotificationTemplateId", "Version", "Locale", "Subject", "HtmlBody", "TextBody",
                     "VariablesJson", "BrandingJson", "Lifecycle", "CreatedByUserId", "ReviewedByUserId",
                     "ApprovedByUserId", "PublishedByUserId", "SubmittedForReviewAtUtc", "ReviewedAtUtc",
                     "ApprovedAtUtc", "PublishedAtUtc", "RetiredAtUtc", "ArchivedAtUtc", "CreatedAtUtc",
                     "UpdatedAtUtc", "TenantId")
                SELECT
                    md5(template."Id"::text || ':alert-center-v1')::uuid,
                    template."Id",
                    GREATEST(1, template."Version"),
                    COALESCE(NULLIF(template."Locale", ''), 'es-PA'),
                    template."Subject",
                    template."Body",
                    template."TextBody",
                    '[]'::jsonb,
                    template."BrandingJson",
                    CASE WHEN template."IsActive" THEN 'Published' ELSE 'Archived' END,
                    actor."UserId",
                    CASE WHEN template."IsActive" THEN actor."UserId" ELSE NULL END,
                    CASE WHEN template."IsActive" THEN actor."UserId" ELSE NULL END,
                    CASE WHEN template."IsActive" THEN actor."UserId" ELSE NULL END,
                    CASE WHEN template."IsActive" THEN template."CreatedAtUtc" ELSE NULL END,
                    CASE WHEN template."IsActive" THEN template."CreatedAtUtc" ELSE NULL END,
                    CASE WHEN template."IsActive" THEN template."CreatedAtUtc" ELSE NULL END,
                    CASE WHEN template."IsActive" THEN template."CreatedAtUtc" ELSE NULL END,
                    NULL,
                    CASE WHEN template."IsActive" THEN NULL ELSE template."CreatedAtUtc" END,
                    template."CreatedAtUtc",
                    template."UpdatedAtUtc",
                    template."TenantId"
                FROM compliance360.notification_templates AS template
                CROSS JOIN LATERAL (
                    SELECT COALESCE(
                        (
                            SELECT user_row."Id"
                            FROM compliance360.users AS user_row
                            WHERE user_row."TenantId" = template."TenantId"
                            ORDER BY user_row."CreatedAtUtc", user_row."Id"
                            LIMIT 1
                        ),
                        md5(template."TenantId"::text || ':alert-center-system')::uuid
                    ) AS "UserId"
                ) AS actor;
                """);

            migrationBuilder.CreateTable(
                name: "alert_definitions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BackupOwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Priority = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Lifecycle = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CurrentPublishedVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_definitions", x => x.Id);
                    table.UniqueConstraint("AK_alert_definitions_TenantId_Id", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_alert_definitions_alert_event_types_TenantId_EventTypeId",
                        columns: x => new { x.TenantId, x.EventTypeId },
                        principalSchema: "compliance360",
                        principalTable: "alert_event_types",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "alert_definition_versions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    ConditionJson = table.Column<string>(type: "jsonb", nullable: false),
                    RecipientRulesJson = table.Column<string>(type: "jsonb", nullable: false),
                    ChannelPoliciesJson = table.Column<string>(type: "jsonb", nullable: false),
                    DedupeExpression = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SilenceWindowMinutes = table.Column<int>(type: "integer", nullable: false),
                    SlaMinutes = table.Column<int>(type: "integer", nullable: true),
                    UnknownPolicy = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Lifecycle = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PublishedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_definition_versions", x => x.Id);
                    table.UniqueConstraint("AK_alert_definition_versions_TenantId_Id", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_alert_definition_versions_alert_definitions_TenantId_Defini~",
                        columns: x => new { x.TenantId, x.DefinitionId },
                        principalSchema: "compliance360",
                        principalTable: "alert_definitions",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "alert_occurrences",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DedupeKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SourceModule = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EvaluatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_occurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_alert_occurrences_alert_definition_versions_TenantId_Defini~",
                        columns: x => new { x.TenantId, x.DefinitionVersionId },
                        principalSchema: "compliance360",
                        principalTable: "alert_definition_versions",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_alert_occurrences_alert_definitions_TenantId_DefinitionId",
                        columns: x => new { x.TenantId, x.DefinitionId },
                        principalSchema: "compliance360",
                        principalTable: "alert_definitions",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_alert_occurrences_alert_event_types_TenantId_EventTypeId",
                        columns: x => new { x.TenantId, x.EventTypeId },
                        principalSchema: "compliance360",
                        principalTable: "alert_event_types",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alert_definition_versions_TenantId_DefinitionId_Version",
                schema: "compliance360",
                table: "alert_definition_versions",
                columns: new[] { "TenantId", "DefinitionId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alert_definition_versions_TenantId_Lifecycle_PublishedAtUtc",
                schema: "compliance360",
                table: "alert_definition_versions",
                columns: new[] { "TenantId", "Lifecycle", "PublishedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_alert_definitions_TenantId_Code",
                schema: "compliance360",
                table: "alert_definitions",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alert_definitions_TenantId_EventTypeId_Lifecycle",
                schema: "compliance360",
                table: "alert_definitions",
                columns: new[] { "TenantId", "EventTypeId", "Lifecycle" });

            migrationBuilder.CreateIndex(
                name: "IX_alert_event_types_TenantId_Code",
                schema: "compliance360",
                table: "alert_event_types",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alert_event_types_TenantId_Module_IsActive",
                schema: "compliance360",
                table: "alert_event_types",
                columns: new[] { "TenantId", "Module", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_alert_occurrences_TenantId_CorrelationId",
                schema: "compliance360",
                table: "alert_occurrences",
                columns: new[] { "TenantId", "CorrelationId" });

            migrationBuilder.CreateIndex(
                name: "IX_alert_occurrences_TenantId_DefinitionId",
                schema: "compliance360",
                table: "alert_occurrences",
                columns: new[] { "TenantId", "DefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_alert_occurrences_TenantId_DefinitionVersionId_DedupeKey",
                schema: "compliance360",
                table: "alert_occurrences",
                columns: new[] { "TenantId", "DefinitionVersionId", "DedupeKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alert_occurrences_TenantId_EventTypeId",
                schema: "compliance360",
                table: "alert_occurrences",
                columns: new[] { "TenantId", "EventTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_alert_occurrences_TenantId_Status_OccurredAtUtc",
                schema: "compliance360",
                table: "alert_occurrences",
                columns: new[] { "TenantId", "Status", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_template_versions_TenantId_Lifecycle_Published~",
                schema: "compliance360",
                table: "notification_template_versions",
                columns: new[] { "TenantId", "Lifecycle", "PublishedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_template_versions_TenantId_NotificationTemplat~",
                schema: "compliance360",
                table: "notification_template_versions",
                columns: new[] { "TenantId", "NotificationTemplateId", "Locale", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alert_occurrences",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "notification_template_versions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "alert_definition_versions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "alert_definitions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "alert_event_types",
                schema: "compliance360");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_notification_templates_TenantId_Id",
                schema: "compliance360",
                table: "notification_templates");
        }
    }
}
