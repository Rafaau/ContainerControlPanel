using ContainerControlPanel.Web.Authentication;
using Locales;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
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

    [Inject]
    IConfiguration Configuration { get; set; }

    [Inject]
    AuthenticationStateProvider authStateProvider { get; set; }

    private bool IsDarkMode { get; set; } = true;

    private MudTheme MyCustomTheme = new MudTheme()
    {
        PaletteDark = new()
        {
            Primary = "#00B577",
            TextPrimary = "#ffffff",
            Dark = "#1e1e1e",
            DarkDarken = "#1c1c1c",
            DarkLighten = "#1f1f1f",
            Surface = "#0d0d0d",
            Background = "#2B2B2B",
            Divider = "#484848",
            Success = "#00B577",
            Error = "#ff0000",
            Info = "#ffffff"
        }
    };

    private string? token { get; set; }

    private bool invalidToken { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        if (Configuration["Token"] == null)
        {
            token = null;
            await Authenticate(reload: false);
        }
    }

    private async Task SwitchLanguage(string language)
    {
        await JS.InvokeVoidAsync("blazorCulture.set", language);
        NavigationManager.Refresh();
    }

    private async Task Authenticate(KeyboardEventArgs? keyboardEventArgs = null, bool reload = true)
    {
        if (keyboardEventArgs != null && keyboardEventArgs.Code != "Enter") return;

        if (token == Configuration["Token"])
        {
            invalidToken = false;
            var customAuthStateProvider = (CustomAuthenticationStateProvider)authStateProvider;
            await customAuthStateProvider.UpdateAuthenticationState(new UserSession
            {
                UserName = "Admin",
                Role = "Admin",
                Token = token,
                ExpiresIn = 30
            });

            if (reload)
                NavigationManager.Refresh(true);
        }
        else
        {
            invalidToken = true;
        }
    }
}