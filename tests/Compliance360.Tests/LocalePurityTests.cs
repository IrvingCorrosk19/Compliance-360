using System.Text.Json;
using System.Text.RegularExpressions;

namespace Compliance360.Tests;

public sealed class LocalePurityTests
{
    private static readonly Regex SpanishMarker = new(
        @"\b(correctamente|creado|creada|ejecutado|programado|guardando|procesando|identificando|validando|estandar|expediente|organizaci[oó]n)\b|[áéíóúñ¿¡]",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    [Fact]
    public void Locale_Files_Have_Equal_Key_Sets()
    {
        var (en, es) = Load();
        Assert.Empty(en.Keys.Except(es.Keys));
        Assert.Empty(es.Keys.Except(en.Keys));
    }

    [Fact]
    public void English_Locale_Has_No_Spanish_Runtime_Markers_On_Dashboard_Login_Common_Keys()
    {
        var (en, _) = Load();
        var offenders = en
            .Where(pair =>
                pair.Key.StartsWith("Dashboard.", StringComparison.Ordinal)
                || pair.Key.StartsWith("Login.", StringComparison.Ordinal)
                || pair.Key.StartsWith("Common.", StringComparison.Ordinal)
                || pair.Key.StartsWith("AlertCenter.", StringComparison.Ordinal)
                || pair.Key.StartsWith("Users.Email", StringComparison.Ordinal))
            .Where(pair => SpanishMarker.IsMatch(pair.Value))
            .Select(pair => $"{pair.Key}={pair.Value}")
            .ToArray();

        Assert.True(offenders.Length == 0, "Spanish markers in EN locale:\n" + string.Join("\n", offenders));
    }

    private static (Dictionary<string, string> En, Dictionary<string, string> Es) Load()
    {
        var root = FindWebRoot();
        var en = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(root, "locales", "en.json")))!;
        var es = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(root, "locales", "es.json")))!;
        return (en, es);
    }

    private static string FindWebRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "Compliance360.Web", "wwwroot");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("wwwroot not found.");
    }
}
