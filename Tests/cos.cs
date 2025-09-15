using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Client.Acceptance.Tests.Tests.Helpers;

internal class PhoneHelpers
{
    // ---- GLOBALNE OPCJE JSON (ładne formatowanie) ----
    private static readonly JsonSerializerOptions PrettyJson = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    // ---- NISKOPOZIOMOWE LOGOWANIE ----
    private static void Log(string msg) => Console.WriteLine(msg);

    private static void LogHeaders(string title, HttpHeaders headers)
    {
        if (!headers.Any()) return;
        var lines = headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}");
        Log($"{title}:\n{string.Join("\n", lines)}");
    }

    private static void LogObject(string title, object? obj)
    {
        try
        {
            var json = JsonSerializer.Serialize(obj, PrettyJson);
            Log($"{title}:\n{json}");
        }
        catch
        {
            Log($"{title}: <UNSERIALIZABLE OBJECT>");
        }
    }

    private static void LogJsonRaw(string title, string raw, string? contentType)
    {
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
                var pretty = JsonSerializer.Serialize(doc, PrettyJson);
                Log($"{title}:\n{pretty}");
                return;
            }
            catch { /* fallback to raw */ }
        }
        Log($"{title} (RAW):\n{raw}");
    }

    private static async Task LogResponseAsync(HttpResponseMessage resp)
    {
        Log($"⬅️ {(int)resp.StatusCode} {resp.ReasonPhrase}");
        LogHeaders("⬅️ Response headers", resp.Headers);

        if (resp.Content != null)
        {
            LogHeaders("⬅️ Content headers", resp.Content.Headers);
            var raw = await resp.Content.ReadAsStringAsync();
            LogJsonRaw("📥 Response body", raw, resp.Content.Headers.ContentType?.MediaType);
        }
    }

    // (opcjonalnie) maskowanie wrażliwych pól
    private static object Mask(object obj) => obj; // podmień jeśli chcesz maskować np. numer telefonu

    // ================== ISTNIEJĄCE METODY – TYLKO DODANE LOGI ==================

    public static async Task<HttpResponseMessage> PostPhoneAsync(HttpClient client, Guid clientId, object phoneObject)
    {
        var postUrl = $"/api/phone/{clientId}/phones";

        Log($"➡️ POST {postUrl}");
        LogHeaders("➡️ Request headers", client.DefaultRequestHeaders);
        LogObject("📤 Request body", Mask(phoneObject));

        var resp = await client.PostAsJsonAsync(postUrl, phoneObject);
        await LogResponseAsync(resp);
        return resp;
    }

    public static async Task<HttpResponseMessage> PutPhoneAsync(HttpClient client, Guid clientId, Guid phoneId, object phoneObject)
    {
        var putUrl = $"/api/phone/{clientId}/phones/{phoneId}";

        Log($"➡️ PUT {putUrl}");
        LogHeaders("➡️ Request headers", client.DefaultRequestHeaders);
        LogObject("📤 Request body", Mask(phoneObject));

        var resp = await client.PutAsJsonAsync(putUrl, phoneObject);
        await LogResponseAsync(resp);
        return resp;
    }

    public static async Task<HttpResponseMessage> PostClientWithPhoneAsync(HttpClient client, object clientObject)
    {
        var postUrl = "/api/clients";

        Log($"➡️ POST {postUrl}");
        LogHeaders("➡️ Request headers", client.DefaultRequestHeaders);
        LogObject("📤 Request body", Mask(clientObject));

        var resp = await client.PostAsJsonAsync(postUrl, clientObject);
        await LogResponseAsync(resp);
        return resp;
    }

    public static async Task<HttpResponseMessage> GetPhonesByInstanceIdAsync(HttpClient client, Guid clientId, Guid? instanceId)
    {
        var getUrl = $"/api/phone/{clientId}/phones/{instanceId}";

        Log($"➡️ GET {getUrl}");
        LogHeaders("➡️ Request headers", client.DefaultRequestHeaders);

        var resp = await client.GetAsync(getUrl);
        await LogResponseAsync(resp);
        return resp;
    }

    public static async Task<HttpResponseMessage> GetPhonesForClientAsync(HttpClient client, Guid clientId)
    {
        var getUrl = $"/api/phone/{clientId}/phones";

        Log($"➡️ GET {getUrl}");
        LogHeaders("➡️ Request headers", client.DefaultRequestHeaders);

        var resp = await client.GetAsync(getUrl);
        await LogResponseAsync(resp);
        return resp;
    }
}
