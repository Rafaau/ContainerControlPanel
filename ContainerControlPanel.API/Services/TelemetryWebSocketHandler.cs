﻿using System.Net.WebSockets;
using System.Text;

public static class TelemetryWebSocketHandler
{
    private static readonly List<WebSocket> WebSockets = new();

    public static async Task HandleWebSocketConnectionAsync(WebSocket webSocket)
    {
        WebSockets.Add(webSocket);

        var buffer = new byte[40960 * 4];
        WebSocketReceiveResult result;

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received: {message}");
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
            }
        }
        finally
        {
            WebSockets.Remove(webSocket);
            if (webSocket.State != WebSocketState.Aborted)
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            webSocket.Dispose();
        }
    }

    public static async Task BroadcastMessageAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        var tasks = WebSockets.Where(ws => ws != null && ws.State == WebSocketState.Open)
                               .Select(ws => ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None));
        await Task.WhenAll(tasks);
    }
}
