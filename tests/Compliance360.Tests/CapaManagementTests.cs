using Compliance360.Application;
using Compliance360.Application.CapaManagement;
using Compliance360.Domain.Audit;
using Compliance360.Domain.CapaManagement;
using Compliance360.Domain.Common;
using Compliance360.Infrastructure.CapaManagement;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Tests;

public sealed class CapaManagementTests
{
    [Fact]
    public async Task Capa_Full_Flow_Closes_And_Reopens_With_Enterprise_Integrations()
    {
        var fixture = CapaFixture.Create();
        var supplierId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var auditId = Guid.NewGuid();
        var findingId = Guid.NewGuid();
        var capa = await fixture.Service.CreateAsync(new CreateCapaCommand(fixture.TenantId, "Audit CAPA", "CAPA-001", "Major audit finding", CapaPriority.High, CapaRiskLevel.High, CapaSourceType.AuditFinding, findingId, supplierId, documentId, auditId, fixture.UserId));
        var dueAt = fixture.Clock.UtcNow.AddDays(5);
        var owner = await fixture.Service.AssignOwnerAsync(new AssignCapaOwnerCommand(fixture.TenantId, capa.Value!.Id, fixture.OwnerId, dueAt, fixture.UserId));
        var approver = await fixture.Service.AddApproverAsync(new AddCapaApproverCommand(fixture.TenantId, capa.Value.Id, fixture.ApproverId, fixture.UserId));
        var rootCause = await fixture.Service.DefineRootCauseAsync(new DefineCapaRootCauseCommand(fixture.TenantId, capa.Value.Id, "Calibration process not standardized", CapaRootCauseMethod.FiveWhy, fixture.UserId));
        var fiveWhy = await fixture.Service.AddFiveWhyAsync(new AddCapaFiveWhyCommand(fixture.TenantId, capa.Value.Id, "Record missing", "Owner unclear", "Procedure incomplete", "Training incomplete", "Governance gap", fixture.UserId));
        var ishikawa = await fixture.Service.AddIshikawaAsync(new AddCapaIshikawaCommand(fixture.TenantId, capa.Value.Id, "Training", "Procedure", "Scale", "Labels", "Plant", "KPI", fixture.UserId));
        var containment = await fixture.Service.AddContainmentActionAsync(new AddCapaActionCommand(fixture.TenantId, capa.Value.Id, "Quarantine affected batch", fixture.OwnerId, dueAt, fixture.UserId));
        var corrective = await fixture.Service.AddCorrectiveActionAsync(new AddCapaActionCommand(fixture.TenantId, capa.Value.Id, "Update calibration SOP", fixture.OwnerId, dueAt, fixture.UserId));
        var preventive = await fixture.Service.AddPreventiveActionAsync(new AddCapaActionCommand(fixture.TenantId, capa.Value.Id, "Monthly calibration audit", fixture.OwnerId, dueAt, fixture.UserId));
        var evidence = await fixture.Service.AddEvidenceAsync(new AddCapaEvidenceCommand(fixture.TenantId, capa.Value.Id, Guid.NewGuid(), "evidence.pdf", "application/pdf", 128, Hash, fixture.UserId));
        var attachment = await fixture.Service.AddAttachmentAsync(new AddCapaAttachmentCommand(fixture.TenantId, capa.Value.Id, Guid.NewGuid(), "analysis.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 256, Hash, fixture.UserId));
        var followUp = await fixture.Service.RegisterFollowUpAsync(new CapaFollowUpCommand(fixture.TenantId, capa.Value.Id, "Actions on track", fixture.UserId));
        var workflow = await fixture.Service.AttachWorkflowAsync(new AttachCapaWorkflowCommand(fixture.TenantId, capa.Value.Id, Guid.NewGuid(), fixture.UserId));
        fixture.Repository.Capas.Single().CorrectiveActions.Single().Complete(fixture.Clock.UtcNow);
        var effectiveness = await fixture.Service.VerifyEffectivenessAsync(new VerifyCapaEffectivenessCommand(fixture.TenantId, capa.Value.Id, true, "No recurrence after verification", fixture.UserId));
        var close = await fixture.Service.ApproveClosureAsync(new CapaActionCommand(fixture.TenantId, capa.Value.Id, fixture.ApproverId));
        var reopen = await fixture.Service.ReopenAsync(new ReopenCapaCommand(fixture.TenantId, capa.Value.Id, "Finding recurrence", fixture.UserId));

        Assert.True(owner.IsSuccess);
        Assert.True(approver.IsSuccess);
        Assert.True(rootCause.IsSuccess);
        Assert.True(fiveWhy.IsSuccess);
        Assert.True(ishikawa.IsSuccess);
        Assert.True(containment.IsSuccess);
        Assert.True(corrective.IsSuccess);
        Assert.True(preventive.IsSuccess);
        Assert.True(evidence.IsSuccess);
        Assert.True(attachment.IsSuccess);
        Assert.True(followUp.IsSuccess);
        Assert.True(workflow.IsSuccess);
        Assert.True(effectiveness.IsSuccess);
        Assert.True(close.IsSuccess);
        Assert.True(reopen.IsSuccess);
        Assert.Equal(CapaStatus.Reopened, fixture.Repository.Capas.Single().Status);
        Assert.Contains(fixture.Repository.AuditLogs, log => log.Action == AuditAction.CapaCreated);
        Assert.Contains(fixture.Repository.AuditLogs, log => log.Action == AuditAction.CapaClosed);
        Assert.Contains(fixture.Repository.AuditLogs, log => log.Action == AuditAction.CapaReopened);
    }

    [Fact]
    public async Task Capa_Service_Rejects_Duplicates_Missing_And_Invalid_State()
    {
        var fixture = CapaFixture.Create();
        var capa = await fixture.Service.CreateAsync(new CreateCapaCommand(fixture.TenantId, "CAPA", "CAPA-002", "Description", CapaPriority.Medium, CapaRiskLevel.Medium, CapaSourceType.Manual, null, null, null, null, fixture.UserId));
        var duplicate = await fixture.Service.CreateAsync(new CreateCapaCommand(fixture.TenantId, "CAPA 2", "CAPA-002", "Description", CapaPriority.Medium, CapaRiskLevel.Medium, CapaSourceType.Manual, null, null, null, null, fixture.UserId));
        var invalidCreate = await fixture.Service.CreateAsync(new CreateCapaCommand(fixture.TenantId, "", "BAD", "Description", CapaPriority.Low, CapaRiskLevel.Low, CapaSourceType.Manual, null, null, null, null, fixture.UserId));
        var missing = await fixture.Service.AssignOwnerAsync(new AssignCapaOwnerCommand(fixture.TenantId, Guid.NewGuid(), fixture.OwnerId, fixture.Clock.UtcNow.AddDays(1), fixture.UserId));
        var invalidOwnerDue = await fixture.Service.AssignOwnerAsync(new AssignCapaOwnerCommand(fixture.TenantId, capa.Value!.Id, fixture.OwnerId, fixture.Clock.UtcNow.AddDays(-1), fixture.UserId));
        var invalidEffectiveness = await fixture.Service.VerifyEffectivenessAsync(new VerifyCapaEffectivenessCommand(fixture.TenantId, capa.Value.Id, true, "Too soon", fixture.UserId));
        var invalidClose = await fixture.Service.ApproveClosureAsync(new CapaActionCommand(fixture.TenantId, capa.Value.Id, fixture.UserId));
        var invalidReopen = await fixture.Service.ReopenAsync(new ReopenCapaCommand(fixture.TenantId, capa.Value.Id, "No close yet", fixture.UserId));
        var invalidEscalation = await fixture.Service.EscalateOverdueAsync(new CapaActionCommand(fixture.TenantId, capa.Value.Id, fixture.UserId));
        var invalidEvidence = await fixture.Service.AddEvidenceAsync(new AddCapaEvidenceCommand(fixture.TenantId, capa.Value.Id, Guid.NewGuid(), "bad.pdf", "application/pdf", 0, Hash, fixture.UserId));
        var invalidAttachment = await fixture.Service.AddAttachmentAsync(new AddCapaAttachmentCommand(fixture.TenantId, capa.Value.Id, Guid.NewGuid(), "bad.pdf", "application/pdf", 0, Hash, fixture.UserId));

        Assert.True(duplicate.IsFailure);
        Assert.True(invalidCreate.IsFailure);
        Assert.True(missing.IsFailure);
        Assert.True(invalidOwnerDue.IsFailure);
        Assert.True(invalidEffectiveness.IsFailure);
        Assert.True(invalidClose.IsFailure);
        Assert.True(invalidReopen.IsFailure);
        Assert.True(invalidEscalation.IsFailure);
        Assert.True(invalidEvidence.IsFailure);
        Assert.True(invalidAttachment.IsFailure);
    }

    [Fact]
    public async Task Capa_Search_Dashboard_Export_Are_Tenant_Isolated()
    {
        var fixture = CapaFixture.Create();
        var supplierId = Guid.NewGuid();
        var auditId = Guid.NewGuid();
        var capa = await fixture.Service.CreateAsync(new CreateCapaCommand(fixture.TenantId, "Supplier Audit CAPA", "CAPA-003", "Description", CapaPriority.Critical, CapaRiskLevel.Critical, CapaSourceType.AuditNonConformity, Guid.NewGuid(), supplierId, null, auditId, fixture.UserId));
        await fixture.Service.ClassifyAsync(new ClassifyCapaCommand(fixture.TenantId, capa.Value!.Id, CapaPriority.Critical, CapaRiskLevel.Critical, fixture.Clock.UtcNow.AddDays(-1), fixture.UserId));
        await fixture.Service.AssignOwnerAsync(new AssignCapaOwnerCommand(fixture.TenantId, capa.Value.Id, fixture.OwnerId, fixture.Clock.UtcNow.AddDays(2), fixture.UserId));
        fixture.Repository.Capas.Add(new Capa(Guid.NewGuid(), "Other", "OTHER-CAPA", "Other tenant", CapaPriority.Critical, CapaRiskLevel.Critical, CapaSourceType.Manual, null, supplierId, null, auditId, fixture.UserId, fixture.Clock.UtcNow));

        var search = await fixture.Service.SearchAsync(new CapaSearchQuery(fixture.TenantId, "Supplier", CapaStatus.InProgress, CapaPriority.Critical, CapaRiskLevel.Critical, fixture.OwnerId, supplierId, auditId, 1, 10));
        var defaultSearch = await fixture.Service.SearchAsync(new CapaSearchQuery(fixture.TenantId, null, null, null, null, null, null, null, 0, 0));
        var dashboard = await fixture.Service.GetDashboardAsync(fixture.TenantId);
        var exportJson = await fixture.Service.ExportAsync(new CapaExportQuery(fixture.TenantId, CapaStatus.InProgress, CapaPriority.Critical, CapaRiskLevel.Critical, "json", fixture.UserId));
        var exportXlsx = await fixture.Service.ExportAsync(new CapaExportQuery(fixture.TenantId, null, null, null, "xlsx", fixture.UserId));
        var exportCsv = await fixture.Service.ExportAsync(new CapaExportQuery(fixture.TenantId, null, null, null, "", fixture.UserId));

        Assert.True(search.IsSuccess);
        Assert.Single(search.Value!.Items);
        Assert.Equal(25, defaultSearch.Value!.PageSize);
        Assert.Equal(1, dashboard.Value!.OpenCapas);
        Assert.Equal(1, dashboard.Value.OverdueCapas);
        Assert.Equal(1, dashboard.Value.CriticalCapas);
        Assert.Equal(1, dashboard.Value.CapasByOwner);
        Assert.Equal(1, dashboard.Value.CapasBySupplier);
        Assert.Equal(1, dashboard.Value.CapasByAudit);
        Assert.Equal("json", exportJson.Value!.Format);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", exportXlsx.Value!.ContentType);
        Assert.Equal("csv", exportCsv.Value!.Format);
        Assert.Equal(3, fixture.Repository.AuditLogs.Count(log => log.Action == AuditAction.Exported));
    }

    [Fact]
    public void Capa_Domain_Validates_Rules_And_Exposes_State()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var capa = new Capa(tenantId, "Domain CAPA", "DOM-CAPA", "Description", CapaPriority.High, CapaRiskLevel.High, CapaSourceType.AuditRecommendation, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), userId, now);
        capa.Classify(CapaPriority.Critical, CapaRiskLevel.Critical, now.AddDays(-1), userId, now);
        var owner = capa.AssignOwner(userId, now.AddDays(1), userId, now);
        var duplicateOwner = capa.AssignOwner(userId, now.AddDays(2), userId, now);
        var approver = capa.AddApprover(approverId, userId, now);
        var duplicateApprover = capa.AddApprover(approverId, userId, now);
        var rootCause = capa.DefineRootCause("Root cause", CapaRootCauseMethod.Other, userId, now);
        var fiveWhy = capa.AddFiveWhyAnalysis("1", "2", "3", "4", "5", userId, now);
        var fishbone = capa.AddIshikawaAnalysis("People", "Process", "Equipment", "Material", "Environment", "Measurement", userId, now);
        var containment = capa.AddContainmentAction("Contain", userId, now.AddDays(1), userId, now);
        var corrective = capa.AddCorrectiveAction("Correct", userId, now.AddDays(1), userId, now);
        var preventive = capa.AddPreventiveAction("Prevent", userId, now.AddDays(1), userId, now);
        capa.AddEvidence(Guid.NewGuid(), "evidence.png", "image/png", 1, Hash, userId, now);
        capa.AddAttachment(Guid.NewGuid(), "attachment.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", 1, Hash, userId, now);
        capa.RegisterFollowUp("Follow-up", userId, now);
        capa.EscalateOverdue(userId, now.AddDays(2));
        corrective.Start();
        corrective.Complete(now);
        containment.MarkOverdue(now.AddDays(2));
        var check = capa.VerifyEffectiveness(false, "Not effective", userId, now);
        capa.AttachWorkflow(Guid.NewGuid(), userId, now);
        preventive.Complete(now);
        capa.VerifyEffectiveness(true, "Effective", userId, now.AddMinutes(1));
        capa.ApproveClosure(approverId, now);
        var dashboard = capa.Dashboard(now);

        Assert.Equal(owner.Id, duplicateOwner.Id);
        Assert.Equal(approver.Id, duplicateApprover.Id);
        Assert.Equal(CapaRootCauseMethod.Other, rootCause.Method);
        Assert.Equal(CapaRootCauseMethod.FiveWhy, fiveWhy.Method);
        Assert.Equal("People", fishbone.People);
        Assert.Equal(CapaActionStatus.Overdue, containment.Status);
        Assert.Equal(CapaActionType.Corrective, corrective.Type);
        Assert.Equal(CapaActionType.Preventive, preventive.Type);
        Assert.False(check.IsEffective);
        Assert.Equal(1, dashboard.Critical);
        Assert.Equal(1, dashboard.Effective);
        Assert.Equal(0, dashboard.Recurrence);
        Assert.Throws<DomainException>(() => new Capa(tenantId, "", "BAD", "Description", CapaPriority.Low, CapaRiskLevel.Low, CapaSourceType.Manual, null, null, null, null, userId, now));
        Assert.Throws<DomainException>(() => new CapaEvidence(tenantId, capa.Id, Guid.NewGuid(), "bad", "text/plain", 0, Hash, userId, now));
        Assert.Throws<DomainException>(() => new CapaAttachment(tenantId, capa.Id, Guid.NewGuid(), "bad", "text/plain", 0, Hash, userId, now));
        Assert.Throws<DomainException>(() => corrective.Start());
        Assert.Throws<DomainException>(() => corrective.Complete(now));
        capa.Reopen("Need more work", userId, now);
        Assert.Throws<DomainException>(() => capa.Reopen("Already reopened", userId, now));
    }

    [Fact]
    public async Task Capa_Branch_Rules_Cover_Alternate_Workflow_And_Dashboard_Paths()
    {
        var fixture = CapaFixture.Create();
        var missingClassify = await fixture.Service.ClassifyAsync(new ClassifyCapaCommand(fixture.TenantId, Guid.NewGuid(), CapaPriority.Low, CapaRiskLevel.Low, null, fixture.UserId));
        var capaResult = await fixture.Service.CreateAsync(new CreateCapaCommand(fixture.TenantId, "Branch CAPA", "CAPA-BRANCH", "Description", CapaPriority.Low, CapaRiskLevel.Low, CapaSourceType.Manual, null, null, null, null, fixture.UserId));
        var capa = fixture.Repository.Capas.Single(capa => capa.Id == capaResult.Value!.Id);
        await fixture.Service.AssignOwnerAsync(new AssignCapaOwnerCommand(fixture.TenantId, capa.Id, fixture.OwnerId, fixture.Clock.UtcNow.AddDays(2), fixture.UserId));
        var classifyNonDraft = await fixture.Service.ClassifyAsync(new ClassifyCapaCommand(fixture.TenantId, capa.Id, CapaPriority.Medium, CapaRiskLevel.Medium, null, fixture.UserId));
        var noChangeDashboard = capa.Dashboard(fixture.Clock.UtcNow);
        var preventive = capa.AddPreventiveAction("Preventive only", fixture.OwnerId, fixture.Clock.UtcNow.AddDays(2), fixture.UserId, fixture.Clock.UtcNow);
        preventive.Start();
        preventive.MarkOverdue(fixture.Clock.UtcNow.AddDays(1));
        preventive.Complete(fixture.Clock.UtcNow);
        preventive.MarkOverdue(fixture.Clock.UtcNow.AddDays(5));
        capa.VerifyEffectiveness(true, "Effective with preventive", fixture.UserId, fixture.Clock.UtcNow);
        var wrongApprover = Assert.Throws<DomainException>(() =>
        {
            capa.AddApprover(fixture.ApproverId, fixture.UserId, fixture.Clock.UtcNow);
            capa.ApproveClosure(Guid.NewGuid(), fixture.Clock.UtcNow);
        });

        var noApproverCapa = new Capa(fixture.TenantId, "No approver", "NO-APPROVER", "Description", CapaPriority.Low, CapaRiskLevel.Low, CapaSourceType.Manual, null, null, null, null, fixture.UserId, fixture.Clock.UtcNow);
        var action = noApproverCapa.AddCorrectiveAction("Correct", fixture.OwnerId, fixture.Clock.UtcNow.AddDays(2), fixture.UserId, fixture.Clock.UtcNow);
        action.Complete(fixture.Clock.UtcNow);
        noApproverCapa.VerifyEffectiveness(true, "Effective", fixture.UserId, fixture.Clock.UtcNow);
        noApproverCapa.ApproveClosure(fixture.UserId, fixture.Clock.UtcNow.AddDays(1));
        var closedDashboard = noApproverCapa.Dashboard(fixture.Clock.UtcNow.AddDays(1));

        Assert.True(missingClassify.IsFailure);
        Assert.True(classifyNonDraft.IsSuccess);
        Assert.Equal(CapaStatus.PendingApproval, capa.Status);
        Assert.Equal(1, noChangeDashboard.Open);
        Assert.Equal(0, noChangeDashboard.SupplierLinked);
        Assert.Equal(0, noChangeDashboard.AuditLinked);
        Assert.Equal(CapaActionStatus.Completed, preventive.Status);
        Assert.Equal("User is not a CAPA approver.", wrongApprover.Message);
        Assert.Equal(0, closedDashboard.Open);
        Assert.Equal(1, closedDashboard.ClosureDays);
    }

    [Fact]
    public async Task EfCapaManagementRepository_Persists_Graph_And_Dashboard()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfCapaManagementRepository(dbContext);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var supplierId = Guid.NewGuid();
        var auditId = Guid.NewGuid();
        var capa = new Capa(tenantId, "DB CAPA", "DB-CAPA", "Description", CapaPriority.Critical, CapaRiskLevel.Critical, CapaSourceType.Supplier, Guid.NewGuid(), supplierId, null, auditId, userId, now);
        capa.Classify(CapaPriority.Critical, CapaRiskLevel.Critical, now.AddDays(-1), userId, now);
        capa.AssignOwner(userId, now.AddDays(1), userId, now);
        capa.AddCorrectiveAction("Correct", userId, now.AddDays(1), userId, now).Complete(now);
        capa.VerifyEffectiveness(true, "Effective", userId, now);
        capa.ApproveClosure(userId, now.AddDays(2));
        await repository.AddAsync(capa);
        await repository.AddAuditLogAsync(AuditLog.Create(tenantId, userId, nameof(Capa), capa.Id, AuditAction.CapaCreated, now));
        await dbContext.SaveChangesAsync();

        var loaded = await repository.GetAsync(tenantId, capa.Id);
        var exists = await repository.CodeExistsAsync(tenantId, "db-capa");
        var search = await repository.SearchAsync(new CapaSearchCriteria(tenantId, "DB", CapaStatus.Closed, CapaPriority.Critical, CapaRiskLevel.Critical, userId, supplierId, auditId, 1, 10));
        var dashboard = await repository.GetDashboardAsync(tenantId, now.AddDays(3));
        var emptyDashboard = await repository.GetDashboardAsync(Guid.NewGuid(), now);

        Assert.NotNull(loaded);
        Assert.True(exists);
        Assert.Single(search.Items);
        Assert.Equal(1, dashboard.CriticalCapas);
        Assert.Equal(2, dashboard.AverageClosureDays);
        Assert.Equal(100, dashboard.EffectivenessPercent);
        Assert.Equal(100, emptyDashboard.EffectivenessPercent);
        Assert.Single(dbContext.AuditLogs);
    }

    private const string Hash = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    private static Compliance360DbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<Compliance360DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new Compliance360DbContext(options, new FixedClock());
    }

    private sealed class CapaFixture
    {
        private CapaFixture()
        {
            TenantId = Guid.NewGuid();
            UserId = Guid.NewGuid();
            OwnerId = Guid.NewGuid();
            ApproverId = Guid.NewGuid();
            Clock = new FixedClock();
            Repository = new InMemoryCapaManagementRepository();
            Service = new CapaManagementService(Repository, new FakeApplicationDbContext(), Clock);
        }

        public Guid TenantId { get; }
        public Guid UserId { get; }
        public Guid OwnerId { get; }
        public Guid ApproverId { get; }
        public FixedClock Clock { get; }
        public InMemoryCapaManagementRepository Repository { get; }
        public CapaManagementService Service { get; }
        public static CapaFixture Create() => new();
    }

    private sealed class InMemoryCapaManagementRepository : ICapaManagementRepository
    {
        public List<Capa> Capas { get; } = [];
        public List<AuditLog> AuditLogs { get; } = [];

        public Task AddAsync(Capa capa, CancellationToken cancellationToken = default)
        {
            Capas.Add(capa);
            return Task.CompletedTask;
        }

        public Task<Capa?> GetAsync(Guid tenantId, Guid capaId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Capas.SingleOrDefault(capa => capa.TenantId == tenantId && capa.Id == capaId));
        }

        public Task<bool> CodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Capas.Any(capa => capa.TenantId == tenantId && capa.Code == code.ToUpperInvariant()));
        }

        public Task<CapaSearchResult> SearchAsync(CapaSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            var query = Capas
                .Where(capa => capa.TenantId == criteria.TenantId)
                .Where(capa => criteria.SearchText is null || capa.Title.Contains(criteria.SearchText) || capa.Code.Contains(criteria.SearchText))
                .Where(capa => !criteria.Status.HasValue || capa.Status == criteria.Status.Value)
                .Where(capa => !criteria.Priority.HasValue || capa.Priority == criteria.Priority.Value)
                .Where(capa => !criteria.RiskLevel.HasValue || capa.RiskLevel == criteria.RiskLevel.Value)
                .Where(capa => !criteria.SupplierId.HasValue || capa.SupplierId == criteria.SupplierId.Value)
                .Where(capa => !criteria.AuditId.HasValue || capa.AuditId == criteria.AuditId.Value)
                .Where(capa => !criteria.OwnerUserId.HasValue || capa.Owners.Any(owner => owner.UserId == criteria.OwnerUserId.Value && owner.IsActive))
                .Select(capa => new CapaSummary(capa.Id, capa.TenantId, capa.Title, capa.Code, capa.Status, capa.Priority, capa.RiskLevel, capa.SourceType, capa.SupplierId, capa.DocumentId, capa.AuditId, capa.CommitmentDueAtUtc, capa.ClosedAtUtc))
                .ToArray();

            return Task.FromResult(new CapaSearchResult(query, query.Length, criteria.Page, criteria.PageSize));
        }

        public Task<CapaDashboardDto> GetDashboardAsync(Guid tenantId, DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            var tenantCapas = Capas.Where(capa => capa.TenantId == tenantId).ToArray();
            var open = tenantCapas.Count(capa => capa.Status != CapaStatus.Closed && capa.Status != CapaStatus.Cancelled);
            var overdue = tenantCapas.Count(capa => capa.IsOverdue(now));
            var critical = tenantCapas.Count(capa => capa.Priority == CapaPriority.Critical || capa.RiskLevel == CapaRiskLevel.Critical);
            var owners = tenantCapas.Sum(capa => capa.Owners.Count(owner => owner.IsActive));
            var suppliers = tenantCapas.Count(capa => capa.SupplierId.HasValue);
            var audits = tenantCapas.Count(capa => capa.AuditId.HasValue);
            var closed = tenantCapas.Where(capa => capa.Status == CapaStatus.Closed && capa.ClosedAtUtc.HasValue).ToArray();
            var averageClosure = closed.Length == 0 ? 0 : Math.Round((decimal)closed.Average(capa => Math.Max(0, (capa.ClosedAtUtc!.Value - capa.CreatedAtUtc).TotalDays)), 2);
            var checks = tenantCapas.SelectMany(capa => capa.EffectivenessChecks).ToArray();
            var effectiveness = checks.Length == 0 ? 100 : (int)Math.Round(checks.Count(check => check.IsEffective) * 100m / checks.Length);
            var recurrence = checks.Count(check => !check.IsEffective);
            return Task.FromResult(new CapaDashboardDto(open, overdue, critical, owners, suppliers, audits, averageClosure, effectiveness, recurrence));
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
