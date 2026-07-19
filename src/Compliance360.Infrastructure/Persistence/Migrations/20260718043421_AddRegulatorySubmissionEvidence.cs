using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRegulatorySubmissionEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubmissionExternalNumber",
                schema: "compliance360",
                table: "registration_dossiers",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmissionProcedureNumber",
                schema: "compliance360",
                table: "registration_dossiers",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmissionProofStoredFileId",
                schema: "compliance360",
                table: "registration_dossiers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmittedByUserId",
                schema: "compliance360",
                table: "registration_dossiers",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubmissionExternalNumber",
                schema: "compliance360",
                table: "registration_dossiers");

            migrationBuilder.DropColumn(
                name: "SubmissionProcedureNumber",
                schema: "compliance360",
                table: "registration_dossiers");

            migrationBuilder.DropColumn(
                name: "SubmissionProofStoredFileId",
                schema: "compliance360",
                table: "registration_dossiers");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                schema: "compliance360",
                table: "registration_dossiers");
        }
    }
}
