using System;
using UnityEngine;

namespace ULTRAKILL_Competitive_Multiplayer;

[Serializable]
public class SerializableVec3
{
    public float x;
    public float y;
    public float z;

    public SerializableVec3() { }

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

#region To everyone

[Serializable]
public class GameEvent { }

[Serializable]
public class PlayerMoveEvent : GameEvent
{
    public SerializableVec3 position;
    public SerializableVec3 velocity;
}

#endregion