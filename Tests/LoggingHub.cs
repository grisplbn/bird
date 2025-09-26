using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tests.Infrastructure.Logging;

internal static class LoggingHub
{
    // --- GLOBAL / PER-TEST PRZEŁĄCZNIKI ---
    private static volatile bool _globalEnabled =
        string.Equals(Environment.GetEnvironmentVariable("HTTP_LOG"), "1", StringComparison.OrdinalIgnoreCase);

    private static readonly AsyncLocal<bool?> _perTestEnabled = new();

    public static void SetGlobal(bool enabled) => _globalEnabled = enabled;
    public static void SetForCurrentTest(bool enabled) => _perTestEnabled.Value = enabled;
    public static bool IsEnabled => _perTestEnabled.Value ?? _globalEnabled;

    // --- JSON pretty ---
    private static readonly JsonSerializerOptions PrettyJson = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    // --- TRACE LISTENERS (jednorazowo) ---
    private static int _listenersReady;
    public static void EnsureTraceListeners()
    {
        if (Interlocked.Exchange(ref _listenersReady, 1) == 1) return;

        Trace.Listeners.Clear();
        Trace.AutoFlush = true;

        // Konsola – widoczna w Test Explorer → Output (Tests)
        Trace.Listeners.Add(new TextWriterTraceListener(Console.Out, "console"));

        // Opcjonalnie plik z logiem
        try
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "TestResults");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, "http.log");
            Trace.Listeners.Add(new TextWriterTraceListener(path, "file"));
        }
        catch { /* brak uprawnień? pomijamy */ }
    }

    // --- API do logowania (używane przez *Helpers) ---
    public static void Log(string msg)
    {
        if (!IsEnabled) return;
        EnsureTraceListeners();
        Trace.WriteLine(msg);
    }

    public static void LogHeaders(string title, HttpHeaders headers)
    {
        if (!IsEnabled || !headers.Any()) return;
        var lines = headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}");
        Log($"{title}:\n{string.Join("\n", lines)}");
    }

    public static void LogObject(string title, object? obj)
    {
        if (!IsEnabled) return;
        try { Log($"{title}:\n{JsonSerializer.Serialize(obj, PrettyJson)}"); }
        catch { Log($"{title}: <UNSERIALIZABLE OBJECT>"); }
    }

    public static void LogJsonRaw(string title, string raw, string? contentType)
    {
        if (!IsEnabled) return;

        if (string.IsNullOrWhiteSpace(raw))
        {
            Log($"{title}: <EMPTY>");
            return;
        }

        var isJson = (contentType ?? "").Contains("json", StringComparison.OrdinalIgnoreCase);
        if (isJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(raw);
                Log($"{title}:\n{JsonSerializer.Serialize(doc, PrettyJson)}");
                return;
            }
            catch { /* fallback to raw */ }
        }
        Log($"{title} (RAW):\n{raw}");
    }

    public static async Task LogResponseAsync(HttpResponseMessage resp)
    {
        if (!IsEnabled) return;

        Log($"⬅️ {(int)resp.StatusCode} {resp.ReasonPhrase}");
        LogHeaders("⬅️ Response headers", resp.Headers);

        if (resp.Content != null)
        {
            LogHeaders("⬅️ Content headers", resp.Content.Headers);
            var raw = await resp.Content.ReadAsStringAsync();
            LogJsonRaw("📥 Response body", raw, resp.Content.Headers.ContentType?.MediaType);
        }
    }
}
