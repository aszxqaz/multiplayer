using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Lobby
{
    public static void Open(List<Room> rooms)
    {
        SetupJoinRoomButtons(rooms);
        SetupCreateRoomButton();
        SetupRefreshButton();
    }

    public static void Close()
    {
        JoinRoomButtonManager.ClearButtons();
        GameObject.Destroy(GameObject.Find(Buttons.RefreshButton));
        GameObject.Destroy(GameObject.Find(Buttons.CreateRoomButton));
    }

    private static void SetupJoinRoomButtons(List<Room> rooms)
    {
        JoinRoomButtonManager.SetButtons(rooms);
    }

    private static void SetupCreateRoomButton()
    {
        var b = Buttons.CreateCreateRoomButton();
        b.GetComponent<Button>().onClick.RemoveAllListeners();
        b.GetComponent<Button>().onClick.AddListener(() => _ = MultiplayerManager.CreateRoom());
    }

    private static void SetupRefreshButton()
    {
        var b = Buttons.CreateRefreshButton();
        b.GetComponent<Button>().onClick.RemoveAllListeners();
        b.GetComponent<Button>().onClick.AddListener(() => _ = Refresh());
    }

    private static async Task Refresh()
    {
        var res = await MultiplayerManager.ApiClient.FetchRooms();
        if (res.IsOk)
        {
            SetupJoinRoomButtons(res.Value);
        }
        else
        {
            Debug.LogError($"Failed to fetch rooms: {res.Error.Message}");
        }
    }
}
