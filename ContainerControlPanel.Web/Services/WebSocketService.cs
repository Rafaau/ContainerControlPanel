using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using ContainerControlPanel.Domain.Models;

namespace ContainerControlPanel.Web.Services;

/// <summary>
/// Class of WebSocket service.
/// </summary>
public class WebSocketService
{
    /// <summary>
    /// WebSocket instance.
    /// </summary>
    private ClientWebSocket _webSocket;

    /// <summary>
    /// Event for traces updated.
    /// </summary>
    public event Action<Trace>? TracesUpdated;

    /// <summary>
    /// Event for metrics updated.
    /// </summary>
    public event Action<Metrics>? MetricsUpdated;

    /// <summary>
    /// Event for logs updated.
    /// </summary>
    public event Action<Log>? LogsUpdated;

    /// <summary>
    /// Event for command output updated.
    /// </summary>
    public event Action<string>? CommandOutputUpdated;

    /// <summary>
    /// Connects to the WebSocket.
    /// </summary>
    /// <param name="uri">URI string.</param>
    /// <returns>Returns a task.</returns>
    public async Task ConnectAsync(string uri)
    {
        _webSocket = new ClientWebSocket();
        await _webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);

        _ = ReceiveMessagesAsync();
    }

    /// <summary>
    /// Handles the received messages.
    /// </summary>
    /// <returns>Returns a task.</returns>
    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[40960 * 4];

        while (_webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

            var message = JsonSerializer.Deserialize<WebSocketMessage>(json);

            switch (message.Type)
            {
                case Domain.Models.WebSocketMessageType.Traces:
                    var trace = JsonSerializer.Deserialize<Trace>(message.Data.ToString());
                    TracesUpdated?.Invoke(trace);
                    break;

                case Domain.Models.WebSocketMessageType.Metrics:
                    var metrics = JsonSerializer.Deserialize<Metrics>(message.Data.ToString());
                    MetricsUpdated?.Invoke(metrics);
                    break;

                case Domain.Models.WebSocketMessageType.Logs:
                    var logs = JsonSerializer.Deserialize<Log>(message.Data.ToString());
                    LogsUpdated?.Invoke(logs);
                    break;

                case Domain.Models.WebSocketMessageType.CommandOutput:
                    CommandOutputUpdated?.Invoke(message.Data.ToString());
                    break;
            }
        }
    }

    /// <summary>
    /// Disposes the WebSocket.
    /// </summary>
    /// <returns>Returns a task.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            _webSocket.Dispose();
        }
    }
}
