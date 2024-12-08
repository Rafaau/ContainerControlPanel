using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Services;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace ContainerControlPanel.Web.Pages;

public partial class Traces
{
    [Inject]
    WebSocketService WebSocketService { get; set; }

    private List<Root> allTraces { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        WebSocketService.TracesUpdated += OnTracesUpdated;
        await WebSocketService.ConnectAsync("ws://localhost:5121/ws");
    }

    private void OnTracesUpdated(Root traces)
    {
        if (traces != null)
        {
            allTraces.Add(traces);
            this.StateHasChanged();
        }
    }

    private string GetHexString(string text)
    {
        byte[] byteArray = Encoding.Default.GetBytes(text);
        return BitConverter.ToString(byteArray).Replace("-", "").Substring(0, 6);
    }
}