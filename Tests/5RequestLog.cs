using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;
using Xunit.Sdk; // DiagnosticMessage

namespace Client.Acceptance.Tests.Infrastructure.Logging;

internal static class RequestLog
{
    // --- włączanie/wyłączanie ---
    private static volatile bool _globalEnabled = true; // domyślnie ON
    private static readonly AsyncLocal<bool?> _perTestEnabled = new();
    public static void SetGlobal(bool enabled) => _globalEnabled = enabled;
    public static void SetForCurrentTest(bool enabled) => _perTestEnabled.Value = enabled;
    private static bool Enabled => _perTestEnabled.Value ?? _globalEnabled;

    // --- pretty JSON ---
    private static readonly JsonSerializerOptions Pretty = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    // --- wybór „sinka”: xUnit (diagnostic) albo Console.Out ---
    private static IMessageSink? _xunitSink;

    // wywołaj raz z fixture’a kolekcji:
    public static void UseXunitDiagnosticSink(IMessageSink sink) => _xunitSink = sink;

    private static void Write(string line)
    {
        if (_xunitSink != null)
        {
            // trafia do Test Explorer → Output (Tests)
            _xunitSink.OnMessage(new DiagnosticMessage(line));
        }
        else
        {
            // fallback – też widoczne w Output (Tests)
            Console.Out.WriteLine(line);
        }
    }

    // --- API dla helperów ---
    public static void Log(string message)
    {
        if (!Enabled) return;
        Write(message);
    }

    public static void LogHeaders(string title, HttpHeaders headers)
    {
        if (!Enabled || !headers.Any()) return;
        var lines = headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}");
        Write($"{title}:\n{string.Join("\n", lines)}");
    }

    public static void LogObject(string title, object? obj)
    {
        if (!Enabled) return;
        try { Write($"{title}:\n{JsonSerializer.Serialize(obj, Pretty)}"); }
        catch { Write($"{title}: <UNSERIALIZABLE OBJECT>"); }
    }

    public static void LogJsonRaw(string title, string raw, string? contentType)
    {
        if (!Enabled) return;

        if (string.IsNullOrWhiteSpace(raw))
        {
            Write($"{title}: <EMPTY>");
            return;
        }

        var isJson = (contentType ?? "").Contains("json", StringComparison.OrdinalIgnoreCase);
        if (isJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(raw);
                Write($"{title}:\n{JsonSerializer.Serialize(doc, Pretty)}");
                return;
            }
            catch { /* fallback */ }
        }

        Write($"{title} (RAW):\n{raw}");
    }

    public static async Task LogResponseAsync(HttpResponseMessage resp)
    {
        if (!Enabled) return;

        Write($"⬅️ {(int)resp.StatusCode} {resp.ReasonPhrase}");
        LogHeaders("⬅️ Response headers", resp.Headers);

        if (resp.Content != null)
        {
            LogHeaders("⬅️ Content headers", resp.Content.Headers);
            var raw = await resp.Content.ReadAsStringAsync();
            LogJsonRaw("📥 Response body", raw, resp.Content.Headers.ContentType?.MediaType);
        }
    }
}
