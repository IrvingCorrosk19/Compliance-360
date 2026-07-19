using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertCenterRecipientResolver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_users_TenantId_Id",
                schema: "compliance360",
                table: "users",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.CreateTable(
                name: "authorized_external_recipients",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authorized_external_recipients", x => x.Id);
                    table.UniqueConstraint("AK_authorized_external_recipients_TenantId_Id", x => new { x.TenantId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "recipient_departments",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipient_departments", x => x.Id);
                    table.UniqueConstraint("AK_recipient_departments_TenantId_Id", x => new { x.TenantId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "recipient_distribution_lists",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipient_distribution_lists", x => x.Id);
                    table.UniqueConstraint("AK_recipient_distribution_lists_TenantId_Id", x => new { x.TenantId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "recipient_fallback_configurations",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Mode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: true),
                    Routing = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipient_fallback_configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recipient_groups",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipient_groups", x => x.Id);
                    table.UniqueConstraint("AK_recipient_groups_TenantId_Id", x => new { x.TenantId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "recipient_directory_profiles",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupervisorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipient_directory_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recipient_directory_profiles_recipient_departments_TenantId~",
                        columns: x => new { x.TenantId, x.DepartmentId },
                        principalSchema: "compliance360",
                        principalTable: "recipient_departments",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recipient_directory_profiles_users_TenantId_SupervisorUserId",
                        columns: x => new { x.TenantId, x.SupervisorUserId },
                        principalSchema: "compliance360",
                        principalTable: "users",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recipient_directory_profiles_users_TenantId_UserId",
                        columns: x => new { x.TenantId, x.UserId },
                        principalSchema: "compliance360",
                        principalTable: "users",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipient_distribution_list_members",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DistributionListId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExternalRecipientId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipient_distribution_list_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recipient_distribution_list_members_authorized_external_rec~",
                        columns: x => new { x.TenantId, x.ExternalRecipientId },
                        principalSchema: "compliance360",
                        principalTable: "authorized_external_recipients",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recipient_distribution_list_members_recipient_distribution_~",
                        columns: x => new { x.TenantId, x.DistributionListId },
                        principalSchema: "compliance360",
                        principalTable: "recipient_distribution_lists",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_recipient_distribution_list_members_users_TenantId_UserId",
                        columns: x => new { x.TenantId, x.UserId },
                        principalSchema: "compliance360",
                        principalTable: "users",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "recipient_group_members",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipient_group_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recipient_group_members_recipient_groups_TenantId_GroupId",
                        columns: x => new { x.TenantId, x.GroupId },
                        principalSchema: "compliance360",
                        principalTable: "recipient_groups",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_recipient_group_members_users_TenantId_UserId",
                        columns: x => new { x.TenantId, x.UserId },
                        principalSchema: "compliance360",
                        principalTable: "users",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_authorized_external_recipients_TenantId_Email",
                schema: "compliance360",
                table: "authorized_external_recipients",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recipient_departments_TenantId_Name",
                schema: "compliance360",
                table: "recipient_departments",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recipient_directory_profiles_TenantId_DepartmentId",
                schema: "compliance360",
                table: "recipient_directory_profiles",
                columns: new[] { "TenantId", "DepartmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_recipient_directory_profiles_TenantId_SupervisorUserId",
                schema: "compliance360",
                table: "recipient_directory_profiles",
                columns: new[] { "TenantId", "SupervisorUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_recipient_directory_profiles_TenantId_UserId",
                schema: "compliance360",
                table: "recipient_directory_profiles",
                columns: new[] { "TenantId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recipient_distribution_list_members_TenantId_DistributionL~1",
                schema: "compliance360",
                table: "recipient_distribution_list_members",
                columns: new[] { "TenantId", "DistributionListId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recipient_distribution_list_members_TenantId_DistributionLi~",
                schema: "compliance360",
                table: "recipient_distribution_list_members",
                columns: new[] { "TenantId", "DistributionListId", "ExternalRecipientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recipient_distribution_list_members_TenantId_ExternalRecipi~",
                schema: "compliance360",
                table: "recipient_distribution_list_members",
                columns: new[] { "TenantId", "ExternalRecipientId" });

            migrationBuilder.CreateIndex(
                name: "IX_recipient_distribution_list_members_TenantId_UserId",
                schema: "compliance360",
                table: "recipient_distribution_list_members",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_recipient_distribution_lists_TenantId_Name",
                schema: "compliance360",
                table: "recipient_distribution_lists",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recipient_fallback_configurations_TenantId",
                schema: "compliance360",
                table: "recipient_fallback_configurations",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recipient_group_members_TenantId_GroupId_UserId",
                schema: "compliance360",
                table: "recipient_group_members",
                columns: new[] { "TenantId", "GroupId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recipient_group_members_TenantId_UserId",
                schema: "compliance360",
                table: "recipient_group_members",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_recipient_groups_TenantId_Name",
                schema: "compliance360",
                table: "recipient_groups",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recipient_directory_profiles",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "recipient_distribution_list_members",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "recipient_fallback_configurations",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "recipient_group_members",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "recipient_departments",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "authorized_external_recipients",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "recipient_distribution_lists",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "recipient_groups",
                schema: "compliance360");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_users_TenantId_Id",
                schema: "compliance360",
                table: "users");
        }
    }
}
