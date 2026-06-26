using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderAdministration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "storage_provider_configurations",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ContainerName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SettingsJson = table.Column<string>(type: "jsonb", nullable: false),
                    LastHealthCheckAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastHealthStatus = table.Column<bool>(type: "boolean", nullable: false),
                    LastHealthMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_storage_provider_configurations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_storage_provider_configurations_TenantId_IsDefault_Priority",
                schema: "compliance360",
                table: "storage_provider_configurations",
                columns: new[] { "TenantId", "IsDefault", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_storage_provider_configurations_TenantId_Provider_Name",
                schema: "compliance360",
                table: "storage_provider_configurations",
                columns: new[] { "TenantId", "Provider", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "storage_provider_configurations",
                schema: "compliance360");
        }
    }
}
