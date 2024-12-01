using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ContainerControlPanel.Web.Components;

public partial class ContainerDetailsDialog(HttpClient client)
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public string ContainerId { get; set; }

    private HttpClient client { get; set; } = client;

    protected override async Task OnInitializedAsync()
    {
        await LoadContainerDetails();
    }

    private async Task LoadContainerDetails()
    {
        string result = await client.GetStringAsync($"api/getContainerDetails?containerId={ContainerId}");
    }

    private void Submit() => MudDialog.Close(DialogResult.Ok(true));

    private void Cancel() => MudDialog.Cancel();
}