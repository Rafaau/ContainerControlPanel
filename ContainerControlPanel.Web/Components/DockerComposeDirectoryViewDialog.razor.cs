using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Enums;
using ContainerControlPanel.Web.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Mime;

namespace ContainerControlPanel.Web.Components;

public partial class DockerComposeDirectoryViewDialog(IContainerAPI containerAPI)
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
    public List<ComposeFile> ComposeFiles { get; set; }

    private IContainerAPI containerAPI { get; set; } = containerAPI;

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

    async Task UploadFile(InputFileChangeEventArgs e)
    {
        var files = e.GetMultipleFiles();

        foreach (var file in files)
        {
            using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = 
                new MediaTypeHeaderValue(string.IsNullOrEmpty(file.ContentType) ? "application/x-yaml" : file.ContentType);

            content.Add(fileContent, "file", file.Name);

            try
            {
                await containerAPI.UploadCompose(content);

                Snackbar.Clear();
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
                Snackbar.Add(Localizer[Locales.Resource.FileUploadSuccess], Severity.Normal);

                ComposeFiles = await containerAPI.SearchForComposes();
            }
            catch (Exception ex)
            {
                Snackbar.Clear();
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
                Snackbar.Add(Localizer[Locales.Resource.FileUploadError], Severity.Error);
                return;
            }         
        }
    }

    private void Close() => MudDialog.Close(DialogResult.Ok(true));
}