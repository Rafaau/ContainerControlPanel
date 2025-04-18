using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;

namespace ContainerControlPanel.Web.Components;

public partial class DockerVolumesListDialog(IContainerAPI containerAPI)
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
    public List<Volume> Volumes { get; set; } = new();

    private Volume chosenVolume { get; set; } = null;

    private bool loading { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        Volumes = await containerAPI.GetVolumes();
    }

    private async Task RemoveVolume()
    {
        if (chosenVolume == null) return;

        try
        {
            loading = true;

            var result = await containerAPI.RemoveImage(chosenVolume.Name);

            if (result)
            {
                Volumes.Remove(chosenVolume);
                chosenVolume = null;

                Snackbar.Clear();
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
                Snackbar.Add(Localizer[Locales.Resource.VolumeRemoveSuccess], Severity.Normal);
            }
            else
            {
                Snackbar.Clear();
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
                Snackbar.Add(Localizer[Locales.Resource.VolumeRemoveError], Severity.Error);
            }
        }
        finally
        {
            loading = false;
        }
    }

    private Task OpenDockerRunDialogAsync()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth = true,
            MaxWidth = MaxWidth.Medium
        };

        return DialogService.ShowAsync<RunCommandDialog>(
            "",
            new DialogParameters()
            {
                { "Command", $"" }
            },
            options
        );
    }

    private void Close() => MudDialog.Close(DialogResult.Ok(true));
}