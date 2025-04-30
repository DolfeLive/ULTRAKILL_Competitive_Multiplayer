using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Clogger = ULTRAKILL_Competitive_Multiplayer.Logger;
using MultiplayerUtil;


namespace ULTRAKILL_Competitive_Multiplayer;

public class LobbyList : MonoBehaviour
{
    public static bool FetchingLobbies = false;
    static List<Lobby> foundLobbies = new List<Lobby>();

    public static Dictionary<Transform, Lobby> lobbies = new Dictionary<Transform, Lobby>();
    void Start()
    {
        Clogger.Log("Fetching lobbies!");

        FetchLobbies();
        InvokeRepeating("FetchLobbies", 0, 20f);
    }
    public async void FetchLobbies()
    {
        FetchingLobbies = true;
        try
        {
            List<Lobby> getthingy = getthingy = await MultiplayerUtil.LobbyManager.FetchLobbies(("UKCM", "EtcEtc"));
        }
        catch (Exception e)
        {
            Clogger.LogError($"Lobby finding exeption: {e}");
        }
        
        foreach (KeyValuePair<Transform, Lobby> KVP in lobbies)
        {
            Destroy(KVP.Key.gameObject);
        }

        //Clogger.Log($"Found Lobbies: {foundLobbies.Count}");

        foreach (Lobby lob in foundLobbies)
        {
            PesudoLobby pesudoLobby = new PesudoLobby();

            pesudoLobby.Name = lob.Data.Where(kvp => kvp.Key == "name" && !string.IsNullOrEmpty(kvp.Value))
                         .Select(kvp => kvp.Value)
                         .FirstOrDefault();
            pesudoLobby.Members = lob.Data.Where(kvp => kvp.Key == "members" && !string.IsNullOrEmpty(kvp.Value))
                         .Select(kvp => kvp.Value)
                         .FirstOrDefault();

            pesudoLobby.ID = lob.Id.ToString();

            Transform lobby = Instantiate(CompMultiplayerMain.LobbyPrefab, CompMultiplayerMain.LobbyParent.transform).transform;
            lobbies.Add(lobby, lob);
            lobby.Find("_/Name").GetComponent<TMP_Text>().text = pesudoLobby.Name;
            lobby.Find("_/Members").GetComponent<TMP_Text>().text = pesudoLobby.Members;
            lobby.Find("_/LobbyID").GetComponent<TMP_Text>().text = pesudoLobby.ID;

            lobby.gameObject.AddComponent<LobbyButton>();
            lobby.GetComponent<LobbyButton>().init(pesudoLobby);
        }
    }
}

class PesudoLobby
{
    public string Name { get; set; }
    public string Members { get; set; }
    public string ID { get; set; }
}

class LobbyButton : MonoBehaviour
{
    public PesudoLobby lobbyRef;
    public void init(PesudoLobby lobbyRef)
    {
        this.lobbyRef = lobbyRef;
        Button button = gameObject.GetComponent<Button>();

        Button.ButtonClickedEvent bce = new Button.ButtonClickedEvent();
        bce.AddListener(click);

        button.onClick = bce;

    }
    public void click()
    {
        Clogger.Log($"Entering Lobby: {lobbyRef.Name}");

        string lobbyId = lobbyRef.ID;

        ulong.TryParse(lobbyId, out ulong id);
        
        SteamManager.instance.JoinLobbyWithID(id);
    }
}
