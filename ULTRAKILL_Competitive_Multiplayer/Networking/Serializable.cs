using System;
using UnityEngine;
namespace ULTRAKILL_Competitive_Multiplayer;

[Serializable]
public struct SerializableVec3
{
    public float x, y, z;

    public SerializableVec3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public SerializableVec3(Vector3 vec) : this(vec.x, vec.y, vec.z) { }

    public static implicit operator Vector3(SerializableVec3 sv) => new Vector3(sv.x, sv.y, sv.z);
    public static implicit operator SerializableVec3(Vector3 v) => new SerializableVec3(v);

    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }
}

[Serializable]
public struct SerializableVec4
{
    public float x, y, z, w;

    public SerializableVec4(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public SerializableVec4(Vector4 vec) : this(vec.x, vec.y, vec.z, vec.w) { }
    public SerializableVec4(Vector3 vec, float w = 0f) : this(vec.x, vec.y, vec.z, w) { }

    public static implicit operator Vector4(SerializableVec4 sv) => new Vector4(sv.x, sv.y, sv.z, sv.w);
    public static implicit operator SerializableVec4(Vector4 v) => new SerializableVec4(v);

    public override string ToString()
    {
        return $"({x}, {y}, {z}, {w})";
    }
}