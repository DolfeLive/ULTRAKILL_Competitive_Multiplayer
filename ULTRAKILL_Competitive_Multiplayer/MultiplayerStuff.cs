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
                    // { jumping, dashing, SSJing, Sliding, Slamming }
                    PlayerMoveEvent move = new(nm.transform.position.ToSVec3(), nm.rb.velocity.ToSVec3(), boolsToBinary(new bool[] { nm.jumping, nm.boost, nm.slamStorage, nm.sliding, nm.slamForce > 0.1f }));
                    LookEvent lookEvent = new();
                    lookEvent.Dir = new SerializableVec3(nm.cc.rotationX, nm.cc.rotationY, 0);

                    //amtSent++;
                    //if (amtSent % 500 == 0)
                    //    Debug.Log($"Sending player pos: ({player.RotationX}, {player.PositionY}, {player.PositionZ})");

                    MU.LobbyManager.SendData(move, SendMethod.UnreliableNoDelay);
                    MU.LobbyManager.SendData(lookEvent, SendMethod.UnreliableNoDelay);

                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send data: {e.Message}");
            }

        });

        MU.Callbacks.TimeToSendUnimportantData.AddListener(() => {
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

        MU.ObserveManager.SubscribeToType(typeof(PlayerMoveEvent), out Callbacks.SenderUnityEvent PlayerDetected);
        PlayerDetected.AddListener(_ =>
        {
            var playerData = Data.Deserialize<PlayerMoveEvent>(_.Item1);
            SteamId senderId = _.Item2.Value;
            print($"player Pos: ({playerData.position}, Sender id: {senderId}");
            
            if (senderId == LobbyManager.selfID) return;

            if (!representativeObjects.Any(p => p.Item1 == senderId))
            {
                GameObject repSphere = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere), Vector3.zero, Quaternion.identity);
                repSphere.name = $"Rep_{senderId}";
                representativeObjects.Add((senderId, repSphere));
            }

            foreach ((uint, GameObject) player in representativeObjects)
            {
                uint Id = player.Item1; GameObject repSphere = player.Item2;

                repSphere.transform.position = playerData.position.ToVec3(); 
            }
        });

        MU.ObserveManager.SubscribeToType(typeof(LookEvent), out Callbacks.SenderUnityEvent PlayerHeadMoved);
        PlayerHeadMoved.AddListener(_ =>
        {
            var playerData = Data.Deserialize<LookEvent>(_.Item1);
            SteamId senderId = _.Item2.Value;
            print($"player look Dir: ({playerData.Dir}, Sender id: {senderId}");

            if (senderId == LobbyManager.selfID) return;

            if (!representativeObjects.Any(p => p.Item1 == senderId))
            {
                GameObject repSphere = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere), Vector3.zero, Quaternion.identity);
                repSphere.name = $"Rep_{senderId}";
                representativeObjects.Add((senderId, repSphere));
            }

            foreach ((uint, GameObject) player in representativeObjects)
            {
                uint Id = player.Item1; GameObject repSphere = player.Item2;

                repSphere.transform.rotation = Quaternion.Euler(playerData.Dir.ToVec3());
            }
        });


        MU.Callbacks.OnLobbyMemberJoined.AddListener((lobby, friend) =>
        {
            Debug.Log($"Lobby member joined: {friend.Name} ({friend.Id})");
            
            if (friend.Id == LobbyManager.selfID) return;

            if (representativeObjects.Any(_ => _.Item1.AccountId == friend.Id))
            {
                Debug.LogWarning($"Representative object already exists for friend ID: {friend.Id}");
                return;
            }

            GameObject repSphere = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere), Vector3.zero, Quaternion.identity);
            repSphere.name = $"Rep_{friend.Id}_{friend.Name}";
            representativeObjects.Add((friend.Id, repSphere));

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
                print($"AWDAWD: {string.Join(", ", representativeObjects)}");
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

            //StartCoroutine(Mcdondaldwifi());
        }

        //public float changeSpeed = 0.5f;
        //private float targetRecvLoss;
        //private float targetSendLoss;
        //private float targetRecvLag;
        //private float targetSendLag;

        //IEnumerator Mcdondaldwifi()
        //{
        //    targetRecvLoss = Random.Range(0f, 30f);
        //    targetSendLoss = Random.Range(0f, 30f);
        //    targetRecvLag = Random.Range(0f, 300f);
        //    targetSendLag = Random.Range(0f, 300f);

        //    while (true)
        //    {
        //        yield return new WaitForSeconds(5.0f);

        //        targetRecvLoss = Random.Range(0f, 50f);  // Up to 50% loss
        //        targetSendLoss = Random.Range(0f, 50f);
        //        targetRecvLag = Random.Range(0f, 500f);  // Up to 500ms lag
        //        targetSendLag = Random.Range(0f, 500f);
        //    }
        //}
        //void Update()
        //{
        //    SteamNetworkingUtils.FakeRecvPacketLoss = Mathf.Lerp(SteamNetworkingUtils.FakeRecvPacketLoss, targetRecvLoss, Time.deltaTime * changeSpeed);
        //    SteamNetworkingUtils.FakeSendPacketLoss = Mathf.Lerp(SteamNetworkingUtils.FakeSendPacketLoss, targetSendLoss, Time.deltaTime * changeSpeed);
        //    SteamNetworkingUtils.FakeRecvPacketLag = Mathf.Lerp(SteamNetworkingUtils.FakeRecvPacketLag, targetRecvLag, Time.deltaTime * changeSpeed);
        //    SteamNetworkingUtils.FakeSendPacketLag = Mathf.Lerp(SteamNetworkingUtils.FakeSendPacketLag, targetSendLag, Time.deltaTime * changeSpeed);
        //}


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
