using ContainerControlPanel.Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace ContainerControlPanel.Web.Components;

public partial class DockerComposeDirectoryViewDialog
{
    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    IDialogService DialogService { get; set; }

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public List<ComposeFile> ComposeFiles { get; set; }

    private Task OpenComposeFileEditDialogAsync(ComposeFile composeFile)
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth = true,
            MaxWidth = MaxWidth.Large
        };

        return DialogService.ShowAsync<ContainerComposeEditDialog>(
            "",
            new DialogParameters()
            {
                { "ComposeFile", composeFile }
            },
            options
        );
    }

    private void Close() => MudDialog.Close(DialogResult.Ok(true));
}