using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static bool IsMultiplayerMode = true;

    void Awake()
    {
        if (IsMultiplayerMode)
        {
            gameObject.AddComponent<MultiplayerManager>();
        }
        else
        {
            Spawner.CreateSpawnPoint();
            Buttons.CreateLoadModelButton();
        }
    }

    public static async Task LoadModel()
    {
        var bytes = FilePicker.Pick();
        if (bytes == null)
        {
            Debug.LogError("No model selected");
            return;
        }

        var checksum = CheckSum.Hex(bytes);

        if (IsMultiplayerMode)
        {
            var r = await MultiplayerManager.SendModel(bytes);
            if (!r.IsOk)
            {
                Debug.LogError("Failed to send model to server");
                return;
            }
        }
        var scene = await Spawner.SpawnModelFromBytes(bytes, checksum, IsMultiplayerMode);
        if (scene == null)
        {
            Debug.LogError("Failed to spawn scene");
            return;
        }
        if (IsMultiplayerMode)
        {
            var serialized = Scene.FromGameObject(scene);
            var r = await MultiplayerManager.AddScene(GameRoom.Room().Id, serialized);
            if (!r.IsOk)
            {
                Debug.LogError("Failed to add scene to room");
                return;
            }
        }
    }
}