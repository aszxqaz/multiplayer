using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.XR.CoreUtils;
using UnityEngine;

public class GameObjectHelpers
{
    public static List<GameObject> GetChildren(GameObject go)
    {
        List<GameObject> children = new();
        foreach (Transform child in go.transform)
        {
            children.Add(child.gameObject);
        }
        return children;
    }

    public static GameObject MustFind(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go == null)
        {
            Debug.LogError($"Cannot find {name}");
            return null;
        }
        return go;
    }

    public static GameObject MustGetNamedChild(GameObject go, string name)
    {
        var r = go.GetNamedChild(name);
        if (r == null)
        {
            Debug.LogError($"Cannot get named child {name}");
            return null;
        }
        return r;
    }
}