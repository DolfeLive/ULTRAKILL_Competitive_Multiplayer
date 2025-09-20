using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ULTRAKILL_Competitive_Multiplayer;

public enum EventCategory : byte
{
    ToEveryone = 0,    // To Everyone
    ToServer = 1,      // Client -> Server only
    ToPlayers = 2      // Server -> Clients only
}

public enum EventType : byte
{
    // To Everyone
    PlayerMove = 0,
    PlayerLook = 1,
    WeaponChange = 2,
    PunchBegin = 3,

    // Player To Server (Client -> Server)
    CoinThrow = 50,
    Parry = 51,
    ShootRequest = 52,

    // Server To Players (Server -> Clients)
    ArenaChange = 100,
    BulletHit = 101,
    ExplosionSpawn = 102,
    ProjectileSpawn = 103,
    ProjectileMove = 104,
    ProjectileDestroy = 105,
    PlayerSpawn = 106,
    PlayerDie = 107,
    ShootConfirm = 108
}

[Flags]
public enum PlayerProperties : byte
{
    None = 0,
    Jumping = 1 << 0,
    Dashing = 1 << 1,
    SSJing = 1 << 2,
    Sliding = 1 << 3,
    Slamming = 1 << 4
}

public enum WeaponType : byte
{
    Revolver = 0,
    Shotgun = 1,
    Nailgun = 2,
    Railcannon = 3,
    RocketLauncher = 4,
    Arm = 5
}

public enum ParryType : byte
{
    Projectile,
    Coin,
    Saw,
    Cannonball
}

public enum ProjectileType : ushort
{
    Bullet = 0,
    Rocket = 1,
    Cannonball = 2,
    Coin = 3,
    Core = 4,
    Saw = 5,
    Zapper = 6,
    Magnet = 7,
    Drill = 8,
    Whiplash = 9,
    Napalm = 10
}

#region To Everyone Events

[Serializable]
public struct PlayerMoveData
{
    public SerializableVec3 position;
    public SerializableVec3 velocity;
    public PlayerProperties properties;

    public PlayerMoveData(Vector3 pos, Vector3 vel, PlayerProperties props)
    {
        position = pos;
        velocity = vel;
        properties = props;
    }

    public override string ToString()
    {
        return $"PlayerMoveData(Position: {position}, Velocity: {velocity}, Properties: {properties})";
    }
}

[Serializable]
public struct LookData
{
    public SerializableVec3 direction;

    public LookData(Vector3 dir)
    {
        direction = dir;
    }

    public override string ToString()
    {
        return $"LookData(Direction: {direction})";
    }
}

[Serializable]
public struct WeaponChangeData
{
    public string weapon;

    public WeaponChangeData(string weapon)
    {
        this.weapon = weapon;
    }

    public override string ToString()
    {
        return $"WeaponChangeData(Weapon: {weapon})";
    }
}

[Serializable]
public struct PunchBeginData
{
    public SerializableVec3 direction;
    public byte punchType;

    public PunchBeginData(Vector3 dir, byte type)
    {
        direction = dir;
        punchType = type;
    }
    public override string ToString()
    {
        return $"PunchBeginData(Direction: {direction}, PunchType: {punchType})";
    }
}

#endregion

#region Player To Server Events

[Serializable]
public struct CoinThrowData
{
    public SerializableVec3 coinPos;
    public SerializableVec3 coinVel;
    public uint coinId;

    public CoinThrowData(Vector3 position, Vector3 velocity, uint id)
    {
        coinPos = new SerializableVec3(position);
        coinVel = new SerializableVec3(velocity);
        coinId = id;
    }

    public override string ToString()
    {
        return $"CoinThrowData(Pos: {coinPos}, Vel: {coinVel}, Id: {coinId})";
    }
}

[Serializable]
public struct ParryData
{
    public SerializableVec3 direction;
    public uint projId;
    public ParryType parryType;

    public ParryData(Vector3 dir, uint id, ParryType type)
    {
        direction = dir;
        projId = id;
        parryType = type;
    }

    public override string ToString()
    {
        return $"ParryData(Direction: {direction}, ParryType: {parryType}, Id: {projId})";
    }
}

[Serializable]
public struct ShootRequestData
{
    public SerializableVec3 source;
    public SerializableVec3 direction;
    public WeaponType weaponType;
    public byte variationIndex;
    public byte chargeValue;
    public bool altFire;

    public ShootRequestData(Vector3 src, Vector3 dir, WeaponType weapon, byte variation, byte charge = 0, bool alt = false)
    {
        source = src;
        direction = dir;
        weaponType = weapon;
        variationIndex = variation;
        chargeValue = charge;
        altFire = alt;
    }

    public override string ToString()
    {
        return $"ShootRequestData(Source: {source}, Direction: {direction}, Weapon: {weaponType}, Variation: {variationIndex}, Charge: {chargeValue}, AltFire: {altFire})";
    }
}

#endregion

#region Server To Players Events

[Serializable]
public struct ArenaChangeData
{
    public byte arenaIndex;
    public byte rngSeed;

    public ArenaChangeData(byte index, byte seed)
    {
        arenaIndex = index;
        rngSeed = seed;
    }

    public override string ToString()
    {
        return $"ArenaChangeData(ArenaIndex: {arenaIndex}, RNGSeed: {rngSeed})";
    }
}

[Serializable]
public struct BulletHitData
{
    public SteamId hitPlayerId;
    public byte damage;
    public uint projId;
    public SerializableVec3 hitPoint;

    public BulletHitData(SteamId playerId, byte dmg, uint id, Vector3 point)
    {
        hitPlayerId = playerId;
        damage = dmg;
        projId = id;
        hitPoint = point;
    }

    public override string ToString()
    {
        return $"BulletHitData(HitPlayerId: {hitPlayerId}, Damage: {damage}, Id: {projId}, HitPoint: {hitPoint})";
    }
}

[Serializable]
public struct ExplosionSpawnData
{
    public SerializableVec3 position;
    public float size;
    public byte explosionType;

    public ExplosionSpawnData(Vector3 pos, float sz, byte type = 0)
    {
        position = pos;
        size = sz;
        explosionType = type;
    }

    public override string ToString()
    {
        return $"ExplosionSpawnData(Position: {position}, Size: {size}, ExplosionType: {explosionType})";
    }
}

[Serializable]
public struct ProjectileSpawnData
{
    public uint projId;
    public SerializableVec3 position;
    public SerializableVec3 velocity;
    public ProjectileType projType;
    public SteamId ownerId;

    public ProjectileSpawnData(uint id, Vector3 pos, Vector3 vel, ProjectileType type, SteamId owner)
    {
        projId = id;
        position = pos;
        velocity = vel;
        projType = type;
        ownerId = owner;
    }

    public override string ToString()
    {
        return $"ProjectileSpawnData(Id: {projId}, Position: {position}, Velocity: {velocity}, ProjectileType: {projType}, OwnerId: {ownerId})";
    }
}

[Serializable]
public struct ProjectileMoveData
{
    public uint projId;
    public SerializableVec3 position;
    public SerializableVec3 velocity;

    public ProjectileMoveData(uint id, Vector3 pos, Vector3 vel)
    {
        projId = id;
        position = pos;
        velocity = vel;
    }
    public override string ToString()
    {
        return $"ProjectileMoveData(Id: {projId}, Position: {position}, Velocity: {velocity})";
    }
}

[Serializable]
public struct ProjectileDestroyData
{
    public uint projId;
    public byte destroyReason; // 0=timeout, 1=hit, 2=explode.
    public SerializableVec3 lastPosition;

    public ProjectileDestroyData(uint id, byte reason, Vector3 pos)
    {
        projId = id;
        destroyReason = reason;
        lastPosition = pos;
    }
    public override string ToString()
    {
        return $"ProjectileDestroyData(Id: {projId}, DestroyReason: {destroyReason}, LastPosition: {lastPosition})";
    }
}

[Serializable]
public struct PlayerSpawnData
{
    public SteamId playerId;
    public SerializableVec3 spawnPosition;
    public byte spawnIndex;

    public PlayerSpawnData(SteamId id, Vector3 pos, byte spawn)
    {
        playerId = id;
        spawnPosition = pos;
        spawnIndex = spawn;
    }
    public override string ToString()
    {
        return $"PlayerSpawnData(PlayerId: {playerId}, SpawnPosition: {spawnPosition}, SpawnIndex: {spawnIndex})";
    }
}

[Serializable]
public struct ShootConfirmData
{
    public uint shootId;
    public SerializableVec3 confirmedSource;
    public SerializableVec3 confirmedDirection;
    public bool approved;

    public ShootConfirmData(uint id, Vector3 src, Vector3 dir, bool valid)
    {
        shootId = id;
        confirmedSource = src;
        confirmedDirection = dir;
        approved = valid;
    }
    public override string ToString()
    {
        return $"ShootConfirmData(ShootId: {shootId}, ConfirmedSource: {confirmedSource}, ConfirmedDirection: {confirmedDirection}, Approved: {approved})";
    }
}

#endregion

public static class EventSecurity
{
    public static bool CanSendEvent(EventType eventType, bool isServer, bool isClient)
    {
        return eventType switch
        {
            // To Everyone
            EventType.PlayerMove or EventType.PlayerLook or
            EventType.WeaponChange or EventType.PunchBegin => true,

            // To Server
            EventType.CoinThrow or EventType.Parry or EventType.ShootRequest => isClient,

            // To Players
            EventType.ArenaChange or EventType.BulletHit or EventType.ExplosionSpawn or
            EventType.ProjectileSpawn or EventType.ProjectileMove or EventType.ProjectileDestroy or
            EventType.PlayerSpawn or EventType.PlayerDie or EventType.ShootConfirm => isServer,

            _ => false
        };
    }
}
