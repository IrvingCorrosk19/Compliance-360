using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicalSheets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "products",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Sku = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "technical_sheets",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CurrentVersionNumber = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PdfObjectKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technical_sheets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "technical_sheet_approvals",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TechnicalSheetId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_technical_sheet_approvals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_technical_sheet_approvals_technical_sheets_TechnicalSheetId",
                        column: x => x.TechnicalSheetId,
                        principalSchema: "compliance360",
                        principalTable: "technical_sheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "technical_sheet_certifications",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TechnicalSheetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Issuer = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technical_sheet_certifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_technical_sheet_certifications_technical_sheets_TechnicalSh~",
                        column: x => x.TechnicalSheetId,
                        principalSchema: "compliance360",
                        principalTable: "technical_sheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "technical_sheet_ingredients",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TechnicalSheetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Percentage = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: false),
                    Allergen = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technical_sheet_ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_technical_sheet_ingredients_technical_sheets_TechnicalSheet~",
                        column: x => x.TechnicalSheetId,
                        principalSchema: "compliance360",
                        principalTable: "technical_sheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "technical_sheet_nutrients",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TechnicalSheetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technical_sheet_nutrients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_technical_sheet_nutrients_technical_sheets_TechnicalSheetId",
                        column: x => x.TechnicalSheetId,
                        principalSchema: "compliance360",
                        principalTable: "technical_sheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "technical_sheet_versions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TechnicalSheetId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    ChangeSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technical_sheet_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_technical_sheet_versions_technical_sheets_TechnicalSheetId",
                        column: x => x.TechnicalSheetId,
                        principalSchema: "compliance360",
                        principalTable: "technical_sheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_products_TenantId_Sku",
                schema: "compliance360",
                table: "products",
                columns: new[] { "TenantId", "Sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_technical_sheet_approvals_TechnicalSheetId",
                schema: "compliance360",
                table: "technical_sheet_approvals",
                column: "TechnicalSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_technical_sheet_approvals_TenantId_TechnicalSheetId_Decided~",
                schema: "compliance360",
                table: "technical_sheet_approvals",
                columns: new[] { "TenantId", "TechnicalSheetId", "DecidedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_technical_sheet_certifications_TechnicalSheetId",
                schema: "compliance360",
                table: "technical_sheet_certifications",
                column: "TechnicalSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_technical_sheet_certifications_TenantId_ExpiresAtUtc",
                schema: "compliance360",
                table: "technical_sheet_certifications",
                columns: new[] { "TenantId", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_technical_sheet_ingredients_TechnicalSheetId",
                schema: "compliance360",
                table: "technical_sheet_ingredients",
                column: "TechnicalSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_technical_sheet_nutrients_TechnicalSheetId",
                schema: "compliance360",
                table: "technical_sheet_nutrients",
                column: "TechnicalSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_technical_sheet_versions_TechnicalSheetId",
                schema: "compliance360",
                table: "technical_sheet_versions",
                column: "TechnicalSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_technical_sheet_versions_TenantId_TechnicalSheetId_VersionN~",
                schema: "compliance360",
                table: "technical_sheet_versions",
                columns: new[] { "TenantId", "TechnicalSheetId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_technical_sheets_TenantId_ProductId_Status",
                schema: "compliance360",
                table: "technical_sheets",
                columns: new[] { "TenantId", "ProductId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "products",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "technical_sheet_approvals",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "technical_sheet_certifications",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "technical_sheet_ingredients",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "technical_sheet_nutrients",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "technical_sheet_versions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "technical_sheets",
                schema: "compliance360");
        }
    }
}
