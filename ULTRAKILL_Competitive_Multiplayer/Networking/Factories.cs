using Steamworks;
using System;
using UnityEngine;

namespace ULTRAKILL_Competitive_Multiplayer;

public static class ToEveryoneEvents
{
    public static GameEvent<PlayerMoveData> PlayerMove(Vector3 pos, Vector3 vel, PlayerProperties props)
        => new(EventType.PlayerMove, EventCategory.ToEveryone, new PlayerMoveData(pos, vel, props));

    public static GameEvent<LookData> PlayerLook(Vector3 dir)
        => new(EventType.PlayerLook, EventCategory.ToEveryone, new LookData(dir));

    public static GameEvent<WeaponChangeData> WeaponChange(string weapon)
        => new(EventType.WeaponChange, EventCategory.ToEveryone, new WeaponChangeData(weapon));

    public static GameEvent<PunchBeginData> PunchBegin(Vector3 dir, byte type)
        => new(EventType.PunchBegin, EventCategory.ToEveryone, new PunchBeginData(dir, type));
}

public static class ToServerEvents
{
    public static GameEvent<CoinThrowData> CoinThrow(Vector3 dir, Vector3 playerVel, uint coinId)
        => new(EventType.CoinThrow, EventCategory.ToServer, new CoinThrowData(dir, playerVel, coinId));

    public static GameEvent<ParryData> Parry(Vector3 dir, uint projId, ParryType type)
        => new(EventType.Parry, EventCategory.ToServer, new ParryData(dir, projId, type));

    public static GameEvent<ShootRequestData> ShootRequest(Vector3 src, Vector3 dir, WeaponType weapon, byte variation, byte charge = 0, bool alt = false)
        => new(EventType.ShootRequest, EventCategory.ToServer, new ShootRequestData(src, dir, weapon, variation, charge, alt));
}

public static class ToPlayersEvents
{
    public static GameEvent<ArenaChangeData> ArenaChange(byte index, byte seed)
        => new(EventType.ArenaChange, EventCategory.ToPlayers, new ArenaChangeData(index, seed));

    public static GameEvent<BulletHitData> BulletHit(SteamId playerId, byte damage, uint projId, Vector3 hitPoint)
        => new(EventType.BulletHit, EventCategory.ToPlayers, new BulletHitData(playerId, damage, projId, hitPoint));

    public static GameEvent<ExplosionSpawnData> ExplosionSpawn(Vector3 pos, float size, byte type = 0)
        => new(EventType.ExplosionSpawn, EventCategory.ToPlayers, new ExplosionSpawnData(pos, size, type));

    public static GameEvent<ProjectileSpawnData> ProjectileSpawn(uint id, Vector3 pos, Vector3 vel, ProjectileType type, SteamId owner)
        => new(EventType.ProjectileSpawn, EventCategory.ToPlayers, new ProjectileSpawnData(id, pos, vel, type, owner));

    public static GameEvent<ProjectileMoveData> ProjectileMove(uint id, Vector3 pos, Vector3 vel)
        => new(EventType.ProjectileMove, EventCategory.ToPlayers, new ProjectileMoveData(id, pos, vel));

    public static GameEvent<ProjectileDestroyData> ProjectileDestroy(uint id, byte reason, Vector3 pos)
        => new(EventType.ProjectileDestroy, EventCategory.ToPlayers, new ProjectileDestroyData(id, reason, pos));

    public static GameEvent<PlayerSpawnData> PlayerSpawn(SteamId playerId, Vector3 pos, byte spawnIndex)
        => new(EventType.PlayerSpawn, EventCategory.ToPlayers, new PlayerSpawnData(playerId, pos, spawnIndex));

    public static GameEvent<ShootConfirmData> ShootConfirm(uint shootId, Vector3 src, Vector3 dir, bool approved)
        => new(EventType.ShootConfirm, EventCategory.ToPlayers, new ShootConfirmData(shootId, src, dir, approved));
}