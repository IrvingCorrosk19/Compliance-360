namespace Compliance360.Application.RegulatoryAffairs;

public interface IRegutrackWorkbookParser
{
    /// <summary>Parses REGUTRACK workbook sheets into normalized import rows JSON.</summary>
    string ParseToRowsJson(byte[] fileBytes);
}
