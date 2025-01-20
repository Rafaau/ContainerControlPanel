using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Enums;
using ContainerControlPanel.Web.Interfaces;
using Locales;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using System.Net.Http.Json;

namespace ContainerControlPanel.Web.Components;

public partial class StartContainerDialog(IContainerAPI containerAPI)
{
    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public Container Container { get; set; }

    [Parameter]
    public ComposeFile? ComposeFile { get; set; } = null;

    [Parameter]
    public ActionType ActionType { get; set; }

    private IContainerAPI containerAPI { get; set; } = containerAPI;

    private bool loading { get; set; } = false;

    private async Task Just()
    {
        try
        {
            loading = true;

            if (ActionType == ActionType.Start)
            {
                await containerAPI.StartContainer(Container.ContainerId);
            }
            else
            {
                await containerAPI.RestartContainer(Container.ContainerId);
            }

            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
            Snackbar.Add(Localizer[ActionType == ActionType.Start
                ? Locales.Resource.StartContainerSuccess
                : Locales.Resource.RestartContainerSuccess], Severity.Normal);
        }
        catch (Exception ex)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
            Snackbar.Add(Localizer[ActionType == ActionType.Start
                ? Locales.Resource.StartContainerError
                : Locales.Resource.RestartContainerError], Severity.Error);
        }
        finally
        {
            loading = false;
        }
    }

    private async Task ByCompose()
    {
        try
        {
            loading = true;

            if (ActionType == ActionType.Start)
            {
                await containerAPI.ExecuteCommand($"compose -f {ComposeFile!.FilePath} up -d");
            }
            else
            {
                await containerAPI.ExecuteCommand($"compose -f {ComposeFile!.FilePath} restart");
            }

            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
            Snackbar.Add(Localizer[ActionType == ActionType.Start
                ? Locales.Resource.StartContainerSuccess
                : Locales.Resource.RestartContainerSuccess], Severity.Normal);
        }
        catch (Exception ex)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
            Snackbar.Add(Localizer[ActionType == ActionType.Start
                ? Locales.Resource.StartContainerError
                : Locales.Resource.RestartContainerError], Severity.Error);
        }
        finally
        {
            loading = false;
        }
    }

    private void Cancel() => MudDialog.Close(DialogResult.Ok(false));
}