using System.IO.Compression;
using ClosedXML.Excel;
using Compliance360.Application.Reporting;
using Compliance360.Domain.Reporting;

namespace Compliance360.Tests;

public sealed class ReportExcelOpenXmlTests
{
    [Fact]
    public void Excel_Export_Is_Valid_Ooxml_Workbook_Readable_By_ClosedXml()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var category = new ReportCategory(tenantId, "Docs", "DOC", ReportModule.DocumentManagement, userId);
        var definition = new ReportDefinition(
            tenantId,
            category.Id,
            "Active documents",
            "DOC-ACTIVE",
            "desc",
            ReportModule.DocumentManagement,
            "documents.active",
            userId,
            now);
        definition.AddTemplate("XLSX", ReportFormat.Excel, "template", userId, now);
        definition.GrantPermission(ReportPermissionScope.Permission, "REPORT.EXECUTE", true, true, true, userId, now);
        definition.Activate(userId, now);
        var execution = definition.StartExecution("{}", userId, now);
        definition.CompleteExecution(execution.Id, 3, """{"dataset":"documents.active","note":"áéíóú 中文"}""", userId, now);
        var export = definition.Export(execution.Id, ReportFormat.Excel, userId, now);
        var output = execution.Outputs.First();

        var content = ReportExportContentBuilder.Build(definition, execution, output, export);

        Assert.Equal(".xlsx", Path.GetExtension(content.FileName));
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", content.ContentType);
        Assert.True(content.Content.Length > 100);
        Assert.Equal((byte)'P', content.Content[0]);
        Assert.Equal((byte)'K', content.Content[1]);

        using (var zip = new ZipArchive(new MemoryStream(content.Content), ZipArchiveMode.Read))
        {
            Assert.Contains(zip.Entries, e => e.FullName == "[Content_Types].xml");
            Assert.Contains(zip.Entries, e => e.FullName == "xl/workbook.xml");
            Assert.Contains(zip.Entries, e => e.FullName == "xl/worksheets/sheet1.xml");
            Assert.Contains(zip.Entries, e => e.FullName == "xl/sharedStrings.xml");
            Assert.Contains(zip.Entries, e => e.FullName == "xl/_rels/workbook.xml.rels");
        }

        using var workbook = new XLWorkbook(new MemoryStream(content.Content));
        var sheet = workbook.Worksheet(1);
        Assert.Equal("Report", sheet.Name);
        Assert.Equal("Field", sheet.Cell(1, 1).GetString());
        Assert.Equal("Value", sheet.Cell(1, 2).GetString());
        Assert.Equal("ReportCode", sheet.Cell(2, 1).GetString());
        Assert.Equal("DOC-ACTIVE", sheet.Cell(2, 2).GetString());
        Assert.Contains(sheet.RowsUsed(), row => row.Cell(2).GetString().Contains("áéíóú", StringComparison.Ordinal));
        Assert.Contains(sheet.RowsUsed(), row => row.Cell(2).GetString().Contains(tenantId.ToString("D"), StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("")]
    [InlineData("simple")]
    [InlineData("Unicode 日本語 한국어 مرحبا")]
    [InlineData("quotes \" and <xml>&")]
    public void Excel_Export_Handles_Special_Characters(string marker)
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var category = new ReportCategory(tenantId, "Docs", "DOC", ReportModule.DocumentManagement, userId);
        var definition = new ReportDefinition(
            tenantId, category.Id, "Name", "CODE-X", "d", ReportModule.DocumentManagement, "ds", userId, now);
        definition.AddTemplate("XLSX", ReportFormat.Excel, "template", userId, now);
        definition.GrantPermission(ReportPermissionScope.Permission, "REPORT.EXECUTE", true, true, true, userId, now);
        definition.Activate(userId, now);
        var execution = definition.StartExecution("{}", userId, now);
        var descriptor = string.IsNullOrEmpty(marker)
            ? "{}"
            : $"{{\"marker\":\"{marker.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"}}";
        definition.CompleteExecution(execution.Id, 1, descriptor, userId, now);
        var export = definition.Export(execution.Id, ReportFormat.Excel, userId, now);
        var content = ReportExportContentBuilder.Build(definition, execution, execution.Outputs.First(), export);
        using var workbook = new XLWorkbook(new MemoryStream(content.Content));
        Assert.NotNull(workbook.Worksheet(1));
        Assert.Equal((byte)'P', content.Content[0]);
        Assert.Equal((byte)'K', content.Content[1]);
    }
}
