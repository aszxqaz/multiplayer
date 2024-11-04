using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Buttons
{
    private static GameObject prefab = Resources.Load<GameObject>("Prefabs/Button");

    public static GameObject CreateButton(string name, Vector2 pos, Vector2 size, string title)
    {
        var obj = GameObject.Find(name);
        if (obj != null)
        {
            Debug.LogWarning($"Button '{name}' already exists. Skipping creation.");
            return obj;
        }
        var prefab = Resources.Load<GameObject>("Prefabs/Button");
        var button = GameObject.Instantiate(prefab, GameObject.Find("Canvas").transform);
        button.name = name;
        var transform = button.GetComponent<RectTransform>();
        transform.anchoredPosition = pos;
        transform.sizeDelta = size;
        var text = button.GetComponentInChildren<TextMeshProUGUI>();
        text.text = title;
        return button;
    }

    public const string RefreshButton = "RefreshButton";
    public const string CreateRoomButton = "CreateRoomButton";
    public const string LoadModelButton = "LoadModelButton";
    public const string LockButton = "LockButton";
    public const string LeaveRoomButton = "LeaveRoomButton";

    public static GameObject CreateRefreshButton() =>
        CreateButton(RefreshButton, new Vector2(-240, 180), new Vector2(100, 30), "Refresh");

    public static GameObject CreateCreateRoomButton() =>
        CreateButton(CreateRoomButton, new Vector2(-240, 145), new Vector2(140, 30), "Create Room");

    public static GameObject CreateLoadModelButton() =>
        CreateButton(LoadModelButton, new Vector2(-220, -180), new Vector2(140, 30), "Load Model");

    public static GameObject CreateLockButton() =>
        CreateButton(LockButton, new Vector2(-220, -145), new Vector2(140, 30), "");

    public static GameObject CreateLeaveRoomButton() =>
        CreateButton(LeaveRoomButton, new Vector2(240, 180), new Vector2(100, 30), "Leave");
}

