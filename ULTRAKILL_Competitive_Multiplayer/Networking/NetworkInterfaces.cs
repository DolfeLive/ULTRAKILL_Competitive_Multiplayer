using Steamworks;
using System;
using UnityEngine;

namespace ULTRAKILL_Competitive_Multiplayer;


public interface IGameEvent
{
    EventType Type { get; }
    EventCategory Category { get; }
    SteamId? SenderId { get; set; }
    uint Timestamp { get; set; }
}

[Serializable]
public class GameEvent<T> : IGameEvent where T : struct
{
    public EventType Type { get; set; }
    public EventCategory Category { get; set; }
    public SteamId? SenderId { get; set; }
    public uint Timestamp { get; set; }
    public T Data;

    public GameEvent(EventType type, EventCategory category, T data)
    {
        Type = type;
        Category = category;
        Data = data;
        Timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}

public interface IEventHandler<T> where T : struct
{
    void Handle(GameEvent<T> eventData);
}