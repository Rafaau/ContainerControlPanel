using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace ContainerControlPanel.Web.Components;

public partial class ContainerDetailsDialog(IContainerAPI containerAPI)
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public string ContainerId { get; set; }

    private IContainerAPI containerAPI { get; set; } = containerAPI;

    private ContainerDetails containerDetails { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadContainerDetails();
    }

    private async Task LoadContainerDetails()
    {
        containerDetails = await containerAPI.GetContainerDetails(ContainerId);
    }

    private void Ok() => MudDialog.Close(DialogResult.Ok(true));
}