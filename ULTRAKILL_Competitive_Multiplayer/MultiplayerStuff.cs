using MultiplayerUtil;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UltraIDK;
using UnityEngine;
using MU = MultiplayerUtil;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Steamworks.ServerList;
using System.Collections;

namespace ULTRAKILL_Competitive_Multiplayer;

public class MultiplayerStuff : MonoBehaviour
{
    public bool DoPlayerStuff = true;

    public List<(SteamId, GameObject, Player)> representativeObjects = new();
    
    public Scoreboard scoreboard;

    public static bool isLobbyOwner { get { return MultiplayerUtil.LobbyManager.isLobbyOwner; } }

    bool NewMovementExists => NewMovement.instance != null;
    NewMovement nm = NewMovement.instance;

    int sent = 0;
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        gameObject.hideFlags = HideFlags.HideAndDontSave;

        #region Data Send Callbacks

        MU.Callbacks.TimeToSendImportantData.AddListener(() =>
        {
            try
            {
                if (DoPlayerStuff && NewMovementExists && CompMultiplayerMain.instance.inMultiplayerScene)
                {
                    // { jumping, dashing, SSJing, Sliding, Slamming }
                    PlayerMoveEvent move = new(nm.transform.position.ToSVec3(), nm.rb.velocity.ToSVec3(), boolsToBinary(new bool[] { nm.jumping, nm.boost, nm.slamStorage, nm.sliding, nm.slamForce > 0.1f }));
                    LookEvent lookEvent = new(new SerializableVec3(nm.cc.rotationX, nm.cc.rotationY, 0));
                    ReliableStateInfo rsi = new(boolsToBinary(new bool[] { nm.jumping, nm.boost, nm.slamStorage, nm.sliding, nm.slamForce > 0.1f }));

                    MU.LobbyManager.SendData(move, SendMethod.UnreliableNoDelay);
                    MU.LobbyManager.SendData(lookEvent, SendMethod.UnreliableNoDelay);
                    MU.LobbyManager.SendData(rsi, SendMethod.Reliable);

                    sent++;
                    if (sent % 500 == 0)
                        Debug.Log($"Sending data move pos: {move.position}, look pos: {lookEvent.Dir}, rsi: {rsi}");

                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send data: {e.Message}");
            }

        });

        MU.Callbacks.TimeToSendUnimportantData.AddListener(() =>
        {
            if (!MU.LobbyManager.isLobbyOwner) return;

            try
            {
                LobbyOwnerStuff();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send data: {e.Message}");
            }
        });

        #endregion

        #region data Recive Callbacks
        MU.ObserveManager.MessageReceivedLogging = true;

        MU.ObserveManager.SubscribeToType(typeof(PlayerMoveEvent), out Callbacks.SenderUnityEvent PlayerDetected);
        PlayerDetected.AddListener(_ =>
        {
            var playerData = Data.Deserialize<PlayerMoveEvent>(_.Item1);
            SteamId senderId = _.Item2.Value;
            print($"player Pos recived: ({playerData.position.ToVec3()}, Sender id: {senderId}");

            if (senderId == LobbyManager.selfID) return;

            RepresentaiveObjectStuff(senderId);

            foreach ((uint, GameObject, Player) player in representativeObjects)
            {
                uint Id = player.Item1;

                if (senderId.Value != Id)
                {
                    Debug.Log($"Skipped: {senderId}");
                    continue;
                }
                GameObject repSphere = player.Item2;

                //repSphere.transform.position = playerData.position.ToVec3();
                player.Item3.Move(playerData.position.ToVec3(), playerData.velocity.ToVec3(), playerData.properties);
            }
        });

        MU.ObserveManager.SubscribeToType(typeof(LookEvent), out Callbacks.SenderUnityEvent playerHeadMoved);
        playerHeadMoved.AddListener(_ =>
        {
            var playerData = Data.Deserialize<LookEvent>(_.Item1);
            SteamId senderId = _.Item2.Value;
            print($"player look Dir: ({playerData.Dir}, Sender id: {senderId}");

            if (senderId == LobbyManager.selfID) return;

            RepresentaiveObjectStuff(senderId);

            foreach ((uint, GameObject, Player) player in representativeObjects)
            {
                uint Id = player.Item1;
                if (senderId != Id) continue;

                //GameObject repSphere = player.Item2;

                //repSphere.transform.rotation = Quaternion.Euler(playerData.Dir.ToVec3());
                player.Item3.Aim(playerData.Dir.ToVec3());
            }
        });

        MU.ObserveManager.SubscribeToType(typeof(ReliableStateInfo), out Callbacks.SenderUnityEvent reliableState);
        reliableState.AddListener(_ =>
        {
            var playerData = Data.Deserialize<ReliableStateInfo>(_.Item1);
            SteamId senderId = _.Item2.Value;
            print($"player reliable info: ({playerData.properties}, Sender id: {senderId}");

            if (senderId == LobbyManager.selfID) return;

            RepresentaiveObjectStuff(senderId);

            foreach ((uint, GameObject, Player) player in representativeObjects)
            {
                uint Id = player.Item1;
                if (senderId != Id) continue;

                GameObject repSphere = player.Item2;

                player.Item3.ReliableStateInfo(playerData.properties);
            }
        });

        #endregion

        #region Lobby callbacks

        MU.Callbacks.OnLobbyMemberJoined.AddListener((lobby, friend) =>
        {
            Debug.Log($"Lobby member joined: {friend.Name} ({friend.Id})");

            if (friend.Id == LobbyManager.selfID) return;

            if (representativeObjects.Any(_ => _.Item1.AccountId == friend.Id))
            {
                Debug.LogWarning($"Representative object already exists for friend ID: {friend.Id}");
            }
            else
                RepresentaiveObjectStuff(friend.Id);


            if (!isLobbyOwner) return;
            if (scoreboard != null)
            {
                scoreboard.addPlayer(new scoreboardPlayer(friend.Name, friend.Id));
            }

        });

        MU.Callbacks.OnLobbyMemberLeave.AddListener((friend) =>
        {
            var repObject = representativeObjects.FirstOrDefault(_ => _.Item1.AccountId == friend.Value);
            if (repObject != default)
            {
                Destroy(repObject.Item2);
                representativeObjects.Remove(repObject);
                print($"Other rep objects: {string.Join(", ", representativeObjects)}");
            }
            else
            {
                Debug.LogWarning($"No representative object found for leaving friend ID: {friend.Value}");
            }
        });

        MU.Callbacks.OnLobbyCreated.AddListener((lobby) =>
        {
            scoreboard = new();
            Debug.Log("Lobby created");
        });

        MU.Callbacks.OnLobbyEntered.AddListener((lobby) =>
        {
            Debug.Log("Lobby Entered");
        });

        #endregion
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            GameObject repSphere = Instantiate(CompMultiplayerMain.playerGO, Vector3.zero, Quaternion.identity);
            //PrintHierarchy(repSphere.transform, 0);
            Player player = repSphere.AddComponent<Player>();
            repSphere.name = $"Rep_awdawdawd";
            representativeObjects.Add((0987654567, repSphere, player));
            print("creating object");
        }

        if (Input.GetKey(KeyCode.K))
        {
            foreach ((uint, GameObject, Player) player in representativeObjects)
            {
                uint Id = player.Item1;

                GameObject repSphere = player.Item2;

                Debug.Log("Set pos");
                repSphere.transform.position = nm.transform.position + new Vector3(5, 0, 0);
            }
        }

    }

    void RepresentaiveObjectStuff(SteamId senderId)
    {
        if (!representativeObjects.Any(p => p.Item1 == senderId))
        {
            GameObject repSphere = Instantiate(CompMultiplayerMain.playerGO, Vector3.zero, Quaternion.identity);
            //PrintHierarchy(repSphere.transform, 0);
            Player player = repSphere.AddComponent<Player>();
            repSphere.name = $"Rep_{senderId}";
            representativeObjects.Add((senderId, repSphere, player));
        }
    }
    public void PrintHierarchy(Transform current, int depth)
    {
        string indent = new string(' ', depth * 2);
        print($"> {indent} {current.name}");

        foreach (Transform child in current)
        {
            PrintHierarchy(child, depth + 1);
        }
    }
    void LobbyOwnerStuff()
    {
        if (scoreboard == null)
        {
            scoreboard = new Scoreboard();
        }

        foreach (Friend friend in MU.LobbyManager.current_lobby?.Members)
        {
            if (scoreboard.players.Any(_ => _.id == friend.Id))
                continue;

            scoreboard.addPlayer(new scoreboardPlayer(friend.Name, friend.Id));
        }
        
    }

    public static void OnEtc()
    {

    }
    public static void OnWeaponChange(int slotIndex, int VarIndex)
    {
        try
        {
            WeaponChangeEvent weaponChangeEvent = new();
            weaponChangeEvent.WeaponIndex = (byte)slotIndex;
            weaponChangeEvent.VariationIndex = (byte)VarIndex;
            Debug.Log($"Weapon changed: index: {slotIndex}, varIndex: {VarIndex}");
            MU.LobbyManager.SendData(weaponChangeEvent);
        }
        catch
        {

        }
    }

    public static byte boolsToBinary(bool[] bools)
    {
        byte binary = 0b00000000;
        int length = Math.Min(bools.Length, 8);

        for (int i = 0; i < length; i++)
        {
            if (bools[i])
            {
                binary |= (byte)(1 << i);
            }
        }
        return binary;
    }
}
