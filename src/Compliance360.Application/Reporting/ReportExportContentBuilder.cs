using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Compliance360.Domain.Reporting;

namespace Compliance360.Application.Reporting;

internal static class ReportExportContentBuilder
{
    private static readonly XNamespace SpreadsheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace PackageRelsNs = "http://schemas.openxmlformats.org/package/2006/relationships";
    private static readonly XNamespace OfficeRelsNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace ContentTypesNs = "http://schemas.openxmlformats.org/package/2006/content-types";

    public static ReportExportContent Build(
        ReportDefinition definition,
        ReportExecution execution,
        ReportOutput? output,
        ReportExport export)
    {
        return export.Format switch
        {
            ReportFormat.Csv => BuildCsv(definition, execution, output, export),
            ReportFormat.Json => BuildJson(definition, execution, output, export),
            ReportFormat.Excel => BuildExcelOpenXml(definition, execution, output, export),
            ReportFormat.Pdf => BuildPdf(definition, execution, output, export),
            _ => BuildCsv(definition, execution, output, export)
        };
    }

    private static ReportExportContent BuildCsv(ReportDefinition definition, ReportExecution execution, ReportOutput? output, ReportExport export)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Field,Value");
        foreach (var (field, value) in BuildRows(definition, execution, output, export))
        {
            sb.AppendLine(Csv(field, value));
        }

        return new ReportExportContent(EnsureExtension(export.FileName, ".csv"), "text/csv; charset=utf-8", Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private static ReportExportContent BuildJson(ReportDefinition definition, ReportExecution execution, ReportOutput? output, ReportExport export)
    {
        object? dataset = null;
        if (!string.IsNullOrWhiteSpace(output?.DatasetDescriptorJson))
        {
            try { dataset = JsonSerializer.Deserialize<JsonElement>(output.DatasetDescriptorJson); }
            catch { dataset = output.DatasetDescriptorJson; }
        }

        var payload = new
        {
            report = new { definition.Id, definition.Code, definition.Name, module = definition.Module.ToString(), definition.DatasetKey, definition.TenantId },
            execution = new { execution.Id, execution.Status, rowCount = output?.RowCount ?? execution.RowCount, execution.QueuedAtUtc, execution.CompletedAtUtc },
            export = new { export.Id, format = export.Format.ToString(), export.FileName, export.ExportedAtUtc },
            dataset
        };
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        return new ReportExportContent(EnsureExtension(export.FileName, ".json"), "application/json; charset=utf-8", Encoding.UTF8.GetBytes(json));
    }

    /// <summary>
    /// Builds a standards-compliant OOXML (.xlsx) workbook (ZIP + spreadsheetml parts).
    /// </summary>
    private static ReportExportContent BuildExcelOpenXml(
        ReportDefinition definition,
        ReportExecution execution,
        ReportOutput? output,
        ReportExport export)
    {
        var rows = BuildRows(definition, execution, output, export).ToList();
        var shared = new List<string> { "Field", "Value" };
        foreach (var (field, value) in rows)
        {
            shared.Add(field);
            shared.Add(value);
        }

        // Deduplicate while preserving first index for SST references.
        var unique = new List<string>();
        var index = new Dictionary<string, int>(StringComparer.Ordinal);
        int Sst(string text)
        {
            if (index.TryGetValue(text, out var existing))
            {
                return existing;
            }

            var i = unique.Count;
            unique.Add(text);
            index[text] = i;
            return i;
        }

        var fieldHeader = Sst("Field");
        var valueHeader = Sst("Value");
        var sheetRows = new XElement(SpreadsheetNs + "sheetData",
            new XElement(SpreadsheetNs + "row",
                new XAttribute("r", 1),
                Cell("A1", fieldHeader),
                Cell("B1", valueHeader)));

        var rowNumber = 2;
        foreach (var (field, value) in rows)
        {
            sheetRows.Add(new XElement(SpreadsheetNs + "row",
                new XAttribute("r", rowNumber),
                Cell($"A{rowNumber}", Sst(field)),
                Cell($"B{rowNumber}", Sst(value))));
            rowNumber++;
        }

        var worksheet = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement(SpreadsheetNs + "worksheet",
                new XAttribute(XNamespace.Xmlns + "r", OfficeRelsNs.NamespaceName),
                sheetRows));

        var sharedStrings = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement(SpreadsheetNs + "sst",
                new XAttribute("count", unique.Count),
                new XAttribute("uniqueCount", unique.Count),
                unique.Select(item =>
                    new XElement(SpreadsheetNs + "si",
                        new XElement(SpreadsheetNs + "t",
                            new XAttribute(XNamespace.Xml + "space", "preserve"),
                            item)))));

        var workbook = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement(SpreadsheetNs + "workbook",
                new XAttribute(XNamespace.Xmlns + "r", OfficeRelsNs.NamespaceName),
                new XElement(SpreadsheetNs + "sheets",
                    new XElement(SpreadsheetNs + "sheet",
                        new XAttribute("name", "Report"),
                        new XAttribute("sheetId", "1"),
                        new XAttribute(OfficeRelsNs + "id", "rId1")))));

        var workbookRels = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement(PackageRelsNs + "Relationships",
                new XElement(PackageRelsNs + "Relationship",
                    new XAttribute("Id", "rId1"),
                    new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"),
                    new XAttribute("Target", "worksheets/sheet1.xml")),
                new XElement(PackageRelsNs + "Relationship",
                    new XAttribute("Id", "rId2"),
                    new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings"),
                    new XAttribute("Target", "sharedStrings.xml")),
                new XElement(PackageRelsNs + "Relationship",
                    new XAttribute("Id", "rId3"),
                    new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"),
                    new XAttribute("Target", "styles.xml"))));

        var packageRels = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement(PackageRelsNs + "Relationships",
                new XElement(PackageRelsNs + "Relationship",
                    new XAttribute("Id", "rId1"),
                    new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                    new XAttribute("Target", "xl/workbook.xml"))));

        var contentTypes = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement(ContentTypesNs + "Types",
                new XElement(ContentTypesNs + "Default",
                    new XAttribute("Extension", "rels"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                new XElement(ContentTypesNs + "Default",
                    new XAttribute("Extension", "xml"),
                    new XAttribute("ContentType", "application/xml")),
                new XElement(ContentTypesNs + "Override",
                    new XAttribute("PartName", "/xl/workbook.xml"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")),
                new XElement(ContentTypesNs + "Override",
                    new XAttribute("PartName", "/xl/worksheets/sheet1.xml"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml")),
                new XElement(ContentTypesNs + "Override",
                    new XAttribute("PartName", "/xl/sharedStrings.xml"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml")),
                new XElement(ContentTypesNs + "Override",
                    new XAttribute("PartName", "/xl/styles.xml"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"))));

        var styles = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement(SpreadsheetNs + "styleSheet",
                new XElement(SpreadsheetNs + "fonts", new XAttribute("count", 1),
                    new XElement(SpreadsheetNs + "font", new XElement(SpreadsheetNs + "sz", new XAttribute("val", "11")), new XElement(SpreadsheetNs + "name", new XAttribute("val", "Calibri")))),
                new XElement(SpreadsheetNs + "fills", new XAttribute("count", 1),
                    new XElement(SpreadsheetNs + "fill", new XElement(SpreadsheetNs + "patternFill", new XAttribute("patternType", "none")))),
                new XElement(SpreadsheetNs + "borders", new XAttribute("count", 1),
                    new XElement(SpreadsheetNs + "border")),
                new XElement(SpreadsheetNs + "cellStyleXfs", new XAttribute("count", 1),
                    new XElement(SpreadsheetNs + "xf")),
                new XElement(SpreadsheetNs + "cellXfs", new XAttribute("count", 1),
                    new XElement(SpreadsheetNs + "xf"))));

        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteXml(zip, "[Content_Types].xml", contentTypes);
            WriteXml(zip, "_rels/.rels", packageRels);
            WriteXml(zip, "xl/workbook.xml", workbook);
            WriteXml(zip, "xl/_rels/workbook.xml.rels", workbookRels);
            WriteXml(zip, "xl/worksheets/sheet1.xml", worksheet);
            WriteXml(zip, "xl/sharedStrings.xml", sharedStrings);
            WriteXml(zip, "xl/styles.xml", styles);
        }

        return new ReportExportContent(
            EnsureExtension(export.FileName, ".xlsx"),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            stream.ToArray());

        static XElement Cell(string reference, int sstIndex) =>
            new(SpreadsheetNs + "c",
                new XAttribute("r", reference),
                new XAttribute("t", "s"),
                new XElement(SpreadsheetNs + "v", sstIndex));
    }

    private static ReportExportContent BuildPdf(ReportDefinition definition, ReportExecution execution, ReportOutput? output, ReportExport export)
    {
        var lines = new[]
        {
            "Compliance 360 Report Export",
            $"Report: {definition.Code} — {definition.Name}",
            $"Module: {definition.Module}",
            $"Dataset: {definition.DatasetKey}",
            $"Execution: {execution.Id:D}",
            $"Rows: {output?.RowCount ?? execution.RowCount}",
            $"Exported: {export.ExportedAtUtc:O}",
            $"Tenant: {definition.TenantId:D}"
        };
        var contentStream = string.Join('\n', lines.Select((line, index) => $"BT /F1 11 Tf 50 {750 - (index * 18)} Td ({EscapePdf(line)}) Tj ET"));
        var pdf = $"""
            %PDF-1.4
            1 0 obj<< /Type /Catalog /Pages 2 0 R >>endobj
            2 0 obj<< /Type /Pages /Kids [3 0 R] /Count 1 >>endobj
            3 0 obj<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources<< /Font<< /F1 5 0 R >> >> >>endobj
            4 0 obj<< /Length {Encoding.ASCII.GetByteCount(contentStream)} >>stream
            {contentStream}
            endstream
            endobj
            5 0 obj<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>endobj
            xref
            0 6
            0000000000 65535 f 
            trailer<< /Size 6 /Root 1 0 R >>
            startxref
            0
            %%EOF
            """;
        return new ReportExportContent(EnsureExtension(export.FileName, ".pdf"), "application/pdf", Encoding.ASCII.GetBytes(pdf));
    }

    private static IEnumerable<(string Field, string Value)> BuildRows(
        ReportDefinition definition,
        ReportExecution execution,
        ReportOutput? output,
        ReportExport export)
    {
        yield return ("ReportCode", definition.Code);
        yield return ("ReportName", definition.Name);
        yield return ("Module", definition.Module.ToString());
        yield return ("DatasetKey", definition.DatasetKey);
        yield return ("ExecutionId", execution.Id.ToString("D"));
        yield return ("RowCount", (output?.RowCount ?? execution.RowCount).ToString(CultureInfo.InvariantCulture));
        yield return ("ExportedAtUtc", export.ExportedAtUtc.ToString("O", CultureInfo.InvariantCulture));
        yield return ("TenantId", definition.TenantId.ToString("D"));
        if (!string.IsNullOrWhiteSpace(output?.DatasetDescriptorJson))
        {
            yield return ("DatasetDescriptorJson", output.DatasetDescriptorJson);
        }
    }

    private static void WriteXml(ZipArchive zip, string entryName, XDocument document)
    {
        var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        document.Save(writer, SaveOptions.DisableFormatting);
    }

    private static string Csv(string field, string value) =>
        $"{Quote(field)},{Quote(value)}";

    private static string Quote(string value)
    {
        var escaped = (value ?? string.Empty).Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }

    private static string EscapePdf(string value) =>
        (value ?? string.Empty).Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    private static string EnsureExtension(string fileName, string extension)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return $"report{extension}";
        }

        return fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
            ? fileName
            : Path.ChangeExtension(fileName, extension.TrimStart('.'));
    }
}
