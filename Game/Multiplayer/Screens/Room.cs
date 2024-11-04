using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameRoom
{
    private static Session session;
    private static Room room;

    public static Room Room() => room;

    public static void Open(Session session, Room room)
    {
        Close();

        GameRoom.session = session;
        GameRoom.room = room;

        SetupLoadModelButton();
        SetupLockButton();
        SetupLeaveRoomButton();
        Spawner.CreateSpawnPoint();
    }

    public static void Close()
    {
        GameObject.Destroy(GameObject.Find(Buttons.LoadModelButton));
        GameObject.Destroy(GameObject.Find(Buttons.LockButton));
        GameObject.Destroy(GameObject.Find(Buttons.LeaveRoomButton));
        GameObject.Destroy(GameObject.Find("SpawnPoint"));
    }

    private static void SetupLoadModelButton()
    {
        if (room.IsLockedBy(session.SessionId))
        {
            var b = Buttons.CreateLoadModelButton();
            b.GetComponent<Button>().onClick.RemoveAllListeners();
            b.GetComponent<Button>().onClick.AddListener(() => _ = GameManager.LoadModel());
        }
        else
        {
            GameObject.Destroy(GameObject.Find("LoadModelButton"));
        }
    }

    private static void SetupLockButton()
    {
        if (room.IsLockedBy(session.SessionId))
        {
            var b = Buttons.CreateLockButton();
            b.GetComponentInChildren<TextMeshProUGUI>().text = "Unlock";
            b.GetComponent<Button>().onClick.RemoveAllListeners();
            b.GetComponent<Button>().onClick.AddListener(() => _ = MultiplayerManager.UnlockRoom(room.Id));
        }
        else if (room.IsUnlocked())
        {
            var b = Buttons.CreateLockButton();
            b.GetComponentInChildren<TextMeshProUGUI>().text = "Lock";
            b.GetComponent<Button>().onClick.RemoveAllListeners();
            b.GetComponent<Button>().onClick.AddListener(() => _ = MultiplayerManager.LockRoom(room.Id));
        }
        else
        {
            GameObject.Destroy(GameObject.Find("LockButton"));
        }
    }

    private static void SetupLeaveRoomButton()
    {
        var b = Buttons.CreateLeaveRoomButton();
        b.GetComponent<Button>().onClick.AddListener(() => _ = MultiplayerManager.LeaveRoom(room.Id));
    }

    public static void SetLock(string lockOwnerId)
    {
        room.LockOwnerID = lockOwnerId;
        SetupLockButton();
        SetupLoadModelButton();
    }
}