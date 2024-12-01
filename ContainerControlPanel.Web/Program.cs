using ContainerControlPanel.Web;
using Majorsoft.Blazor.Components.Common.JsInterop.Scroll;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri("http://localhost:5121")
});

builder.Services.AddTransient<IScrollHandler, ScrollHandler>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
