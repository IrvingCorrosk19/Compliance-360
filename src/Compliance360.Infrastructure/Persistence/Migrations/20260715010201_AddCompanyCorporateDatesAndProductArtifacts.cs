using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyCorporateDatesAndProductArtifacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "CompanyConstitutedOn",
                schema: "compliance360",
                table: "operating_licenses",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "OperationsStartedOn",
                schema: "compliance360",
                table: "operating_licenses",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FormDocumentId",
                schema: "compliance360",
                table: "medical_device_products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormStatus",
                schema: "compliance360",
                table: "medical_device_products",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "FormStoredFileId",
                schema: "compliance360",
                table: "medical_device_products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FormUpdatedAtUtc",
                schema: "compliance360",
                table: "medical_device_products",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FormUpdatedByUserId",
                schema: "compliance360",
                table: "medical_device_products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TechnicalSheetDocumentId",
                schema: "compliance360",
                table: "medical_device_products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicalSheetStatus",
                schema: "compliance360",
                table: "medical_device_products",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TechnicalSheetStoredFileId",
                schema: "compliance360",
                table: "medical_device_products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TechnicalSheetUpdatedAtUtc",
                schema: "compliance360",
                table: "medical_device_products",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TechnicalSheetUpdatedByUserId",
                schema: "compliance360",
                table: "medical_device_products",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyConstitutedOn",
                schema: "compliance360",
                table: "operating_licenses");

            migrationBuilder.DropColumn(
                name: "OperationsStartedOn",
                schema: "compliance360",
                table: "operating_licenses");

            migrationBuilder.DropColumn(
                name: "FormDocumentId",
                schema: "compliance360",
                table: "medical_device_products");

            migrationBuilder.DropColumn(
                name: "FormStatus",
                schema: "compliance360",
                table: "medical_device_products");

            migrationBuilder.DropColumn(
                name: "FormStoredFileId",
                schema: "compliance360",
                table: "medical_device_products");

            migrationBuilder.DropColumn(
                name: "FormUpdatedAtUtc",
                schema: "compliance360",
                table: "medical_device_products");

            migrationBuilder.DropColumn(
                name: "FormUpdatedByUserId",
                schema: "compliance360",
                table: "medical_device_products");

            migrationBuilder.DropColumn(
                name: "TechnicalSheetDocumentId",
                schema: "compliance360",
                table: "medical_device_products");

            migrationBuilder.DropColumn(
                name: "TechnicalSheetStatus",
                schema: "compliance360",
                table: "medical_device_products");

            migrationBuilder.DropColumn(
                name: "TechnicalSheetStoredFileId",
                schema: "compliance360",
                table: "medical_device_products");

            migrationBuilder.DropColumn(
                name: "TechnicalSheetUpdatedAtUtc",
                schema: "compliance360",
                table: "medical_device_products");

            migrationBuilder.DropColumn(
                name: "TechnicalSheetUpdatedByUserId",
                schema: "compliance360",
                table: "medical_device_products");
        }
    }
}
