using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using Clogger = ULTRAKILL_Competitive_Multiplayer.Logger;

namespace ULTRAKILL_Competitive_Multiplayer;

public class LobbyJoin : MonoBehaviour
{
    public async void JoinLobby()
    {
        Debug.Log("Join lobby button pressed!");

        if (ulong.TryParse(transform.Find("CodeInput").GetComponent<InputField>().text, out ulong lobbyId))
        {
            MultiplayerUtil.LobbyManager.JoinLobbyWithID(lobbyId);
            CompMultiplayerMain.instance.LoadMultiplayerScene();
        }
        else
        {
            Clogger.LogError("Invalid lobby ID input. Please enter a valid number.");
        }
    }
}
