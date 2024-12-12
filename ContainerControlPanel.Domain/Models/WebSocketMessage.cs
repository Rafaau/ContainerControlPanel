using System;
namespace ContainerControlPanel.Domain.Models;

public class WebSocketMessage
{
    public WebSocketMessageType Type { get; set; }
    public object? Data { get; set; }
}

public enum WebSocketMessageType
{
    Traces,
    Metrics
}
