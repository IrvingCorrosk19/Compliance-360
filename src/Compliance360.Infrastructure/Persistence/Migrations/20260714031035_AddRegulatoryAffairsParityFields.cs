using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRegulatoryAffairsParityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DistributorName",
                schema: "compliance360",
                table: "medical_device_products",
                type: "character varying(220)",
                maxLength: 220,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormReference",
                schema: "compliance360",
                table: "medical_device_products",
                type: "character varying(220)",
                maxLength: 220,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegisteredSuppliersCount",
                schema: "compliance360",
                table: "medical_device_products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceLineNumber",
                schema: "compliance360",
                table: "medical_device_products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicalSheetReference",
                schema: "compliance360",
                table: "medical_device_products",
                type: "character varying(220)",
                maxLength: 220,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RequestedOn",
                schema: "compliance360",
                table: "manufacturer_certificates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "dossier_history_events",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DossierId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dossier_history_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dossier_history_events_registration_dossiers_DossierId",
                        column: x => x.DossierId,
                        principalSchema: "compliance360",
                        principalTable: "registration_dossiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dossier_history_events_DossierId",
                schema: "compliance360",
                table: "dossier_history_events",
                column: "DossierId");

            migrationBuilder.CreateIndex(
                name: "IX_dossier_history_events_TenantId_DossierId_OccurredAtUtc",
                schema: "compliance360",
                table: "dossier_history_events",
                columns: new[] { "TenantId", "DossierId", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dossier_history_events",
                schema: "compliance360");

            migrationBuilder.DropColumn(
                name: "DistributorName",
                schema: "compliance360",
                table: "medical_device_products");

            migrationBuilder.DropColumn(
                name: "FormReference",
                schema: "compliance360",
                table: "medical_device_products");

            migrationBuilder.DropColumn(
                name: "RegisteredSuppliersCount",
                schema: "compliance360",
                table: "medical_device_products");

            migrationBuilder.DropColumn(
                name: "SourceLineNumber",
                schema: "compliance360",
                table: "medical_device_products");

            migrationBuilder.DropColumn(
                name: "TechnicalSheetReference",
                schema: "compliance360",
                table: "medical_device_products");

            migrationBuilder.DropColumn(
                name: "RequestedOn",
                schema: "compliance360",
                table: "manufacturer_certificates");
        }
    }
}
