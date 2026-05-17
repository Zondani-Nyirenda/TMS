using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TMS.Web.Services;

public class ApiResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}

public class ApiClient
{
    private readonly HttpClient _http;

    // Enum-as-string so "Admin", "Paid", "Present" etc. round-trip correctly
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiClient(HttpClient http) => _http = http;

    // ── GET ───────────────────────────────────────────────────────────────────

    public async Task<ApiResult<T>> GetAsync<T>(string url)
    {
        try
        {
            var response = await _http.GetAsync(url);
            return await ParseAsync<T>(response);
        }
        catch (Exception ex) { return Fail<T>(ex.Message); }
    }

    // ── POST ──────────────────────────────────────────────────────────────────

    public async Task<ApiResult<T>> PostAsync<T>(string url, object? body = null)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(url, body, _json);
            return await ParseAsync<T>(response);
        }
        catch (Exception ex) { return Fail<T>(ex.Message); }
    }

    // ── PUT ───────────────────────────────────────────────────────────────────

    public async Task<ApiResult<T>> PutAsync<T>(string url, object? body = null)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(url, body, _json);
            return await ParseAsync<T>(response);
        }
        catch (Exception ex) { return Fail<T>(ex.Message); }
    }

    // ── DELETE ────────────────────────────────────────────────────────────────

    public async Task<ApiResult<bool>> DeleteAsync(string url)
    {
        try
        {
            var response = await _http.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
                return new ApiResult<bool> { Success = true, Data = true };

            var error = await response.Content.ReadAsStringAsync();
            return new ApiResult<bool> { Success = false, Error = ExtractError(error) };
        }
        catch (Exception ex) { return Fail<bool>(ex.Message); }
    }

    // ── Multipart (file upload) ───────────────────────────────────────────────
    // Used by ContentItemUploadDialog to POST multipart/form-data
    // with the JSON metadata in "requestJson" and the file in "file".

    public async Task<ApiResult<T>> PostMultipartAsync<T>(
        string url, MultipartFormDataContent content)
    {
        try
        {
            var response = await _http.PostAsync(url, content);
            return await ParseAsync<T>(response);
        }
        catch (Exception ex) { return Fail<T>(ex.Message); }
    }

    // ── Shared parser — reads the stream exactly once ─────────────────────────

    private static async Task<ApiResult<T>> ParseAsync<T>(HttpResponseMessage response)
    {
        var raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return Fail<T>(ExtractError(raw));

        // 1. Try wrapped envelope: { success, data, message }
        try
        {
            var wrapped = JsonSerializer.Deserialize<ApiWrapper<T>>(raw, _json);
            if (wrapped?.Success == true)
                return new ApiResult<T> { Success = true, Data = wrapped.Data };
        }
        catch { /* not a wrapped response — fall through */ }

        // 2. Try direct deserialization (bare JSON objects/arrays)
        try
        {
            var data = JsonSerializer.Deserialize<T>(raw, _json);
            return new ApiResult<T> { Success = true, Data = data };
        }
        catch (Exception ex)
        {
            return Fail<T>($"Parse error: {ex.Message}");
        }
    }

    // ── Error helpers ─────────────────────────────────────────────────────────

    private static string ExtractError(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Unknown error";
        try
        {
            var doc = JsonDocument.Parse(raw);

            // Try common error-body field names in order of preference
            foreach (var field in new[] { "message", "title", "detail", "error" })
            {
                if (doc.RootElement.TryGetProperty(field, out var prop))
                {
                    var text = prop.GetString();
                    if (!string.IsNullOrWhiteSpace(text)) return text;
                }
            }

            // FluentValidation / ASP.NET validation problem details have an "errors" object
            if (doc.RootElement.TryGetProperty("errors", out var errors))
            {
                if (errors.ValueKind == JsonValueKind.Object)
                {
                    var messages = errors.EnumerateObject()
                        .SelectMany(p => p.Value.ValueKind == JsonValueKind.Array
                            ? p.Value.EnumerateArray().Select(v => v.GetString() ?? string.Empty)
                            : new[] { p.Value.GetString() ?? string.Empty })
                        .Where(s => !string.IsNullOrWhiteSpace(s));
                    return string.Join(" | ", messages);
                }
                if (errors.ValueKind == JsonValueKind.Array)
                {
                    var messages = errors.EnumerateArray()
                        .Select(v => v.GetString() ?? string.Empty)
                        .Where(s => !string.IsNullOrWhiteSpace(s));
                    return string.Join(" | ", messages);
                }
            }
        }
        catch { /* not valid JSON — return raw */ }

        return raw.Length > 300 ? raw[..300] + "…" : raw;
    }

    private static ApiResult<T> Fail<T>(string error)
        => new() { Success = false, Error = error };
}

// ── Internal envelope model ───────────────────────────────────────────────────

internal class ApiWrapper<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
}