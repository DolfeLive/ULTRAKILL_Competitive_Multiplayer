//using Steamworks.Data;
//using Steamworks;
//using System;
//using UnityEngine;
//using System.Threading.Tasks;
//using System.Collections.Generic;
//using System.Linq;
//using GameConsole.pcon;
//using UltraIDK;
//using System.IO;
//using System.Runtime.InteropServices;
//using System.Collections;
//using Clogger = UltraIDK.Logger;

//public class SteamManager : MonoBehaviour
//{
//    public static SteamManager instance;
//    public float importantUpdatesASec = 64;
//    public float unimportantUpdatesASec = 0.5f;
//    bool loadScene = true; // For testing
//    // Runtime
//    public Lobby? current_lobby;


//    public SteamId selfID;
//    private string playerName;
//    public bool isLobbyOwner = false;
//    string LobbyName;
//    int maxPlayers;
//    bool publicLobby;
//    bool cracked; 
//    public Coroutine? dataLoop;
    
//    private Serveier server;
//    private Client client;

//    List<Tuple<SteamId, GameObject, Rigidbody>> representativeObjects = new List<Tuple<SteamId, GameObject, Rigidbody>>();
//    // End

//    void Awake()
//    {
//        instance = this;
//        Callbacks();

//        //playerName = SteamClient.Name;
//    }

//    void Callbacks()
//    {
//        SteamMatchmaking.OnLobbyMemberJoined += (l, f) =>
//        {
//            Clogger.Log($"Lobby member joined: {f.Name}");
//            if (f.Id != selfID)
//            {
//                SteamNetworking.AcceptP2PSessionWithUser(f.Id);
//                server.besties.Add(f);

//                MakeModel(f.Id);

//                l.SendChatString($":::{f.Id} Joined"); // ::: will be a hidden message marking a user joining for the host and clients to process

//            }
//        };
//        SteamMatchmaking.OnLobbyEntered += (l) => {
//            if (!String.IsNullOrEmpty(l.Owner.Name) && l.Owner.Id != selfID)
//            {
//                Clogger.Log($"Joined Lobby: {l.Owner.Name}");
//                client.Connect(l.Owner.Id);


//                foreach (var member in l.Members)
//                {
//                    MakeModel(member.Id);
//                    client.connectedPeers.Add(member.Id);
//                }
//            }
//        };

//        SteamMatchmaking.OnChatMessage += (lo, fr, st) =>
//        {
//            Clogger.Log($"Chat message recived from {fr.Name}: {st}");

//        };

//        SteamMatchmaking.OnLobbyMemberLeave += (Lob, Fri) =>
//        {
//            Destroy(representativeObjects.Find(_ => _.Item1 == Fri.Id).Item2);
//            representativeObjects.RemoveAll(item => item.Item1 == Fri.Id);
//            if (isLobbyOwner)
//            {
//                server.besties.Remove(Fri);
//                SteamNetworking.CloseP2PSessionWithUser(Fri.Id);
//            }
//            else
//            {
//                client.connectedPeers.Remove(Fri.Id);
//                SteamNetworking.CloseP2PSessionWithUser(Fri.Id);
//            }
//            Clogger.Log($"Lobby member left: {Fri.Name}");
//        };

//        SteamMatchmaking.OnLobbyMemberDisconnected += (Lob, Fri) =>
//        {
//            representativeObjects.RemoveAll(item => item.Item1 == Fri.Id);
//            if (isLobbyOwner)
//            {
//                server.besties.Remove(Fri);
//                SteamNetworking.CloseP2PSessionWithUser(Fri.Id);
//            }
//            else
//            {
//                client.connectedPeers.Remove(Fri.Id);
//                SteamNetworking.CloseP2PSessionWithUser(Fri.Id);
//            }
//            Clogger.Log($"Lobby member disconnected: {Fri.Name}");
//        };

//        SteamMatchmaking.OnLobbyMemberKicked += (Lob, Fri, Kicker) =>
//        {
//            representativeObjects.RemoveAll(item => item.Item1 == Fri.Id);
//            if (isLobbyOwner)
//            {
//                server.besties.Remove(Fri);
//                SteamNetworking.CloseP2PSessionWithUser(Fri.Id);
//            }
//            else
//            {
//                client.connectedPeers.Remove(Fri.Id);
//                SteamNetworking.CloseP2PSessionWithUser(Fri.Id);
//            }
//            Clogger.Log($"Lobby Member kicked: {Fri.Name}, Kicker: {Kicker.Name}");
//        };
//    }
    
//    public IEnumerator DataLoopInit()
//    {
//        if (dataLoop != null)
//            yield break;


//        Clogger.Log("Data Loop Init Activated");
//        float interval = 1f / importantUpdatesASec;
//        float unimportantInterval = 1f / unimportantUpdatesASec;

//        float unimportantTimeElapsed = 0f;

//        Coroutine checkloop = StartCoroutine(CheckForP2PLoop());
        


//        while (true)
//        {
//            float startTime = Time.time;

//            DataSend();

//            if (isLobbyOwner)
//            {
//                unimportantTimeElapsed += Time.time - startTime;

//                if (unimportantTimeElapsed >= unimportantInterval)
//                {
//                    Clogger.Log($"Lobby members: {current_lobby?.Members}");
//                    current_lobby?.SetData("members", $"{current_lobby?.Members.Count()}/{maxPlayers}");
//                    unimportantTimeElapsed = 0f;
//                }
//            }

//            float elapsedTime = Time.time - startTime;

//            float waitTime = Mathf.Max(0, interval - elapsedTime);
                        
//            yield return new WaitForSeconds(waitTime);

//            if (current_lobby == null)
//            {
//                Clogger.LogWarning("breaking out of DataLoopInit");
//                StopCoroutine(checkloop);

//                yield break;
//            }
//        }
//    }

//    private IEnumerator CheckForP2PLoop()
//    {
//        while (true)
//        {
//            CheckForP2PMessages();
//            yield return null;
//        }
//    }

//    private void DataSend()
//    {
//        try
//        {
//            if (current_lobby != null)
//            {
//                NewMovement nm = NewMovement.Instance;
//                if (nm == null)
//                {
//                    Clogger.LogWarning("New Movement is null");
//                    return;
//                }

//                GunControl gc = GunControl.Instance;
//                if (gc == null) 
//                {
//                    Clogger.LogWarning("GunControl is null");
//                    return;
//                }
                
//                Rigidbody rb = nm.rb;
//                if (rb == null)
//                {
//                    Clogger.LogWarning("nm rb is null");
//                    return;
//                }

//                FistControl fist = FistControl.Instance;
//                if (fist == null)
//                {
//                    Clogger.LogWarning("Fist is null");
//                    return;
//                }


//                DataPacket data = new DataPacket(
//                    nm.hp,
//                    rb.position,
//                    rb.velocity,
//                    new Vector3(nm.cc.rotationX, nm.cc.rotationY, 0),
//                    gc.currentSlotIndex,
//                    gc.currentVariationIndex,
//                    nm.sliding,
//                    fist.fistCooldown > 0 ? true : false,
//                    nm.wc.onWall,
//                    nm.slamStorage
//                );

//                if (!isLobbyOwner)
//                    client.Send(data);
//                else
//                    server.Send(data);
//            }
//            else
//            {
//                Clogger.Log("Current Lobby is null");
//            }
//        }
//        catch (Exception e)
//        {
//            Clogger.Log($"Data Send Exception: {e}");
//        }
//    }

//    void Update()
//    {
//        SteamClient.RunCallbacks();
//    }
//    public async void HostLobby(string LobbyName, int? maxPlayers, bool publicLobby, bool cracked)
//    {
//        if (!SteamClient.IsValid)
//        {
//            Clogger.LogWarning("Steam client is not initialized");

//            try
//            {
//                SteamClient.Init(Class1.appId);
//                Clogger.Log("Reinited steam");
//            }
//            catch (Exception e) { Clogger.LogError($"STEAM ERROR: {e}"); Clogger.LogWarning("Try launching steam if it isnt launched!"); }

//            return;
//        }

//        if (current_lobby != null)
//        {
//            if (isLobbyOwner)
//            {
//                if (server.besties.Count > 0)
//                {
//                    current_lobby.Value.SetData("Owner", server.besties[0].Name);

//                    if (current_lobby.Value is Lobby lobby)
//                    {
//                        lobby.Owner = server.besties[0];
//                    }

//                }

//            }

//            current_lobby.Value.SendChatString($":::Leaving.{selfID.Value}");
//            current_lobby.Value.Leave();
//            current_lobby = null;
//        }


//        Lobby? createdLobby = await SteamMatchmaking.CreateLobbyAsync(maxPlayers ?? 8);
//        if (createdLobby == null)
//        {
//            Clogger.LogError("Lobby creation failed - Result is null");
//            return;
//        }
                
//        server = new Serveier();

//        this.LobbyName = LobbyName;


//        if (maxPlayers <= 0) maxPlayers = 8;

//        this.maxPlayers = maxPlayers ?? 8;
//        this.publicLobby = publicLobby;

//        isLobbyOwner = true;
//        current_lobby = createdLobby;

//        current_lobby?.SetJoinable(true);
//        if (publicLobby)
//            current_lobby?.SetPublic();
//        else
//            current_lobby?.SetPrivate();

//        current_lobby?.SetData("INEEDANAME", "pvp");
//        current_lobby?.SetData("ModName", "true");
//        current_lobby?.SetData("name", LobbyName);
//        current_lobby?.SetData("cheats", "False");
//        current_lobby?.SetData("mods", "False");
//        current_lobby?.SetData("members", $"1/{maxPlayers}");

//        Clogger.Log($"Lobby Created, id: {current_lobby?.Id}");

//        LoadMultiplayerScene();
//    }

//    // Help collected from jaket github https://github.com/xzxADIxzx/Join-and-kill-em-together/blob/main/src/Jaket/Net/LobbyController.cs
//    public async void JoinLobbyWithID(ulong id)
//    {
//        try
//        {
//            Clogger.Log("Joining Lobby with ID");
//            Lobby lob = new Lobby(id);

//            RoomEnter result = await lob.Join();

//            if (result == RoomEnter.Success)
//            {
//                Clogger.Log($"Lobby join Success: {result}");
//                isLobbyOwner = false;
//                current_lobby = lob;
//                LoadMultiplayerScene();

//                client = new Client();
//            }
//            else
//            {
//                Clogger.LogWarning($"Couldn't join the lobby. Result is {result}");
//            }
//        }
//        catch (Exception ex)
//        {
//            Clogger.LogError($"An error occurred while trying to join the lobby: {ex.Message}, The error might be because steam isnt launched");
//        }
//    }
    
    
//    private void MakeModel(SteamId id)
//    {
//        if (id == selfID) return;

//        GameObject enemyModel = Instantiate(Class1.MultiplayerModel, Vector3.zero, Quaternion.identity);
//        enemyModel.name = id.ToString();
//        enemyModel.transform.localScale = new Vector3(1.9f, 1.9f, 1.9f);
//        Transform enem = enemyModel.transform.Find("v1_mdl_0");
//        Mesh mesh = enem.GetComponent<SkinnedMeshRenderer>().sharedMesh;
//        MeshCollider enemMC = enem.gameObject.AddComponent<MeshCollider>();
//        enemMC.sharedMesh = mesh;
//        enemMC.convex = true;
//        enem.gameObject.layer = 12;

//        Rigidbody rb = enemyModel.AddComponent<Rigidbody>();
//        rb.angularDrag = 0.05f;
//        rb.mass = 10000f;
//        rb.drag = 0;
//        rb.maxAngularVelocity = 7f;
//        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
//        rb.centerOfMass = new Vector3(0f, 0.25f, 0f);
//        rb.interpolation = RigidbodyInterpolation.Interpolate;
//        rb.maxDepenetrationVelocity = Mathf.Infinity;

//        rb.constraints = RigidbodyConstraints.FreezeRotation;


//        representativeObjects.Add(new Tuple<SteamId, GameObject, Rigidbody>(id, enemyModel, rb));
//    }
    
//    public void CheckForP2PMessages()
//    {
//        byte[] buffer = new byte[64];
//        uint size = (uint)buffer.Length;
//        SteamId steamId = new SteamId();
//        int channel = 0;
//        try
//        {
//            while (SteamNetworking.ReadP2PPacket(buffer, ref size, ref steamId, channel))
//            {
//                if (steamId == selfID) continue;

//                byte[] receivedData = new byte[size];
//                Array.Copy(buffer, receivedData, (int)size);

//                var dataPacket = DataPacket.Deserialize(receivedData);
                
//                Clogger.Log($"Received P2P message from {steamId}, Data: {JsonUtility.ToJson(dataPacket)}");

//                var representative = representativeObjects.FirstOrDefault(item => item.Item1 == steamId);

//                if (representative == null || representative.Item2 == null)
//                {
//                    Clogger.Log("represantiveObject of steamId is null or za gameobject");

//                    representativeObjects.RemoveAll(item => item.Item1 == steamId);

//                    MakeModel(steamId);

//                    return;
//                }

//                if (dataPacket.PositionX == null || dataPacket.PositionY == null || dataPacket.PositionZ == null)
//                {
//                    Clogger.Log("Any of the poses may be null");
//                    return;
//                }

//                representative.Item2.transform.position = new Vector3(dataPacket.PositionX, dataPacket.PositionY - 1.5f, dataPacket.PositionZ);
//                representative.Item2.transform.rotation = Quaternion.Euler(new Vector3(0, dataPacket.RotationY + 180, 0));

//                representative.Item2.transform.transform.Find("Node_10/Armature/Bone/Bone.001/Bone.002/Bone.003/Bone.004").transform.localRotation = Quaternion.Euler(new Vector3(dataPacket.RotationX - 110, 0, 0));

//                representative.Item3.velocity = new Vector3(dataPacket.VelocityX, dataPacket.VelocityY, dataPacket.VelocityZ);

//            }
//        }
//        catch (ArgumentException)
//        {
//        }
//    }

//    public static void InviteFriend() => SteamFriends.OpenGameInviteOverlay(SteamManager.instance.current_lobby.Value.Id);

//    public void LoadMultiplayerScene()
//    {
//        if (loadScene == false) return;

//        string[] scenePaths = Class1.sceneBundle.GetAllScenePaths();
//        if (scenePaths.Length > 0)
//        {
//            string sceneName = Path.GetFileNameWithoutExtension(scenePaths.FirstOrDefault());

//            if (!string.IsNullOrEmpty(sceneName))
//            {
//                Clogger.Log($"Found scene: {sceneName}");
//                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
//            }
//            else
//            {
//                Clogger.LogWarning("Scene 'PVPWorld' not found.");
//            }
//        }
//        else
//        {
//            Clogger.LogWarning("No scenes found in the scene bundle.");
//        }
//    }

//    void OnApplicationQuit()
//    {
//        if (isLobbyOwner)
//        {
//            if (server.besties.Count > 0)
//            {
//                foreach (var item in server.besties)
//                {
//                    SteamNetworking.CloseP2PSessionWithUser(item.Id);
//                }

//                current_lobby?.SendChatString($"||| Setting Lobby Owner To: {server.besties[0].Name}");
//                current_lobby?.SetData("Owner", server.besties[0].Name);
//                current_lobby?.IsOwnedBy(server.besties[0].Id);
                
//                Clogger.Log($"Setting Lobby Owner to: {server.besties[0].Name}");

//            }

//        }
//        else
//        {
//            foreach (var item in client.connectedPeers)
//            {
//                SteamNetworking.CloseP2PSessionWithUser(item);
//            }
//        }
//        current_lobby?.Leave();
//    }
//}



