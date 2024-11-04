public class WsMessage
{
    public string Method { get; set; }
}

public class WsBroadcast
{
    public string Method { get; set; }
}

public class WsAuthRequest : WsMessage
{
    public string Token { get; set; }

    public WsAuthRequest(string token)
    {
        Method = "auth";
        Token = token;
    }
}

public class WsTransformRequest : WsMessage
{
    public string SessionId { get; set; }
    public string RoomId { get; set; }
    public TransformJson Transform { get; set; }
    public string Checksum { get; set; }
    public string Name { get; set; }

    public WsTransformRequest(string roomId, string checksum, string name, TransformJson dimensions)
    {
        Method = "transform";
        RoomId = roomId;
        Checksum = checksum;
        Name = name;
        Transform = dimensions;
    }
}

public class WsTransformBroadcast : WsBroadcast
{
    public string SessionId { get; set; }
    public string RoomId { get; set; }
    public TransformJson Transform { get; set; }
    public string Checksum { get; set; }
    public string Name { get; set; }
}

public class WsBroadcastScene : WsBroadcast
{
    public string SessionId { get; set; }
    public string RoomId { get; set; }
    public Scene Scene { get; set; }
}

public class WsLockBroadcast : WsBroadcast
{
    public string SessionId { get; set; }
    public string RoomId { get; set; }
    public string LockOwnerId { get; set; }
}

public class WsBroadcastToggleJoined : WsBroadcast
{
    public string SessionId { get; set; }
    public string RoomId { get; set; }
    public bool IsJoined { get; set; }
    public string Checksum { get; set; }
}