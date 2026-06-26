using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantAdministrationOmega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ForcePasswordChangeRequired",
                schema: "compliance360",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "tenant_api_credentials",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    KeyPrefix = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    KeyHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Scopes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastUsedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_api_credentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_api_credentials_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "compliance360",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_backup_records",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BackupKind = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Result = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Rpo = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Rto = table.Column<TimeSpan>(type: "interval", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_backup_records", x => x.Id);
                    table.CheckConstraint("CK_tenant_backup_records_SizeBytes", "\"SizeBytes\" >= 0");
                    table.CheckConstraint("CK_tenant_backup_records_Window", "\"CompletedAtUtc\" >= \"StartedAtUtc\"");
                    table.ForeignKey(
                        name: "FK_tenant_backup_records_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "compliance360",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_domains",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HostName = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: false),
                    Kind = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    VerificationToken = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DnsStatus = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CertificateStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    HttpsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RedirectToHostName = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: true),
                    VerifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastCheckedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_domains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_domains_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "compliance360",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_health_signals",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Component = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CheckedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_health_signals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_health_signals_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "compliance360",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_licenses",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LicenseNumber = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    FeaturesJson = table.Column<string>(type: "jsonb", nullable: false),
                    ModulesJson = table.Column<string>(type: "jsonb", nullable: false),
                    EntitlementsJson = table.Column<string>(type: "jsonb", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    RenewalDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SeatsPurchased = table.Column<int>(type: "integer", nullable: false),
                    SeatsUsed = table.Column<int>(type: "integer", nullable: false),
                    StorageGbPurchased = table.Column<int>(type: "integer", nullable: false),
                    StorageBytesUsed = table.Column<long>(type: "bigint", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_licenses", x => x.Id);
                    table.CheckConstraint("CK_tenant_licenses_Period", "\"PeriodEnd\" >= \"PeriodStart\"");
                    table.CheckConstraint("CK_tenant_licenses_Seats", "\"SeatsPurchased\" >= 0 AND \"SeatsUsed\" >= 0");
                    table.CheckConstraint("CK_tenant_licenses_Storage", "\"StorageGbPurchased\" >= 0 AND \"StorageBytesUsed\" >= 0");
                    table.ForeignKey(
                        name: "FK_tenant_licenses_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "compliance360",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_sso_configurations",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Authority = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MetadataUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ClientId = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    SecretReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CertificateThumbprint = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ClaimsMappingJson = table.Column<string>(type: "jsonb", nullable: false),
                    RoleMappingJson = table.Column<string>(type: "jsonb", nullable: false),
                    JitProvisioningEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ScimEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    HealthStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    HealthMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    LastTestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_sso_configurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_sso_configurations_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "compliance360",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_webhook_endpoints",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Events = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SecretHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    SigningAlgorithm = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    LastDeliveryAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastDeliveryStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    LastDeliveryMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_webhook_endpoints", x => x.Id);
                    table.CheckConstraint("CK_tenant_webhook_endpoints_MaxRetries", "\"MaxRetries\" >= 0 AND \"MaxRetries\" <= 25");
                    table.ForeignKey(
                        name: "FK_tenant_webhook_endpoints_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "compliance360",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_webhook_delivery_logs",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WebhookId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Attempt = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_webhook_delivery_logs", x => x.Id);
                    table.CheckConstraint("CK_tenant_webhook_delivery_logs_Attempt", "\"Attempt\" > 0");
                    table.ForeignKey(
                        name: "FK_tenant_webhook_delivery_logs_tenant_webhook_endpoints_Webho~",
                        column: x => x.WebhookId,
                        principalSchema: "compliance360",
                        principalTable: "tenant_webhook_endpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_webhook_delivery_logs_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "compliance360",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenants_CreatedByUserId",
                schema: "compliance360",
                table: "tenants",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_api_credentials_KeyHash",
                schema: "compliance360",
                table: "tenant_api_credentials",
                column: "KeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_api_credentials_TenantId_Name",
                schema: "compliance360",
                table: "tenant_api_credentials",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_backup_records_TenantId_CompletedAtUtc",
                schema: "compliance360",
                table: "tenant_backup_records",
                columns: new[] { "TenantId", "CompletedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_domains_HostName",
                schema: "compliance360",
                table: "tenant_domains",
                column: "HostName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_domains_TenantId_IsDefault",
                schema: "compliance360",
                table: "tenant_domains",
                columns: new[] { "TenantId", "IsDefault" },
                unique: true,
                filter: "\"IsDefault\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_domains_TenantId_Kind_IsDefault",
                schema: "compliance360",
                table: "tenant_domains",
                columns: new[] { "TenantId", "Kind", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_health_signals_TenantId_Component",
                schema: "compliance360",
                table: "tenant_health_signals",
                columns: new[] { "TenantId", "Component" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_licenses_LicenseNumber",
                schema: "compliance360",
                table: "tenant_licenses",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_licenses_TenantId",
                schema: "compliance360",
                table: "tenant_licenses",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_sso_configurations_TenantId_Provider_Name",
                schema: "compliance360",
                table: "tenant_sso_configurations",
                columns: new[] { "TenantId", "Provider", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_webhook_delivery_logs_TenantId_WebhookId_OccurredAtU~",
                schema: "compliance360",
                table: "tenant_webhook_delivery_logs",
                columns: new[] { "TenantId", "WebhookId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_webhook_delivery_logs_WebhookId",
                schema: "compliance360",
                table: "tenant_webhook_delivery_logs",
                column: "WebhookId");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_webhook_endpoints_TenantId_Name",
                schema: "compliance360",
                table: "tenant_webhook_endpoints",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tenants_users_CreatedByUserId",
                schema: "compliance360",
                table: "tenants",
                column: "CreatedByUserId",
                principalSchema: "compliance360",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tenants_users_CreatedByUserId",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropTable(
                name: "tenant_api_credentials",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "tenant_backup_records",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "tenant_domains",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "tenant_health_signals",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "tenant_licenses",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "tenant_sso_configurations",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "tenant_webhook_delivery_logs",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "tenant_webhook_endpoints",
                schema: "compliance360");

            migrationBuilder.DropIndex(
                name: "IX_tenants_CreatedByUserId",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "ForcePasswordChangeRequired",
                schema: "compliance360",
                table: "users");
        }
    }
}
