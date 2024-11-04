using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinRoomButtonManager
{
    private static UnityEngine.Vector3 StartPosition = new(0, 130, 0);
    private static UnityEngine.Vector3 OffsetPosition = new(0, -15, 0);
    private static readonly List<GameObject> Buttons = new();

    public static void ClearButtons()
    {
        foreach (var button in Buttons)
        {
            GameObject.Destroy(button);
        }
        Buttons.Clear();
    }

    public static void SetButtons(List<Room> rooms)
    {
        ClearButtons();
        var canvas = GameObject.Find("Canvas");
        var prefab = Resources.Load<GameObject>("Prefabs/JoinRoomButton");
        foreach (var room in rooms)
        {
            Buttons.Add(AddButton(prefab, canvas, room.Id));
        }
    }

    private static GameObject AddButton(GameObject prefab, GameObject canvas, string roomId)
    {

        var b = GameObject.Instantiate(prefab, canvas.transform);
        b.transform.localPosition = StartPosition + OffsetPosition * Buttons.Count;
        b.GetComponent<Button>().onClick.AddListener(() => _ = MultiplayerManager.JoinRoom(roomId));
        var tmp = b.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = roomId;
        Buttons.Add(b);
        return b;
    }
}