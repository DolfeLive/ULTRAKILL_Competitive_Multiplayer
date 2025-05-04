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
        int amtSent = 0;
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
                        amtSent++;
                        if (amtSent % 100 == 0)
                            Debug.Log($"Sending player pos: ({player.RotationX}, {player.PositionY}, {player.PositionZ})");

                        MU.LobbyManager.SendData(player, SendMethod.UnreliableNoDelay);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to send data: {e.Message}");
                        //$"exists: {NewMovementExists} \n" +
                        //$"nm: {nm} \n" +
                        //$"gunControl: {nm.gunc} \n" +
                        //$"punch: {nm.punch} \n" +
                        //$"cc: {nm.cc} \n" +
                        //$"rb: {nm.rb} \n" +
                        //$"LobbyMgr: {MU.LobbyManager.selfID.Value} \n" +
                        //$"Player Posses: ({player})");
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
                var playerData = Data.Deserialize<DataPacket>(_.Item1);
                print($"player Pos: ({playerData.PositionX}, {playerData.PositionY}, {playerData.PositionZ}), Sender id: {_.Item2.Value}");
                //player.Display();

                foreach ((uint, GameObject) player in representativeObjects)
                {
                    uint Id = player.Item1; GameObject repSphere = player.Item2;

                    repSphere.transform.position = new Vector3(playerData.PositionX, playerData.PositionY, playerData.PositionZ);
                    repSphere.transform.rotation = Quaternion.Euler(playerData.RotationX, playerData.RotationY, 0f);
                    
                }

            });

            MU.Callbacks.OnLobbyMemberJoined.AddListener((lobby, friend) =>
            {
                if (representativeObjects.Any(_ => _.Item1.AccountId == friend.Id))
                {
                    Debug.LogWarning($"Representative object already exists for friend ID: {friend.Id}");
                    return;
                }

                GameObject repSphere = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere), Vector3.zero, Quaternion.identity);
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

            StartCoroutine(Mcdondaldwifi());
        }

        public float changeSpeed = 0.5f;
        private float targetRecvLoss;
        private float targetSendLoss;
        private float targetRecvLag;
        private float targetSendLag;

        IEnumerator Mcdondaldwifi()
        {
            targetRecvLoss = Random.Range(0f, 30f);
            targetSendLoss = Random.Range(0f, 30f);
            targetRecvLag = Random.Range(0f, 300f);
            targetSendLag = Random.Range(0f, 300f);

            while (true)
            {
                yield return new WaitForSeconds(5.0f);

                targetRecvLoss = Random.Range(0f, 50f);  // Up to 50% loss
                targetSendLoss = Random.Range(0f, 50f);
                targetRecvLag = Random.Range(0f, 500f);  // Up to 500ms lag
                targetSendLag = Random.Range(0f, 500f);
            }
        }
        void Update()
        {
            SteamNetworkingUtils.FakeRecvPacketLoss = Mathf.Lerp(SteamNetworkingUtils.FakeRecvPacketLoss, targetRecvLoss, Time.deltaTime * changeSpeed);
            SteamNetworkingUtils.FakeSendPacketLoss = Mathf.Lerp(SteamNetworkingUtils.FakeSendPacketLoss, targetSendLoss, Time.deltaTime * changeSpeed);
            SteamNetworkingUtils.FakeRecvPacketLag = Mathf.Lerp(SteamNetworkingUtils.FakeRecvPacketLag, targetRecvLag, Time.deltaTime * changeSpeed);
            SteamNetworkingUtils.FakeSendPacketLag = Mathf.Lerp(SteamNetworkingUtils.FakeSendPacketLag, targetSendLag, Time.deltaTime * changeSpeed);
        }


        void LobbyOwnerStuff()
        {

        }
    }
}
