using MultiplayerUtil;
using Steamworks;
using Steamworks.ServerList;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UltraIDK;
using UnityEngine;
using MU = MultiplayerUtil;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace ULTRAKILL_Competitive_Multiplayer;

public class MultiplayerStuff : MonoBehaviour
{
    public static MultiplayerStuff Instance;
    public List<(SteamId, GameObject, Player)> representativeObjects = new();

    private NetworkEventDispatcher _eventDispatcher;
    private bool _isInitialized = false;

    public Scoreboard scoreboard;
    public static bool isLobbyOwner => MultiplayerUtil.LobbyManager.isLobbyOwner;

    private NewMovement _newMovement => NewMovement.instance;
    private bool NewMovementExists => _newMovement != null;

    private int _sentCount = 0;

    void Start()
    {
        Instance = this;
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
        gameObject.hideFlags = HideFlags.HideAndDontSave;

        InitializeEventSystem();
        SetupCallbacks();

        // jank ass shit to get the OnWeaponChange event
        var gunControl = GunControl.Instance;
        if (gunControl == null)
        {
            Debug.LogError("GunControl instance not found");
            return;
        }

        EventInfo eventInfo = typeof(GunControl).GetEvent("OnWeaponChange", BindingFlags.Public | BindingFlags.Instance);
        if (eventInfo == null)
        {
            Debug.LogError("OnWeaponChange event not found");
            return;
        }
        Action<GameObject> handler = WeaponChange;
        eventInfo.AddEventHandler(gunControl, handler);

        _isInitialized = true;
    }
    private void InitializeEventSystem()
    {
        _eventDispatcher = new NetworkEventDispatcher();

        _eventDispatcher.Subscribe<PlayerMoveData>(EventType.PlayerMove, new PlayerMoveHandler(this));
        _eventDispatcher.Subscribe<LookData>(EventType.PlayerLook, new PlayerLookHandler(this));
        _eventDispatcher.Subscribe<WeaponChangeData>(EventType.WeaponChange, new WeaponChangeHandler(this));
        _eventDispatcher.Subscribe<PunchBeginData>(EventType.PunchBegin, new PunchHandler(this));
        //if (isLobbyOwner)
        //{
        //    _eventDispatcher.Subscribe<ShootRequestData>(EventType.ShootRequest, new ShootRequestHandler(this));
        //    _eventDispatcher.Subscribe<CoinThrowData>(EventType.CoinThrow, new CoinThrowHandler(this));
        //    _eventDispatcher.Subscribe<ParryData>(EventType.Parry, new ParryHandler(this));
        //}
    }

    private void SetupCallbacks()
    {
        MU.Callbacks.TimeToSendImportantData.AddListener(SendImportantData);
        MU.Callbacks.TimeToSendUnimportantData.AddListener(SendUnimportantData);

        MU.ObserveManager.SubscribeToType(typeof(SerializedGameEvent), out var eventReceived);
        eventReceived.AddListener(OnEventReceived);

        SetupLobbyCallbacks();
    }

    private void SendImportantData()
    {
        if (!NewMovementExists || !CompMultiplayerMain.instance.inMultiplayerScene)
            return;

        try
        {
            var moveEvent = ToEveryoneEvents.PlayerMove(
                _newMovement.transform.position,
                _newMovement.rb.velocity,
                GetPlayerProperties()
            );
            SendGameEvent(moveEvent);

            var lookEvent = ToEveryoneEvents.PlayerLook(
                new Vector3(_newMovement.cc.rotationX, _newMovement.cc.rotationY, 0)
            );
            SendGameEvent(lookEvent);

            _sentCount++;
            if (_sentCount % 1000 == 0)
                Debug.Log($"Sent {_sentCount} movement updates");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send important data: {e.Message}");
        }
    }

    private void SendUnimportantData()
    {
        if (!isLobbyOwner) return;

        try
        {
            UpdateScoreboard();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send unimportant data: {e.Message}");
        }
    }

    private PlayerProperties GetPlayerProperties()
    {
        var props = PlayerProperties.None;
        if (_newMovement.jumping) props |= PlayerProperties.Jumping;
        if (_newMovement.boost) props |= PlayerProperties.Dashing;
        if (_newMovement.slamStorage) props |= PlayerProperties.SSJing;
        if (_newMovement.sliding) props |= PlayerProperties.Sliding;
        if (_newMovement.slamForce > 0.1f) props |= PlayerProperties.Slamming;
        return props;
    }

    private void OnEventReceived((byte[], SteamId?) data)
    {
        if (!data.Item2.HasValue || data.Item2.Value == LobbyManager.selfID)
            return;

        try
        {
            var eventData = Data.Deserialize<SerializedGameEvent>(data.Item1);
            if (eventData != null)
            {
                var gameEvent = DeserializeToGameEvent(eventData);
                if (gameEvent != null)
                {
                    gameEvent.SenderId = data.Item2.Value;

                    if (!EventSecurity.CanSendEvent(gameEvent.Type, isLobbyOwner, !isLobbyOwner))
                    {
                        Debug.LogWarning($"Player {data.Item2.Value} tried to send unauthorized event {gameEvent.Type}");
                        return;
                    }

                    ProcessReceivedEvent(gameEvent);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to process received event: {e.Message}");
        }
    }

    private void ProcessReceivedEvent(IGameEvent eventData)
    {
        if (eventData.SenderId.HasValue)
        {
            EnsurePlayerObject(eventData.SenderId.Value);
        }

        switch (eventData.Type)
        {
            case EventType.PlayerMove:
                _eventDispatcher.HandleEvent((GameEvent<PlayerMoveData>)eventData);
                break;
            case EventType.PlayerLook:
                _eventDispatcher.HandleEvent((GameEvent<LookData>)eventData);
                break;
            case EventType.WeaponChange:
                _eventDispatcher.HandleEvent((GameEvent<WeaponChangeData>)eventData);
                break;
            case EventType.PunchBegin:
                _eventDispatcher.HandleEvent((GameEvent<PunchBeginData>)eventData);
                break;
            //case EventType.ShootRequest when isLobbyOwner:
            //    _eventDispatcher.HandleEvent((GameEvent<ShootRequestData>)eventData);
            //    break;
            //case EventType.CoinThrow when isLobbyOwner: // turn to CoinThrowRequest
            //    _eventDispatcher.HandleEvent((GameEvent<CoinThrowData>)eventData);
            //    break;
            //case EventType.Parry when isLobbyOwner: // also turn to ParryRequest
            //    _eventDispatcher.HandleEvent((GameEvent<ParryData>)eventData);
            //    break;
        }
    }

    private void EnsurePlayerObject(SteamId playerId)
    {
        if (representativeObjects.Any(p => p.Item1 == playerId))
            return;

        var playerGO = Instantiate(CompMultiplayerMain.playerGO, Vector3.zero, Quaternion.identity);
        var player = playerGO.AddComponent<Player>();
        playerGO.name = $"Rep_{playerId}";

        representativeObjects.Add((playerId, playerGO, player));

        Debug.Log($"Created representative object for player {playerId}");
    }

    private void RemovePlayerObject(SteamId playerId)
    {
        var playerData = representativeObjects.FirstOrDefault(p => p.Item1 == playerId);
        if (playerData != default)
        {
            if (playerData.Item2 != null)
                Destroy(playerData.Item2);

            representativeObjects.Remove(playerData);
            Debug.Log($"Removed representative object for player {playerId}");
        }
    }

    private void SendGameEvent<T>(GameEvent<T> gameEvent) where T : struct
    {
        var skipEvents = new HashSet<EventType>
        {
            EventType.PlayerMove,
            EventType.PlayerLook
        };

        if (!skipEvents.Contains(gameEvent.Type))
        {
            print($"Sending event: [{gameEvent.Type}], [{gameEvent.Data}]");
        }

        var serializedEvent = new SerializedGameEvent
        {
            Type = gameEvent.Type,
            Category = gameEvent.Category,
            Data = Data.Serialize(gameEvent.Data),
            Timestamp = gameEvent.Timestamp
        };

        SendMethod method = gameEvent.Category switch {
            EventCategory.ToEveryone => SendMethod.UnreliableNoDelay,
            EventCategory.ToServer => SendMethod.Reliable,
            EventCategory.ToPlayers => SendMethod.Reliable,
            _ => SendMethod.Reliable
        };

        MU.LobbyManager.SendData(serializedEvent, method);
        
    }

    private IGameEvent DeserializeToGameEvent(SerializedGameEvent serialized)
    {
        var type = serialized.Type;
        var category = serialized.Category;
        var data = serialized.Data;
        var timestamp = serialized.Timestamp;
        return serialized.Type switch
        {
            EventType.PlayerMove => new GameEvent<PlayerMoveData>(type, category, Data.Deserialize<PlayerMoveData>(data)) { Timestamp = timestamp },
            EventType.PlayerLook => new GameEvent<LookData>(type, category, Data.Deserialize<LookData>(data)) { Timestamp = timestamp },
            EventType.WeaponChange => new GameEvent<WeaponChangeData>(type, category, Data.Deserialize<WeaponChangeData>(data)) { Timestamp = timestamp },
            EventType.PunchBegin => new GameEvent<PunchBeginData>(type, category, Data.Deserialize<PunchBeginData>(data)) { Timestamp = timestamp },
            EventType.ShootRequest => new GameEvent<ShootRequestData>(type, category, Data.Deserialize<ShootRequestData>(data)) { Timestamp = timestamp },
            EventType.CoinThrow => new GameEvent<CoinThrowData>(type, category, Data.Deserialize<CoinThrowData>(data)) { Timestamp = timestamp },
            EventType.Parry => new GameEvent<ParryData>(type, category, Data.Deserialize<ParryData>(data)) { Timestamp = timestamp },
            _ => null
        };
    }

    public static void SendShootRequest(Vector3 source, Vector3 direction, WeaponType weapon, byte variation, byte charge = 0, bool altFire = false)
    {
        var shootEvent = ToServerEvents.ShootRequest(source, direction, weapon, variation, charge, altFire);
        MultiplayerStuff.Instance.SendGameEvent(shootEvent);
    }

    public static void SendCoinThrow(Vector3 direction, Vector3 playerVelocity, uint coinId)
    {
        var coinEvent = ToServerEvents.CoinThrow(direction, playerVelocity, coinId);
        MultiplayerStuff.Instance.SendGameEvent(coinEvent);
    }

    public static void SendParry(Vector3 direction, uint projId, ParryType type)
    {
        var parryEvent = ToServerEvents.Parry(direction, projId, type);
        MultiplayerStuff.Instance.SendGameEvent(parryEvent);
    }

    public void ConfirmBulletHit(SteamId playerId, byte damage, uint projId, Vector3 hitPoint)
    {
        if (!isLobbyOwner) return;

        var hitEvent = ToPlayersEvents.BulletHit(playerId, damage, projId, hitPoint);
        SendGameEvent(hitEvent);
    }

    public void SpawnProjectile(uint projId, Vector3 pos, Vector3 velocity, ProjectileType type, SteamId owner)
    {
        if (!isLobbyOwner) return;

        var spawnEvent = ToPlayersEvents.ProjectileSpawn(projId, pos, velocity, type, owner);
        SendGameEvent(spawnEvent);
    }

    private void SetupLobbyCallbacks()
    {
        MU.Callbacks.OnLobbyMemberJoined.AddListener((lobby, friend) =>
        {
            Debug.Log($"Lobby member joined: {friend.Name} ({friend.Id})");

            if (friend.Id != LobbyManager.selfID)
            {
                EnsurePlayerObject(friend.Id);
            }

            if (isLobbyOwner && scoreboard != null)
            {
                scoreboard.addPlayer(new scoreboardPlayer(friend.Name, friend.Id));
            }
        });

        MU.Callbacks.OnLobbyMemberLeave.AddListener((friend) =>
        {
            RemovePlayerObject(friend);
        });

        MU.Callbacks.OnLobbyCreated.AddListener((lobby) =>
        {
            scoreboard = new Scoreboard();
            Debug.Log("Lobby created");
        });

        MU.Callbacks.OnLobbyEntered.AddListener((lobby) =>
        {
            Debug.Log("Lobby entered");
        });
    }

    private void UpdateScoreboard()
    {
        if (scoreboard == null)
            scoreboard = new Scoreboard();

        foreach (var friend in MU.LobbyManager.current_lobby?.Members ?? Enumerable.Empty<Friend>())
        {
            if (!scoreboard.players.Any(p => p.id == friend.Id))
            {
                scoreboard.addPlayer(new scoreboardPlayer(friend.Name, friend.Id));
            }
        }
    }

    public static void WeaponChange(GameObject go)
    {
        print($"Weapon changed to: {go.name}");
        try
        {
            var weaponEvent = ToEveryoneEvents.WeaponChange(go.name);
            Instance.SendGameEvent(weaponEvent);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send weapon change: {e.Message}");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            EnsurePlayerObject(new SteamId { Value = 123456789 });
            Debug.Log("Created debug player object");
        }

        if (Input.GetKey(KeyCode.K) && _newMovement != null)
        {
            foreach ((SteamId steamId, GameObject gameObject, Player player) in representativeObjects)
            {
                gameObject.transform.position = _newMovement.transform.position + Vector3.right * 5;
            }
        }
    }
}

[Serializable]
public class SerializedGameEvent
{
    public EventType Type;
    public EventCategory Category;
    public byte[] Data;
    public uint Timestamp;
}
