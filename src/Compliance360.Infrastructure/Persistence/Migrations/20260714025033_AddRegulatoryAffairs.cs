using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRegulatoryAffairs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "license_renewal_cases",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OperatingLicenseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssembledOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SubmittedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ManualPlatformTaskNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_license_renewal_cases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "manufacturer_certificate_dossier_links",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificateId = table.Column<Guid>(type: "uuid", nullable: false),
                    DossierId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manufacturer_certificate_dossier_links", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "manufacturer_certificates",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ManufacturerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Number = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IssuedBy = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    IssuedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    LegalFormat = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Apostilled = table.Column<bool>(type: "boolean", nullable: false),
                    Notarized = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    StoredFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manufacturer_certificates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "manufacturer_profiles",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalName = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    CommercialName = table.Column<string>(type: "text", nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContactEmail = table.Column<string>(type: "text", nullable: true),
                    ContactPhone = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manufacturer_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "medical_device_products",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Category = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Brand = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    RegulatoryName = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    CommercialName = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CatalogCode = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    InternalCode = table.Column<string>(type: "text", nullable: true),
                    ProductType = table.Column<string>(type: "text", nullable: true),
                    RiskClass = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ManufacturerId = table.Column<Guid>(type: "uuid", nullable: true),
                    DistributorCompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Initiative = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<string>(type: "text", nullable: true),
                    SalesMarketingInput = table.Column<string>(type: "text", nullable: true),
                    OpportunityAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsCommercializable = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medical_device_products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "operating_licenses",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    LicenseType = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    AuthorityId = table.Column<Guid>(type: "uuid", nullable: true),
                    LicenseNumber = table.Column<string>(type: "text", nullable: true),
                    IssuedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActiveRenewalCaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operating_licenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "registration_dossiers",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExistingRegistrationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResultingRegistrationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcessType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Priority = table.Column<string>(type: "text", nullable: true),
                    RegulatoryOwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesMarketingInput = table.Column<string>(type: "text", nullable: true),
                    OpportunityAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    RequirementPackId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequirementPackVersionLabel = table.Column<string>(type: "text", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    ClosedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RequestedFromFactoryOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EstimatedReceptionOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MaximumReceptionOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReceivedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AssembledOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EstimatedSubmissionOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SubmittedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ObservationReceivedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EstimatedApprovalOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TargetExpirationOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registration_dossiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "regulatory_alert_logs",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    EntityName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    DaysRemaining = table.Column<int>(type: "integer", nullable: false),
                    RecipientUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Channel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DeliveredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regulatory_alert_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "regulatory_alert_settings",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThresholdsCsv = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regulatory_alert_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "regulatory_authorities",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    AuthorityType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regulatory_authorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "regulatory_requirement_packs",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    AuthorityId = table.Column<Guid>(type: "uuid", nullable: true),
                    RiskClass = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ProcessType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ProductCategory = table.Column<string>(type: "text", nullable: true),
                    VersionLabel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OptionalFormTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regulatory_requirement_packs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "regutrack_import_jobs",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    StagingPayloadJson = table.Column<string>(type: "jsonb", nullable: true),
                    ValidationReportJson = table.Column<string>(type: "jsonb", nullable: true),
                    WarningCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorCount = table.Column<int>(type: "integer", nullable: false),
                    ImportedRowCount = table.Column<int>(type: "integer", nullable: false),
                    CommittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regutrack_import_jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "regutrack_import_rows",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    SheetName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    RowNumber = table.Column<int>(type: "integer", nullable: false),
                    RawJson = table.Column<string>(type: "jsonb", nullable: false),
                    NormalizedJson = table.Column<string>(type: "jsonb", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDossierId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regutrack_import_rows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sanitary_registrations",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorityId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IssuedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    RenewalDueOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReplacesRegistrationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sanitary_registrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "license_milestones",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LicenseRenewalCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    PlannedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActualDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_license_milestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_license_milestones_license_renewal_cases_LicenseRenewalCase~",
                        column: x => x.LicenseRenewalCaseId,
                        principalSchema: "compliance360",
                        principalTable: "license_renewal_cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "license_requirements",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LicenseRenewalCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    StoredFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_license_requirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_license_requirements_license_renewal_cases_LicenseRenewalCa~",
                        column: x => x.LicenseRenewalCaseId,
                        principalSchema: "compliance360",
                        principalTable: "license_renewal_cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "authority_observations",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DossierId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObservationNumber = table.Column<int>(type: "integer", nullable: false),
                    ReceivedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DueOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResponseSubmittedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authority_observations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_authority_observations_registration_dossiers_DossierId",
                        column: x => x.DossierId,
                        principalSchema: "compliance360",
                        principalTable: "registration_dossiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dossier_milestones",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DossierId = table.Column<Guid>(type: "uuid", nullable: false),
                    MilestoneType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PlannedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActualDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dossier_milestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dossier_milestones_registration_dossiers_DossierId",
                        column: x => x.DossierId,
                        principalSchema: "compliance360",
                        principalTable: "registration_dossiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dossier_requirements",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DossierId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsCritical = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ValidationStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ValidationNotes = table.Column<string>(type: "text", nullable: true),
                    CurrentDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    StoredFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    SourceRequirementPackId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceRequirementPackVersionLabel = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dossier_requirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dossier_requirements_registration_dossiers_DossierId",
                        column: x => x.DossierId,
                        principalSchema: "compliance360",
                        principalTable: "registration_dossiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "requirement_definitions",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Category = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsCritical = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_requirement_definitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_requirement_definitions_regulatory_requirement_packs_PackId",
                        column: x => x.PackId,
                        principalSchema: "compliance360",
                        principalTable: "regulatory_requirement_packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "authority_observation_requirements",
                schema: "compliance360",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ObservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authority_observation_requirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_authority_observation_requirements_authority_observations_O~",
                        column: x => x.ObservationId,
                        principalSchema: "compliance360",
                        principalTable: "authority_observations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_authority_observation_requirements_ObservationId_Requiremen~",
                schema: "compliance360",
                table: "authority_observation_requirements",
                columns: new[] { "ObservationId", "RequirementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_authority_observations_DossierId",
                schema: "compliance360",
                table: "authority_observations",
                column: "DossierId");

            migrationBuilder.CreateIndex(
                name: "IX_dossier_milestones_DossierId",
                schema: "compliance360",
                table: "dossier_milestones",
                column: "DossierId");

            migrationBuilder.CreateIndex(
                name: "IX_dossier_milestones_TenantId_DossierId_MilestoneType",
                schema: "compliance360",
                table: "dossier_milestones",
                columns: new[] { "TenantId", "DossierId", "MilestoneType" });

            migrationBuilder.CreateIndex(
                name: "IX_dossier_requirements_DossierId",
                schema: "compliance360",
                table: "dossier_requirements",
                column: "DossierId");

            migrationBuilder.CreateIndex(
                name: "IX_dossier_requirements_TenantId_DossierId_Code",
                schema: "compliance360",
                table: "dossier_requirements",
                columns: new[] { "TenantId", "DossierId", "Code" });

            migrationBuilder.CreateIndex(
                name: "IX_license_milestones_LicenseRenewalCaseId",
                schema: "compliance360",
                table: "license_milestones",
                column: "LicenseRenewalCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_license_requirements_LicenseRenewalCaseId",
                schema: "compliance360",
                table: "license_requirements",
                column: "LicenseRenewalCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_manufacturer_certificate_dossier_links_CertificateId_Dossie~",
                schema: "compliance360",
                table: "manufacturer_certificate_dossier_links",
                columns: new[] { "CertificateId", "DossierId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_manufacturer_certificates_TenantId_ManufacturerId_Type",
                schema: "compliance360",
                table: "manufacturer_certificates",
                columns: new[] { "TenantId", "ManufacturerId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_manufacturer_profiles_TenantId_LegalName",
                schema: "compliance360",
                table: "manufacturer_profiles",
                columns: new[] { "TenantId", "LegalName" });

            migrationBuilder.CreateIndex(
                name: "IX_medical_device_products_TenantId_Brand_IsDeleted",
                schema: "compliance360",
                table: "medical_device_products",
                columns: new[] { "TenantId", "Brand", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_medical_device_products_TenantId_CatalogCode",
                schema: "compliance360",
                table: "medical_device_products",
                columns: new[] { "TenantId", "CatalogCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_operating_licenses_TenantId_CompanyName_LicenseType",
                schema: "compliance360",
                table: "operating_licenses",
                columns: new[] { "TenantId", "CompanyName", "LicenseType" });

            migrationBuilder.CreateIndex(
                name: "IX_registration_dossiers_TenantId_CaseNumber",
                schema: "compliance360",
                table: "registration_dossiers",
                columns: new[] { "TenantId", "CaseNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_registration_dossiers_TenantId_Status_IsDeleted",
                schema: "compliance360",
                table: "registration_dossiers",
                columns: new[] { "TenantId", "Status", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_regulatory_alert_logs_TenantId_AlertType_EntityId_DaysRemai~",
                schema: "compliance360",
                table: "regulatory_alert_logs",
                columns: new[] { "TenantId", "AlertType", "EntityId", "DaysRemaining", "DeliveredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_regulatory_alert_settings_TenantId",
                schema: "compliance360",
                table: "regulatory_alert_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_regulatory_authorities_TenantId_Code",
                schema: "compliance360",
                table: "regulatory_authorities",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_regulatory_requirement_packs_TenantId_Code",
                schema: "compliance360",
                table: "regulatory_requirement_packs",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_regutrack_import_rows_TenantId_JobId_RowNumber",
                schema: "compliance360",
                table: "regutrack_import_rows",
                columns: new[] { "TenantId", "JobId", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_requirement_definitions_PackId",
                schema: "compliance360",
                table: "requirement_definitions",
                column: "PackId");

            migrationBuilder.CreateIndex(
                name: "IX_sanitary_registrations_TenantId_ProductId_AuthorityId_IsCur~",
                schema: "compliance360",
                table: "sanitary_registrations",
                columns: new[] { "TenantId", "ProductId", "AuthorityId", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_sanitary_registrations_TenantId_RegistrationNumber",
                schema: "compliance360",
                table: "sanitary_registrations",
                columns: new[] { "TenantId", "RegistrationNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "authority_observation_requirements",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "dossier_milestones",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "dossier_requirements",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "license_milestones",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "license_requirements",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "manufacturer_certificate_dossier_links",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "manufacturer_certificates",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "manufacturer_profiles",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "medical_device_products",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "operating_licenses",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "regulatory_alert_logs",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "regulatory_alert_settings",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "regulatory_authorities",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "regutrack_import_jobs",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "regutrack_import_rows",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "requirement_definitions",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "sanitary_registrations",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "authority_observations",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "license_renewal_cases",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "regulatory_requirement_packs",
                schema: "compliance360");

            migrationBuilder.DropTable(
                name: "registration_dossiers",
                schema: "compliance360");
        }
    }
}
