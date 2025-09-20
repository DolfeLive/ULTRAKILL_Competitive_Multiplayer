namespace ULTRAKILL_Competitive_Multiplayer;

#region all player handlers
using Steamworks;
using System.Linq;
using ULTRAKILL_Competitive_Multiplayer;
using UnityEngine;

public class PlayerMoveHandler : IEventHandler<PlayerMoveData>
{
    private readonly MultiplayerStuff _multiplayer;

    public PlayerMoveHandler(MultiplayerStuff multiplayer)
    {
        _multiplayer = multiplayer;
    }

    public void Handle(GameEvent<PlayerMoveData> eventData)
    {
        if (!eventData.SenderId.HasValue) return;

        if (_multiplayer.representativeObjects.Any(p => p.Item1 == eventData.SenderId.Value))
        {
            var playerData = _multiplayer.representativeObjects.First(p => p.Item1 == eventData.SenderId.Value);
            var data = eventData.Data;
            playerData.Item3.Move(data.position, data.velocity, (byte)data.properties);
        }
    }
}

public class PlayerLookHandler : IEventHandler<LookData>
{
    private readonly MultiplayerStuff _multiplayer;

    public PlayerLookHandler(MultiplayerStuff multiplayer)
    {
        _multiplayer = multiplayer;
    }

    public void Handle(GameEvent<LookData> eventData)
    {
        if (!eventData.SenderId.HasValue) return;

        if (_multiplayer.representativeObjects.Any(p => p.Item1 == eventData.SenderId.Value))
        {
            var playerData = _multiplayer.representativeObjects.First(p => p.Item1 == eventData.SenderId.Value);
            playerData.Item3.Aim(eventData.Data.direction);
        }
    }
}

public class WeaponChangeHandler : IEventHandler<WeaponChangeData>
{
    private readonly MultiplayerStuff _multiplayer;

    public WeaponChangeHandler(MultiplayerStuff multiplayer)
    {
        _multiplayer = multiplayer;
    }

    public void Handle(GameEvent<WeaponChangeData> eventData)
    {
        if (!eventData.SenderId.HasValue) return;

        if (_multiplayer.representativeObjects.Any(p => p.Item1 == eventData.SenderId.Value))
        {
            var playerData = _multiplayer.representativeObjects.First(p => p.Item1 == eventData.SenderId.Value);
            var data = eventData.Data;
            Debug.Log($"Player {eventData.SenderId} changed weapon to {data.weapon}");
        }
    }
}

public class PunchHandler : IEventHandler<PunchBeginData>
{
    private readonly MultiplayerStuff _multiplayer;

    public PunchHandler(MultiplayerStuff multiplayer)
    {
        _multiplayer = multiplayer;
    }

    public void Handle(GameEvent<PunchBeginData> eventData)
    {
        if (!eventData.SenderId.HasValue) return;

        Debug.Log($"Player {eventData.SenderId} began punch in direction {eventData.Data.direction}");
    }
}
#endregion

#region Server Only Handlers
public class ShootRequestHandler : IEventHandler<ShootRequestData>
{
    private readonly MultiplayerStuff _multiplayer;

    public ShootRequestHandler(MultiplayerStuff multiplayer)
    {
        _multiplayer = multiplayer;
    }

    public void Handle(GameEvent<ShootRequestData> eventData)
    {
        if (!eventData.SenderId.HasValue) return;

        var data = eventData.Data;
        bool isValidShot = ValidateShot(eventData.SenderId.Value, data);

        if (isValidShot)
        {
            uint projId = GenerateProjectileId();
            _multiplayer.SpawnProjectile(projId, data.source, data.direction, GetProjectileType(data.weaponType), eventData.SenderId.Value);
        }

        Debug.Log($"Shoot request from {eventData.SenderId}: {(isValidShot ? "approved" : "denied")}");
    }

    private bool ValidateShot(SteamId playerId, ShootRequestData data)
    {
        // implement
        return true;
    }

    private uint GenerateProjectileId()
    {
        return (uint)UnityEngine.Random.Range(1000000, 9999999);
    }

    private ProjectileType GetProjectileType(WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Revolver => ProjectileType.Bullet,
            WeaponType.RocketLauncher => ProjectileType.Rocket,
            _ => ProjectileType.Bullet
        };
    }
}

public class CoinThrowHandler : IEventHandler<CoinThrowData>
{
    private readonly MultiplayerStuff _multiplayer;

    public CoinThrowHandler(MultiplayerStuff multiplayer)
    {
        _multiplayer = multiplayer;
    }

    public void Handle(GameEvent<CoinThrowData> eventData)
    {
        if (!eventData.SenderId.HasValue) return;

        var data = eventData.Data;
        // add cooldown tracking
        _multiplayer.SpawnProjectile(data.coinId, data.coinPos, data.coinVel, ProjectileType.Coin, eventData.SenderId.Value);
        Debug.Log($"Coin thrown by {eventData.SenderId}");
    }
}

public class ParryHandler : IEventHandler<ParryData>
{
    private readonly MultiplayerStuff _multiplayer;

    public ParryHandler(MultiplayerStuff multiplayer)
    {
        _multiplayer = multiplayer;
    }

    public void Handle(GameEvent<ParryData> eventData)
    {
        if (!eventData.SenderId.HasValue) return;

        var data = eventData.Data;
        bool parrySuccess = ValidateParry(eventData.SenderId.Value, data);

        if (parrySuccess)
        {
            // return to people that it worked
            Debug.Log($"Successful parry by {eventData.SenderId}!");
        }
    }

    private bool ValidateParry(SteamId playerId, ParryData data)
    {
        // check if player is within parry range, proj existts, etc
        return true;
    }
}

#endregion