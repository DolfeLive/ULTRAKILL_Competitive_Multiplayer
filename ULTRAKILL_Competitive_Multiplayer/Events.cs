using Steamworks;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ULTRAKILL_Competitive_Multiplayer;

[Serializable]
public class SerializableVec3
{
    public float x;
    public float y;
    public float z;

    public SerializableVec3() { }

    public SerializableVec3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public SerializableVec3(Vector3 vec)
    {
        x = vec.x;
        y = vec.y;
        z = vec.z;
    }

    public Vector3 ToVec3()
    {
        return new Vector3(x, y, z);
    }
}

[Serializable]
public class SerializableVec4
{
    public float x;
    public float y;
    public float z;
    public float w;

    public SerializableVec4() { }

    public SerializableVec4(Vector3 vec)
    {
        x = vec.x;
        y = vec.y;
        z = vec.z;
        w = 0f;
    }

    public SerializableVec4(Vector4 vec)
    {
        x = vec.x;
        y = vec.y;
        z = vec.z;
        w = vec.w;
    }
    
    public Vector4 ToVec4()
    {
        return new Vector4(x, y, z, w);
    }
}

public static class Vec3Extentions
{
    public static SerializableVec3 ToSVec3(this Vector3 vec)
    {
        return new SerializableVec3(vec);
    }
}

#region To everyone

[Serializable]
public class PlayerMoveEvent
{
    public SerializableVec3 position;
    public SerializableVec3 velocity;
    public byte properties; // { jumping, dashing, SSJing, Sliding, Slamming }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="velocity"></param>
    /// <param name="properties">{ jumping, dashing, SSJing, Slidin, Slamming }</param>
    public PlayerMoveEvent(SerializableVec3 position, SerializableVec3 velocity, byte properties)
    {
        this.position = position;
        this.velocity = velocity;
        this.properties = properties;
    }
}

[Serializable]
public class LookEvent
{
    public SerializableVec3 Dir;
}



[Serializable]
public class WeaponChangeEvent
{
    public byte WeaponIndex;
    public byte VariationIndex;
}

/// <summary>
/// The server will still handle the hit reg
/// </summary>
[Serializable]
public class PunchBeginEvent
{
    public SerializableVec3 Dir;
    public byte Type;
}

//TODO Later: Emote Begin and gun property change

#endregion

#region Player To Server

[Serializable]
/// <summary>
/// The server will need to keep track of player coin cooldowns
/// </summary>
public class CoinThrowEvent
{
    public SerializableVec3 Dir;
    public SerializableVec3 PlayerVelocity;

}

/// <summary>
/// Parry
/// </summary>
[Serializable]
public class ParryEvent
{
    public SerializableVec3 Dir;
    public uint projId;
}

/// <summary>
/// handles all types of shooting, gun, shotgun, rocket
/// </summary>
[Serializable]
public class ShootEvent
{
    public SerializableVec3 Source;
    public SerializableVec3 Dir;
    public bool AltFire;
    public byte WeaponIndex;
    public byte VariationIndex;
}

/// <summary>
/// for jackhammer
/// </summary>
/*[Serializable]
public class JackhammerEvent
{
    public byte velocity;
    public byte VarIndex;
    public SerializableVec3 Dir;
}*/

#endregion

#region ServerToPlayers

[Serializable]
public class ArenaChangeEvent
{
    public byte Index;
    public byte RNGSeed;
}

[Serializable]
public class BuletHitEvent
{
    public SteamId HitPlayerID;
    public byte Damage;
    public uint projID;
}

[Serializable]
public class ExplosionSpawnEvent
{
    public SerializableVec4 details; // xyz: pos, w: size
}

[Serializable]
public class ProjSpawnEvent
{
    public uint ProjID;
    public SerializableVec3 Pos;
    public SerializableVec3 Velocity;
    public byte type;
}

[Serializable]
public class ProjDestroyEvent
{
    public uint ProjID;
    public byte type; // { Explode On destroy, Size }
    public SerializableVec3 Pos;
}

/// <summary>
/// Spawn/Die
/// </summary>
[Serializable]
public class PlayerExistEvent
{
    public SteamId playerId;
    public SerializableVec3 pos;
}

#endregion