using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ULTRAKILL_Competitive_Multiplayer;

public class NetworkEventDispatcher
{
    private readonly Dictionary<EventType, List<object>> _handlers = new();

    public void Subscribe<T>(EventType eventType, IEventHandler<T> handler) where T : struct
    {
        if (!_handlers.ContainsKey(eventType))
            _handlers[eventType] = new List<object>();

        _handlers[eventType].Add(handler);
    }

    //public void SendEvent<T>(GameEvent<T> gameEvent) where T : struct
    //{
    //    switch (gameEvent.Category)
    //    {
    //        case EventCategory.ToEveryone:
    //            BroadcastToAll(gameEvent);
    //            break;
    //        case EventCategory.ToServer:
    //            SendToServer(gameEvent);
    //            break;
    //        case EventCategory.ToPlayers:
    //            SendToClients(gameEvent);
    //            break;
    //    }
    //}

    public void HandleEvent<T>(GameEvent<T> gameEvent) where T : struct
    {
        if (_handlers.TryGetValue(gameEvent.Type, out var handlers))
        {
            foreach (var handler in handlers.OfType<IEventHandler<T>>())
            {
                handler.Handle(gameEvent);
            }
        }
    }

    //// Network sending methods (implement with your networking solution)
    //private void BroadcastToAll<T>(GameEvent<T> gameEvent) where T : struct
    //{
    //    // Serialize and send to all connected players
    //    var serialized = SerializeEvent(gameEvent);
    //    // NetworkManager.BroadcastToAll(serialized);
    //}

    //private void SendToServer<T>(GameEvent<T> gameEvent) where T : struct
    //{
    //    // Serialize and send to server
    //    var serialized = SerializeEvent(gameEvent);
    //    // NetworkManager.SendToServer(serialized);
    //}

    //private void SendToClients<T>(GameEvent<T> gameEvent) where T : struct
    //{
    //    // Serialize and send to all clients (server only)
    //    var serialized = SerializeEvent(gameEvent);
    //    // NetworkManager.SendToClients(serialized);
    //}

    //private byte[] SerializeEvent<T>(GameEvent<T> gameEvent) where T : struct
    //{
        
    //    // Implement your serialization logic here
    //    // Could use JSON, MessagePack, or custom binary serialization
    //    return new byte[0]; // Placeholder
    //}
}

