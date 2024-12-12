using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using ContainerControlPanel.Domain.Models;

namespace ContainerControlPanel.Web.Services;

public class WebSocketService
{
    private ClientWebSocket _webSocket;

    public event Action<TracesRoot>? TracesUpdated;

    public event Action<MetricsRoot>? MetricsUpdated;

    public async Task ConnectAsync(string uri)
    {
        _webSocket = new ClientWebSocket();
        await _webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);

        _ = ReceiveMessagesAsync(); // Rozpocznij odbieranie wiadomości
    }

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
                    var trace = JsonSerializer.Deserialize<TracesRoot>(message.Data.ToString());
                    TracesUpdated?.Invoke(trace);
                    break;

                case Domain.Models.WebSocketMessageType.Metrics:
                    var metrics = JsonSerializer.Deserialize<MetricsRoot>(message.Data.ToString());
                    MetricsUpdated?.Invoke(metrics);
                    break;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_webSocket != null)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            _webSocket.Dispose();
        }
    }
}
