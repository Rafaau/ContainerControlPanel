using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net.Http.Headers;

namespace ContainerControlPanel.Web.Components;

public partial class DockerImagesListDialog(IContainerAPI containerAPI)
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
    public List<Image> Images { get; set; } = new();

    private Image chosenImage { get; set; } = null;

    private bool loading { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        Images = await containerAPI.GetImages();
    }

    private async Task RemoveImage()
    {
        if (chosenImage == null) return;

        try
        {
            loading = true;

            var result = await containerAPI.RemoveImage(chosenImage.ImageId);

            if (result)
            {
                Images.Remove(chosenImage);
                chosenImage = null;

                Snackbar.Clear();
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
                Snackbar.Add(Localizer[Locales.Resource.ImageRemoveSuccess], Severity.Normal);
            }
            else
            {
                Snackbar.Clear();
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
                Snackbar.Add(Localizer[Locales.Resource.ImageRemoveError], Severity.Error);
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
                { "Command", $"run --name {chosenImage.Repository} -d {chosenImage.Repository}:{chosenImage.Tag}" }
            },
            options
        );
    }

    private void Close() => MudDialog.Close(DialogResult.Ok(true));
}