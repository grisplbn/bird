using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tests.Infrastructure.Logging;

namespace Client.Acceptance.Tests.Tests.Helpers;

internal static class PhoneHelpers
{
    private static object Mask(object obj) => obj; // ewentualne maskowanie

    public static async Task<HttpResponseMessage> PostPhoneAsync(HttpClient client, Guid clientId, object phoneObject)
    {
        var postUrl = $"/api/phone/{clientId}/phones";

        LoggingHub.Log($"➡️ POST {postUrl}");
        LoggingHub.LogHeaders("➡️ Request headers", client.DefaultRequestHeaders);
        LoggingHub.LogObject("📤 Request body", Mask(phoneObject));

        var resp = await client.PostAsJsonAsync(postUrl, phoneObject);
        await LoggingHub.LogResponseAsync(resp);
        return resp;
    }

    public static async Task<HttpResponseMessage> PutPhoneAsync(HttpClient client, Guid clientId, Guid phoneId, object phoneObject)
    {
        var putUrl = $"/api/phone/{clientId}/phones/{phoneId}";

        LoggingHub.Log($"➡️ PUT {putUrl}");
        LoggingHub.LogHeaders("➡️ Request headers", client.DefaultRequestHeaders);
        LoggingHub.LogObject("📤 Request body", Mask(phoneObject));

        var resp = await client.PutAsJsonAsync(putUrl, phoneObject);
        await LoggingHub.LogResponseAsync(resp);
        return resp;
    }

    public static async Task<HttpResponseMessage> PostClientWithPhoneAsync(HttpClient client, object clientObject)
    {
        var postUrl = "/api/clients";

        LoggingHub.Log($"➡️ POST {postUrl}");
        LoggingHub.LogHeaders("➡️ Request headers", client.DefaultRequestHeaders);
        LoggingHub.LogObject("📤 Request body", Mask(clientObject));

        var resp = await client.PostAsJsonAsync(postUrl, clientObject);
        await LoggingHub.LogResponseAsync(resp);
        return resp;
    }

    public static async Task<HttpResponseMessage> GetPhonesByInstanceIdAsync(HttpClient client, Guid clientId, Guid? instanceId)
    {
        var getUrl = $"/api/phone/{clientId}/phones/{instanceId}";

        LoggingHub.Log($"➡️ GET {getUrl}");
        LoggingHub.LogHeaders("➡️ Request headers", client.DefaultRequestHeaders);

        var resp = await client.GetAsync(getUrl);
        await LoggingHub.LogResponseAsync(resp);
        return resp;
    }

    public static async Task<HttpResponseMessage> GetPhonesForClientAsync(HttpClient client, Guid clientId)
    {
        var getUrl = $"/api/phone/{clientId}/phones";

        LoggingHub.Log($"➡️ GET {getUrl}");
        LoggingHub.LogHeaders("➡️ Request headers", client.DefaultRequestHeaders);

        var resp = await client.GetAsync(getUrl);
        await LoggingHub.LogResponseAsync(resp);
        return resp;
    }
}
