using System.Text.RegularExpressions;
using Compliance360.Application.Notifications;
using Ganss.Xss;

namespace Compliance360.Infrastructure.Notifications;

public sealed class NotificationContentSecurity : INotificationContentSecurity
{
    private static readonly Regex VariablePattern = new(
        @"\{\{\s*([A-Za-z][A-Za-z0-9_.]{0,119})\s*\}\}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(250));

    private readonly HtmlSanitizer _sanitizer;

    public NotificationContentSecurity()
    {
        _sanitizer = new HtmlSanitizer();
        _sanitizer.AllowedTags.Add("table");
        _sanitizer.AllowedTags.Add("thead");
        _sanitizer.AllowedTags.Add("tbody");
        _sanitizer.AllowedTags.Add("tr");
        _sanitizer.AllowedTags.Add("th");
        _sanitizer.AllowedTags.Add("td");
        _sanitizer.AllowedAttributes.Add("class");
        _sanitizer.AllowedAttributes.Add("role");
        _sanitizer.AllowedAttributes.Add("aria-label");
        _sanitizer.AllowedSchemes.Remove("data");
    }

    public string SanitizeHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            throw new ArgumentException("Template HTML is required.", nameof(html));
        }

        return _sanitizer.Sanitize(html);
    }

    public IReadOnlyCollection<string> ExtractVariables(params string?[] values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .SelectMany(value => VariablePattern.Matches(value!).Select(match => match.Groups[1].Value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
