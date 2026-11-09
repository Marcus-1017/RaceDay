using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Web.Services;

public class ApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _accessor;
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    public ApiClient(IHttpClientFactory factory, IHttpContextAccessor accessor)
    {
        _factory = factory;
        _accessor = accessor;
    }

    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null)
    {
        var client = _factory.CreateClient("RaceDayApi");
        using var request = new HttpRequestMessage(method, url);

        var cookie = _accessor.HttpContext?.Session.GetString("ApiCookie");
        if (!string.IsNullOrEmpty(cookie))
            request.Headers.TryAddWithoutValidation("Cookie", cookie);

        if (body != null)
            request.Content = JsonContent.Create(body);

        return await client.SendAsync(request);
    }

    public async Task<T?> ReadAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode) return default;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, Json);
    }

    // pulls ".AspNetCore.Session=xyz" out of the API's login response
    public static string? ExtractCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values)) return null;
        var header = values.FirstOrDefault(v =>
            v.StartsWith(".AspNetCore.Session=", StringComparison.OrdinalIgnoreCase));
        return header?.Split(';', 2)[0];
    }

    // turns an API error into a friendly message, never a raw error
    public static async Task<string> ErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("message", out var m))
                return m.GetString() ?? "Something went wrong.";
        }
        catch { }
        return "Something went wrong. Please check your details and try again.";
    }
}