using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using ContainerControlPanel.Domain.Models;

namespace ContainerControlPanel.Web.Services;

public class WebSocketService
{
    private ClientWebSocket _webSocket;

    public event Action<Root>? TracesUpdated;

    public async Task ConnectAsync(string uri)
    {
        _webSocket = new ClientWebSocket();
        await _webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);

        _ = ReceiveMessagesAsync(); // Rozpocznij odbieranie wiadomości
    }

    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[1024 * 4];

        while (_webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

            var trace = JsonSerializer.Deserialize<Root>(json);
            TracesUpdated?.Invoke(trace);
            //var trace = JsonSerializer.Deserialize<Root>(json);
            //TracesUpdated?.Invoke(trace);
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
