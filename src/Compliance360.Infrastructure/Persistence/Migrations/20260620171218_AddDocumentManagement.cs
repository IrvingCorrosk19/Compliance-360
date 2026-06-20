using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "document_categories",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "document_types",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    RetentionDays = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewCycleMonths = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "document_approvals",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_approvals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_approvals_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "compliance360",
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_history",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_history_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "compliance360",
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_permissions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrincipalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    GrantedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrantedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_permissions_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "compliance360",
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_versions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    StoredFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_versions_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "compliance360",
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_approvals_DocumentId",
                schema: "compliance360",
                table: "document_approvals",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_document_approvals_TenantId_DocumentId_DecidedAtUtc",
                schema: "compliance360",
                table: "document_approvals",
                columns: new[] { "TenantId", "DocumentId", "DecidedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_document_categories_TenantId_Code",
                schema: "compliance360",
                table: "document_categories",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_history_DocumentId",
                schema: "compliance360",
                table: "document_history",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_document_history_TenantId_DocumentId_OccurredAtUtc",
                schema: "compliance360",
                table: "document_history",
                columns: new[] { "TenantId", "DocumentId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_document_permissions_DocumentId",
                schema: "compliance360",
                table: "document_permissions",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_document_permissions_TenantId_DocumentId_PrincipalId_Level",
                schema: "compliance360",
                table: "document_permissions",
                columns: new[] { "TenantId", "DocumentId", "PrincipalId", "Level" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_types_TenantId_Code",
                schema: "compliance360",
                table: "document_types",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_DocumentId",
                schema: "compliance360",
                table: "document_versions",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_TenantId_DocumentId_VersionNumber",
                schema: "compliance360",
                table: "document_versions",
                columns: new[] { "TenantId", "DocumentId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_documents_TenantId_Code",
                schema: "compliance360",
                table: "documents",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_documents_TenantId_Status_ExpiresAtUtc",
                schema: "compliance360",
                table: "documents",
                columns: new[] { "TenantId", "Status", "ExpiresAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_approvals",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "document_categories",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "document_history",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "document_permissions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "document_types",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "document_versions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "documents",
                schema: "compliance360");
        }
    }
}
