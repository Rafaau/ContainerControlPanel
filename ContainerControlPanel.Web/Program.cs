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
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri("http://localhost:5121")
});

builder.Services
    .AddRefitClient<IContainerAPI>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5121"));

builder.Services
    .AddRefitClient<ITelemetryAPI>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5121"));

builder.Services.AddScoped<IServiceProvider, ServiceProvider>();
builder.Services.AddTransient<IScrollHandler, ScrollHandler>();
builder.Services.AddSingleton<WebSocketService>();
builder.Services.AddMudServices();
builder.Services.AddLocalization(opt => opt.ResourcesPath = "Locales");

builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

builder.Services.AddMemoryCache();

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

