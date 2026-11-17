using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ST10335260_PROG6212_POE_Part3.Services;

public class ApiResult<T>
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public Dictionary<string, string[]> Errors { get; init; } = new();

    public bool IsNotFound => StatusCode == (int)HttpStatusCode.NotFound;
    public bool IsForbidden => StatusCode == (int)HttpStatusCode.Forbidden;
    public bool IsConflict => StatusCode == (int)HttpStatusCode.Conflict;
}

public class SessionExpiredException : Exception
{
    public SessionExpiredException() : base("The API session has expired.") { }
}

public class ApiUnavailableException : Exception
{
    public ApiUnavailableException(Exception inner) : base("The RaceDay API could not be reached.", inner) { }
}

public class ApiClient
{
    public const string ApiCookieKey = "ApiCookie";
    private const string ApiSessionCookieName = ".AspNetCore.Session";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;

    public ApiClient(HttpClient http, IHttpContextAccessor accessor)
    {
        _http = http;
        _accessor = accessor;
    }

    private ISession Session => _accessor.HttpContext!.Session;

    public Task<ApiResult<T>> GetAsync<T>(string path) => SendAsync<T>(HttpMethod.Get, path);

    public Task<ApiResult<T>> PostAsync<T>(string path, object? body = null) => SendAsync<T>(HttpMethod.Post, path, body);

    public Task<ApiResult<T>> PutAsync<T>(string path, object? body = null) => SendAsync<T>(HttpMethod.Put, path, body);

    public Task<ApiResult<object>> DeleteAsync(string path) => SendAsync<object>(HttpMethod.Delete, path);

    public async Task<ApiResult<T>> UploadAsync<T>(string path, IFormFile file)
    {
        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", file.FileName);
        return await SendAsync<T>(HttpMethod.Post, path, null, content);
    }

    public void ClearSession() => Session.Remove(ApiCookieKey);

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string path, object? body = null, HttpContent? content = null)
    {
        using var request = new HttpRequestMessage(method, path);

        var cookie = Session.GetString(ApiCookieKey);
        if (!string.IsNullOrEmpty(cookie))
            request.Headers.TryAddWithoutValidation("Cookie", cookie);

        if (content is not null)
            request.Content = content;
        else if (body is not null)
            request.Content = JsonContent.Create(body, body.GetType(), options: JsonOptions);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ApiUnavailableException(ex);
        }

        using (response)
        {
            CaptureCookie(response);
            var text = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(cookie))
            {
                Session.Clear();
                throw new SessionExpiredException();
            }

            if (response.IsSuccessStatusCode)
            {
                T? data = default;
                if (!string.IsNullOrWhiteSpace(text) && typeof(T) != typeof(object))
                {
                    try
                    {
                        data = JsonSerializer.Deserialize<T>(text, JsonOptions);
                    }
                    catch (JsonException)
                    {
                        return new ApiResult<T>
                        {
                            Success = false,
                            StatusCode = (int)HttpStatusCode.BadGateway,
                            Message = "The server sent a response we could not read."
                        };
                    }
                }

                return new ApiResult<T> { Success = true, StatusCode = (int)response.StatusCode, Data = data };
            }

            var (message, errors) = ParseError(text, (int)response.StatusCode);
            return new ApiResult<T>
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = message,
                Errors = errors
            };
        }
    }

    private void CaptureCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values)) return;

        foreach (var value in values)
        {
            if (value.StartsWith(ApiSessionCookieName + "=", StringComparison.Ordinal))
            {
                var pair = value.Split(';')[0];
                Session.SetString(ApiCookieKey, pair);
            }
        }
    }

    private static (string Message, Dictionary<string, string[]> Errors) ParseError(string body, int status)
    {
        var errors = new Dictionary<string, string[]>();
        string? message = null;

        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    if (doc.RootElement.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
                        message = m.GetString();

                    if (doc.RootElement.TryGetProperty("errors", out var e) && e.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in e.EnumerateObject())
                        {
                            if (prop.Value.ValueKind != JsonValueKind.Array) continue;
                            var items = prop.Value.EnumerateArray()
                                .Where(x => x.ValueKind == JsonValueKind.String)
                                .Select(x => x.GetString() ?? string.Empty)
                                .ToArray();
                            errors[prop.Name] = items;
                        }
                    }
                }
            }
            catch (JsonException)
            {
            }
        }

        message ??= status switch
        {
            400 => "Some of the details were not valid. Please check them and try again.",
            401 => "Please sign in to continue.",
            403 => "You do not have permission to do that.",
            404 => "We could not find what you were looking for.",
            409 => "That action conflicts with the current state of the data.",
            >= 500 => "Something went wrong on our side. Please try again in a moment.",
            _ => "The request could not be completed."
        };

        return (message, errors);
    }
}
