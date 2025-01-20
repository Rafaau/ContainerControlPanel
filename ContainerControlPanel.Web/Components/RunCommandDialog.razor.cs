using ContainerControlPanel.Web.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;

namespace ContainerControlPanel.Web.Components;

public partial class RunCommandDialog(IContainerAPI containerAPI)
{
    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    IDialogService DialogService { get; set; }

    [Inject]
    IJSRuntime JSRuntime { get; set; }

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public string Command { get; set; }

    private bool loading { get; set; } = false;

    private async Task ExecuteCommand()
    {
        try
        {
            loading = true;
            var result = await containerAPI.ExecuteCommand(Command);

            if (result)
            {
                Snackbar.Clear();
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
                Snackbar.Add(Localizer[Locales.Resource.CommandExecuteSuccess], Severity.Normal);
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                Snackbar.Clear();
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
                Snackbar.Add(Localizer[Locales.Resource.CommandExecuteError], Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
            Snackbar.Add(Localizer[Locales.Resource.CommandExecuteError], Severity.Error);
        }
        finally
        {
            loading = false;
        }
    }

    private void Close() => MudDialog.Close(DialogResult.Ok(true));
}