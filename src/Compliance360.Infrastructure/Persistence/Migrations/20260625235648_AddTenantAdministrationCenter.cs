using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantAdministrationCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                schema: "compliance360",
                table: "tenants",
                type: "character varying(220)",
                maxLength: 220,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "compliance360",
                table: "tenants",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommercialName",
                schema: "compliance360",
                table: "tenants",
                type: "character varying(180)",
                maxLength: 180,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                schema: "compliance360",
                table: "tenants",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "compliance360",
                table: "tenants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "compliance360",
                table: "tenants",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "compliance360",
                table: "tenants",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "compliance360",
                table: "tenants",
                type: "character varying(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                schema: "compliance360",
                table: "tenants",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                schema: "compliance360",
                table: "tenants",
                type: "character varying(220)",
                maxLength: 220,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                schema: "compliance360",
                table: "tenants",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                schema: "compliance360",
                table: "tenants",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                schema: "compliance360",
                table: "tenants",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxIdentifier",
                schema: "compliance360",
                table: "tenants",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Website",
                schema: "compliance360",
                table: "tenants",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpWhitelist",
                schema: "compliance360",
                table: "tenant_settings",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                schema: "compliance360",
                table: "tenant_settings",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LockoutMaxFailedAttempts",
                schema: "compliance360",
                table: "tenant_settings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LockoutMinutes",
                schema: "compliance360",
                table: "tenant_settings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PasswordExpirationDays",
                schema: "compliance360",
                table: "tenant_settings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SecurityScore",
                schema: "compliance360",
                table: "tenant_settings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SessionTimeoutMinutes",
                schema: "compliance360",
                table: "tenant_settings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "TrustedDevicesEnabled",
                schema: "compliance360",
                table: "tenant_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CorporateEmail",
                schema: "compliance360",
                table: "tenant_branding",
                type: "character varying(180)",
                maxLength: 180,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FaviconUri",
                schema: "compliance360",
                table: "tenant_branding",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterText",
                schema: "compliance360",
                table: "tenant_branding",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LoginBackgroundUri",
                schema: "compliance360",
                table: "tenant_branding",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Theme",
                schema: "compliance360",
                table: "tenant_branding",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE compliance360.tenants
                SET
                    "LegalName" = CASE WHEN "LegalName" = '' THEN "Name" ELSE "LegalName" END,
                    "CommercialName" = CASE WHEN "CommercialName" = '' THEN "Name" ELSE "CommercialName" END,
                    "TaxIdentifier" = CASE WHEN "TaxIdentifier" = '' THEN 'TENANT-' || substring("Id"::text from 1 for 18) ELSE "TaxIdentifier" END,
                    "Industry" = CASE WHEN "Industry" = '' THEN 'Compliance' ELSE "Industry" END,
                    "CountryCode" = CASE WHEN "CountryCode" = '' THEN 'PA' ELSE "CountryCode" END,
                    "Currency" = CASE WHEN "Currency" = '' THEN 'USD' ELSE "Currency" END;

                UPDATE compliance360.tenant_settings
                SET
                    "Language" = CASE WHEN "Language" = '' THEN split_part("Culture", '-', 1) ELSE "Language" END,
                    "SessionTimeoutMinutes" = CASE WHEN "SessionTimeoutMinutes" = 0 THEN 30 ELSE "SessionTimeoutMinutes" END,
                    "PasswordExpirationDays" = CASE WHEN "PasswordExpirationDays" = 0 THEN 90 ELSE "PasswordExpirationDays" END,
                    "LockoutMaxFailedAttempts" = CASE WHEN "LockoutMaxFailedAttempts" = 0 THEN 5 ELSE "LockoutMaxFailedAttempts" END,
                    "LockoutMinutes" = CASE WHEN "LockoutMinutes" = 0 THEN 15 ELSE "LockoutMinutes" END,
                    "SecurityScore" = CASE WHEN "SecurityScore" = 0 THEN 75 ELSE "SecurityScore" END;

                UPDATE compliance360.tenant_branding
                SET
                    "Theme" = CASE WHEN "Theme" = '' THEN 'System' ELSE "Theme" END,
                    "FooterText" = CASE WHEN "FooterText" = '' THEN 'Compliance 360' ELSE "FooterText" END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_TaxIdentifier",
                schema: "compliance360",
                table: "tenants",
                column: "TaxIdentifier",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tenants_TaxIdentifier",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "AddressLine1",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "CommercialName",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "Industry",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "LegalName",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "Phone",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "Province",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "TaxIdentifier",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "Website",
                schema: "compliance360",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "IpWhitelist",
                schema: "compliance360",
                table: "tenant_settings");

            migrationBuilder.DropColumn(
                name: "Language",
                schema: "compliance360",
                table: "tenant_settings");

            migrationBuilder.DropColumn(
                name: "LockoutMaxFailedAttempts",
                schema: "compliance360",
                table: "tenant_settings");

            migrationBuilder.DropColumn(
                name: "LockoutMinutes",
                schema: "compliance360",
                table: "tenant_settings");

            migrationBuilder.DropColumn(
                name: "PasswordExpirationDays",
                schema: "compliance360",
                table: "tenant_settings");

            migrationBuilder.DropColumn(
                name: "SecurityScore",
                schema: "compliance360",
                table: "tenant_settings");

            migrationBuilder.DropColumn(
                name: "SessionTimeoutMinutes",
                schema: "compliance360",
                table: "tenant_settings");

            migrationBuilder.DropColumn(
                name: "TrustedDevicesEnabled",
                schema: "compliance360",
                table: "tenant_settings");

            migrationBuilder.DropColumn(
                name: "CorporateEmail",
                schema: "compliance360",
                table: "tenant_branding");

            migrationBuilder.DropColumn(
                name: "FaviconUri",
                schema: "compliance360",
                table: "tenant_branding");

            migrationBuilder.DropColumn(
                name: "FooterText",
                schema: "compliance360",
                table: "tenant_branding");

            migrationBuilder.DropColumn(
                name: "LoginBackgroundUri",
                schema: "compliance360",
                table: "tenant_branding");

            migrationBuilder.DropColumn(
                name: "Theme",
                schema: "compliance360",
                table: "tenant_branding");
        }
    }
}
