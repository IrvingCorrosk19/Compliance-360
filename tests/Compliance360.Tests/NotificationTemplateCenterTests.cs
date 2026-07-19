using Compliance360.Application.Notifications;
using Compliance360.Domain.Common;
using Compliance360.Domain.Notifications;
using Compliance360.Infrastructure.Notifications;

namespace Compliance360.Tests;

public sealed class NotificationTemplateCenterTests
{
    [Fact]
    public void Template_Version_Enforces_Maker_Checker_Lifecycle()
    {
        var maker = Guid.NewGuid();
        var reviewer = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 7, 19, 12, 0, 0, TimeSpan.Zero);
        var version = new NotificationTemplateVersion(
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            "es-PA",
            "Expediente {{Dossier.Code}}",
            "<p>Hola {{User.Name}}</p>",
            "Hola",
            "[\"Dossier.Code\",\"User.Name\"]",
            null,
            maker,
            now);

        version.SubmitForReview(now.AddMinutes(1));
        Assert.Throws<DomainException>(() => version.RecordReview(maker, now.AddMinutes(2)));
        version.RecordReview(reviewer, now.AddMinutes(2));
        Assert.Throws<DomainException>(() => version.Approve(maker, now.AddMinutes(3)));
        version.Approve(reviewer, now.AddMinutes(3));
        version.Publish(reviewer, now.AddMinutes(4));

        Assert.Equal(NotificationTemplateLifecycle.Published, version.Lifecycle);
        Assert.Equal(reviewer, version.ApprovedByUserId);
        Assert.NotNull(version.PublishedAtUtc);
    }

    [Fact]
    public void Content_Security_Removes_Active_Content_And_Extracts_Variables()
    {
        var security = new NotificationContentSecurity();

        var sanitized = security.SanitizeHtml(
            "<p>Hello {{User.Name}}</p><img src=\"https://safe.test/logo.png\" onerror=\"alert(1)\"><script>alert(2)</script>");
        var variables = security.ExtractVariables("Subject {{Dossier.Code}}", sanitized);

        Assert.DoesNotContain("<script", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onerror", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Dossier.Code", variables);
        Assert.Contains("User.Name", variables);
    }

    [Fact]
    public void Template_Engine_Encodes_Html_Variables_And_Blocks_Header_Newlines()
    {
        var engine = new NotificationTemplateEngine();

        var rendered = engine.Render(
            "Alert {{Name}}",
            "<p>{{Name}}</p>",
            "{{Name}}",
            new Dictionary<string, string> { ["Name"] = "<img src=x onerror=alert(1)>\r\nBcc: attacker@test" },
            null);

        Assert.DoesNotContain("<img", rendered.HtmlBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("&lt;img", rendered.HtmlBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\r", rendered.Subject);
        Assert.DoesNotContain("\n", rendered.Subject);
    }
}
