using System.Collections.Generic;
using UnityEngine;

public class Session
{
    public string SessionId { get; set; }
    public string Token { get; set; }
}

public class Room
{
    public string Id { get; set; }
    public string OwnerID { get; set; }
    public string LockOwnerID { get; set; }
    public List<Scene> Scenes { get; set; }

    public bool IsUnlocked()
    {
        return LockOwnerID == null || LockOwnerID == "";
    }

    public bool IsLockedBy(string sessionId)
    {
        return LockOwnerID == sessionId;
    }
}

public class Scene
{
    public TransformJson Transform { get; set; }
    public string Checksum { get; set; }
    public List<Object> Objects { get; set; }
    public bool IsJoined { get; set; }
    static public Scene FromGameObject(GameObject g)
    {
        Scene scene = new()
        {
            Checksum = g.name,
            Transform = TransformJson.FromUnity(g.transform),
            Objects = new List<Object>(),
            IsJoined = g.GetComponent<ModelScene>().IsJoined
        };

        var children = GameObjectHelpers.GetChildren(g);

        foreach (var child in children)
        {
            var o = Object.FromGameObject(child);
            scene.Objects.Add(o);
        }

        Debug.Log(Serializer.Serialize(scene));

        return scene;
    }

    public void ApplyToGameObject(GameObject go)
    {
        go.transform.SetLocalPositionAndRotation(Transform.Position.ToUnity(), Transform.Rotation.ToUnity());
        go.transform.localScale = Transform.Scale.ToUnity();

        foreach (var obj in Objects)
        {
            var child = GameObjectHelpers.MustGetNamedChild(go, obj.Name);
            obj.Transform.ApplyToGameObject(child);
        }
    }
}

public class Object
{
    public TransformJson Transform { get; set; }
    public string Name { get; set; }

    static public Object FromGameObject(GameObject o)
    {
        return new Object
        {
            Transform = TransformJson.FromUnity(o.transform),
            Name = o.name
        };
    }
}

public class TransformJson
{
    public Vector3Json Position { get; set; }
    public Vector3Json Scale { get; set; }
    public QuaternionJson Rotation { get; set; }

    static public TransformJson FromUnity(Transform t)
    {
        return new TransformJson
        {
            Position = Vector3Json.FromUnity(t.localPosition),
            Rotation = QuaternionJson.FromUnity(t.localRotation),
            Scale = Vector3Json.FromUnity(t.localScale)
        };
    }

    public void ApplyToGameObject(GameObject go)
    {
        go.transform.SetLocalPositionAndRotation(Position.ToUnity(), Rotation.ToUnity());
        go.transform.localScale = Scale.ToUnity();
    }
}

public class Vector3Json
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    static public Vector3Json FromUnity(Vector3 v)
    {
        return new Vector3Json { X = v.x, Y = v.y, Z = v.z };
    }

    public Vector3 ToUnity()
    {
        return new Vector3(X, Y, Z);
    }
}

public class Model
{
    public string Checksum { get; set; }
    public string RoomId { get; set; }
    public string UploaderId { get; set; }
    public float SizeMb { get; set; }
}

public class QuaternionJson
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float W { get; set; }

    public static QuaternionJson FromUnity(Quaternion q)
    {
        return new QuaternionJson { X = q.x, Y = q.y, Z = q.z, W = q.w };
    }

    public Quaternion ToUnity()
    {
        return new Quaternion(X, Y, Z, W);
    }
}