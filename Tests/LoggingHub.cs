using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Client.Acceptance.Tests.Infrastructure.Logging;

internal static class LoggingHub
{
    // ===== PRZEŁĄCZNIKI =====
    private static volatile bool _globalEnabled = false;
    private static readonly AsyncLocal<bool?> _perTestEnabled = new();

    public static void SetGlobal(bool enabled) => _globalEnabled = enabled;
    public static void SetForCurrentTest(bool enabled) => _perTestEnabled.Value = enabled;
    public static bool IsEnabled => _perTestEnabled.Value ?? _globalEnabled;

    // ===== PRETTY JSON =====
    private static readonly JsonSerializerOptions Pretty = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    // ===== TRACE LISTENERS =====
    private static int _listenersReady;
    public static void Initialize()
    {
        if (Interlocked.Exchange(ref _listenersReady, 1) == 1) return;

        Trace.Listeners.Clear();
        Trace.AutoFlush = true;

        // Konsola → widoczne w Test Explorer → Output (Tests)
        Trace.Listeners.Add(new TextWriterTraceListener(Console.Out, "console"));

        // Opcjonalnie zapis do pliku
        try
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "TestResults");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, "http.log");
            Trace.Listeners.Add(new TextWriterTraceListener(path, "file"));
        }
        catch { /* ignoruj brak dostępu */ }
    }

    // ===== API LOGOWANIA =====
    public static void Log(string message)
    {
        if (!IsEnabled) return;
        Initialize();
        Trace.WriteLine(message);
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
        try { Log($"{title}:\n{JsonSerializer.Serialize(obj, Pretty)}"); }
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
                Log($"{title}:\n{JsonSerializer.Serialize(doc, Pretty)}");
                return;
            }
            catch { /* fallback */ }
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
