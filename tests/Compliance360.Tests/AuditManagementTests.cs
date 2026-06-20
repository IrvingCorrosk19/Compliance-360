using Compliance360.Application;
using Compliance360.Application.AuditManagement;
using Compliance360.Domain.Audit;
using Compliance360.Domain.AuditManagement;
using Compliance360.Domain.Common;
using Compliance360.Infrastructure.AuditManagement;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Tests;

public sealed class AuditManagementTests
{
    [Fact]
    public async Task AuditManagement_Full_Flow_Closes_And_Reopens_Enterprise_Audit()
    {
        var fixture = AuditFixture.Create();
        var program = await fixture.Service.CreateProgramAsync(new CreateAuditProgramCommand(fixture.TenantId, "Annual ISO Program", "ISO-2026", 2026, fixture.UserId));
        var checklist = await fixture.Service.CreateChecklistAsync(new CreateAuditChecklistCommand(fixture.TenantId, "ISO 9001 Checklist", "ISO-9001", AuditChecklistType.Iso9001, 1, fixture.UserId));
        var item = await fixture.Service.AddChecklistItemAsync(new AddAuditChecklistItemCommand(fixture.TenantId, checklist.Value!.Id, "8.5", "Is production controlled?", 10, fixture.UserId));
        var plan = await fixture.Service.CreatePlanAsync(new CreateAuditPlanCommand(fixture.TenantId, program.Value!.Id, "Production processes", "ISO 9001:2015", fixture.Clock.UtcNow.AddDays(1), fixture.Clock.UtcNow.AddDays(2), fixture.UserId));
        var audit = await fixture.Service.CreateAuditAsync(new CreateManagedAuditCommand(fixture.TenantId, program.Value.Id, plan.Value!.Id, "Plant ISO Audit", "AUD-001", ManagedAuditType.Iso9001, fixture.UserId));

        await fixture.Service.AssignChecklistAsync(new AssignAuditChecklistCommand(fixture.TenantId, audit.Value!.Id, checklist.Value.Id, fixture.UserId));
        await fixture.Service.ScheduleAsync(new ScheduleAuditCommand(fixture.TenantId, audit.Value.Id, fixture.Clock.UtcNow.AddDays(1), fixture.Clock.UtcNow.AddDays(2), "Plant A", fixture.UserId));
        await fixture.Service.AddParticipantAsync(new AddAuditParticipantCommand(fixture.TenantId, audit.Value.Id, Guid.NewGuid(), AuditParticipantRole.Auditee, fixture.UserId));
        await fixture.Service.AddAreaAsync(new AddAuditAreaCommand(fixture.TenantId, audit.Value.Id, "Production", "Manufacturing", fixture.UserId));
        await fixture.Service.StartAsync(new ManagedAuditActionCommand(fixture.TenantId, audit.Value.Id, fixture.UserId));
        var finding = await fixture.Service.AddFindingAsync(new AddAuditFindingCommand(fixture.TenantId, audit.Value.Id, "Missing calibration record", "Scale records incomplete.", AuditFindingSeverity.Major, item.Value!.Id, fixture.UserId));
        var evidence = await fixture.Service.AddEvidenceAsync(new AddAuditEvidenceCommand(fixture.TenantId, audit.Value.Id, finding.Value!.Id, AuditEvidenceType.Pdf, Guid.NewGuid(), "evidence.pdf", "application/pdf", 128, Hash, fixture.UserId));
        var observation = await fixture.Service.AddObservationAsync(new AddAuditObservationCommand(fixture.TenantId, audit.Value.Id, "Operators know the procedure.", fixture.UserId));
        var nonConformity = await fixture.Service.AddNonConformityAsync(new AddAuditNonConformityCommand(fixture.TenantId, audit.Value.Id, finding.Value.Id, "ISO 9001 clause 7.1.5", fixture.UserId));
        var recommendation = await fixture.Service.AddRecommendationAsync(new AddAuditRecommendationCommand(fixture.TenantId, audit.Value.Id, finding.Value.Id, "Calibrate and archive certificates monthly.", fixture.UserId));
        var capa = await fixture.Service.LinkCorrectiveActionAsync(new LinkAuditCorrectiveActionCommand(fixture.TenantId, audit.Value.Id, finding.Value.Id, Guid.NewGuid(), fixture.UserId));
        var attachment = await fixture.Service.AddAttachmentAsync(new AddAuditAttachmentCommand(fixture.TenantId, audit.Value.Id, Guid.NewGuid(), "scope.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", 256, Hash, fixture.UserId));
        await fixture.Service.CompleteAsync(new ManagedAuditActionCommand(fixture.TenantId, audit.Value.Id, fixture.UserId));
        var closed = await fixture.Service.CloseAsync(new ManagedAuditActionCommand(fixture.TenantId, audit.Value.Id, fixture.UserId));
        var reopened = await fixture.Service.ReopenAsync(new ManagedAuditActionCommand(fixture.TenantId, audit.Value.Id, fixture.UserId));

        Assert.True(evidence.IsSuccess);
        Assert.True(observation.IsSuccess);
        Assert.True(nonConformity.IsSuccess);
        Assert.True(recommendation.IsSuccess);
        Assert.True(capa.IsSuccess);
        Assert.True(attachment.IsSuccess);
        Assert.True(closed.IsSuccess);
        Assert.True(reopened.IsSuccess);
        Assert.Equal(ManagedAuditStatus.Reopened, fixture.Repository.Audits.Single().Status);
        Assert.Contains(fixture.Repository.AuditLogs, log => log.Action == AuditAction.AuditClosed);
        Assert.Contains(fixture.Repository.AuditLogs, log => log.Action == AuditAction.AuditReopened);
    }

    [Fact]
    public async Task AuditManagement_Rejects_Duplicates_Missing_References_And_Invalid_Transitions()
    {
        var fixture = AuditFixture.Create();
        var program = await fixture.Service.CreateProgramAsync(new CreateAuditProgramCommand(fixture.TenantId, "Program", "PROG", 2026, fixture.UserId));
        var duplicateProgram = await fixture.Service.CreateProgramAsync(new CreateAuditProgramCommand(fixture.TenantId, "Program 2", "PROG", 2026, fixture.UserId));
        var checklist = await fixture.Service.CreateChecklistAsync(new CreateAuditChecklistCommand(fixture.TenantId, "BPM Checklist", "BPM", AuditChecklistType.Bpm, 1, fixture.UserId));
        var duplicateChecklist = await fixture.Service.CreateChecklistAsync(new CreateAuditChecklistCommand(fixture.TenantId, "BPM Checklist", "BPM", AuditChecklistType.Bpm, 1, fixture.UserId));
        var missingPlan = await fixture.Service.CreatePlanAsync(new CreateAuditPlanCommand(fixture.TenantId, Guid.NewGuid(), "Scope", "Criteria", fixture.Clock.UtcNow, fixture.Clock.UtcNow.AddDays(1), fixture.UserId));
        var missingChecklistItem = await fixture.Service.AddChecklistItemAsync(new AddAuditChecklistItemCommand(fixture.TenantId, Guid.NewGuid(), "A", "Question", 10, fixture.UserId));
        var missingProgramAudit = await fixture.Service.CreateAuditAsync(new CreateManagedAuditCommand(fixture.TenantId, Guid.NewGuid(), Guid.NewGuid(), "Audit", "AUD-MISSING-PROGRAM", ManagedAuditType.Internal, fixture.UserId));
        var plan = await fixture.Service.CreatePlanAsync(new CreateAuditPlanCommand(fixture.TenantId, program.Value!.Id, "Scope", "Criteria", fixture.Clock.UtcNow, fixture.Clock.UtcNow.AddDays(1), fixture.UserId));
        var missingPlanAudit = await fixture.Service.CreateAuditAsync(new CreateManagedAuditCommand(fixture.TenantId, program.Value.Id, Guid.NewGuid(), "Audit", "AUD-MISSING-PLAN", ManagedAuditType.Internal, fixture.UserId));
        var audit = await fixture.Service.CreateAuditAsync(new CreateManagedAuditCommand(fixture.TenantId, program.Value.Id, plan.Value!.Id, "Audit", "AUD", ManagedAuditType.Internal, fixture.UserId));
        var duplicateAudit = await fixture.Service.CreateAuditAsync(new CreateManagedAuditCommand(fixture.TenantId, program.Value.Id, plan.Value.Id, "Audit 2", "AUD", ManagedAuditType.Internal, fixture.UserId));
        var missingChecklist = await fixture.Service.AssignChecklistAsync(new AssignAuditChecklistCommand(fixture.TenantId, audit.Value!.Id, Guid.NewGuid(), fixture.UserId));
        var invalidStart = await fixture.Service.StartAsync(new ManagedAuditActionCommand(fixture.TenantId, audit.Value.Id, fixture.UserId));
        var missingFinding = await fixture.Service.AddEvidenceAsync(new AddAuditEvidenceCommand(fixture.TenantId, audit.Value.Id, Guid.NewGuid(), AuditEvidenceType.Image, Guid.NewGuid(), "x.png", "image/png", 1, Hash, fixture.UserId));

        Assert.True(duplicateProgram.IsFailure);
        Assert.True(duplicateChecklist.IsFailure);
        Assert.True(missingPlan.IsFailure);
        Assert.True(missingChecklistItem.IsFailure);
        Assert.True(missingProgramAudit.IsFailure);
        Assert.True(missingPlanAudit.IsFailure);
        Assert.True(duplicateAudit.IsFailure);
        Assert.True(missingChecklist.IsFailure);
        Assert.True(invalidStart.IsFailure);
        Assert.True(missingFinding.IsFailure);
        Assert.True(checklist.IsSuccess);
    }

    [Fact]
    public async Task AuditManagement_Search_Dashboard_Export_Are_Tenant_Isolated()
    {
        var fixture = AuditFixture.Create();
        var auditId = await fixture.CreateScheduledAuditAsync("Tenant Audit", "TENANT-AUD", ManagedAuditType.Supplier);
        fixture.Repository.Audits.Add(new ManagedAudit(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Other", "OTHER", ManagedAuditType.Supplier, fixture.UserId, fixture.Clock.UtcNow));
        await fixture.Service.StartAsync(new ManagedAuditActionCommand(fixture.TenantId, auditId, fixture.UserId));
        await fixture.Service.AddFindingAsync(new AddAuditFindingCommand(fixture.TenantId, auditId, "Critical", "Critical finding", AuditFindingSeverity.Critical, null, fixture.UserId));

        var search = await fixture.Service.SearchAsync(new ManagedAuditSearchQuery(fixture.TenantId, "Tenant", ManagedAuditType.Supplier, ManagedAuditStatus.InProgress, 1, 10));
        var defaultPagedSearch = await fixture.Service.SearchAsync(new ManagedAuditSearchQuery(fixture.TenantId, null, null, null, 0, 0));
        var dashboard = await fixture.Service.GetDashboardAsync(fixture.TenantId);
        var export = await fixture.Service.ExportAsync(new ManagedAuditExportQuery(fixture.TenantId, ManagedAuditType.Supplier, ManagedAuditStatus.InProgress, "json", fixture.UserId));
        var exportXlsx = await fixture.Service.ExportAsync(new ManagedAuditExportQuery(fixture.TenantId, null, null, "xlsx", fixture.UserId));
        var exportCsv = await fixture.Service.ExportAsync(new ManagedAuditExportQuery(fixture.TenantId, null, null, "", fixture.UserId));

        Assert.True(search.IsSuccess);
        Assert.Single(search.Value!.Items);
        Assert.Equal(25, defaultPagedSearch.Value!.PageSize);
        Assert.Equal(1, dashboard.Value!.OpenAudits);
        Assert.Equal(1, dashboard.Value.CriticalFindings);
        Assert.Equal("json", export.Value!.Format);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", exportXlsx.Value!.ContentType);
        Assert.Equal("csv", exportCsv.Value!.Format);
        Assert.Equal(3, fixture.Repository.AuditLogs.Count(log => log.Action == AuditAction.Exported));
    }

    [Fact]
    public void AuditManagement_Domain_Validates_Rules_And_Exposes_State()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var audit = new ManagedAudit(tenantId, Guid.NewGuid(), Guid.NewGuid(), "HACCP Audit", "HACCP-1", ManagedAuditType.Haccp, userId, now);
        var checklist = new AuditChecklist(tenantId, "HACCP", "HACCP", AuditChecklistType.Haccp, 1, userId);
        var item = checklist.AddItem("CCP", "Are CCP records complete?", 50);
        audit.AssignChecklist(checklist.Id, userId, now);
        audit.Schedule(now.AddHours(1), now.AddHours(2), "Line 1", userId, now);
        var participantUserId = Guid.NewGuid();
        var participant = audit.AddParticipant(participantUserId, AuditParticipantRole.LeadAuditor, userId, now);
        var duplicateParticipant = audit.AddParticipant(participantUserId, AuditParticipantRole.LeadAuditor, userId, now);
        var auditor = audit.AddAuditor(Guid.NewGuid(), false);
        audit.AddArea("Quality", "QA");
        audit.Start(userId, now);
        var finding = audit.AddFinding("Finding", "Description", AuditFindingSeverity.OpportunityForImprovement, item.Id, userId, now);
        audit.AddEvidence(finding.Id, AuditEvidenceType.Video, Guid.NewGuid(), "video.mp4", "video/mp4", 10, Hash, userId, now);
        audit.AddObservation("Observation", userId, now);
        audit.AddNonConformity(finding.Id, "Requirement", userId, now);
        audit.AddRecommendation(finding.Id, "Recommendation", userId, now);
        audit.LinkCorrectiveAction(finding.Id, Guid.NewGuid(), userId, now);
        audit.AddAttachment(Guid.NewGuid(), "file.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 10, Hash, userId, now);
        var dashboard = audit.Dashboard();

        Assert.Equal(1, dashboard.OpenAudits);
        Assert.Single(audit.Findings);
        Assert.Equal(participant.Id, duplicateParticipant.Id);
        Assert.False(auditor.IsLead);
        Assert.Equal(1, audit.History.Count(history => history.Action == "Evidence attached."));
        Assert.Throws<DomainException>(() => new AuditPlan(tenantId, Guid.NewGuid(), "Scope", "Criteria", now, now.AddSeconds(-1)));
        Assert.Throws<DomainException>(() => new AuditEvidence(tenantId, audit.Id, finding.Id, AuditEvidenceType.Pdf, Guid.NewGuid(), "bad.pdf", "application/pdf", 0, Hash, userId, now));
        Assert.Throws<DomainException>(() => audit.Close(userId, now));
        Assert.Throws<DomainException>(() => audit.AddEvidence(Guid.NewGuid(), AuditEvidenceType.Pdf, Guid.NewGuid(), "bad.pdf", "application/pdf", 1, Hash, userId, now));
        audit.Complete(userId, now);
        audit.Close(userId, now);
        var closedDashboard = audit.Dashboard();
        var emptyDashboard = new ManagedAudit(tenantId, Guid.NewGuid(), Guid.NewGuid(), "Empty", "EMPTY", ManagedAuditType.Internal, userId, now).Dashboard();
        Assert.Equal(1, closedDashboard.ClosedAudits);
        Assert.Equal(100, emptyDashboard.ComplianceScore);
    }

    [Fact]
    public async Task EfAuditManagementRepository_Persists_Graph_And_Dashboard()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfAuditManagementRepository(dbContext);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var program = new AuditProgram(tenantId, "DB Program", "DB-PROG", 2026, userId);
        await repository.AddProgramAsync(program);
        await dbContext.SaveChangesAsync();
        var plan = new AuditPlan(tenantId, program.Id, "Scope", "Criteria", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1));
        await repository.AddPlanAsync(plan);
        var audit = new ManagedAudit(tenantId, program.Id, plan.Id, "DB Audit", "DB-AUD", ManagedAuditType.Regulatory, userId, DateTimeOffset.UtcNow);
        audit.Schedule(DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2), "HQ", userId, DateTimeOffset.UtcNow);
        audit.Start(userId, DateTimeOffset.UtcNow);
        audit.AddFinding("Major", "Major finding", AuditFindingSeverity.Major, null, userId, DateTimeOffset.UtcNow);
        await repository.AddAuditAsync(audit);
        await repository.AddAuditLogAsync(AuditLog.Create(tenantId, userId, nameof(ManagedAudit), audit.Id, AuditAction.AuditCreated, DateTimeOffset.UtcNow));
        await dbContext.SaveChangesAsync();

        var loaded = await repository.GetAuditAsync(tenantId, audit.Id);
        var exists = await repository.AuditCodeExistsAsync(tenantId, "db-aud");
        var search = await repository.SearchAsync(new ManagedAuditSearchCriteria(tenantId, "DB", ManagedAuditType.Regulatory, ManagedAuditStatus.InProgress, 1, 10));
        var dashboard = await repository.GetDashboardAsync(tenantId);
        var emptyDashboard = await repository.GetDashboardAsync(Guid.NewGuid());

        Assert.NotNull(loaded);
        Assert.True(exists);
        Assert.Single(search.Items);
        Assert.Equal(1, dashboard.MajorFindings);
        Assert.Equal(100, emptyDashboard.ComplianceScore);
        Assert.Single(dbContext.AuditLogs);
    }

    [Fact]
    public async Task AuditManagement_Service_Returns_Failures_For_Missing_Audit_On_All_Mutations()
    {
        var fixture = AuditFixture.Create();
        var auditId = Guid.NewGuid();
        var findingId = Guid.NewGuid();

        Assert.True((await fixture.Service.AssignChecklistAsync(new AssignAuditChecklistCommand(fixture.TenantId, auditId, Guid.NewGuid(), fixture.UserId))).IsFailure);
        Assert.True((await fixture.Service.ScheduleAsync(new ScheduleAuditCommand(fixture.TenantId, auditId, fixture.Clock.UtcNow, fixture.Clock.UtcNow.AddHours(1), "HQ", fixture.UserId))).IsFailure);
        Assert.True((await fixture.Service.AddParticipantAsync(new AddAuditParticipantCommand(fixture.TenantId, auditId, Guid.NewGuid(), AuditParticipantRole.Observer, fixture.UserId))).IsFailure);
        Assert.True((await fixture.Service.AddAreaAsync(new AddAuditAreaCommand(fixture.TenantId, auditId, "Area", "Process", fixture.UserId))).IsFailure);
        Assert.True((await fixture.Service.AddFindingAsync(new AddAuditFindingCommand(fixture.TenantId, auditId, "Finding", "Description", AuditFindingSeverity.Minor, null, fixture.UserId))).IsFailure);
        Assert.True((await fixture.Service.AddEvidenceAsync(new AddAuditEvidenceCommand(fixture.TenantId, auditId, findingId, AuditEvidenceType.Word, Guid.NewGuid(), "file.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", 10, Hash, fixture.UserId))).IsFailure);
        Assert.True((await fixture.Service.AddObservationAsync(new AddAuditObservationCommand(fixture.TenantId, auditId, "Observation", fixture.UserId))).IsFailure);
        Assert.True((await fixture.Service.AddNonConformityAsync(new AddAuditNonConformityCommand(fixture.TenantId, auditId, findingId, "Requirement", fixture.UserId))).IsFailure);
        Assert.True((await fixture.Service.AddRecommendationAsync(new AddAuditRecommendationCommand(fixture.TenantId, auditId, findingId, "Recommendation", fixture.UserId))).IsFailure);
        Assert.True((await fixture.Service.LinkCorrectiveActionAsync(new LinkAuditCorrectiveActionCommand(fixture.TenantId, auditId, findingId, Guid.NewGuid(), fixture.UserId))).IsFailure);
        Assert.True((await fixture.Service.AddAttachmentAsync(new AddAuditAttachmentCommand(fixture.TenantId, auditId, Guid.NewGuid(), "file.pdf", "application/pdf", 10, Hash, fixture.UserId))).IsFailure);
        Assert.True((await fixture.Service.CompleteAsync(new ManagedAuditActionCommand(fixture.TenantId, auditId, fixture.UserId))).IsFailure);
        Assert.True((await fixture.Service.CloseAsync(new ManagedAuditActionCommand(fixture.TenantId, auditId, fixture.UserId))).IsFailure);
        Assert.True((await fixture.Service.ReopenAsync(new ManagedAuditActionCommand(fixture.TenantId, auditId, fixture.UserId))).IsFailure);
    }

    [Fact]
    public async Task AuditManagement_Service_Captures_Domain_Exceptions_As_Result_Failures()
    {
        var fixture = AuditFixture.Create();
        var invalidProgram = await fixture.Service.CreateProgramAsync(new CreateAuditProgramCommand(fixture.TenantId, "", "BAD", 2026, fixture.UserId));
        var invalidChecklist = await fixture.Service.CreateChecklistAsync(new CreateAuditChecklistCommand(fixture.TenantId, "", "BAD", AuditChecklistType.Custom, 1, fixture.UserId));
        var checklist = await fixture.Service.CreateChecklistAsync(new CreateAuditChecklistCommand(fixture.TenantId, "Custom", "CUSTOM", AuditChecklistType.Custom, 1, fixture.UserId));
        var invalidItem = await fixture.Service.AddChecklistItemAsync(new AddAuditChecklistItemCommand(fixture.TenantId, checklist.Value!.Id, "", "Question", 10, fixture.UserId));
        var program = await fixture.Service.CreateProgramAsync(new CreateAuditProgramCommand(fixture.TenantId, "Program", "PROG-VALID", 2026, fixture.UserId));
        var invalidPlan = await fixture.Service.CreatePlanAsync(new CreateAuditPlanCommand(fixture.TenantId, program.Value!.Id, "Scope", "Criteria", fixture.Clock.UtcNow.AddDays(1), fixture.Clock.UtcNow, fixture.UserId));
        var plan = await fixture.Service.CreatePlanAsync(new CreateAuditPlanCommand(fixture.TenantId, program.Value.Id, "Scope", "Criteria", fixture.Clock.UtcNow.AddDays(1), fixture.Clock.UtcNow.AddDays(2), fixture.UserId));
        var invalidAudit = await fixture.Service.CreateAuditAsync(new CreateManagedAuditCommand(fixture.TenantId, program.Value.Id, plan.Value!.Id, "", "BAD-AUD", ManagedAuditType.External, fixture.UserId));
        var audit = await fixture.Service.CreateAuditAsync(new CreateManagedAuditCommand(fixture.TenantId, program.Value.Id, plan.Value.Id, "Audit", "AUD-VALID", ManagedAuditType.External, fixture.UserId));
        var invalidSchedule = await fixture.Service.ScheduleAsync(new ScheduleAuditCommand(fixture.TenantId, audit.Value!.Id, fixture.Clock.UtcNow.AddDays(2), fixture.Clock.UtcNow.AddDays(1), "HQ", fixture.UserId));
        await fixture.Service.ScheduleAsync(new ScheduleAuditCommand(fixture.TenantId, audit.Value.Id, fixture.Clock.UtcNow.AddDays(1), fixture.Clock.UtcNow.AddDays(2), "HQ", fixture.UserId));
        await fixture.Service.StartAsync(new ManagedAuditActionCommand(fixture.TenantId, audit.Value.Id, fixture.UserId));
        var invalidFinding = await fixture.Service.AddFindingAsync(new AddAuditFindingCommand(fixture.TenantId, audit.Value.Id, "", "Description", AuditFindingSeverity.Minor, null, fixture.UserId));
        var finding = await fixture.Service.AddFindingAsync(new AddAuditFindingCommand(fixture.TenantId, audit.Value.Id, "Finding", "Description", AuditFindingSeverity.Minor, null, fixture.UserId));
        var invalidEvidence = await fixture.Service.AddEvidenceAsync(new AddAuditEvidenceCommand(fixture.TenantId, audit.Value.Id, finding.Value!.Id, AuditEvidenceType.Excel, Guid.NewGuid(), "file.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 0, Hash, fixture.UserId));
        var invalidAttachment = await fixture.Service.AddAttachmentAsync(new AddAuditAttachmentCommand(fixture.TenantId, audit.Value.Id, Guid.NewGuid(), "file.pdf", "application/pdf", 0, Hash, fixture.UserId));
        await fixture.Service.CompleteAsync(new ManagedAuditActionCommand(fixture.TenantId, audit.Value.Id, fixture.UserId));
        await fixture.Service.CloseAsync(new ManagedAuditActionCommand(fixture.TenantId, audit.Value.Id, fixture.UserId));
        var invalidComplete = await fixture.Service.CompleteAsync(new ManagedAuditActionCommand(fixture.TenantId, audit.Value.Id, fixture.UserId));

        Assert.True(invalidProgram.IsFailure);
        Assert.True(invalidChecklist.IsFailure);
        Assert.True(invalidItem.IsFailure);
        Assert.True(invalidPlan.IsFailure);
        Assert.True(invalidAudit.IsFailure);
        Assert.True(invalidSchedule.IsFailure);
        Assert.True(invalidFinding.IsFailure);
        Assert.True(invalidEvidence.IsFailure);
        Assert.True(invalidAttachment.IsFailure);
        Assert.True(invalidComplete.IsFailure);
    }

    [Fact]
    public void AuditManagement_Entity_Properties_Are_Complete_For_Auxiliary_Types()
    {
        var tenantId = Guid.NewGuid();
        var auditId = Guid.NewGuid();
        var findingId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var program = new AuditProgram(tenantId, "Program", "prog", 2026, userId);
        var checklist = new AuditChecklist(tenantId, "Checklist", "chk", AuditChecklistType.Supplier, 2, userId);
        var item = checklist.AddItem("Supplier", "Question", 20);
        var plan = new AuditPlan(tenantId, program.Id, "Scope", "Criteria", now, now.AddDays(1));
        var schedule = new AuditSchedule(tenantId, auditId, now, now.AddHours(1), "Remote");
        var participant = new AuditParticipant(tenantId, auditId, userId, AuditParticipantRole.Auditor);
        var auditor = new AuditAuditor(tenantId, auditId, userId, true);
        var area = new AuditArea(tenantId, auditId, "Warehouse", "Storage");
        var finding = new AuditFinding(tenantId, auditId, "Title", "Description", AuditFindingSeverity.Critical, item.Id, userId, now);
        var evidence = new AuditEvidence(tenantId, auditId, findingId, AuditEvidenceType.Attachment, fileId, "file.bin", "application/octet-stream", 1, Hash, userId, now);
        var observation = new AuditObservation(tenantId, auditId, "Observation", userId, now);
        var nonConformity = new AuditNonConformity(tenantId, auditId, findingId, "Requirement", userId, now);
        var recommendation = new AuditRecommendation(tenantId, auditId, findingId, "Recommendation", userId, now);
        var link = new AuditCorrectiveActionLink(tenantId, auditId, findingId, Guid.NewGuid());
        var history = new AuditHistory(tenantId, auditId, "Action", userId, now);
        var attachment = new AuditAttachment(tenantId, auditId, fileId, "file.bin", "application/octet-stream", 1, Hash, userId, now);

        Assert.True(program.IsActive);
        Assert.True(checklist.IsTemplate);
        Assert.Equal(2, checklist.Version);
        Assert.Equal(20, item.Weight);
        Assert.Equal(plan.AuditProgramId, program.Id);
        Assert.Equal("Remote", schedule.Location);
        Assert.Equal(AuditParticipantRole.Auditor, participant.Role);
        Assert.True(auditor.IsLead);
        Assert.Equal("Storage", area.Process);
        Assert.Equal(AuditFindingSeverity.Critical, finding.Severity);
        Assert.Equal(AuditEvidenceType.Attachment, evidence.Type);
        Assert.Equal("Observation", observation.Description);
        Assert.Equal("Requirement", nonConformity.Requirement);
        Assert.Equal("Recommendation", recommendation.Recommendation);
        Assert.Equal(findingId, link.FindingId);
        Assert.Equal("Action", history.Action);
        Assert.Equal(Hash, attachment.Sha256Hash);
    }

    private const string Hash = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    private static Compliance360DbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<Compliance360DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new Compliance360DbContext(options, new FixedClock());
    }

    private sealed class AuditFixture
    {
        private AuditFixture()
        {
            TenantId = Guid.NewGuid();
            UserId = Guid.NewGuid();
            Clock = new FixedClock();
            Repository = new InMemoryAuditManagementRepository();
            Service = new AuditManagementService(Repository, new FakeApplicationDbContext(), Clock);
        }

        public Guid TenantId { get; }
        public Guid UserId { get; }
        public FixedClock Clock { get; }
        public InMemoryAuditManagementRepository Repository { get; }
        public AuditManagementService Service { get; }

        public static AuditFixture Create() => new();

        public async Task<Guid> CreateScheduledAuditAsync(string title, string code, ManagedAuditType type)
        {
            var program = await Service.CreateProgramAsync(new CreateAuditProgramCommand(TenantId, "Program " + code, "PROG-" + code, 2026, UserId));
            var plan = await Service.CreatePlanAsync(new CreateAuditPlanCommand(TenantId, program.Value!.Id, "Scope", "Criteria", Clock.UtcNow.AddDays(1), Clock.UtcNow.AddDays(2), UserId));
            var audit = await Service.CreateAuditAsync(new CreateManagedAuditCommand(TenantId, program.Value.Id, plan.Value!.Id, title, code, type, UserId));
            await Service.ScheduleAsync(new ScheduleAuditCommand(TenantId, audit.Value!.Id, Clock.UtcNow.AddDays(1), Clock.UtcNow.AddDays(2), "HQ", UserId));
            return audit.Value.Id;
        }
    }

    private sealed class InMemoryAuditManagementRepository : IAuditManagementRepository
    {
        public List<AuditProgram> Programs { get; } = [];
        public List<AuditChecklist> Checklists { get; } = [];
        public List<AuditPlan> Plans { get; } = [];
        public List<ManagedAudit> Audits { get; } = [];
        public List<AuditLog> AuditLogs { get; } = [];

        public Task AddProgramAsync(AuditProgram program, CancellationToken cancellationToken = default)
        {
            Programs.Add(program);
            return Task.CompletedTask;
        }

        public Task<AuditProgram?> GetProgramAsync(Guid tenantId, Guid programId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Programs.SingleOrDefault(program => program.TenantId == tenantId && program.Id == programId));
        }

        public Task<bool> ProgramCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Programs.Any(program => program.TenantId == tenantId && program.Code == code.ToUpperInvariant()));
        }

        public Task AddChecklistAsync(AuditChecklist checklist, CancellationToken cancellationToken = default)
        {
            Checklists.Add(checklist);
            return Task.CompletedTask;
        }

        public Task<AuditChecklist?> GetChecklistAsync(Guid tenantId, Guid checklistId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Checklists.SingleOrDefault(checklist => checklist.TenantId == tenantId && checklist.Id == checklistId));
        }

        public Task<bool> ChecklistCodeVersionExistsAsync(Guid tenantId, string code, int version, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Checklists.Any(checklist => checklist.TenantId == tenantId && checklist.Code == code.ToUpperInvariant() && checklist.Version == version));
        }

        public Task AddPlanAsync(AuditPlan plan, CancellationToken cancellationToken = default)
        {
            Plans.Add(plan);
            return Task.CompletedTask;
        }

        public Task<AuditPlan?> GetPlanAsync(Guid tenantId, Guid planId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Plans.SingleOrDefault(plan => plan.TenantId == tenantId && plan.Id == planId));
        }

        public Task AddAuditAsync(ManagedAudit audit, CancellationToken cancellationToken = default)
        {
            Audits.Add(audit);
            return Task.CompletedTask;
        }

        public Task<ManagedAudit?> GetAuditAsync(Guid tenantId, Guid auditId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Audits.SingleOrDefault(audit => audit.TenantId == tenantId && audit.Id == auditId));
        }

        public Task<bool> AuditCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Audits.Any(audit => audit.TenantId == tenantId && audit.Code == code.ToUpperInvariant()));
        }

        public Task<ManagedAuditSearchResult> SearchAsync(ManagedAuditSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            var audits = Audits
                .Where(audit => audit.TenantId == criteria.TenantId)
                .Where(audit => criteria.SearchText is null || audit.Title.Contains(criteria.SearchText) || audit.Code.Contains(criteria.SearchText))
                .Where(audit => !criteria.Type.HasValue || audit.Type == criteria.Type.Value)
                .Where(audit => !criteria.Status.HasValue || audit.Status == criteria.Status.Value)
                .Select(audit => new ManagedAuditSummary(audit.Id, audit.TenantId, audit.Title, audit.Code, audit.Type, audit.Status, audit.ChecklistId))
                .ToArray();

            return Task.FromResult(new ManagedAuditSearchResult(audits, audits.Length, criteria.Page, criteria.PageSize));
        }

        public Task<AuditDashboardDto> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            var tenantAudits = Audits.Where(audit => audit.TenantId == tenantId).ToArray();
            var findings = tenantAudits.SelectMany(audit => audit.Findings).ToArray();
            var critical = findings.Count(finding => finding.Severity == AuditFindingSeverity.Critical);
            var major = findings.Count(finding => finding.Severity == AuditFindingSeverity.Major);
            return Task.FromResult(new AuditDashboardDto(
                tenantAudits.Count(audit => audit.Status != ManagedAuditStatus.Closed && audit.Status != ManagedAuditStatus.Cancelled),
                tenantAudits.Count(audit => audit.Status == ManagedAuditStatus.Closed),
                critical,
                major,
                tenantAudits.Sum(audit => audit.CorrectiveActionLinks.Count),
                findings.Length == 0 ? 100 : Math.Max(0, 100 - ((critical + major) * 10)),
                findings.Length));
        }

        public Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            AuditLogs.Add(auditLog);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeApplicationDbContext : IApplicationDbContext
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
    }
}
