using System;
using UnityEngine;
using System.Net.WebSockets;
using System.Threading;

namespace Multishock.API
{
    public class MultiShockAPI
    {
        private ClientWebSocket? websocket;
        private Uri? serverUri;

        public event Action<string>? MessageReceived;
        public event Action<string>? ConnectionClosed;
        public event Action<string>? LogMessage;
        public event Action<string>? LogError;

        public async void ConnectToServer(Uri uri)
        {
            serverUri = uri;
            websocket = new ClientWebSocket();
            try
            {
                await websocket.ConnectAsync(serverUri, CancellationToken.None);
                LogMessage?.Invoke("WebSocket connection established.");
                StartListening();
            }
            catch (Exception e)
            {
                LogError?.Invoke($"WebSocket connection failed: {e.Message}");
            }
        }

        public async void DisconnectFromServer()
        {
            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                LogMessage?.Invoke("WebSocket connection closed.");
                ConnectionClosed?.Invoke("WebSocket connection closed.");
            }
        }

        public void SendShock(int intensity, int duration, string module, string evnt)
        {
            sendAction("shock", intensity, duration, module, evnt);
        }

        public void SendVibrate(int intensity, int duration, string module, string evnt)
        {
            sendAction("vibrate", intensity, duration, module, evnt);
        }

        public void SendBeep(int intensity, int duration, string module, string evnt)
        {
            sendAction("beep", intensity, duration, module, evnt);
        }

        private void sendAction(string action, int intensity, int duration, string module, string evnt)
        {
            string value = $"{{\"action\": \"{action}\", \"intensity\": {intensity}, \"duration\": {duration}, \"module\": \"{module}\", \"evnt\": \"{evnt}\"}}";
            SendCommand("action", value);
        }

        private void SendCommand(string command, string value)
        {
            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                string cmd = $"{{\"cmd\": \"{command}\", \"value\": {value}}}";
                SendMessageToServer(cmd);
            }
            else
            {
                LogError?.Invoke("WebSocket connection is not open.");
            }
        }


        public async void SendMessageToServer(string message)
        {
            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
                try
                {
                    await websocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    LogMessage?.Invoke("Message sent successfully.");
                }
                catch (Exception e)
                {
                    LogError?.Invoke($"Failed to send message: {e.Message}");
                }
            }
            else
            {
                LogError?.Invoke("WebSocket connection is not open.");
            }
        }

        private async void StartListening()
        {
            try
            {
                byte[] buffer = new byte[1024];
                while (websocket != null && websocket.State == WebSocketState.Open)
                {
                    var result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                        MessageReceived?.Invoke(message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        ConnectionClosed?.Invoke("WebSocket connection closed by server.");
                    }
                }
            }
            catch (WebSocketException ex)
            {
                LogError?.Invoke($"WebSocket error: {ex.Message}");
            }
            catch (OperationCanceledException)
            {
                LogMessage?.Invoke("WebSocket connection closed.");
            }
        }
    }
}
