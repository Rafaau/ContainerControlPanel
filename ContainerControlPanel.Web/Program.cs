using Blazored.SessionStorage;
using ContainerControlPanel.Web;
using ContainerControlPanel.Web.Authentication;
using ContainerControlPanel.Web.Interfaces;
using ContainerControlPanel.Web.Services;
using Majorsoft.Blazor.Components.Common.JsInterop.Scroll;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Refit;
using System.Globalization;
using static System.Net.WebRequestMethods;
using System.Text.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

#if RELEASE
using var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var configJson = await http.GetStringAsync("config.json");
var configData = JsonSerializer.Deserialize<Dictionary<string, string>>(configJson);

foreach (var keyValue in configData)
{
    if (string.IsNullOrWhiteSpace(keyValue.Value))
        continue;

    builder.Configuration[keyValue.Key] = keyValue.Value;
}
#endif

builder.Services.AddScoped(sp => new HttpClient {
    BaseAddress = new Uri($"http://{builder.Configuration["WebAPIHost"]}:{builder.Configuration["WebAPIPort"]}"),
    DefaultRequestHeaders = { { "Authorization", builder.Configuration["AuthToken"] } }
});

builder.Services
    .AddRefitClient<IContainerAPI>()
    .ConfigureHttpClient(config =>
    {
        config.BaseAddress = new Uri($"http://{builder.Configuration["WebAPIHost"]}:{builder.Configuration["WebAPIPort"]}");
        config.DefaultRequestHeaders.Add("Authorization", builder.Configuration["AuthToken"]);
    });

builder.Services
    .AddRefitClient<ITelemetryAPI>()
    .ConfigureHttpClient(config =>
    {
        config.BaseAddress = new Uri($"http://{builder.Configuration["WebAPIHost"]}:{builder.Configuration["WebAPIPort"]}");
        config.DefaultRequestHeaders.Add("Authorization", builder.Configuration["AuthToken"]);
    });

builder.Services.AddScoped<IServiceProvider, ServiceProvider>();
builder.Services.AddTransient<IScrollHandler, ScrollHandler>();
builder.Services.AddSingleton<WebSocketService>();
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.VisibleStateDuration = 2000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
});
builder.Services.AddLocalization(opt => opt.ResourcesPath = "Locales");

builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

builder.Services.AddMemoryCache();

builder.Logging.SetMinimumLevel(LogLevel.None);

var host = builder.Build();

const string defaultCulture = "en-US";

var js = host.Services.GetRequiredService<IJSRuntime>();
var result = await js.InvokeAsync<string>("blazorCulture.get");
var culture = CultureInfo.GetCultureInfo(result ?? defaultCulture);

var configuration = host.Services.GetRequiredService<IConfiguration>();

if (configuration["AppName"] != null)
{
    await js.InvokeVoidAsync("title.set", configuration["AppName"]);
}

if (result == null)
{
    await js.InvokeVoidAsync("blazorCulture.set", defaultCulture);
}

CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await host.RunAsync();

