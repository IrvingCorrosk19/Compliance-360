using System.Globalization;
using System.Text.Json;
using ClosedXML.Excel;
using Compliance360.Application.RegulatoryAffairs;

namespace Compliance360.Infrastructure.RegulatoryAffairs;

public sealed class ClosedXmlRegutrackWorkbookParser : IRegutrackWorkbookParser
{
    private static readonly string[] PreferredSheets =
    [
        "CTT REGISTROS (2)",
        "CTT REGISTROS",
        "CTT REGISTROS TUBERIA"
    ];

    public string ParseToRowsJson(byte[] fileBytes)
    {
        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);
        var rows = new List<Dictionary<string, object?>>();

        foreach (var sheetName in PreferredSheets)
        {
            if (!workbook.Worksheets.TryGetWorksheet(sheetName, out var ws))
            {
                continue;
            }

            rows.AddRange(ParseRegistrationsSheet(ws, sheetName));
        }

        if (workbook.Worksheets.TryGetWorksheet("DOCUMENTACION", out var docs))
        {
            rows.AddRange(ParseDocumentationSheet(docs));
        }

        if (workbook.Worksheets.TryGetWorksheet("CTT LICENCIAS OP", out var lic))
        {
            rows.AddRange(ParseLicensesSheet(lic));
        }

        if (rows.Count == 0)
        {
            throw new InvalidOperationException("No REGUTRACK registration rows found in workbook.");
        }

        return JsonSerializer.Serialize(rows);
    }

    private static IEnumerable<Dictionary<string, object?>> ParseRegistrationsSheet(IXLWorksheet ws, string sheetName)
    {
        var headerRow = DetectHeaderRow(ws);
        var map = BuildHeaderMap(ws, headerRow);
        var last = ws.LastRowUsed()?.RowNumber() ?? headerRow;
        for (var r = headerRow + 1; r <= last; r++)
        {
            var name = Cell(ws, r, map, "nombre del producto", "producto");
            if (string.IsNullOrWhiteSpace(name) || name.Contains("Nombre del Producto", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            yield return new Dictionary<string, object?>
            {
                ["sheet"] = sheetName,
                ["row"] = r,
                ["regulatoryName"] = name,
                ["catalogCode"] = Cell(ws, r, map, "catálogo", "catalogo", "código de producto", "codigo"),
                ["brand"] = Cell(ws, r, map, "marca"),
                ["category"] = Cell(ws, r, map, "categoria", "categoría"),
                ["description"] = Cell(ws, r, map, "descripcion", "descripción"),
                ["manufacturerName"] = Cell(ws, r, map, "fabricante"),
                ["manufacturerCountry"] = ExtractCountry(Cell(ws, r, map, "fabricante")),
                ["distributorName"] = Cell(ws, r, map, "distribuidor", "importador"),
                ["initiative"] = Cell(ws, r, map, "iniciativa"),
                ["authorityCode"] = Cell(ws, r, map, "entidad emisora", "entidad"),
                ["registrationNumber"] = Cell(ws, r, map, "criterio técnico", "registro sanitario", "ct"),
                ["riskClass"] = NormalizeRisk(Cell(ws, r, map, "clase de riesgo", "riesgo")),
                ["processType"] = Cell(ws, r, map, "tipo de proceso"),
                ["opportunityAmount"] = ParseDecimal(Cell(ws, r, map, "oportunidad")),
                ["priority"] = Cell(ws, r, map, "prioridad"),
                ["salesMarketingInput"] = Cell(ws, r, map, "sales", "mkt"),
                ["comments"] = Cell(ws, r, map, "comentarios"),
                ["issuedOn"] = ParseDate(Cell(ws, r, map, "fecha criterio", "fecha registro")),
                ["expiresOn"] = ParseDate(Cell(ws, r, map, "fecha de vencimiento", "vencimiento")),
                ["registeredSuppliersCount"] = ParseInt(Cell(ws, r, map, "proveedores registrados")),
                ["sourceLineNumber"] = ParseInt(Cell(ws, r, map, "linea", "línea")) ?? r,
                ["technicalSheetReference"] = Cell(ws, r, map, "ficha tecnica", "ficha técnica"),
                ["formReference"] = Cell(ws, r, map, "formulario"),
                ["estimatedReceptionOn"] = ParseDate(Cell(ws, r, map, "fecha estimada de recepción", "recepcion de documentos")),
                ["maximumReceptionOn"] = ParseDate(Cell(ws, r, map, "fecha máxima", "maxima para recepcion")),
                ["assembledOn"] = ParseDate(Cell(ws, r, map, "fecha de armado")),
                ["estimatedSubmissionOn"] = ParseDate(Cell(ws, r, map, "fecha estimada de sometimiento")),
                ["submittedOn"] = ParseDate(Cell(ws, r, map, "fecha de sometimiento")),
                ["approvedOn"] = ParseDate(Cell(ws, r, map, "fecha de la aprobación", "fecha de aprobacion")),
                ["countryCode"] = "PA"
            };
        }
    }

    private static IEnumerable<Dictionary<string, object?>> ParseDocumentationSheet(IXLWorksheet ws)
    {
        var headerRow = DetectHeaderRow(ws);
        var map = BuildHeaderMap(ws, headerRow);
        var last = ws.LastRowUsed()?.RowNumber() ?? headerRow;
        for (var r = headerRow + 1; r <= last; r++)
        {
            var mfr = Cell(ws, r, map, "fabricante");
            var doc = Cell(ws, r, map, "documento");
            if (string.IsNullOrWhiteSpace(mfr) || string.IsNullOrWhiteSpace(doc))
            {
                continue;
            }

            yield return new Dictionary<string, object?>
            {
                ["sheet"] = "DOCUMENTACION",
                ["row"] = r,
                ["recordType"] = "ManufacturerCertificate",
                ["manufacturerName"] = mfr,
                ["manufacturerCountry"] = Cell(ws, r, map, "pais", "país") ?? "XX",
                ["certificateType"] = doc,
                ["expiresOn"] = ParseDate(Cell(ws, r, map, "vencimiento")),
                ["requestedOn"] = ParseDate(Cell(ws, r, map, "fecha cierta de solicitud", "solicitud")),
                ["legalFormat"] = Cell(ws, r, map, "formato"),
                ["status"] = Cell(ws, r, map, "estatus"),
                ["comments"] = Cell(ws, r, map, "comentarios", "seguimiento"),
                // placeholder so staging validator accepts when mixed with product rows
                ["regulatoryName"] = $"[CERT] {mfr} · {doc}"
            };
        }
    }

    private static IEnumerable<Dictionary<string, object?>> ParseLicensesSheet(IXLWorksheet ws)
    {
        // Rows 1–3: company header (col B=Multimed, col C=4 Hospitals) with constitution / ops-start.
        // Row 5: license table headers. Rows 6+: license cases.
        var companyDates = new Dictionary<string, (DateOnly? Constituted, DateOnly? OpsStart)>(StringComparer.OrdinalIgnoreCase);
        for (var col = 2; col <= 10; col++)
        {
            var company = ws.Cell(1, col).GetFormattedString()?.Trim();
            if (string.IsNullOrWhiteSpace(company))
            {
                continue;
            }

            companyDates[company.Trim()] = (
                ParseDateOnlyCell(ws.Cell(2, col)),
                ParseDateOnlyCell(ws.Cell(3, col)));
        }

        const int headerRow = 5;
        var map = BuildHeaderMap(ws, headerRow);
        var last = ws.LastRowUsed()?.RowNumber() ?? headerRow;
        for (var r = headerRow + 1; r <= last; r++)
        {
            var docType = Cell(ws, r, map, "documento");
            var company = Cell(ws, r, map, "compañia", "compania", "compañía");
            if (string.IsNullOrWhiteSpace(docType) || string.IsNullOrWhiteSpace(company))
            {
                continue;
            }

            DateOnly? constituted = null;
            DateOnly? opsStart = null;
            foreach (var kv in companyDates)
            {
                if (company.Contains(kv.Key, StringComparison.OrdinalIgnoreCase) ||
                    kv.Key.Contains(company, StringComparison.OrdinalIgnoreCase) ||
                    FuzzyCompanyMatch(company, kv.Key))
                {
                    constituted = kv.Value.Constituted;
                    opsStart = kv.Value.OpsStart;
                    break;
                }
            }

            yield return new Dictionary<string, object?>
            {
                ["sheet"] = "CTT LICENCIAS OP",
                ["row"] = r,
                ["recordType"] = "OperatingLicense",
                ["licenseType"] = docType,
                ["companyName"] = company,
                ["companyConstitutedOn"] = constituted?.ToString("yyyy-MM-dd"),
                ["operationsStartedOn"] = opsStart?.ToString("yyyy-MM-dd"),
                ["expiresOn"] = ParseDate(Cell(ws, r, map, "expiración", "expiracion", "feha de expiración")),
                ["assembledOn"] = ParseDate(Cell(ws, r, map, "armado")),
                ["estimatedSubmissionOn"] = ParseDate(Cell(ws, r, map, "estimada de sometimiento")),
                ["submittedOn"] = ParseDate(Cell(ws, r, map, "fecha de sometimiento")),
                ["approvedOn"] = ParseDate(Cell(ws, r, map, "aprobac")),
                ["comments"] = Cell(ws, r, map, "comentarios"),
                ["status"] = Cell(ws, r, map, "estatus"),
                ["regulatoryName"] = $"[LIC] {company} · {docType}"
            };
        }
    }

    private static bool FuzzyCompanyMatch(string a, string b)
    {
        static string Norm(string s) => s.ToLowerInvariant().Replace(" ", "").Replace("hospitals", "hospital").Replace("hospital", "hospital");
        return Norm(a).Contains(Norm(b)) || Norm(b).Contains(Norm(a));
    }

    private static DateOnly? ParseDateOnlyCell(IXLCell cell)
    {
        if (cell.TryGetValue(out DateTime dt))
        {
            return DateOnly.FromDateTime(dt);
        }

        var formatted = cell.GetFormattedString()?.Trim();
        if (string.IsNullOrWhiteSpace(formatted))
        {
            return null;
        }

        if (DateOnly.TryParse(formatted, out var d))
        {
            return d;
        }

        return DateTime.TryParse(formatted, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto)
            ? DateOnly.FromDateTime(dto)
            : null;
    }

    private static int DetectHeaderRow(IXLWorksheet ws)
    {
        for (var r = 1; r <= Math.Min(8, ws.LastRowUsed()?.RowNumber() ?? 1); r++)
        {
            var filled = ws.Row(r).CellsUsed().Count();
            if (filled >= 3)
            {
                return r;
            }
        }

        return 1;
    }

    private static Dictionary<string, int> BuildHeaderMap(IXLWorksheet ws, int headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in ws.Row(headerRow).CellsUsed())
        {
            var key = NormalizeHeader(cell.GetString());
            if (!string.IsNullOrWhiteSpace(key) && !map.ContainsKey(key))
            {
                map[key] = cell.Address.ColumnNumber;
            }
        }

        return map;
    }

    private static string? Cell(IXLWorksheet ws, int row, Dictionary<string, int> map, params string[] hints)
    {
        foreach (var hint in hints)
        {
            foreach (var kv in map)
            {
                if (kv.Key.Contains(NormalizeHeader(hint), StringComparison.OrdinalIgnoreCase))
                {
                    var value = ws.Cell(row, kv.Value).GetFormattedString()?.Trim();
                    return string.IsNullOrWhiteSpace(value) ? null : value;
                }
            }
        }

        return null;
    }

    private static string NormalizeHeader(string? value) =>
        (value ?? string.Empty).Trim().ToLowerInvariant()
            .Replace('á', 'a').Replace('é', 'e').Replace('í', 'i').Replace('ó', 'o').Replace('ú', 'u').Replace('ñ', 'n');

    private static string? ExtractCountry(string? manufacturerAndCountry)
    {
        if (string.IsNullOrWhiteSpace(manufacturerAndCountry))
        {
            return null;
        }

        var parts = manufacturerAndCountry.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length > 1 ? parts[^1] : null;
    }

    private static string NormalizeRisk(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Unknown";
        }

        value = value.Trim().ToUpperInvariant();
        return value is "A" or "B" or "C" ? value : "Unknown";
    }

    private static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var cleaned = value.Replace("$", "").Replace(",", "").Trim();
        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var n) ? n : null;
    }

    private static int? ParseInt(string? value) =>
        int.TryParse(value?.Trim(), out var n) ? n : null;

    private static string? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)
            ? dt.ToUniversalTime().ToString("o")
            : null;
    }
}
