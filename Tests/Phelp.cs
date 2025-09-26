using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tests.Infrastructure.Logging;

namespace Client.Acceptance.Tests.Tests.Helpers;

internal static class PhoneHelpers
{
    public static async Task<HttpResponseMessage> PostPhoneAsync(HttpClient client, Guid clientId, object phoneObject)
    {
        var url = $"/api/phone/{clientId}/phones";

        LoggingHub.Log($"➡️ POST {url}");
        LoggingHub.LogHeaders("➡️ Request headers", client.DefaultRequestHeaders);
        LoggingHub.LogObject("📤 Request body", phoneObject);

        var resp = await client.PostAsJsonAsync(url, phoneObject);
        await LoggingHub.LogResponseAsync(resp);
        return resp;
    }

    public static async Task<HttpResponseMessage> PutPhoneAsync(HttpClient client, Guid clientId, Guid phoneId, object phoneObject)
    {
        var url = $"/api/phone/{clientId}/phones/{phoneId}";

        LoggingHub.Log($"➡️ PUT {url}");
        LoggingHub.LogHeaders("➡️ Request headers", client.DefaultRequestHeaders);
        LoggingHub.LogObject("📤 Request body", phoneObject);

        var resp = await client.PutAsJsonAsync(url, phoneObject);
        await LoggingHub.LogResponseAsync(resp);
        return resp;
    }

    public static async Task<HttpResponseMessage> PostClientWithPhoneAsync(HttpClient client, object clientObject)
    {
        var url = "/api/clients";

        LoggingHub.Log($"➡️ POST {url}");
        LoggingHub.LogHeaders("➡️ Request headers", client.DefaultRequestHeaders);
        LoggingHub.LogObject("📤 Request body", clientObject);

        var resp = await client.PostAsJsonAsync(url, clientObject);
        await LoggingHub.LogResponseAsync(resp);
        return resp;
    }

    public static async Task<HttpResponseMessage> GetPhonesByInstanceIdAsync(HttpClient client, Guid clientId, Guid? instanceId)
    {
        var url = $"/api/phone/{clientId}/phones/{instanceId}";
        LoggingHub.Log($"➡️ GET {url}");
        LoggingHub.LogHeaders("➡️ Request headers", client.DefaultRequestHeaders);

        var resp = await client.GetAsync(url);
        await LoggingHub.LogResponseAsync(resp);
        return resp;
    }

    public static async Task<HttpResponseMessage> GetPhonesForClientAsync(HttpClient client, Guid clientId)
    {
        var url = $"/api/phone/{clientId}/phones";
        LoggingHub.Log($"➡️ GET {url}");
        LoggingHub.LogHeaders("➡️ Request headers", client.DefaultRequestHeaders);

        var resp = await client.GetAsync(url);
        await LoggingHub.LogResponseAsync(resp);
        return resp;
    }
}
