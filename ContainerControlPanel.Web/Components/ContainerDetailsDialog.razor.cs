using ContainerControlPanel.Domain.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace ContainerControlPanel.Web.Components;

public partial class ContainerDetailsDialog(HttpClient client)
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public string ContainerId { get; set; }

    private HttpClient client { get; set; } = client;

    private ContainerDetails containerDetails { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadContainerDetails();
    }

    private async Task LoadContainerDetails()
    {
        containerDetails = await client.GetFromJsonAsync<ContainerDetails>($"api/getContainerDetails?containerId={ContainerId}");
    }

    private void Ok() => MudDialog.Close(DialogResult.Ok(true));
}