using Locales;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;
using System.Globalization;

namespace ContainerControlPanel.Web.Layout;

public partial class MainLayout
{
    [Inject]
    IStringLocalizer<Resource> Localizer { get; set; }

    [Inject]
    IJSRuntime JS { get; set; }

    [Inject]
    NavigationManager NavigationManager { get; set; }

    private bool IsDarkMode { get; set; } = true;

    private MudTheme MyCustomTheme = new MudTheme()
    {
        PaletteDark = new()
        {
            Primary = "#ffffff",
            TextPrimary = "#ffffff",
            Dark = "#1e1e1e",
            DarkDarken = "#1c1c1c",
            DarkLighten = "#1f1f1f",
            Surface = "#0d0d0d"
        }
    };

    private async Task SwitchLanguage(string language)
    {
        await JS.InvokeVoidAsync("blazorCulture.set", language);
        NavigationManager.Refresh();
    }
}