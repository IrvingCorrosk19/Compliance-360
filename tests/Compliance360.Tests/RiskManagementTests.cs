using Compliance360.Application;
using Compliance360.Application.RiskManagement;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.RiskManagement;
using Compliance360.Infrastructure.Persistence;
using Compliance360.Infrastructure.RiskManagement;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Tests;

public sealed class RiskManagementTests
{
    [Fact]
    public async Task Risk_Full_Flow_Calculates_Mitigates_Closes_And_Reopens()
    {
        var fixture = RiskFixture.Create();
        var category = await fixture.Service.CreateCategoryAsync(new CreateRiskCategoryCommand(fixture.TenantId, "Operational", "OPS", fixture.UserId));
        var matrix = await fixture.Service.CreateMatrixAsync(new CreateRiskMatrixCommand(fixture.TenantId, "5x5 Enterprise", 10, fixture.UserId));
        var risk = await fixture.Service.CreateRiskAsync(new CreateRiskCommand(fixture.TenantId, category.Value!.Id, "Supplier HACCP risk", "RISK-001", "Supplier HACCP lapse", RiskType.Supplier, "Quality", "Supplier approval", fixture.SupplierId, fixture.DocumentId, fixture.AuditId, fixture.CapaId, fixture.UserId));
        var owner = await fixture.Service.AssignOwnerAsync(new AssignRiskOwnerCommand(fixture.TenantId, risk.Value!.Id, fixture.OwnerId, fixture.UserId));
        var assessment = await fixture.Service.AssessAsync(new AssessRiskCommand(fixture.TenantId, risk.Value.Id, RiskProbability.AlmostCertain, RiskImpact.Severe, RiskProbability.Possible, RiskImpact.Major, matrix.Value!.ToleranceScore, fixture.UserId));
        var treatment = await fixture.Service.AddTreatmentAsync(new AddRiskTreatmentCommand(fixture.TenantId, risk.Value.Id, RiskTreatmentStrategy.Reduce, "Reduce through supplier controls", fixture.UserId));
        var mitigation = await fixture.Service.AddMitigationPlanAsync(new AddRiskMitigationPlanCommand(fixture.TenantId, risk.Value.Id, "Supplier requalification", fixture.OwnerId, fixture.Clock.UtcNow.AddDays(5), fixture.UserId));
        var control = await fixture.Service.AddControlAsync(new AddRiskControlCommand(fixture.TenantId, risk.Value.Id, "Document validation", RiskControlType.Preventive, "Validate certificates monthly", true, fixture.UserId));
        var evidence = await fixture.Service.AddEvidenceAsync(new AddRiskEvidenceCommand(fixture.TenantId, risk.Value.Id, Guid.NewGuid(), "risk.pdf", "application/pdf", 128, Hash, fixture.UserId));
        var attachment = await fixture.Service.AddAttachmentAsync(new AddRiskAttachmentCommand(fixture.TenantId, risk.Value.Id, Guid.NewGuid(), "matrix.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 256, Hash, fixture.UserId));
        var review = await fixture.Service.ScheduleReviewAsync(new ScheduleRiskReviewCommand(fixture.TenantId, risk.Value.Id, fixture.Clock.UtcNow.AddDays(10), fixture.UserId));
        var completeReview = await fixture.Service.CompleteReviewAsync(new CompleteRiskReviewCommand(fixture.TenantId, risk.Value.Id, review.Value!.Id, "Risk reviewed", fixture.UserId));
        var indicator = await fixture.Service.AddIndicatorAsync(new AddRiskIndicatorCommand(fixture.TenantId, risk.Value.Id, "Supplier incidents", 3, 2, fixture.UserId));
        var escalation = await fixture.Service.EscalateCriticalAsync(new RiskActionCommand(fixture.TenantId, risk.Value.Id, fixture.UserId));
        var workflow = await fixture.Service.AttachWorkflowAsync(new AttachRiskWorkflowCommand(fixture.TenantId, risk.Value.Id, Guid.NewGuid(), fixture.UserId));
        var close = await fixture.Service.CloseAsync(new RiskActionCommand(fixture.TenantId, risk.Value.Id, fixture.UserId));
        var reopen = await fixture.Service.ReopenAsync(new ReopenRiskCommand(fixture.TenantId, risk.Value.Id, "New exposure", fixture.UserId));

        Assert.True(owner.IsSuccess);
        Assert.Equal(25, assessment.Value!.InherentScore);
        Assert.Equal(RiskLevel.Critical, assessment.Value.InherentLevel);
        Assert.Equal(12, assessment.Value.ResidualScore);
        Assert.Equal(RiskLevel.High, assessment.Value.ResidualLevel);
        Assert.False(assessment.Value.IsWithinTolerance);
        Assert.True(treatment.IsSuccess);
        Assert.True(mitigation.IsSuccess);
        Assert.True(control.IsSuccess);
        Assert.True(evidence.IsSuccess);
        Assert.True(attachment.IsSuccess);
        Assert.True(completeReview.IsSuccess);
        Assert.True(indicator.Value!.IsBreached);
        Assert.True(escalation.IsSuccess);
        Assert.True(workflow.IsSuccess);
        Assert.True(close.IsSuccess);
        Assert.True(reopen.IsSuccess);
        Assert.Equal(CapaSafeRiskStatus.Reopened, (CapaSafeRiskStatus)fixture.Repository.Risks.Single().Status);
        Assert.Contains(fixture.Repository.AuditLogs, log => log.Action == AuditAction.RiskCreated);
        Assert.Contains(fixture.Repository.AuditLogs, log => log.Action == AuditAction.RiskClosed);
        Assert.Contains(fixture.Repository.AuditLogs, log => log.Action == AuditAction.RiskReopened);
    }

    [Fact]
    public async Task Risk_Service_Rejects_Duplicates_Missing_And_Invalid_Rules()
    {
        var fixture = RiskFixture.Create();
        var category = await fixture.Service.CreateCategoryAsync(new CreateRiskCategoryCommand(fixture.TenantId, "Regulatory", "REG", fixture.UserId));
        var duplicateCategory = await fixture.Service.CreateCategoryAsync(new CreateRiskCategoryCommand(fixture.TenantId, "Regulatory 2", "REG", fixture.UserId));
        var missingCategoryRisk = await fixture.Service.CreateRiskAsync(new CreateRiskCommand(fixture.TenantId, Guid.NewGuid(), "Risk", "BAD-CAT", "Description", RiskType.Regulatory, "Legal", "Permits", null, null, null, null, fixture.UserId));
        var risk = await fixture.Service.CreateRiskAsync(new CreateRiskCommand(fixture.TenantId, category.Value!.Id, "Risk", "RISK-002", "Description", RiskType.Regulatory, "Legal", "Permits", null, null, null, null, fixture.UserId));
        var duplicateRisk = await fixture.Service.CreateRiskAsync(new CreateRiskCommand(fixture.TenantId, category.Value.Id, "Risk 2", "RISK-002", "Description", RiskType.Regulatory, "Legal", "Permits", null, null, null, null, fixture.UserId));
        var invalidCreate = await fixture.Service.CreateRiskAsync(new CreateRiskCommand(fixture.TenantId, category.Value.Id, "", "BAD", "Description", RiskType.Regulatory, "Legal", "Permits", null, null, null, null, fixture.UserId));
        var missing = await fixture.Service.AssignOwnerAsync(new AssignRiskOwnerCommand(fixture.TenantId, Guid.NewGuid(), fixture.OwnerId, fixture.UserId));
        var invalidMitigation = await fixture.Service.AddMitigationPlanAsync(new AddRiskMitigationPlanCommand(fixture.TenantId, risk.Value!.Id, "Bad due", fixture.OwnerId, fixture.Clock.UtcNow.AddDays(-1), fixture.UserId));
        var invalidEvidence = await fixture.Service.AddEvidenceAsync(new AddRiskEvidenceCommand(fixture.TenantId, risk.Value.Id, Guid.NewGuid(), "bad.pdf", "application/pdf", 0, Hash, fixture.UserId));
        var invalidAttachment = await fixture.Service.AddAttachmentAsync(new AddRiskAttachmentCommand(fixture.TenantId, risk.Value.Id, Guid.NewGuid(), "bad.pdf", "application/pdf", 0, Hash, fixture.UserId));
        var invalidReview = await fixture.Service.ScheduleReviewAsync(new ScheduleRiskReviewCommand(fixture.TenantId, risk.Value.Id, fixture.Clock.UtcNow.AddDays(-1), fixture.UserId));
        var invalidEscalation = await fixture.Service.EscalateCriticalAsync(new RiskActionCommand(fixture.TenantId, risk.Value.Id, fixture.UserId));
        var invalidReopen = await fixture.Service.ReopenAsync(new ReopenRiskCommand(fixture.TenantId, risk.Value.Id, "Not closed", fixture.UserId));

        Assert.True(duplicateCategory.IsFailure);
        Assert.True(missingCategoryRisk.IsFailure);
        Assert.True(duplicateRisk.IsFailure);
        Assert.True(invalidCreate.IsFailure);
        Assert.True(missing.IsFailure);
        Assert.True(invalidMitigation.IsFailure);
        Assert.True(invalidEvidence.IsFailure);
        Assert.True(invalidAttachment.IsFailure);
        Assert.True(invalidReview.IsFailure);
        Assert.True(invalidEscalation.IsFailure);
        Assert.True(invalidReopen.IsFailure);
    }

    [Fact]
    public async Task Risk_Search_Dashboard_HeatMap_Export_Are_Tenant_Isolated()
    {
        var fixture = RiskFixture.Create();
        var category = await fixture.Service.CreateCategoryAsync(new CreateRiskCategoryCommand(fixture.TenantId, "Supplier", "SUP", fixture.UserId));
        var risk = await fixture.Service.CreateRiskAsync(new CreateRiskCommand(fixture.TenantId, category.Value!.Id, "Supplier risk", "RISK-003", "Description", RiskType.Supplier, "Quality", "Supplier", fixture.SupplierId, null, fixture.AuditId, fixture.CapaId, fixture.UserId));
        await fixture.Service.AssessAsync(new AssessRiskCommand(fixture.TenantId, risk.Value!.Id, RiskProbability.Likely, RiskImpact.Major, RiskProbability.Likely, RiskImpact.Major, 10, fixture.UserId));
        fixture.Repository.Risks.Single(riskItem => riskItem.Id == risk.Value.Id)
            .ScheduleReview(fixture.Clock.UtcNow.AddDays(-1), fixture.UserId, fixture.Clock.UtcNow.AddDays(-2));
        fixture.Repository.Risks.Add(new Risk(Guid.NewGuid(), category.Value.Id, "Other", "OTHER-RISK", "Other tenant", RiskType.Supplier, "Quality", "Supplier", fixture.SupplierId, null, fixture.AuditId, fixture.CapaId, fixture.UserId, fixture.Clock.UtcNow));

        var search = await fixture.Service.SearchAsync(new RiskSearchQuery(fixture.TenantId, "Supplier", RiskStatus.Monitoring, RiskType.Supplier, RiskLevel.High, "Quality", fixture.SupplierId, fixture.AuditId, fixture.CapaId, 1, 10));
        var defaultSearch = await fixture.Service.SearchAsync(new RiskSearchQuery(fixture.TenantId, null, null, null, null, null, null, null, null, 0, 0));
        var dashboard = await fixture.Service.GetDashboardAsync(fixture.TenantId);
        var heatMap = await fixture.Service.GetHeatMapAsync(fixture.TenantId);
        var exportJson = await fixture.Service.ExportAsync(new RiskExportQuery(fixture.TenantId, RiskStatus.Monitoring, RiskType.Supplier, RiskLevel.High, "json", fixture.UserId));
        var exportXlsx = await fixture.Service.ExportAsync(new RiskExportQuery(fixture.TenantId, null, null, null, "xlsx", fixture.UserId));
        var exportCsv = await fixture.Service.ExportAsync(new RiskExportQuery(fixture.TenantId, null, null, null, "", fixture.UserId));

        Assert.Single(search.Value!.Items);
        Assert.Equal(25, defaultSearch.Value!.PageSize);
        Assert.Equal(1, dashboard.Value!.HighRisks);
        Assert.Equal(1, dashboard.Value.OverdueRisks);
        Assert.Equal(1, dashboard.Value.RisksByArea);
        Assert.Equal(1, dashboard.Value.RisksBySupplier);
        Assert.Equal(1, dashboard.Value.RisksByProcess);
        Assert.Single(heatMap.Value!);
        Assert.Equal("json", exportJson.Value!.Format);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", exportXlsx.Value!.ContentType);
        Assert.Equal("csv", exportCsv.Value!.Format);
        Assert.Equal(3, fixture.Repository.AuditLogs.Count(log => log.Action == AuditAction.Exported));
    }

    [Fact]
    public void Risk_Domain_Matrix_And_Entities_Expose_State()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var category = new RiskCategory(tenantId, "Document", "doc", userId);
        var matrix = new RiskMatrix(tenantId, "5x5", 9, userId);
        var risk = new Risk(tenantId, category.Id, "Document risk", "DOC-RISK", "Description", RiskType.Document, "QA", "Document control", null, Guid.NewGuid(), null, null, userId, now);
        risk.Classify(RiskType.Iso9001, "QA", "Audit", userId, now);
        var owner = risk.AssignOwner(userId, userId, now);
        var duplicateOwner = risk.AssignOwner(userId, userId, now);
        var assessment = risk.Assess(RiskProbability.Rare, RiskImpact.Minor, RiskProbability.Rare, RiskImpact.Minor, matrix.ToleranceScore, userId, now);
        var treatment = risk.AddTreatment(RiskTreatmentStrategy.Accept, "Within tolerance", userId, now);
        var mitigation = risk.AddMitigationPlan("Mitigate", userId, now.AddDays(1), userId, now);
        mitigation.Complete();
        var control = risk.AddControl("Control", RiskControlType.Detective, "Description", false, userId, now);
        var review = risk.ScheduleReview(now.AddDays(1), userId, now);
        risk.CompleteReview(review.Id, "Reviewed", userId, now);
        var indicator = risk.AddIndicator("KRI", 1, 2, userId, now);
        var evidence = risk.AddEvidence(Guid.NewGuid(), "risk.png", "image/png", 1, Hash, userId, now);
        var attachment = risk.AddAttachment(Guid.NewGuid(), "risk.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", 1, Hash, userId, now);
        risk.AttachWorkflow(Guid.NewGuid(), userId, now);
        var heat = risk.HeatMapPoint();

        Assert.True(category.IsActive);
        Assert.True(matrix.IsDefault);
        Assert.True(matrix.IsWithinTolerance(assessment.ResidualScore));
        Assert.Equal(owner.Id, duplicateOwner.Id);
        Assert.True(risk.IsAccepted);
        Assert.True(risk.IsWithinTolerance);
        Assert.True(mitigation.IsCompleted);
        Assert.False(control.IsEffective);
        Assert.Equal(RiskReviewStatus.Completed, review.Status);
        Assert.False(indicator.IsBreached);
        Assert.Equal(Hash, evidence.Sha256Hash);
        Assert.Equal(Hash, attachment.Sha256Hash);
        Assert.Equal(2, heat.Score);
        Assert.Equal(RiskLevel.Low, RiskMatrix.CalculateLevel(1));
        Assert.Equal(RiskLevel.Medium, RiskMatrix.CalculateLevel(6));
        Assert.Equal(RiskLevel.High, RiskMatrix.CalculateLevel(12));
        Assert.Equal(RiskLevel.Critical, RiskMatrix.CalculateLevel(20));
        Assert.Throws<DomainException>(() => new Risk(tenantId, category.Id, "", "BAD", "Description", RiskType.Document, "QA", "Process", null, null, null, null, userId, now));
        Assert.Throws<DomainException>(() => new RiskEvidence(tenantId, risk.Id, Guid.NewGuid(), "bad", "text/plain", 0, Hash, userId, now));
        Assert.Throws<DomainException>(() => new RiskAttachment(tenantId, risk.Id, Guid.NewGuid(), "bad", "text/plain", 0, Hash, userId, now));
        Assert.Throws<DomainException>(() => risk.CompleteReview(Guid.NewGuid(), "Missing", userId, now));
        risk.Close(userId, now);
        Assert.Throws<DomainException>(() => risk.Close(userId, now));
        risk.Reopen("New exposure", userId, now);
        Assert.Throws<DomainException>(() => risk.Reopen("Again", userId, now));
    }

    [Fact]
    public async Task Risk_Branch_Rules_Cover_Alternate_Paths()
    {
        var fixture = RiskFixture.Create();
        var missingClose = await fixture.Service.CloseAsync(new RiskActionCommand(fixture.TenantId, Guid.NewGuid(), fixture.UserId));
        var category = await fixture.Service.CreateCategoryAsync(new CreateRiskCategoryCommand(fixture.TenantId, "Branch", "BR", fixture.UserId));
        var riskResult = await fixture.Service.CreateRiskAsync(new CreateRiskCommand(fixture.TenantId, category.Value!.Id, "Branch Risk", "RISK-BR", "Description", RiskType.Operational, "Ops", "Process", null, null, null, null, fixture.UserId));
        var risk = fixture.Repository.Risks.Single(item => item.Id == riskResult.Value!.Id);
        risk.Classify(RiskType.Operational, "Ops", "Process", fixture.UserId, fixture.Clock.UtcNow);
        risk.Classify(RiskType.Bpm, "Quality", "BPM", fixture.UserId, fixture.Clock.UtcNow);
        var emptyHeat = risk.HeatMapPoint();
        Assert.False(risk.IsOverdue(fixture.Clock.UtcNow));
        risk.Assess(RiskProbability.AlmostCertain, RiskImpact.Severe, RiskProbability.AlmostCertain, RiskImpact.Severe, 10, fixture.UserId, fixture.Clock.UtcNow);
        risk.EscalateCritical(fixture.UserId, fixture.Clock.UtcNow);
        risk.ScheduleReview(fixture.Clock.UtcNow.AddDays(-1), fixture.UserId, fixture.Clock.UtcNow.AddDays(-2));
        risk.Close(fixture.UserId, fixture.Clock.UtcNow);

        Assert.True(missingClose.IsFailure);
        Assert.Equal(RiskStatus.Closed, risk.Status);
        Assert.False(risk.IsOverdue(fixture.Clock.UtcNow));
        Assert.Equal(1, emptyHeat.Probability);
        Assert.Equal(1, emptyHeat.Impact);
        Assert.Equal(RiskLevel.Critical, risk.ResidualLevel);
    }

    [Fact]
    public async Task EfRiskManagementRepository_Persists_Graph_Dashboard_And_HeatMap()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfRiskManagementRepository(dbContext);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var category = new RiskCategory(tenantId, "DB", "DB", userId);
        await repository.AddCategoryAsync(category);
        await dbContext.SaveChangesAsync();
        var risk = new Risk(tenantId, category.Id, "DB Risk", "DB-RISK", "Description", RiskType.Operational, "Ops", "Process", Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), userId, now);
        risk.AssignOwner(userId, userId, now);
        risk.Assess(RiskProbability.AlmostCertain, RiskImpact.Severe, RiskProbability.AlmostCertain, RiskImpact.Severe, 10, userId, now);
        risk.ScheduleReview(now.AddDays(-1), userId, now.AddDays(-2));
        await repository.AddRiskAsync(risk);
        await repository.AddAuditLogAsync(AuditLog.Create(tenantId, userId, nameof(Risk), risk.Id, AuditAction.RiskCreated, now));
        await dbContext.SaveChangesAsync();

        var loaded = await repository.GetRiskAsync(tenantId, risk.Id);
        var exists = await repository.RiskCodeExistsAsync(tenantId, "db-risk");
        var search = await repository.SearchAsync(new RiskSearchCriteria(tenantId, "DB", RiskStatus.Monitoring, RiskType.Operational, RiskLevel.Critical, "Ops", risk.SupplierId, risk.AuditId, risk.CapaId, 1, 10));
        var dashboard = await repository.GetDashboardAsync(tenantId, now);
        var heatMap = await repository.GetHeatMapAsync(tenantId);

        Assert.NotNull(loaded);
        Assert.True(exists);
        Assert.Single(search.Items);
        Assert.Equal(1, dashboard.CriticalRisks);
        Assert.Equal(1, dashboard.OverdueRisks);
        Assert.Single(heatMap);
        Assert.Single(dbContext.AuditLogs);
    }

    private const string Hash = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    private static Compliance360DbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<Compliance360DbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new Compliance360DbContext(options, new FixedClock());
    }

    private enum CapaSafeRiskStatus { Draft, Identified, Assessed, TreatmentPlanned, Mitigating, Monitoring, Accepted, Closed, Reopened }

    private sealed class RiskFixture
    {
        private RiskFixture()
        {
            TenantId = Guid.NewGuid();
            UserId = Guid.NewGuid();
            OwnerId = Guid.NewGuid();
            SupplierId = Guid.NewGuid();
            DocumentId = Guid.NewGuid();
            AuditId = Guid.NewGuid();
            CapaId = Guid.NewGuid();
            Clock = new FixedClock();
            Repository = new InMemoryRiskManagementRepository();
            Service = new RiskManagementService(Repository, new FakeApplicationDbContext(), Clock);
        }

        public Guid TenantId { get; }
        public Guid UserId { get; }
        public Guid OwnerId { get; }
        public Guid SupplierId { get; }
        public Guid DocumentId { get; }
        public Guid AuditId { get; }
        public Guid CapaId { get; }
        public FixedClock Clock { get; }
        public InMemoryRiskManagementRepository Repository { get; }
        public RiskManagementService Service { get; }
        public static RiskFixture Create() => new();
    }

    private sealed class InMemoryRiskManagementRepository : IRiskManagementRepository
    {
        public List<RiskCategory> Categories { get; } = [];
        public List<RiskMatrix> Matrices { get; } = [];
        public List<Risk> Risks { get; } = [];
        public List<AuditLog> AuditLogs { get; } = [];
        public Task AddCategoryAsync(RiskCategory category, CancellationToken cancellationToken = default) { Categories.Add(category); return Task.CompletedTask; }
        public Task<RiskCategory?> GetCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default) => Task.FromResult(Categories.SingleOrDefault(category => category.TenantId == tenantId && category.Id == categoryId));
        public Task<bool> CategoryCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) => Task.FromResult(Categories.Any(category => category.TenantId == tenantId && category.Code == code.ToUpperInvariant()));
        public Task AddMatrixAsync(RiskMatrix matrix, CancellationToken cancellationToken = default) { Matrices.Add(matrix); return Task.CompletedTask; }
        public Task<RiskMatrix?> GetMatrixAsync(Guid tenantId, Guid matrixId, CancellationToken cancellationToken = default) => Task.FromResult(Matrices.SingleOrDefault(matrix => matrix.TenantId == tenantId && matrix.Id == matrixId));
        public Task AddRiskAsync(Risk risk, CancellationToken cancellationToken = default) { Risks.Add(risk); return Task.CompletedTask; }
        public Task<Risk?> GetRiskAsync(Guid tenantId, Guid riskId, CancellationToken cancellationToken = default) => Task.FromResult(Risks.SingleOrDefault(risk => risk.TenantId == tenantId && risk.Id == riskId));
        public Task<bool> RiskCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) => Task.FromResult(Risks.Any(risk => risk.TenantId == tenantId && risk.Code == code.ToUpperInvariant()));
        public Task<RiskSearchResult> SearchAsync(RiskSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            var items = Risks.Where(risk => risk.TenantId == criteria.TenantId)
                .Where(risk => criteria.SearchText is null || risk.Title.Contains(criteria.SearchText) || risk.Code.Contains(criteria.SearchText))
                .Where(risk => !criteria.Status.HasValue || risk.Status == criteria.Status.Value)
                .Where(risk => !criteria.Type.HasValue || risk.Type == criteria.Type.Value)
                .Where(risk => !criteria.Level.HasValue || risk.ResidualLevel == criteria.Level.Value || risk.InherentLevel == criteria.Level.Value)
                .Where(risk => criteria.Area is null || risk.Area == criteria.Area)
                .Where(risk => !criteria.SupplierId.HasValue || risk.SupplierId == criteria.SupplierId.Value)
                .Where(risk => !criteria.AuditId.HasValue || risk.AuditId == criteria.AuditId.Value)
                .Where(risk => !criteria.CapaId.HasValue || risk.CapaId == criteria.CapaId.Value)
                .Select(risk => new RiskSummary(risk.Id, risk.TenantId, risk.Title, risk.Code, risk.Type, risk.Status, risk.InherentLevel, risk.ResidualLevel, risk.InherentScore, risk.ResidualScore, risk.Area, risk.Process, risk.SupplierId, risk.AuditId, risk.CapaId))
                .ToArray();
            return Task.FromResult(new RiskSearchResult(items, items.Length, criteria.Page, criteria.PageSize));
        }
        public Task<RiskDashboardDto> GetDashboardAsync(Guid tenantId, DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            var risks = Risks.Where(risk => risk.TenantId == tenantId).ToArray();
            return Task.FromResult(new RiskDashboardDto(
                risks.Count(risk => risk.ResidualLevel == RiskLevel.Critical || risk.InherentLevel == RiskLevel.Critical),
                risks.Count(risk => risk.ResidualLevel == RiskLevel.High || risk.InherentLevel == RiskLevel.High),
                risks.Count(risk => risk.ResidualLevel == RiskLevel.Medium || risk.InherentLevel == RiskLevel.Medium),
                risks.Count(risk => risk.ResidualLevel == RiskLevel.Low && risk.InherentLevel == RiskLevel.Low),
                risks.Count(risk => risk.IsOverdue(now)),
                risks.Select(risk => risk.Area).Distinct().Count(),
                risks.Count(risk => risk.SupplierId.HasValue),
                risks.Select(risk => risk.Process).Distinct().Count(),
                risks.Length,
                risks.Count(risk => risk.ResidualScore > 0)));
        }
        public Task<IReadOnlyCollection<RiskHeatMapPoint>> GetHeatMapAsync(Guid tenantId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<RiskHeatMapPoint>>(Risks.Where(risk => risk.TenantId == tenantId && risk.ResidualScore > 0).Select(risk => risk.HeatMapPoint()).ToArray());
        public Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default) { AuditLogs.Add(auditLog); return Task.CompletedTask; }
    }

    private sealed class FakeApplicationDbContext : IApplicationDbContext { public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1); }
    private sealed class FixedClock : IClock { public DateTimeOffset UtcNow { get; } = new(2026, 6, 20, 12, 0, 0, TimeSpan.Zero); }
}
