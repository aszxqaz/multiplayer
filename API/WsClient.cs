using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class ApiWsClient
{
    private readonly ClientWebSocket wsClient = new();
    private Action<WsTransformBroadcast> onTransform;
    private Action<WsBroadcastScene> onScene;
    private Action<WsLockBroadcast> onLock;
    private Action<WsBroadcastToggleJoined> onToggleJoined;


    public async Task Connect(
        string token,
        Action<WsTransformBroadcast> onTransform,
        Action<WsBroadcastScene> onScene,
        Action<WsLockBroadcast> onLock,
        Action<WsBroadcastToggleJoined> onToggleJoined
    )
    {
        this.onTransform = onTransform;
        this.onScene = onScene;
        this.onLock = onLock;
        this.onToggleJoined = onToggleJoined;

        var uri = new Uri(ApiConfig.GetWsBaseUrl());
        await wsClient.ConnectAsync(uri, CancellationToken.None);
        var req = new WsAuthRequest(token);
        _ = Listen();
        await Send(req);
    }

    public async Task Listen()
    {
        while (wsClient.State == WebSocketState.Open)
        {
            UnityEngine.Debug.Log($"Receiving message...");
            var buffer = new byte[1 << 20];
            var result = await wsClient.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                UnityEngine.Debug.Log($"[WsClient] WebSocket received message {json}");
                var gen = Serializer.Deserialize<WsBroadcast>(json);

                // if (gen.RoomId != GameRoom.Room().Id)
                // {
                //     UnityEngine.Debug.LogWarning($"Received message from room {gen.RoomId}, ignoring");
                //     continue;  // Ignore messages from other rooms
                // }

                switch (gen.Method)
                {
                    case "transform":
                        var transformMsg = Serializer.Deserialize<WsTransformBroadcast>(json);
                        onTransform(transformMsg);
                        break;
                    case "scene":
                        var sceneMsg = Serializer.Deserialize<WsBroadcastScene>(json);
                        onScene(sceneMsg);
                        break;
                    case "lock":
                        var lockMsg = Serializer.Deserialize<WsLockBroadcast>(json);
                        onLock(lockMsg);
                        break;
                    case "toggleJoined":
                        var toggleJoinedMsg = Serializer.Deserialize<WsBroadcastToggleJoined>(json);
                        onToggleJoined(toggleJoinedMsg);
                        break;
                    default:
                        UnityEngine.Debug.LogWarning($"Unknown method in WsTransformBroadcast: {gen.Method}");
                        break;
                }
            }
        }
    }

    public async Task SendTransform(string roomId, string checksum, string name, TransformJson dimensions)
    {
        var req = new WsTransformRequest(roomId, checksum, name, dimensions);
        await Send(req);
    }

    private async Task Send<T>(T v)
    {
        var json = Serializer.Serialize(v);
        UnityEngine.Debug.Log($"[WsClient] WebSocket sending message {json}");
        var buffer = Encoding.UTF8.GetBytes(json);
        await wsClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
