using IBTS2026.Web;
using IBTS2026.Web.Components;
using IBTS2026.Web.Services.ApiClients;
using IBTS2026.Web.Services.Auth;
using IBTS2026.Web.Services.Http;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using NLog;
using NLog.Web;

// Configure NLog early to catch startup errors
var logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

try
{
    logger.Debug("Starting IBTS2026 Web Application");

    var builder = WebApplication.CreateBuilder(args);

    // Configure NLog
    builder.ConfigureNLog();

    // Add service defaults & Aspire client integrations.
    builder.AddServiceDefaults();

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddOutputCache();

    // Register circuit token cache (singleton for cross-scope token access)
    builder.Services.AddSingleton<ICircuitTokenCache, CircuitTokenCache>();
    builder.Services.AddScoped<CircuitIdProvider>();
    builder.Services.AddScoped<CircuitHandler, TokenCircuitHandler>();

    // Register ASP.NET Core authentication/authorization services (required for AuthorizeView in interactive components)
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();

    // Register Blazor authentication services
    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
    builder.Services.AddScoped<IAuthTokenStore, AuthTokenStore>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    // Register the authorization message handler (transient for HttpClient factory)
    builder.Services.AddTransient<AuthorizationMessageHandler>();

    // Register Auth API client (no auth required for login/register)
    builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
    {
        client.BaseAddress = new Uri("https+http://apiservice");
    });

    // Register HTTP clients for API access with authorization
    builder.Services.AddHttpClient<IUserApiClient, UserApiClient>(client =>
    {
        client.BaseAddress = new Uri("https+http://apiservice");
    }).AddHttpMessageHandler<AuthorizationMessageHandler>();

    builder.Services.AddHttpClient<IIncidentApiClient, IncidentApiClient>(client =>
    {
        client.BaseAddress = new Uri("https+http://apiservice");
    }).AddHttpMessageHandler<AuthorizationMessageHandler>();

    builder.Services.AddHttpClient<ILookupApiClient, LookupApiClient>(client =>
    {
        client.BaseAddress = new Uri("https+http://apiservice");
    }).AddHttpMessageHandler<AuthorizationMessageHandler>();

    builder.Services.AddHttpClient<IIncidentNoteApiClient, IncidentNoteApiClient>(client =>
    {
        client.BaseAddress = new Uri("https+http://apiservice");
    }).AddHttpMessageHandler<AuthorizationMessageHandler>();

    builder.Services.AddHttpClient<IInvitationApiClient, InvitationApiClient>(client =>
    {
        client.BaseAddress = new Uri("https+http://apiservice");
    }).AddHttpMessageHandler<AuthorizationMessageHandler>();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    // Note: Authentication/Authorization middleware is NOT used here because:
    // 1. Blazor Server auth is handled by AuthorizeView + CustomAuthStateProvider
    // 2. The middleware would try to "challenge" and redirect, which breaks Blazor
    // The AddAuthentication()/AddAuthorization() service registrations are still needed
    // for AuthorizeView to resolve IAuthenticationService

    app.UseAntiforgery();

    app.UseOutputCache();

    app.MapStaticAssets();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.MapDefaultEndpoints();

    app.Run();
}
catch (Exception ex)
{
    // NLog: catch setup errors
    logger.Error(ex, "Application stopped due to exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application exit
    LogManager.Shutdown();
}
