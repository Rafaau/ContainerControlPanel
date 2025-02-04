using ContainerControlPanel.Domain.Models;
using ContainerControlPanel.Web.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text.Json;
using Microsoft.JSInterop;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Text;
using System.Net;

namespace ContainerControlPanel.Web.Pages;

public partial class ApiDocs(IContainerAPI containerAPI)
{
    [Inject]
    IStringLocalizer<Locales.Resource> Localizer { get; set; }

    [Inject]
    IJSRuntime JS { get; set; }

    [Inject]
    IMemoryCache MemoryCache { get; set; }

    [Inject]
    NavigationManager NavigationManager { get; set; }

    [Inject]
    IConfiguration Configuration { get; set; }

    [Parameter]
    public string ContainerId { get; set; }

    private List<Container> containers { get; set; } = new();

    private ContainerDetails containerDetails { get; set; }

    private List<ControllerView> controllers { get; set; } = new();

    private IContainerAPI containerAPI { get; set; } = containerAPI;

    private HttpClient httpClient { get; set; }

    private List<SavedRequest> savedRequests { get; set; } = new();

    private List<DateTime> historyDates
    {
        get
        {
            return savedRequests.Select(r => r.CallTime.Date).Distinct().ToList();
        }
    }

    private string? containerId
    {
        get => ContainerId ?? null;
        set
        {
            ContainerId = value;
            MemoryCache.Set("lastApiDocsHref", $"/apidocs/{value}");
            NavigationManager.NavigateTo($"/apidocs/{value}");
            LoadData();
        }
    }

    private string formattedRequestJson { get; set; }

    private string formattedResponseJson { get; set; }

    private bool apiDocsNotImplemented { get; set; } = false;

    private List<Header> headers { get; set; } = new();

    private bool historyExpanded { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadContainers();

        if (MemoryCache.TryGetValue("lastApiDocsHref", out string lastApiDocsHref))
        {
            NavigationManager.NavigateTo(lastApiDocsHref);
        }      

        if (ContainerId != null)
        {
            MemoryCache.Set("lastApiDocsHref", $"/apidocs/{ContainerId}");
            await LoadData();
        }

        await LoadHistory();
    }

    private async Task LoadData()
    {
        if (MemoryCache.TryGetValue($"endpoints_{ContainerId}", out List<ControllerView> cachedControllers))
        {
            controllers = cachedControllers;
            this.StateHasChanged();
        }

        containerDetails = await containerAPI.GetContainerDetails(ContainerId);
        var port = containerDetails.NetworkSettings.Ports.FirstOrDefault().Value.FirstOrDefault().HostPort;

        if (port != null)
        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://{Configuration["WebAPIHost"]}:{port}")
            };

            await LoadEndpoints();
        }
    }

    private async Task LoadContainers()
    {
        var result = await containerAPI.GetContainers(true, true);

        foreach (var container in result)
        {
            if (container.Status.Contains("Up") && !MemoryCache.TryGetValue($"containerDetails_{container.ContainerId}", out _))
            {
                ContainerDetails containerDetails = await containerAPI.GetContainerDetails(container.ContainerId);

                MemoryCache.Set($"containerDetails_{container.ContainerId}", containerDetails);
            }
        }

        containers = result.Where(c => IsASPNETCoreContainer(c.ContainerId)).ToList();
        MemoryCache.Set("apiDocsContainers", containers);

        this.StateHasChanged();
    }

    private async Task LoadHistory()
    {
        savedRequests = await containerAPI.GetSavedRequests();
    }

    private async Task LoadEndpoints()
    {
        try
        {
            var docs = await httpClient.GetFromJsonAsync<List<EndpointInfo>>("docs");

            if (docs != null)
            {
                controllers = docs.GetControllers();

                foreach (var action in controllers.SelectMany(c => c.Actions))
                {
                    action.RequestBodyFormatted = await FormatJson(action.Parameters.Find(p => p.Source == "Body")?.Type?.GetRequestBodyJson() ?? "");
                    
                    if (action.Parameters.Find(p => p.Source == "Body")?.Type?.Structure?.Properties?.Count == 0)
                    {
                        action.TestRequestBody = $"\"{action.Parameters.Find(p => p.Source == "Body").Type.Name.ToLower()}\"";
                    }
                    else
                    {
                        action.TestRequestBody = await FormatJson(action.Parameters.Find(p => p.Source == "Body")?.Type?.GetRequestBodyJson() ?? "", false);
                    }
                    
                    action.ResponseBodyFormatted = await FormatJson(action.ReturnType.GetResponseBodyJson());
                }

                MemoryCache.Set($"endpoints_{ContainerId}", controllers);
                apiDocsNotImplemented = false;
            }
            else
            {
                apiDocsNotImplemented = true;
            }
        }
        catch (HttpRequestException e)
        {
            apiDocsNotImplemented = true;
        }

        this.StateHasChanged();
    }

    private bool IsASPNETCoreContainer(string containerId)
    {
        if (MemoryCache.TryGetValue($"containerDetails_{containerId}", out ContainerDetails _containerDetails))
        {
            return _containerDetails.Config.EnvironmentVariables.Any(x => x.Name.Contains("ASPNETCORE"));
        }
        else
        {
            return false;
        }
    }

    private async Task<string> FormatJson(string body, bool colorize = true)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        var preJson = JsonObject.Parse(body);
        var json = JsonSerializer.Serialize(preJson, new JsonSerializerOptions { WriteIndented = true });
        return colorize 
            ? await JS.InvokeAsync<string>("colorizeJson", json)
            : json;
    }

    private async Task ExecuteAction(ActionView action)
    {
        foreach (var param in action.Parameters.Where(p => p.Source == "Query" || p.Source == "Route"))
        {
            if (string.IsNullOrEmpty(param.TestValue))
            {
                param.Error = true;
                return;
            }
            else
            {
                param.Error = false;
            }
        }

        action.Loading = true;

        switch (action.HttpMethod)
        {
            case "Get":
                await ExecuteGetAction(action);
                break;
            case "Post":
                await ExecutePostAction(action);
                break;
            case "Put":
                await ExecutePutAction(action);
                break;
            case "Delete":
                await ExecuteDeleteAction(action);
                break;
            case "Patch":
                await ExecutePatchAction(action);
                break;
            default:
                break;
        }

        action.Loading = false;
    }

    private async Task ExecuteGetAction(ActionView action)
    {
        string route = GetRouteString(action);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, route);
        
        AssignHeaders(action, request);

        var response = await httpClient.SendAsync(request);

        await ReadResponse(response, action);

        await SaveRequest(route, action);
    }

    private async Task ExecutePostAction(ActionView action)
    {
        string route = GetRouteString(action);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, route);

        request.Content = new StringContent(action.TestRequestBody, encoding: Encoding.UTF8, "application/json");

        AssignHeaders(action, request);

        var response = await httpClient.SendAsync(request);

        await ReadResponse(response, action);

        await SaveRequest(route, action);
    }

    private async Task ExecutePutAction(ActionView action)
    {
        string route = GetRouteString(action);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, route);

        request.Content = new StringContent(action.TestRequestBody, encoding: Encoding.UTF8, "application/json");

        AssignHeaders(action, request);

        var response = await httpClient.SendAsync(request);

        await ReadResponse(response, action);

        await SaveRequest(route, action);
    }

    private async Task ExecuteDeleteAction(ActionView action)
    {
        string route = GetRouteString(action);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, route);

        AssignHeaders(action, request);

        var response = await httpClient.SendAsync(request);

        await ReadResponse(response, action);

        await SaveRequest(route, action);
    }

    private async Task ExecutePatchAction(ActionView action)
    {
        string route = GetRouteString(action);

        HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), route);

        request.Content = new StringContent(action.TestRequestBody, encoding: Encoding.UTF8, "application/json");

        AssignHeaders(action, request);

        var response = await httpClient.SendAsync(request);

        await ReadResponse(response, action);
    }

    private async Task SaveRequest(string route, ActionView action)
    {
        var request = new SavedRequest
        {
            HttpMethod = action.HttpMethod,
            URL = httpClient.BaseAddress + route,
            RequestBody = action.TestRequestBody,
            ResponseBody = action.TestResponseBody,
            ResponseHeaders = action.TestResponseHeaders,
            ResponseStatusCode = action.TestResponseStatusCode,
            ResponseStatusDescription = action.TestResponseStatusDescription,
            RequestCurl = action.TestRequestCurl,
            CallTime = DateTime.Now
        };

        await containerAPI.SaveRequest(request);
        savedRequests.Add(request);

        this.StateHasChanged();
    }
    private string GetRouteString(ActionView action)
    {
        string route = action.Route;

        foreach (var param in action.Parameters)
        {
            if (param.Source == "Query")
            {
                route += $"?{param.Name}={param.TestValue}";
            }
            else if (param.Source == "Route")
            {
                int index = route.IndexOf($"{{{param.Name}:");
                if (index != -1)
                {
                    int endIndex = route.IndexOf("}", index);
                    route = route.Remove(index, endIndex - index + 1);
                    route = route.Insert(index, param.TestValue);
                }
            }
        }

        return route;
    }

    private string GetRequestCurlString(ActionView action)
    {
        string curl = $"curl -X {action.HttpMethod.ToUpper()} \\\n" +
            $" '{httpClient.BaseAddress}{GetRouteString(action).Substring(1)}' \\\n";

        curl += " -H 'Accept: text/plain' \\\n";

        if (action.TestRequestBody != string.Empty)
        {
            curl += " -H 'Content-Type: application/json' \\\n";
            curl += $" -d '{action.TestRequestBody}'";
        }

        return curl;
    }

    private async Task ReadResponse(HttpResponseMessage response, ActionView action)
    {
        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            if (IsValidJson(responseBody))
            {
                action.TestResponseBody = await FormatJson(responseBody);
            }
            else
            {
                action.TestResponseBody = responseBody;
            }
        }
        else
        {
            action.TestResponseBody = $"Error: {response.StatusCode} \n" +
                $"{responseBody.ToString()}";
        }

        action.TestResponseHeaders = response.Content.Headers.ToString();
        action.TestResponseStatusCode = response.ToString().Substring(ToString().IndexOf("StatusCode: ") + 13, 3);
        action.TestResponseStatusDescription = response.ReasonPhrase;
        action.TestRequestCurl = GetRequestCurlString(action);
    }

    private void TryOut(ActionView action)
    {
        action.TryOut = !action.TryOut;

        action.TestResponseBody = string.Empty;
        action.TestResponseHeaders = string.Empty;
        action.TestResponseStatusCode = string.Empty;
        action.TestResponseStatusDescription = string.Empty;
    }

    private bool IsValidJson(string json)
    {
        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private void AssignHeaders(ActionView action, HttpRequestMessage request)
    {
        foreach (var header in headers.Where(h => h.Action == action.Name))
        {
            if (string.IsNullOrEmpty(header.Key) || string.IsNullOrEmpty(header.Value))
            {
                header.Error = true;
                return;
            }
            else
            {
                header.Error = false;
            }

            request.Headers.Add(header.Key, header.Value);
        }
    }

    private (string Primary, string ListItemStyle) GetActionStyle(string httpMethod)
    {
        string primary = "";
        string listItemStyle = "";

        switch (httpMethod)
        {
            case "Get":
                primary = "#61AFFE";
                listItemStyle = "background-color: rgba(97, 175, 254, 0.1); border: 2px solid #61AFFE;";
                break;
            case "Post":
                primary = "#49CC90";
                listItemStyle = "background-color: rgba(73, 204, 144, 0.1); border: 2px solid #49CC90;";
                break;
            case "Put":
                primary = "#FCA130";
                listItemStyle = "background-color: rgba(252, 161, 48, 0.1); border: 2px solid #FCA130;";
                break;
            case "Delete":
                primary = "#F93E3E";
                listItemStyle = "background-color: rgba(249, 62, 62, 0.1); border: 2px solid #F93E3E;";
                break;
            case "Patch":
                primary = "#50E3C2";
                listItemStyle = "background-color: rgba(80, 227, 194, 0.1); border: 2px solid #50E3C2;";
                break;
            default:
                primary = "";
                break;
        }

        return (primary, listItemStyle);
    }
}

public class Header
{
    public string Action { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public bool Error { get; set; } = false;
}