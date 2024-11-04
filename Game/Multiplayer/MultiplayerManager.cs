using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum Stage
{
    Lobby,
    Room,
}

public enum GameAction
{
    Join,
    Leave,
    StartGame,
    Disconnect,
}

struct PendingTransformData
{
    public string checksum;
    public string name;
    public TransformJson transform;
    public double timestamp;
}

public class MultiplayerManager : MonoBehaviour
{
    private static Stage stage = Stage.Lobby;
    private static Room room;
    public static ApiHttpClient ApiClient = new();
    public static ApiWsClient WsClient = new();
    private static bool connected = false;
    private static readonly Dictionary<string, PendingTransformData> pendingTransforms = new();
    private static double throttleInterval = 100;
    private static double lastUpdateTime = 0;

    public static async Task<bool> Connect()
    {
        var res = await ApiClient.Connect();
        if (res.IsOk)
        {
            await WsClient.Connect(ApiClient.GetSession().Token, HandleTransform, HandleScene, HandleLock, HandleToggleJoined);
            connected = true;
        }
        else
        {
            Debug.LogError($"Failed to connect to the API: {res.Error.Message}");
        }
        return res.IsOk;
    }

    void Awake()
    {
        _ = FetchRoomsInitial();
    }

    async Task FetchRoomsInitial()
    {
        if (!await Connect())
        {
            return;
        }
        await FetchRooms();
    }

    public static async Task FetchRooms()
    {
        var res = await ApiClient.FetchRooms();
        if (res.IsOk)
        {
            Lobby.Open(res.Value);
        }
        else
        {
            Debug.LogError($"Failed to fetch rooms: {res.Error.Message}");
        }
    }

    public static async Task JoinRoom(string roomId)
    {
        var r = await ApiClient.JoinRoom(roomId);
        if (!r.IsOk)
        {
            Debug.LogError($"Failed to join room {roomId}: {r.Error.Message}");
            return;
        }
        var room = r.Value;
        Lobby.Close();
        GameRoom.Open(ApiClient.GetSession(), room);

        var models = new List<byte[]>();
        foreach (var scene in room.Scenes)
        {
            var bytesResp = await ApiClient.FetchModelBytes(scene.Checksum);
            if (!bytesResp.IsOk)
            {
                Debug.LogError($"Failed to fetch model bytes for scene {scene.Checksum}: {bytesResp.Error.Message}");
                continue;
            }
            models.Add(bytesResp.Value);
        }

        for (var i = 0; i < models.Count; i++)
        {
            var checksum = room.Scenes[i].Checksum;
            var scene = room.Scenes[i];
            var instScene = await Spawner.SpawnModelFromBytes(models[i], checksum, true);
            if (!instScene.TryGetComponent<ModelUpdatable>(out var sceneUpdatable))
            {
                Debug.LogError($"Failed to find ModelUpdatable component on scene {checksum}");
                continue;
            }
            sceneUpdatable.ScheduleForUpdate(scene.Transform);
            var instChildren = GameObjectHelpers.GetChildren(instScene);
            foreach (var instChild in instChildren)
            {
                foreach (var obj in scene.Objects)
                {
                    if (obj.Name == instChild.name)
                    {
                        if (!instChild.TryGetComponent<ModelUpdatable>(out var objUpdatable))
                        {
                            Debug.LogError($"Failed to find ModelUpdatable component on scene {checksum}");
                            continue;
                        }
                        objUpdatable.ScheduleForUpdate(obj.Transform);
                    }
                }
            }
        }
    }

    public static async Task CreateRoom()
    {
        var r = await ApiClient.CreateRoom();
        if (!r.IsOk)
        {
            Debug.LogError($"Failed to create room: {r.Error.Message}");
            return;
        }
        Lobby.Close();
        GameRoom.Open(ApiClient.GetSession(), r.Value);
    }

    public static async Task LockRoom(string roomId)
    {
        var r = await ApiClient.LockRoom(roomId);
        if (!r.IsOk)
        {
            Debug.LogError($"Failed to lock room {roomId}: {r.Error.Message}");
            return;
        }
        GameRoom.SetLock(ApiClient.GetSession().SessionId);
    }

    public static async Task UnlockRoom(string roomId)
    {
        var r = await ApiClient.UnlockRoom(roomId);
        if (!r.IsOk)
        {
            Debug.LogError($"Failed to unlock room {roomId}: {r.Error.Message}");
            return;
        }
        GameRoom.SetLock("");
    }

    public static async Task LeaveRoom(string roomId)
    {
        var r = await ApiClient.LeaveRoom(roomId);
        if (!r.IsOk)
        {
            Debug.LogError($"Failed to leave room {roomId}: {r.Error.Message}");
            return;
        }
        GameRoom.Close();
        await FetchRooms();
    }

    public static async Task PublishTransform(string checksum, string name, Transform t)
    {
        await WsClient.SendTransform(
            GameRoom.Room().Id,
            checksum,
            name,
            TransformJson.FromUnity(t)
        );
    }

    public static async Task<Result<Model, Error>> SendModel(byte[] bytes)
    {
        var r = await ApiClient.PostModelBytes(bytes);
        if (!r.IsOk)
        {
            Debug.LogError($"Failed to send model: {r.Error.Message}");
        }
        return r;
    }

    public static async Task<Result<Null, Error>> AddScene(string roomId, Scene scene)
    {
        var r = await ApiClient.AddScene(roomId, scene);
        if (!r.IsOk)
        {
            Debug.LogError($"Failed to add scene to room {roomId}: {r.Error.Message}");
        }
        return r;
    }

    private static void HandleTransform(WsTransformBroadcast message)
    {
        if (message.RoomId != GameRoom.Room().Id ||
            message.SessionId == ApiClient.GetSession().SessionId)
        {
            return;
        }

        var scene = GameObject.Find(message.Checksum);
        if (scene == null)
        {
            Debug.LogError($"Failed to find scene {message.Checksum}");
            return;
        }

        if (message.Name == null || message.Name == "")
        {
            scene.GetComponent<ModelUpdatable>().ScheduleForUpdate(message.Transform);
        }
        else
        {
            var children = GameObjectHelpers.GetChildren(scene);
            foreach (var child in children)
            {
                if (child.name == message.Name)
                {
                    child.GetComponent<ModelUpdatable>().ScheduleForUpdate(message.Transform);
                    break;
                }
            }
        }
    }

    private static async Task HandleSceneAsync(WsBroadcastScene message)
    {
        Debug.Log("HERE");

        if (message.RoomId != GameRoom.Room().Id || message.SessionId == ApiClient.GetSession().SessionId)
        {
            return;
        }

        var modelResult = await ApiClient.FetchModelBytes(message.Scene.Checksum);
        if (!modelResult.IsOk)
        {
            Debug.LogError($"Failed to fetch model bytes for scene {message.Scene.Checksum}: {modelResult.Error.Message}");
            return;
        }
        await Spawner.SpawnScene(modelResult.Value, message.Scene);
    }

    private static void HandleScene(WsBroadcastScene message)
    {
        _ = HandleSceneAsync(message);
    }

    private static void HandleLock(WsLockBroadcast message)
    {
        GameRoom.SetLock(message.LockOwnerId);
    }

    private static void HandleToggleJoined(WsBroadcastToggleJoined message)
    {
        Debug.Log("ToggleJoined handler");
        var scene = GameObject.Find(message.Checksum);
        if (scene == null)
        {
            Debug.LogError($"Failed to find scene {message.Checksum} (HandleToggleJoined)");
            return;
        }

        if (!scene.TryGetComponent<ModelScene>(out var modelScene))
        {
            Debug.LogError($"Failed to find ModelScene component on scene {message.Checksum} (HandleToggleJoined)");
            return;
        }

        if (message.IsJoined ^ modelScene.IsJoined)
        {
            modelScene.IsJoined = message.IsJoined;
        }
    }

    public static async void JoinScene(string checksum)
    {
        var roomId = GameRoom.Room().Id;
        var r = await ApiClient.JoinScene(roomId, checksum);
        if (!r.IsOk)
        {
            Debug.LogError($"Failed to join scene {checksum} in room {roomId}: {r.Error.Message}");
            return;
        }
    }

    public static async void UnjoinScene(string checksum)
    {
        var roomId = GameRoom.Room().Id;
        var r = await ApiClient.UnjoinScene(roomId, checksum);
        if (!r.IsOk)
        {
            Debug.LogError($"Failed to unjoin scene {checksum} in room {roomId}: {r.Error.Message}");
            return;
        }
    }
}
