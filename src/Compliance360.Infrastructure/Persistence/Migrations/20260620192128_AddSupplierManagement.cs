using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "suppliers",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalName = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    TaxIdentifier = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomologatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplier_documents",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    StoredFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssuedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplier_documents_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "compliance360",
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplier_evaluations",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    EvaluatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_evaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplier_evaluations_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "compliance360",
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplier_expiration_alerts",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AlertAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_expiration_alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplier_expiration_alerts_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "compliance360",
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_documents_SupplierId",
                schema: "compliance360",
                table: "supplier_documents",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_documents_TenantId_ExpiresAtUtc",
                schema: "compliance360",
                table: "supplier_documents",
                columns: new[] { "TenantId", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_documents_TenantId_SupplierId_Type",
                schema: "compliance360",
                table: "supplier_documents",
                columns: new[] { "TenantId", "SupplierId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_evaluations_SupplierId",
                schema: "compliance360",
                table: "supplier_evaluations",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_evaluations_TenantId_SupplierId_EvaluatedAtUtc",
                schema: "compliance360",
                table: "supplier_evaluations",
                columns: new[] { "TenantId", "SupplierId", "EvaluatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_expiration_alerts_SupplierId",
                schema: "compliance360",
                table: "supplier_expiration_alerts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_expiration_alerts_TenantId_Status_ExpiresAtUtc",
                schema: "compliance360",
                table: "supplier_expiration_alerts",
                columns: new[] { "TenantId", "Status", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_TenantId_Status",
                schema: "compliance360",
                table: "suppliers",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_TenantId_TaxIdentifier",
                schema: "compliance360",
                table: "suppliers",
                columns: new[] { "TenantId", "TaxIdentifier" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplier_documents",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "supplier_evaluations",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "supplier_expiration_alerts",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "suppliers",
                schema: "compliance360");
        }
    }
}
