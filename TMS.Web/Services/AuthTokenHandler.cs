using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace TMS.Web.Services;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly ILocalStorageService _storage;
    private const string AccessTokenKey = "tms_access_token";

    public AuthTokenHandler(ILocalStorageService storage)
    {
        _storage = storage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Always pull fresh from storage for every request to avoid stale headers
        var token = await _storage.GetItemAsync<string>("tms_access_token");

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}