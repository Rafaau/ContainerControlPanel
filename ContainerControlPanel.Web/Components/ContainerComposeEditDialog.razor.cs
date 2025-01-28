using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace ContainerControlPanel.Web.Components;

public partial class ContainerComposeEditDialog(IContainerAPI containerAPI)
{
    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    IDialogService DialogService { get; set; }

    [Inject]
    IConfiguration Configuration { get; set; }

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public ComposeFile ComposeFile { get; set; } 

    private void Cancel() => MudDialog.Close(DialogResult.Ok(true));

    private async void Save()
    {
        try
        {
            await containerAPI.UpdateCompose(ComposeFile.FilePath, ComposeFile.FileContent);
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
            Snackbar.Add(Localizer[Locales.Resource.SaveSuccessful], Severity.Normal);
        }
        catch (Exception ex)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
            Snackbar.Add(Localizer[Locales.Resource.SaveError], Severity.Error);
            MudDialog.Close(DialogResult.Ok(false));
            return;
        }

        MudDialog.Close(DialogResult.Ok(true));
    }

    private Task OpenDockerComposeDialogAsync()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth = true,
            MaxWidth = MaxWidth.Medium
        };

        string context = string.IsNullOrEmpty(Configuration["Context"])
                    ? ""
                    : $" -p {Configuration["Context"]}";

        return DialogService.ShowAsync<RunCommandDialog>(
            "",
            new DialogParameters()
            {
                { "Command", $"compose -f {ComposeFile.FilePath}{context} up -d" }
            },
            options
        );
    }
}