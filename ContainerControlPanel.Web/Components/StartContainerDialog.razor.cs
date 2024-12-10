using ContainerControlPanel.Web.Enums;
using Locales;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using System.Net.Http.Json;

namespace ContainerControlPanel.Web.Components;

public partial class StartContainerDialog(HttpClient client)
{
    [Inject]
    IStringLocalizer<Resource> Localizer { get; set; }

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public string ContainerId { get; set; }

    [Parameter]
    public ActionType ActionType { get; set; }

    private HttpClient client { get; set; } = client;

    private void ChooseOption(StartOption option)
    {
        MudDialog.Close(DialogResult.Ok(option));
    }

    private void Cancel() => MudDialog.Close(DialogResult.Ok(false));
}