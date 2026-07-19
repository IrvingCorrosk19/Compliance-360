using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReconcileRegulatoryRolePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO compliance360.role_permissions
                    ("Id", "RoleId", "PermissionId", "CreatedAtUtc", "UpdatedAtUtc", "TenantId")
                SELECT
                    md5(role."Id"::text || permission."Id"::text)::uuid,
                    role."Id",
                    permission."Id",
                    CURRENT_TIMESTAMP,
                    NULL,
                    role."TenantId"
                FROM compliance360.roles AS role
                CROSS JOIN compliance360.permissions AS permission
                WHERE role."Name" = 'Regulatory Administrator'
                  AND permission."Code" IN (
                      'REGULATORY.PRODUCT.MANAGE',
                      'REGULATORY.DOSSIER.CREATE')
                  AND NOT EXISTS (
                      SELECT 1
                      FROM compliance360.role_permissions AS grant_row
                      WHERE grant_row."TenantId" = role."TenantId"
                        AND grant_row."RoleId" = role."Id"
                        AND grant_row."PermissionId" = permission."Id");

                DELETE FROM compliance360.role_permissions AS grant_row
                USING compliance360.roles AS role, compliance360.permissions AS permission
                WHERE grant_row."RoleId" = role."Id"
                  AND grant_row."TenantId" = role."TenantId"
                  AND grant_row."PermissionId" = permission."Id"
                  AND permission."Code" = 'AUDIT.READ'
                  AND role."Name" IN (
                      'Regulatory Administrator',
                      'Regulatory Manager',
                      'Regulatory Specialist',
                      'Regulatory Reviewer',
                      'Regulatory Approver',
                      'Regulatory Submitter',
                      'Regulatory Viewer');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Security correction: rollback must not silently restore AUDIT.READ
            // or remove grants that may have existed before this migration.
        }
    }
}
