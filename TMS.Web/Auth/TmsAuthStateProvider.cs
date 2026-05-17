using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using TMS.Application.DTOs.Auth;
using TMS.Domain.Enums;

namespace TMS.Web.Auth;

public class TmsAuthStateProvider : AuthenticationStateProvider
{
    private const string AccessTokenKey = "tms_access_token";
    private const string RefreshTokenKey = "tms_refresh_token";
    private const string UserKey = "tms_user";

    private readonly ILocalStorageService _storage;
    private readonly HttpClient _http;

    // WHY: _cachedState is intentionally NOT seeded with Anonymous() at
    // construction time. A null value means "not yet initialised" and
    // forces a real localStorage read on first call. An Anonymous() value
    // would look like "we checked and the user is logged out", which would
    // skip the localStorage read on subsequent calls after a page reload.
    private AuthenticationState? _cachedState;

    // WHY: TaskCompletionSource is used as a one-time ready signal.
    // Components that need auth data (like MainLayout) await AuthReady
    // before calling GetCurrentUserAsync, ensuring localStorage has been
    // read and the token validated before any UI tries to use it.
    private readonly TaskCompletionSource _authReady = new(
        TaskCreationOptions.RunContinuationsAsynchronously);

    public Task AuthReady => _authReady.Task;

    public TmsAuthStateProvider(ILocalStorageService storage, HttpClient http)
    {
        _storage = storage;
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_cachedState?.User.Identity?.IsAuthenticated == true)
        {
            Console.WriteLine("Auth is ready!");
            _authReady.TrySetResult();
            return _cachedState;
        }

        try
        {
            var token = await _storage.GetItemAsync<string>(AccessTokenKey);

            if (string.IsNullOrWhiteSpace(token) || IsTokenExpired(token))
            {
                var refreshed = await TryRefreshAsync();
                if (!refreshed) return SetAnonymousState();
                token = await _storage.GetItemAsync<string>(AccessTokenKey);
            }

            if (string.IsNullOrWhiteSpace(token)) return SetAnonymousState();

            var claims = ParseClaimsFromJwt(token).ToList();

            // Ensure the Name claim is set so the UI has a fallback
            if (!claims.Any(c => c.Type == ClaimTypes.Name))
            {
                var nameClaim = claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == "name");
                if (nameClaim != null) claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value));
            }

            // Standardize the Role claim
            var roleClaim = claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Role ||
                c.Type == "role" ||
                c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

            if (roleClaim is not null)
            {
                claims.Remove(roleClaim);
                string roleValue = int.TryParse(roleClaim.Value, out var roleInt)
                    ? ((UserRole)roleInt).ToString()
                    : roleClaim.Value;
                claims.Add(new Claim(ClaimTypes.Role, roleValue));
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            _cachedState = new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            _cachedState = Anonymous();
        }

        _authReady.TrySetResult();
        return _cachedState!;
    }

    public async Task LoginAsync(LoginResponse response)
    {
        // 1. Save data
        await _storage.SetItemAsync(AccessTokenKey, response.AccessToken);
        await _storage.SetItemAsync(RefreshTokenKey, response.RefreshToken);
        if (response.User is not null)
            await _storage.SetItemAsync(UserKey, JsonSerializer.Serialize(response.User));

        // 2. Immediate header update
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", response.AccessToken);

        // 3. Robust Role Mapping (Matches your DB string "Admin")
        var claims = ParseClaimsFromJwt(response.AccessToken).ToList();
        var roleClaim = claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.Role ||
            c.Type == "role" ||
            c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

        if (roleClaim is not null)
        {
            claims.Remove(roleClaim);
            // Map whatever value is in the DB (string or int) to the standard Role type
            string roleValue = int.TryParse(roleClaim.Value, out var roleInt)
                ? ((UserRole)roleInt).ToString()
                : roleClaim.Value;

            claims.Add(new Claim(ClaimTypes.Role, roleValue));
        }

        // 4. Update state and NOTIFY the UI
        var identity = new ClaimsIdentity(claims, "jwt");
        var principal = new ClaimsPrincipal(identity);
        _cachedState = new AuthenticationState(principal);

        // This is what tells MainLayout and NavMenu to refresh!
        NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));
    }

    public async Task LogoutAsync()
    {
        await _storage.RemoveItemAsync(AccessTokenKey);
        await _storage.RemoveItemAsync(RefreshTokenKey);
        await _storage.RemoveItemAsync(UserKey);

        _http.DefaultRequestHeaders.Authorization = null;

        // WHY: Set the cache to Anonymous BEFORE calling Notify.
        // If we notify first, any subscriber that immediately calls
        // GetAuthenticationStateAsync gets the old authenticated state
        // because the cache hasn't been cleared yet.
        _cachedState = Anonymous();
        NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        try
        {
            var json = await _storage.GetItemAsync<string>(UserKey);
            if (string.IsNullOrEmpty(json)) return null;

            return JsonSerializer.Deserialize<UserDto>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return null;
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────

    private AuthenticationState SetAnonymousState()
    {
        _cachedState = Anonymous();
        _authReady.TrySetResult(); // CRITICAL: Release the hang in MainLayout
        NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));
        return _cachedState;
    }

    private static AuthenticationState Anonymous()
        => new(new ClaimsPrincipal(new ClaimsIdentity()));

    private static bool IsTokenExpired(string token)
    {
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            // Refresh if token expires within the next 2 minutes
            return jwt.ValidTo < DateTime.UtcNow.AddMinutes(2);
        }
        catch
        {
            return true;
        }
    }

    private async Task<bool> TryRefreshAsync()
    {
        try
        {
            var refreshToken = await _storage.GetItemAsync<string>(RefreshTokenKey);
            var accessToken = await _storage.GetItemAsync<string>(AccessTokenKey);

            if (string.IsNullOrEmpty(refreshToken)) return false;

            var request = new RefreshTokenRequest
            {
                AccessToken = accessToken ?? "",
                RefreshToken = refreshToken
            };

            var response = await _http.PostAsJsonAsync("api/auth/refresh", request);
            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content
                .ReadFromJsonAsync<ApiWrapper<LoginResponse>>();

            if (result?.Data is null) return false;

            // WHY: Reuse LoginAsync so the cache, HttpClient header, and
            // localStorage are all updated atomically in one place.
            await LoginAsync(result.Data);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            handler.InboundClaimTypeMap.Clear();
            return handler.ReadJwtToken(token).Claims;
        }
        catch
        {
            return Enumerable.Empty<Claim>();
        }
    }
    public async Task<string?> GetTokenAsync()
    {
        return await _storage.GetItemAsync<string>(AccessTokenKey);
    }

}

// WHY: Internal wrapper matches the standard API envelope
// { success: bool, data: T, message: string } used across the backend.
internal class ApiWrapper<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
}