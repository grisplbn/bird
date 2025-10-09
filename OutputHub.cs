using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace Client.Acceptance.Tests.Infrastructure.Logging;

internal static class OutputHub
{
    // ===== PRZEŁĄCZNIKI =====
    private static volatile bool _globalEnabled = true;         // domyślnie ON (zmień na false jeśli wolisz)
    private static readonly AsyncLocal<bool?> _perTestEnabled = new();

    public static void SetGlobal(bool enabled) => _globalEnabled = enabled;
    public static void SetForCurrentTest(bool enabled) => _perTestEnabled.Value = enabled;
    public static bool IsEnabled => _perTestEnabled.Value ?? _globalEnabled;

    // ===== xUnit sink + fallback =====
    private static ITestOutputHelper? _output;
    private static bool _useConsoleFallback = true;              // konsola jako backup

    public static void UseITestOutput(ITestOutputHelper output) => _output = output;
    public static void EnableConsoleFallback(bool enabled) => _useConsoleFallback = enabled;

    private static void WriteLine(string line)
    {
        if (_output != null)
        {
            _output.WriteLine(line);
            return;
        }
        if (_useConsoleFallback)
            Console.Out.WriteLine(line);
    }

    // ===== PRETTY JSON =====
    private static readonly JsonSerializerOptions Pretty = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    // ===== API LOGOWANIA (używane przez helpery) =====
    public static void Log(string message)
    {
        if (!IsEnabled) return;
        WriteLine(message);
    }

    public static void LogHeaders(string title, HttpHeaders headers)
    {
        if (!IsEnabled || !headers.Any()) return;
        var lines = headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}");
        WriteLine($"{title}:\n{string.Join("\n", lines)}");
    }

    public static void LogObject(string title, object? obj)
    {
        if (!IsEnabled) return;
        try { WriteLine($"{title}:\n{JsonSerializer.Serialize(obj, Pretty)}"); }
        catch { WriteLine($"{title}: <UNSERIALIZABLE OBJECT>"); }
    }

    public static void LogJsonRaw(string title, string raw, string? contentType)
    {
        if (!IsEnabled) return;

        if (string.IsNullOrWhiteSpace(raw))
        {
            WriteLine($"{title}: <EMPTY>");
            return;
        }

        var isJson = (contentType ?? "").Contains("json", StringComparison.OrdinalIgnoreCase);
        if (isJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(raw);
                WriteLine($"{title}:\n{JsonSerializer.Serialize(doc, Pretty)}");
                return;
            }
            catch { /* fallback do RAW */ }
        }

        WriteLine($"{title} (RAW):\n{raw}");
    }

    public static async Task LogResponseAsync(HttpResponseMessage resp)
    {
        if (!IsEnabled) return;

        WriteLine($"⬅️ {(int)resp.StatusCode} {resp.ReasonPhrase}");
        LogHeaders("⬅️ Response headers", resp.Headers);

        if (resp.Content != null)
        {
            LogHeaders("⬅️ Content headers", resp.Content.Headers);
            var raw = await resp.Content.ReadAsStringAsync();
            LogJsonRaw("📥 Response body", raw, resp.Content.Headers.ContentType?.MediaType);
        }
    }
}
