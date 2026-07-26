# REM-XLSX-OOXML — CLOSED — VERIFIED

## Root cause
Exports used SpreadsheetML-as-.xls semantics; residual required real OOXML `.xlsx`.

## Fix
`ReportExportContentBuilder.BuildExcelOpenXml` emits ZIP + Content_Types + workbook + sheet1 + sharedStrings + rels; MIME `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`.

## Validation
- Unit ClosedXML parse + unicode/special chars (`ReportExcelOpenXmlTests`)
- Live download PK ZIP parts in final harness `XLSX-OOXML`
- Remediation RPT-CONTENT-EXCEL updated to OOXML expectations

## Final status
**CLOSED — VERIFIED**
