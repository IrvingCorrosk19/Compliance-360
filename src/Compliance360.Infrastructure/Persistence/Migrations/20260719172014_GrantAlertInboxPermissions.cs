using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GrantAlertInboxPermissions : Migration
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
                WHERE role."Name" IN (
                    'Quality Manager',
                    'Regulatory Administrator',
                    'Regulatory Manager',
                    'Regulatory Specialist',
                    'Regulatory Reviewer',
                    'Regulatory Approver',
                    'Regulatory Submitter',
                    'Regulatory Viewer')
                  AND permission."Code" = 'NOTIFICATION.READ'
                  AND NOT EXISTS (
                      SELECT 1
                      FROM compliance360.role_permissions AS grant_row
                      WHERE grant_row."TenantId" = role."TenantId"
                        AND grant_row."RoleId" = role."Id"
                        AND grant_row."PermissionId" = permission."Id");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Additive RBAC migration. Existing grants are preserved on rollback
            // because they may have predated this release or been assigned manually.
        }
    }
}
