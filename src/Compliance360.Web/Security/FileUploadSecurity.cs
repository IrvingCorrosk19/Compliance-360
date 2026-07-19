using Microsoft.AspNetCore.Http;

namespace Compliance360.Web.Security;

public enum FileUploadProfile
{
    RegulatoryEvidence,
    GeneralDocument,
    RegutrackWorkbook
}

public sealed record FileUploadValidationResult(bool IsValid, string? Error)
{
    public static FileUploadValidationResult Success() => new(true, null);
    public static FileUploadValidationResult Failure(string error) => new(false, error);
}

public static class FileUploadSecurity
{
    public const long MaximumFileSizeBytes = 25L * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string[]> AllowedContentTypes =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = ["application/pdf"],
            [".png"] = ["image/png"],
            [".jpg"] = ["image/jpeg"],
            [".jpeg"] = ["image/jpeg"],
            [".doc"] = ["application/msword", "application/octet-stream"],
            [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/octet-stream"],
            [".xls"] = ["application/vnd.ms-excel", "application/octet-stream"],
            [".xlsx"] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "application/octet-stream"],
            [".txt"] = ["text/plain"],
            [".csv"] = ["text/csv", "application/csv", "text/plain"]
        };

    public static async Task<FileUploadValidationResult> ValidateAsync(
        IFormFile? file,
        FileUploadProfile profile,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return FileUploadValidationResult.Failure("A non-empty file is required.");
        }

        if (file.Length > MaximumFileSizeBytes)
        {
            return FileUploadValidationResult.Failure($"File size exceeds the {MaximumFileSizeBytes / 1024 / 1024} MB limit.");
        }

        var fileName = file.FileName.Trim();
        if (fileName.Length is 0 or > 200
            || fileName.Contains("..", StringComparison.Ordinal)
            || fileName.IndexOfAny(['/', '\\', '<', '>', ':', '"', '|', '?', '*']) >= 0
            || fileName.Any(char.IsControl))
        {
            return FileUploadValidationResult.Failure("File name contains unsafe characters or path segments.");
        }

        var extension = Path.GetExtension(fileName);
        var allowedExtensions = profile switch
        {
            FileUploadProfile.RegulatoryEvidence => new HashSet<string>(
                [".pdf", ".png", ".jpg", ".jpeg", ".doc", ".docx", ".xls", ".xlsx"],
                StringComparer.OrdinalIgnoreCase),
            FileUploadProfile.RegutrackWorkbook => new HashSet<string>([".xlsx"], StringComparer.OrdinalIgnoreCase),
            _ => new HashSet<string>(
                [".pdf", ".png", ".jpg", ".jpeg", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".csv"],
                StringComparer.OrdinalIgnoreCase)
        };

        if (!allowedExtensions.Contains(extension))
        {
            return FileUploadValidationResult.Failure("File extension is not permitted.");
        }

        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType.Split(';', 2)[0].Trim();
        if (!AllowedContentTypes.TryGetValue(extension, out var acceptedContentTypes)
            || !acceptedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            return FileUploadValidationResult.Failure("File content type does not match its extension.");
        }

        await using var stream = file.OpenReadStream();
        var signature = new byte[Math.Min(8, checked((int)file.Length))];
        var read = await stream.ReadAsync(signature, cancellationToken);
        if (!SignatureMatches(extension, signature.AsSpan(0, read)))
        {
            return FileUploadValidationResult.Failure("File signature does not match its declared format.");
        }

        return FileUploadValidationResult.Success();
    }

    private static bool SignatureMatches(string extension, ReadOnlySpan<byte> signature)
    {
        if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return signature.StartsWith("%PDF-"u8);
        }

        if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
        {
            return signature.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        }

        if (extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
        {
            return signature.Length >= 3 && signature[0] == 0xFF && signature[1] == 0xD8 && signature[2] == 0xFF;
        }

        if (extension.Equals(".docx", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return signature.Length >= 4 && signature[0] == 0x50 && signature[1] == 0x4B
                && signature[2] is 0x03 or 0x05 or 0x07
                && signature[3] is 0x04 or 0x06 or 0x08;
        }

        if (extension.Equals(".doc", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".xls", StringComparison.OrdinalIgnoreCase))
        {
            return signature.StartsWith(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 });
        }

        return true;
    }
}
