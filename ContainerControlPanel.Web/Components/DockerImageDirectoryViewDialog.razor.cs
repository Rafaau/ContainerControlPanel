using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net.Http.Headers;

namespace ContainerControlPanel.Web.Components;

public partial class DockerImageDirectoryViewDialog(IContainerAPI containerAPI)
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
    public List<ImageFile> ImageFiles { get; set; }

    private bool loading { get; set; } = false;

    private ImageFile chosenFile { get; set; } = null;

    async Task UploadFile(InputFileChangeEventArgs e)
    {
        try
        {
            loading = true;

            const int chunkSize = 10 * 1024 * 1024; // 10MB
            var file = e.File;
            using var stream = file.OpenReadStream(maxAllowedSize: long.MaxValue);
            var s = stream.Length;

            byte[] buffer = new byte[chunkSize];
            int bytesRead;
            int chunkIndex = 0;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                byte[] chunkBuffer = new byte[bytesRead];
                Array.Copy(buffer, 0, chunkBuffer, 0, bytesRead);
                using var chunkStream = new MemoryStream(chunkBuffer);
                using var chunkContent = new StreamContent(chunkStream);
                chunkContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var content = new MultipartFormDataContent
                {
                    { chunkContent, "chunk", $"{file.Name}.part{chunkIndex++}" }
                };

                await containerAPI.UploadChunk(content);             
            }

            await containerAPI.MergeChunks(file.Name);

            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
            Snackbar.Add(Localizer[Locales.Resource.FileUploadSuccess], Severity.Normal);

            ImageFiles = await containerAPI.SearchForImages();
        }
        catch (Exception)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
            Snackbar.Add(Localizer[Locales.Resource.FileUploadError], Severity.Error);
            return;
        }
        finally
        {
            loading = false;
        }
    }

    private Task OpenInstallImageDialogAsync()
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
                { "Command", $"load --input {chosenFile.FilePath}" }
            },
            options
        );
    }

    private void Close() => MudDialog.Close(DialogResult.Ok(true));
}