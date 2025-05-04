using MultiplayerUtil;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using UltraIDK;
using UnityEngine;
using MU = MultiplayerUtil;

namespace ULTRAKILL_Competitive_Multiplayer
{
    public class MultiplayerStuff : MonoBehaviour
    {
        public DataPacket player;
        public bool DoPlayerStuff = true;

        public List<(SteamId, GameObject)> representativeObjects = new List<(SteamId, GameObject)>();
        public Scoreboard scoreboard;

        public static bool isLobbyOwner { get { return MultiplayerUtil.LobbyManager.isLobbyOwner; } }

        bool NewMovementExists => NewMovement.instance != null;
        NewMovement nm = NewMovement.instance;
        
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            MU.Callbacks.TimeToSendImportantData.AddListener(() =>
            {
                try
                {
                    if (DoPlayerStuff && NewMovementExists && CompMultiplayerMain.instance.inMultiplayerScene)
                    {
                        player = new(
                            MU.LobbyManager.selfID.Value,
                            nm.hp,
                            nm.transform.position,
                            nm.rb.velocity,
                            new(nm.cc.rotationX, nm.cc.rotationY, 0),
                            0, //nm.gunc.currentSlotIndex,
                            0, //nm.gunc.currentVariationIndex,
                            nm.sliding,
                            nm.punch.fistCooldown > 0.1f,
                            false,
                            nm.slamForce > 0.1f
                        );
                        Debug.Log($"Sending player pos: ({player.RotationX}, {player.PositionY}, {player.PositionZ})");
                        MU.LobbyManager.SendData(player, SendMethod.UnreliableNoDelay);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to send data: {e.Message}" +
                        $"exists: {NewMovementExists} \n" +
                        $"nm: {nm} \n" +
                        $"gunControl: {nm.gunc} \n" +
                        $"punch: {nm.punch} \n" +
                        $"cc: {nm.cc} \n" +
                        $"rb: {nm.rb} \n" +
                        $"LobbyMgr: {MU.LobbyManager.selfID.Value} \n" +
                        $"Player Posses: ({player})");
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

            MU.ObserveManager.SubscribeToType(typeof(DataPacket), out Callbacks.SenderUnityEvent PlayerDetected);
            PlayerDetected.AddListener(_ =>
            {
                var player = Data.Deserialize<DataPacket>(_.Item1);
                print($"player Pos: ({player.PositionX}, {player.PositionY}, {player.PositionZ}), Sender id: {_.Item2.Value}");
                player.Display();
            });

            MU.Callbacks.OnLobbyMemberJoined.AddListener((lobby, friend) =>
            {
                if (representativeObjects.Any(_ => _.Item1.AccountId == friend.Id))
                {
                    Debug.LogWarning($"Representative object already exists for friend ID: {friend.Id}");
                    return;
                }

                GameObject repSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                repSphere.name = $"Rep_{friend.Id}_{friend.Name}";
                representativeObjects.Add((friend.Id, repSphere));
            });

            MU.Callbacks.OnLobbyMemberLeave.AddListener((lobby, friend) =>
            {
                var repObject = representativeObjects.FirstOrDefault(_ => _.Item1.AccountId == friend.Id);
                if (repObject != default)
                {
                    Destroy(repObject.Item2);
                    representativeObjects.Remove(repObject);
                    print($"AWDAWD: {string.Join(", ", representativeObjects)}");
                }
                else
                {
                    Debug.LogWarning($"No representative object found for leaving friend ID: {friend.Id}");
                }
            });

            MU.Callbacks.OnLobbyCreated.AddListener((lobby) =>
            {
                Debug.Log("Lobby created");
            });

            MU.Callbacks.OnLobbyEntered.AddListener((lobby) =>
            {
                Debug.Log("Lobby Entered");
            });
        }
        
        void Update()
        {
            
        }


        void LobbyOwnerStuff()
        {

        }
    }
}
