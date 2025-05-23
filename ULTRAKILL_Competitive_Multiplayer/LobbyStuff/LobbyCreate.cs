﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using Clogger = ULTRAKILL_Competitive_Multiplayer.Logger;

namespace ULTRAKILL_Competitive_Multiplayer;

public class LobbyCreate : MonoBehaviour
{
    string LobbyName;
    int? maxPlayers;
    bool publicLobby;
    bool cracked;
    bool mods;
    bool cheats;
    public void Lobby_Name()
    {
        Clogger.Log("Lobby Name Recived");
        LobbyName = gameObject.transform.Find("Lobby_Name").GetComponent<InputField>().text;
    }

    public void Max_Players()
    {
        Clogger.Log("Max Players Recived");

        int.TryParse(gameObject.transform.Find("Max_Players").GetComponent<InputField>().text, out int result);
        maxPlayers = result;
    }

    void Public_Lobby()
    {
        publicLobby = transform.Find("PublicLobbyToggle").GetComponent<Toggle>().isOn;
    }
    void Cracked()
    {
        cracked = CompMultiplayerMain.CrackedToggle.isOn;
    }

    void Mods()
    {
        mods = transform.Find("Mods Toggle").GetComponent<Toggle>().isOn;
    }

    public async void CreateLobby()
    {
        Clogger.Log("Create lobby button clicked");
        Cracked();
        Lobby_Name();
        Max_Players();
        Public_Lobby();
        Mods();

        MultiplayerUtil.LobbyManager.CreateLobby(LobbyName, maxPlayers, publicLobby, cracked, mods, ("UKCM", "EtcEtc"));
        CompMultiplayerMain.instance.LoadMultiplayerScene();
    }

}
