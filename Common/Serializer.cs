using System;
using System.Text.Json;
using TMPro;
using UnityEngine;

public class Serializer
{
    private static JsonSerializerOptions opts = new(JsonSerializerDefaults.Web);
    public static T Deserialize<T>(string doc)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(doc, opts);

        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to deserialize JSON: {doc} - {e.Message}");
            return default;
        }
    }

    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, opts);
    }
}
