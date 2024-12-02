using ContainerControlPanel.Domain.Models;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public static class ContainerWebSocketHandler
{
    private static List<WebSocket> _clients = new();

    public static async Task HandleWebSocketAsync(WebSocket webSocket)
    {
        _clients.Add(webSocket);

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var buffer = new byte[1024 * 4];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _clients.Remove(webSocket);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
            }
        }
        catch
        {
            _clients.Remove(webSocket);
        }
    }

    public static async Task BroadcastContainersAsync(List<Container> containers)
    {
        var message = JsonSerializer.Serialize(containers);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        foreach (var client in _clients.ToList())
        {
            if (client.State == WebSocketState.Open)
            {
                await client.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
