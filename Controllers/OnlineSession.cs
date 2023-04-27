using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebSockets.Model;

namespace WebSockets.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OnlineSession : ControllerBase
    {
        private RoverData RoverData;

        public OnlineSession(RoverData roverData)
        {
            this.RoverData = roverData;
        }
        
        [HttpGet("/video")]
        public async Task Get()
        {
            if(HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Echo(webSocket);
            }
        }
        private static async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var sendBuffer = new byte[] { 54, 57 };
            await webSocket.SendAsync(
                    new ArraySegment<byte>(sendBuffer, 0, 2),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);

            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!receiveResult.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                    receiveResult.MessageType,
                    receiveResult.EndOfMessage,
                    CancellationToken.None);

                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }
    }
}