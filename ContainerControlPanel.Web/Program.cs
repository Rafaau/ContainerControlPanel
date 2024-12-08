using ContainerControlPanel.Web;
using ContainerControlPanel.Web.Interfaces;
using ContainerControlPanel.Web.Services;
using Majorsoft.Blazor.Components.Common.JsInterop.Scroll;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Refit;

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

builder.Services.AddScoped<IServiceProvider, ServiceProvider>();
builder.Services.AddTransient<IScrollHandler, ScrollHandler>();
builder.Services.AddSingleton<WebSocketService>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
