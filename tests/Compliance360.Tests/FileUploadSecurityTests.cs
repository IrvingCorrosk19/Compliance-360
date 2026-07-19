using Compliance360.Web.Security;
using Microsoft.AspNetCore.Http;

namespace Compliance360.Tests;

public sealed class FileUploadSecurityTests
{
    [Fact]
    public async Task Regulatory_evidence_accepts_valid_pdf()
    {
        var file = CreateFile("%PDF-1.7\nvalid"u8.ToArray(), "evidence.pdf", "application/pdf");

        var result = await FileUploadSecurity.ValidateAsync(file, FileUploadProfile.RegulatoryEvidence);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("payload.exe", "application/octet-stream")]
    [InlineData("payload.svg", "image/svg+xml")]
    [InlineData("payload.html", "text/html")]
    public async Task Regulatory_evidence_rejects_disallowed_extensions(string fileName, string contentType)
    {
        var file = CreateFile("malicious"u8.ToArray(), fileName, contentType);

        var result = await FileUploadSecurity.ValidateAsync(file, FileUploadProfile.RegulatoryEvidence);

        Assert.False(result.IsValid);
        Assert.Contains("extension", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Regulatory_evidence_rejects_mime_spoofing()
    {
        var file = CreateFile("%PDF-1.7"u8.ToArray(), "evidence.pdf", "image/png");

        var result = await FileUploadSecurity.ValidateAsync(file, FileUploadProfile.RegulatoryEvidence);

        Assert.False(result.IsValid);
        Assert.Contains("content type", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Regulatory_evidence_rejects_signature_spoofing()
    {
        var file = CreateFile("MZ executable"u8.ToArray(), "evidence.pdf", "application/pdf");

        var result = await FileUploadSecurity.ValidateAsync(file, FileUploadProfile.RegulatoryEvidence);

        Assert.False(result.IsValid);
        Assert.Contains("signature", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("../evidence.pdf")]
    [InlineData("..\\evidence.pdf")]
    [InlineData("folder/evidence.pdf")]
    [InlineData("folder\\evidence.pdf")]
    public async Task Upload_rejects_path_traversal_and_path_segments(string fileName)
    {
        var file = CreateFile("%PDF-1.7"u8.ToArray(), fileName, "application/pdf");

        var result = await FileUploadSecurity.ValidateAsync(file, FileUploadProfile.RegulatoryEvidence);

        Assert.False(result.IsValid);
        Assert.Contains("unsafe", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Upload_rejects_declared_file_over_size_limit()
    {
        var file = CreateFile(
            "%PDF-1.7"u8.ToArray(),
            "large.pdf",
            "application/pdf",
            FileUploadSecurity.MaximumFileSizeBytes + 1);

        var result = await FileUploadSecurity.ValidateAsync(file, FileUploadProfile.RegulatoryEvidence);

        Assert.False(result.IsValid);
        Assert.Contains("limit", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Regutrack_profile_only_accepts_valid_xlsx_container()
    {
        var workbook = CreateFile([0x50, 0x4B, 0x03, 0x04, 0, 0, 0, 0], "REGUTRACK.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        var pdf = CreateFile("%PDF-1.7"u8.ToArray(), "REGUTRACK.pdf", "application/pdf");

        var accepted = await FileUploadSecurity.ValidateAsync(workbook, FileUploadProfile.RegutrackWorkbook);
        var rejected = await FileUploadSecurity.ValidateAsync(pdf, FileUploadProfile.RegutrackWorkbook);

        Assert.True(accepted.IsValid);
        Assert.False(rejected.IsValid);
    }

    private static FormFile CreateFile(byte[] bytes, string fileName, string contentType, long? declaredLength = null)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, declaredLength ?? bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
