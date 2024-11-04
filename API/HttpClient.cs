using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class ApiHttpClient
{
    private readonly HttpClient httpClient = new();
    private Session session;

    public ApiHttpClient()
    {
        SetupHttpClient();
    }

    public async Task<Result<bool, Error>> Connect()
    {
        var r = await CreateSession();
        if (r.IsOk)
        {
            session = r.Value;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
            return Result<bool, Error>.Ok(true);
        }
        return Result<bool, Error>.Err(r.Error);
    }

    public async Task<Result<List<Room>, Error>> FetchRooms()
    {
        return await TryExecRequest(async () =>
        {
            var rsp = await httpClient.GetAsync("/rooms");
            var content = await rsp.Content.ReadAsStringAsync();
            Log($"FetchRooms response: status code = {rsp.StatusCode}, content = {content}");
            if (rsp.IsSuccessStatusCode)
            {
                var rooms = Serializer.Deserialize<List<Room>>(content);
                return Result<List<Room>, Error>.Ok(rooms);
            }
            return Result<List<Room>, Error>.Err(new Error(0, content));
        });
    }

    public async Task<Result<Room, Error>> JoinRoom(string roomId)
    {
        return await TryExecRequest(async () =>
        {
            var rsp = await httpClient.PostAsync($"/rooms/{roomId}/sessions", null);
            var content = await rsp.Content.ReadAsStringAsync();
            Log($"JoinRoom response: status code = {rsp.StatusCode}, content = {content}");
            if (rsp.IsSuccessStatusCode)
            {
                var room = Serializer.Deserialize<Room>(content);
                return Result<Room, Error>.Ok(room);
            }
            return Result<Room, Error>.Err(new Error(0, content));
        });
    }

    public async Task<Result<Null, Error>> LeaveRoom(string roomId)
    {
        return await TryExecRequest(async () =>
        {
            var rsp = await httpClient.DeleteAsync($"/rooms/{roomId}/sessions");
            var content = await rsp.Content.ReadAsStringAsync();
            Log($"LeaveRoom response: status code = {rsp.StatusCode}, content = {content}");
            if (rsp.IsSuccessStatusCode)
            {
                return Result<Null, Error>.Ok(null);
            }
            return Result<Null, Error>.Err(new Error(0, content));
        });
    }

    public async Task<Result<Room, Error>> CreateRoom()
    {
        return await TryExecRequest(async () =>
        {
            var rsp = await httpClient.PostAsync("/rooms", null);
            var content = await rsp.Content.ReadAsStringAsync();
            Log($"CreateRoom response: status code = {rsp.StatusCode}, content = {content}");
            if (rsp.IsSuccessStatusCode)
            {
                var room = Serializer.Deserialize<Room>(content);
                return Result<Room, Error>.Ok(room);
            }
            return Result<Room, Error>.Err(new Error(0, content));
        });
    }

    public async Task<Result<byte[], Error>> FetchModelBytes(string checksum)
    {
        return await TryExecRequest(async () =>
        {
            var rsp = await httpClient.GetAsync($"/models/{checksum}/bytes");
            var bytes = await rsp.Content.ReadAsByteArrayAsync();
            Log($"FetchModelBytes response: status code = {rsp.StatusCode}");
            if (rsp.IsSuccessStatusCode)
            {
                return Result<byte[], Error>.Ok(bytes);
            }
            var msg = System.Text.Encoding.UTF8.GetString(bytes);
            Log($"FetchModelBytes response: error message = {msg}");
            return Result<byte[], Error>.Err(new Error(0, msg));
        });
    }

    public async Task<Result<Model, Error>> PostModelBytes(byte[] bytes)
    {
        return await TryExecRequest(async () =>
        {
            var rsp = await httpClient.PostAsync("/models", new ByteArrayContent(bytes));
            var content = await rsp.Content.ReadAsStringAsync();
            Log($"PostModelBytes response: status code = {rsp.StatusCode}, content = {content}");
            if (rsp.IsSuccessStatusCode)
            {
                var model = Serializer.Deserialize<Model>(content);
                return Result<Model, Error>.Ok(model);
            }
            return Result<Model, Error>.Err(new Error(0, content));
        });
    }

    public async Task<Result<Null, Error>> AddScene(string roomId, Scene scene)
    {
        Debug.Log("Adding scene 2");
        return await TryExecRequest(async () =>
        {
            var body = Serializer.Serialize(scene);
            var rsp = await httpClient.PostAsync($"/rooms/{roomId}/scenes", new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
            var content = await rsp.Content.ReadAsStringAsync();
            Log($"AddScene response: status code = {rsp.StatusCode}, content = {content}");
            if (rsp.IsSuccessStatusCode)
            {
                return Result<Null, Error>.Ok(null);
            }
            return Result<Null, Error>.Err(new Error(0, content));
        });
    }

    public async Task<Result<Null, Error>> LockRoom(string roomId)
    {
        return await TryExecRequest(async () =>
        {
            var rsp = await httpClient.PostAsync($"/rooms/{roomId}/lock", null);
            var content = await rsp.Content.ReadAsStringAsync();
            Log($"LockRoom response: status code = {rsp.StatusCode}, content = {content}");
            if (rsp.IsSuccessStatusCode)
            {
                return Result<Null, Error>.Ok(null);
            }
            return Result<Null, Error>.Err(new Error(0, content));
        });
    }

    public async Task<Result<Null, Error>> UnlockRoom(string roomId)
    {
        return await TryExecRequest(async () =>
        {
            var rsp = await httpClient.DeleteAsync($"/rooms/{roomId}/lock");
            var content = await rsp.Content.ReadAsStringAsync();
            Log($"UnlockRoom response: status code = {rsp.StatusCode}, content = {content}");
            if (rsp.IsSuccessStatusCode)
            {
                return Result<Null, Error>.Ok(null);
            }
            return Result<Null, Error>.Err(new Error(0, content));
        });
    }

    private async Task<Result<Session, Error>> CreateSession()
    {
        return await TryExecRequest(async () =>
        {
            var rsp = await httpClient.PostAsync("/sessions", null);
            var content = await rsp.Content.ReadAsStringAsync();
            Log($"CreateSession response: status code = {rsp.StatusCode}, content = {content}");
            if (rsp.IsSuccessStatusCode)
            {
                session = Serializer.Deserialize<Session>(content);
                return Result<Session, Error>.Ok(session);
            }
            return Result<Session, Error>.Err(new Error(0, content));
        });
    }

    public async Task<Result<Null, Error>> JoinScene(string roomId, string checksum)
    {
        return await TryExecRequest(async () =>
        {
            var rsp = await httpClient.PostAsync($"/rooms/{roomId}/scenes/{checksum}/join", null);
            var content = await rsp.Content.ReadAsStringAsync();
            Log($"JoinScene response: status code = {rsp.StatusCode}, content = {content}");
            if (rsp.IsSuccessStatusCode)
            {
                return Result<Null, Error>.Ok(null);
            }
            return Result<Null, Error>.Err(new Error(0, content));
        });
    }

    public async Task<Result<Null, Error>> UnjoinScene(string roomId, string checksum)
    {
        return await TryExecRequest(async () =>
        {
            var rsp = await httpClient.DeleteAsync($"/rooms/{roomId}/scenes/{checksum}/join");
            var content = await rsp.Content.ReadAsStringAsync();
            Log($"UnjoinScene response: status code = {rsp.StatusCode}, content = {content}");
            if (rsp.IsSuccessStatusCode)
            {
                return Result<Null, Error>.Ok(null);
            }
            return Result<Null, Error>.Err(new Error(0, content));
        });
    }

    private async Task<Result<T, Error>> TryExecRequest<T>(Func<Task<Result<T, Error>>> fn)
    {
        try
        {
            return await fn();
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"Failed to execute request: {e}");
            return Result<T, Error>.Err(new Error(0, e.Message));
        }
    }

    public Session GetSession() => session;

    private void SetupHttpClient()
    {
        httpClient.BaseAddress = new System.Uri(ApiConfig.GetHttpBaseUrl());
        httpClient.Timeout = TimeSpan.FromSeconds(2);
    }

    private void Log(object message)
    {
        Debug.Log($"[ApiClient] {message}");
    }
}

