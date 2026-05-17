using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using TMS.Web;
using TMS.Web.Auth;
using TMS.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ── Local Storage ─────────────────────────────────────────────────────────────
builder.Services.AddBlazoredLocalStorage();

// ── Auth Token Handler (must be Transient — DelegatingHandler requirement) ────
builder.Services.AddTransient<AuthTokenHandler>();

// ── Named HttpClient with auto token attachment ───────────────────────────────
var apiBase = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7114/";

builder.Services.AddHttpClient("TmsApi", client =>
{
    client.BaseAddress = new Uri(apiBase);
})
.AddHttpMessageHandler<AuthTokenHandler>();

// ── Default HttpClient (scoped) — used by ApiClient & TmsAuthStateProvider ───
// Always resolved from the named factory so AuthTokenHandler is always active.
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("TmsApi"));

// ── Authentication & Authorization ───────────────────────────────────────────
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<TmsAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<TmsAuthStateProvider>());

// ── API Client ────────────────────────────────────────────────────────────────
builder.Services.AddScoped<ApiClient>();

// ── MudBlazor ─────────────────────────────────────────────────────────────────
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass =
        MudBlazor.Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 4000;
    config.SnackbarConfiguration.HideTransitionDuration = 300;
    config.SnackbarConfiguration.ShowTransitionDuration = 300;
});

await builder.Build().RunAsync();