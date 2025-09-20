//using Steamworks;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using UnityEngine;

//namespace ULTRAKILL_Competitive_Multiplayer;

//public class NetworkGameplayManager
//{
//    private NetworkEventDispatcher _eventDispatcher = new();

//    public void Initialize()
//    {
//        _eventDispatcher.Subscribe<PlayerMoveData>(EventType.PlayerMove, new PlayerMoveHandler());
//        _eventDispatcher.Subscribe<ShootRequestData>(EventType.ShootRequest, new ShootRequestHandler());
//        _eventDispatcher.Subscribe<BulletHitData>(EventType.BulletHit, new BulletHitHandler());
//    }

//    public void SendPlayerMovement(Vector3 pos, Vector3 vel, PlayerProperties props)
//    {
//        var moveEvent = ToEveryoneEvents.PlayerMove(pos, vel, props);
//        _eventDispatcher.SendEvent(moveEvent);
//    }

//    public void RequestShoot(Vector3 src, Vector3 dir, WeaponType weapon, byte variation)
//    {
//        var shootEvent = ToServerEvents.ShootRequest(src, dir, weapon, variation);
//        _eventDispatcher.SendEvent(shootEvent);
//    }

//    public void ConfirmBulletHit(SteamId playerId, byte damage, uint projId, Vector3 hitPoint)
//    {
//        var hitEvent = ToPlayersEvents.BulletHit(playerId, damage, projId, hitPoint);
//        _eventDispatcher.SendEvent(hitEvent);
//    }
//}

//public class PlayerMoveHandler : IEventHandler<PlayerMoveData>
//{
//    public void Handle(GameEvent<PlayerMoveData> eventData)
//    {
//        var data = eventData.Data;
//        Debug.Log($"Player {eventData.SenderId} moved to {data.position} with {data.properties}");
//    }
//}

//public class ShootRequestHandler : IEventHandler<ShootRequestData>
//{
//    public void Handle(GameEvent<ShootRequestData> eventData)
//    {
//        var data = eventData.Data;
//        Debug.Log($"Player {eventData.SenderId} wants to shoot {data.weaponType}");
//    }
//}

//public class BulletHitHandler : IEventHandler<BulletHitData>
//{
//    public void Handle(GameEvent<BulletHitData> eventData)
//    {
//        var data = eventData.Data;
//        Debug.Log($"Player {data.hitPlayerId} hit for {data.damage} damage");
//    }
//}