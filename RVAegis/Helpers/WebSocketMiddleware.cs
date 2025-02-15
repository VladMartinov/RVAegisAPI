using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace RVAegis.Helpers
{
    public class WebSocketMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;
        private static readonly ConcurrentBag<WebSocket> _activeSockets = [];

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                _activeSockets.Add(webSocket);

                await HandleWebSocketConnection(webSocket);
            }
            else
            {
                await _next(context);
            }
        }

        private static async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;

            while (webSocket.State == WebSocketState.Open)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    _activeSockets.TryTake(out _);
                }
            }
        }

        public static async Task BroadcastJsonAsync(string jsonMessage)
        {
            var messageBytes = System.Text.Encoding.UTF8.GetBytes(jsonMessage);
            foreach (var socket in _activeSockets)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        public static bool HasActiveConnections()
        {
            return !_activeSockets.IsEmpty;
        }
    }
}