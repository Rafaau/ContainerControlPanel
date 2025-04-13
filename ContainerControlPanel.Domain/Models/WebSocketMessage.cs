namespace ContainerControlPanel.Domain.Models;

/// <summary>
/// Class to represent a WebSocket message
/// </summary>
public class WebSocketMessage
{
    /// <summary>
    /// Gets or sets the type of the message
    /// </summary>
    public WebSocketMessageType Type { get; set; }

    /// <summary>
    /// Gets or sets the data of the message
    /// </summary>
    public object? Data { get; set; }
}

/// <summary>
/// Enum to represent the type of WebSocket message
/// </summary>
public enum WebSocketMessageType
{
    Traces,
    Metrics,
    Logs,
    CommandOutput
}
