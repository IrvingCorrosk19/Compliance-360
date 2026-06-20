using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialEnterpriseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "compliance360");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    Role = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    EntityName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Category = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    RequestId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BeforeValuesJson = table.Column<string>(type: "text", nullable: true),
                    AfterValuesJson = table.Column<string>(type: "text", nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "mfa_configurations",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SecretEncrypted = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ConfiguredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastVerifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailedAttempts = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mfa_configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notification_messages",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Channel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Recipient = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Subject = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Body = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Priority = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    QueuedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SentAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notification_templates",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Channel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Subject = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Body = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Module = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Action = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Code = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "stored_files",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageProvider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ContainerName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ObjectKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256Hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OwnerEntityName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    OwnerEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stored_files", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    FullName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    MfaEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MfaSecretEncrypted = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LastLoginAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false),
                    LockoutEndAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PasswordChangedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "compliance360",
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalName = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    TaxIdentifier = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_companies_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "compliance360",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Plan = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    MaxUsers = table.Column<int>(type: "integer", nullable: false),
                    MaxStorageGb = table.Column<int>(type: "integer", nullable: false),
                    ExpiresOn = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_subscriptions_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "compliance360",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_branding",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    LogoUri = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PrimaryColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SecondaryColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_branding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_branding_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "compliance360",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_settings",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Culture = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    TimeZone = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    RequireMfa = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentRetentionDays = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_settings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_settings_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "compliance360",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "password_history",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ChangedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_password_history_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "compliance360",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "compliance360",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_roles_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "compliance360",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_sessions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_sessions_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "compliance360",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EntityName_EntityId",
                schema: "compliance360",
                table: "audit_logs",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_TenantId_Action_OccurredAtUtc",
                schema: "compliance360",
                table: "audit_logs",
                columns: new[] { "TenantId", "Action", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_TenantId_Category_OccurredAtUtc",
                schema: "compliance360",
                table: "audit_logs",
                columns: new[] { "TenantId", "Category", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_TenantId_OccurredAtUtc",
                schema: "compliance360",
                table: "audit_logs",
                columns: new[] { "TenantId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_TenantId_UserId_OccurredAtUtc",
                schema: "compliance360",
                table: "audit_logs",
                columns: new[] { "TenantId", "UserId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_companies_TenantId_TaxIdentifier",
                schema: "compliance360",
                table: "companies",
                columns: new[] { "TenantId", "TaxIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mfa_configurations_TenantId_IsEnabled",
                schema: "compliance360",
                table: "mfa_configurations",
                columns: new[] { "TenantId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_mfa_configurations_TenantId_UserId_Method",
                schema: "compliance360",
                table: "mfa_configurations",
                columns: new[] { "TenantId", "UserId", "Method" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_messages_TenantId_Status_Priority_QueuedAtUtc",
                schema: "compliance360",
                table: "notification_messages",
                columns: new[] { "TenantId", "Status", "Priority", "QueuedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_messages_TenantId_TargetUserId_QueuedAtUtc",
                schema: "compliance360",
                table: "notification_messages",
                columns: new[] { "TenantId", "TargetUserId", "QueuedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_templates_TenantId_Code_Channel",
                schema: "compliance360",
                table: "notification_templates",
                columns: new[] { "TenantId", "Code", "Channel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_history_TenantId_UserId_ChangedAtUtc",
                schema: "compliance360",
                table: "password_history",
                columns: new[] { "TenantId", "UserId", "ChangedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_password_history_UserId",
                schema: "compliance360",
                table: "password_history",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_Code",
                schema: "compliance360",
                table: "permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_TenantId_UserId",
                schema: "compliance360",
                table: "refresh_tokens",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_TokenHash",
                schema: "compliance360",
                table: "refresh_tokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId",
                schema: "compliance360",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_RoleId",
                schema: "compliance360",
                table: "role_permissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_TenantId_RoleId_PermissionId",
                schema: "compliance360",
                table: "role_permissions",
                columns: new[] { "TenantId", "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_TenantId_NormalizedName",
                schema: "compliance360",
                table: "roles",
                columns: new[] { "TenantId", "NormalizedName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_ContainerName_ObjectKey",
                schema: "compliance360",
                table: "stored_files",
                columns: new[] { "ContainerName", "ObjectKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_TenantId_OwnerEntityName_OwnerEntityId",
                schema: "compliance360",
                table: "stored_files",
                columns: new[] { "TenantId", "OwnerEntityName", "OwnerEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_TenantId",
                schema: "compliance360",
                table: "subscriptions",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_branding_TenantId",
                schema: "compliance360",
                table: "tenant_branding",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_settings_TenantId",
                schema: "compliance360",
                table: "tenant_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_Slug",
                schema: "compliance360",
                table: "tenants",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_TenantId_UserId_RoleId",
                schema: "compliance360",
                table: "user_roles",
                columns: new[] { "TenantId", "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_UserId",
                schema: "compliance360",
                table: "user_roles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_sessions_TenantId_UserId_ExpiresAtUtc",
                schema: "compliance360",
                table: "user_sessions",
                columns: new[] { "TenantId", "UserId", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_user_sessions_UserId",
                schema: "compliance360",
                table: "user_sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_TenantId_NormalizedEmail",
                schema: "compliance360",
                table: "users",
                columns: new[] { "TenantId", "NormalizedEmail" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "companies",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "mfa_configurations",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "notification_messages",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "notification_templates",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "password_history",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "stored_files",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "subscriptions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "tenant_branding",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "tenant_settings",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "user_sessions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "tenants",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "users",
                schema: "compliance360");
        }
    }
}
