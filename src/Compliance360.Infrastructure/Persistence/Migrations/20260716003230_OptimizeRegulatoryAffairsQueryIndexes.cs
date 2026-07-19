using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeRegulatoryAffairsQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_sanitary_registrations_TenantId_ExpiresOn",
                schema: "compliance360",
                table: "sanitary_registrations",
                columns: new[] { "TenantId", "ExpiresOn" });

            migrationBuilder.CreateIndex(
                name: "IX_sanitary_registrations_TenantId_Status_IsCurrent",
                schema: "compliance360",
                table: "sanitary_registrations",
                columns: new[] { "TenantId", "Status", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_regutrack_import_jobs_TenantId_CreatedAtUtc",
                schema: "compliance360",
                table: "regutrack_import_jobs",
                columns: new[] { "TenantId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_registration_dossiers_TenantId_AuthorityId_IsDeleted",
                schema: "compliance360",
                table: "registration_dossiers",
                columns: new[] { "TenantId", "AuthorityId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_registration_dossiers_TenantId_IsDeleted_CreatedAtUtc",
                schema: "compliance360",
                table: "registration_dossiers",
                columns: new[] { "TenantId", "IsDeleted", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_registration_dossiers_TenantId_ProductId_IsDeleted",
                schema: "compliance360",
                table: "registration_dossiers",
                columns: new[] { "TenantId", "ProductId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_operating_licenses_TenantId_ExpiresOn",
                schema: "compliance360",
                table: "operating_licenses",
                columns: new[] { "TenantId", "ExpiresOn" });

            migrationBuilder.CreateIndex(
                name: "IX_operating_licenses_TenantId_Status",
                schema: "compliance360",
                table: "operating_licenses",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_medical_device_products_TenantId_IsDeleted",
                schema: "compliance360",
                table: "medical_device_products",
                columns: new[] { "TenantId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_medical_device_products_TenantId_ManufacturerId_IsDeleted",
                schema: "compliance360",
                table: "medical_device_products",
                columns: new[] { "TenantId", "ManufacturerId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_medical_device_products_TenantId_RiskClass_IsDeleted",
                schema: "compliance360",
                table: "medical_device_products",
                columns: new[] { "TenantId", "RiskClass", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_manufacturer_profiles_TenantId_IsActive",
                schema: "compliance360",
                table: "manufacturer_profiles",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_manufacturer_certificates_TenantId_ExpiresOn",
                schema: "compliance360",
                table: "manufacturer_certificates",
                columns: new[] { "TenantId", "ExpiresOn" });

            migrationBuilder.CreateIndex(
                name: "IX_manufacturer_certificates_TenantId_Status",
                schema: "compliance360",
                table: "manufacturer_certificates",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_dossier_requirements_TenantId_IsCritical_Status",
                schema: "compliance360",
                table: "dossier_requirements",
                columns: new[] { "TenantId", "IsCritical", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_authority_observations_TenantId_DossierId",
                schema: "compliance360",
                table: "authority_observations",
                columns: new[] { "TenantId", "DossierId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_sanitary_registrations_TenantId_ExpiresOn",
                schema: "compliance360",
                table: "sanitary_registrations");

            migrationBuilder.DropIndex(
                name: "IX_sanitary_registrations_TenantId_Status_IsCurrent",
                schema: "compliance360",
                table: "sanitary_registrations");

            migrationBuilder.DropIndex(
                name: "IX_regutrack_import_jobs_TenantId_CreatedAtUtc",
                schema: "compliance360",
                table: "regutrack_import_jobs");

            migrationBuilder.DropIndex(
                name: "IX_registration_dossiers_TenantId_AuthorityId_IsDeleted",
                schema: "compliance360",
                table: "registration_dossiers");

            migrationBuilder.DropIndex(
                name: "IX_registration_dossiers_TenantId_IsDeleted_CreatedAtUtc",
                schema: "compliance360",
                table: "registration_dossiers");

            migrationBuilder.DropIndex(
                name: "IX_registration_dossiers_TenantId_ProductId_IsDeleted",
                schema: "compliance360",
                table: "registration_dossiers");

            migrationBuilder.DropIndex(
                name: "IX_operating_licenses_TenantId_ExpiresOn",
                schema: "compliance360",
                table: "operating_licenses");

            migrationBuilder.DropIndex(
                name: "IX_operating_licenses_TenantId_Status",
                schema: "compliance360",
                table: "operating_licenses");

            migrationBuilder.DropIndex(
                name: "IX_medical_device_products_TenantId_IsDeleted",
                schema: "compliance360",
                table: "medical_device_products");

            migrationBuilder.DropIndex(
                name: "IX_medical_device_products_TenantId_ManufacturerId_IsDeleted",
                schema: "compliance360",
                table: "medical_device_products");

            migrationBuilder.DropIndex(
                name: "IX_medical_device_products_TenantId_RiskClass_IsDeleted",
                schema: "compliance360",
                table: "medical_device_products");

            migrationBuilder.DropIndex(
                name: "IX_manufacturer_profiles_TenantId_IsActive",
                schema: "compliance360",
                table: "manufacturer_profiles");

            migrationBuilder.DropIndex(
                name: "IX_manufacturer_certificates_TenantId_ExpiresOn",
                schema: "compliance360",
                table: "manufacturer_certificates");

            migrationBuilder.DropIndex(
                name: "IX_manufacturer_certificates_TenantId_Status",
                schema: "compliance360",
                table: "manufacturer_certificates");

            migrationBuilder.DropIndex(
                name: "IX_dossier_requirements_TenantId_IsCritical_Status",
                schema: "compliance360",
                table: "dossier_requirements");

            migrationBuilder.DropIndex(
                name: "IX_authority_observations_TenantId_DossierId",
                schema: "compliance360",
                table: "authority_observations");
        }
    }
}
