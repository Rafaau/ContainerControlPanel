using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text.Json;
using Microsoft.JSInterop;

namespace ContainerControlPanel.Web.Pages;

public partial class ApiDocs(IContainerAPI containerAPI)
{
    [Inject]
    IJSRuntime JS { get; set; }

    [Parameter]
    public string ContainerId { get; set; }

    private ContainerDetails containerDetails { get; set; }

    private List<ControllerView> controllers { get; set; } = new();

    private IContainerAPI containerAPI { get; set; } = containerAPI;

    private HttpClient httpClient { get; set; }

    private string formattedRequestJson { get; set; }

    private string formattedResponseJson { get; set; }

    protected override async Task OnInitializedAsync()
    {
        containerDetails = await containerAPI.GetContainerDetails(ContainerId);
        var port = containerDetails.NetworkSettings.Ports.FirstOrDefault().Value.FirstOrDefault().HostPort;

        if (port != null)
        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://localhost:{port}")
            };

            await LoadEndpoints();
        }      
    }

    private async Task LoadEndpoints()
    {
        var docs = await httpClient.GetFromJsonAsync<List<EndpointInfo>>("docs");

        if (docs != null)
        {
            controllers = docs.GetControllers();

            foreach (var action in controllers.SelectMany(c => c.Actions))
            {
                action.RequestBodyFormatted = await FormatJson(action.Parameters.Find(p => p.Source == "Body")?.Type?.GetRequestBodyJson() ?? "");
                action.ResponseBodyFormatted = await FormatJson(action.ReturnType.GetResponseBodyJson());
            }
        }
    }

    private async Task<string> FormatJson(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        var preJson = JsonObject.Parse(body);
        var json = JsonSerializer.Serialize(preJson, new JsonSerializerOptions { WriteIndented = true });
        return await JS.InvokeAsync<string>("colorizeJson", json);
    }
}