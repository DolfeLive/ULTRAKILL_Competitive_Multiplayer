using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using Clogger = UltraIDK.Logger;

namespace UltraIDK
{
    public class LobbyJoin : MonoBehaviour
    {
        public async void JoinLobby()
        {
            Debug.Log("Join lobby button pressed!");

            if (ulong.TryParse(transform.Find("CodeInput").GetComponent<InputField>().text, out ulong lobbyId))
            {
                MultiplayerUtil.LobbyManager.JoinLobbyWithID(lobbyId);
            }
            else
            {
                Clogger.LogError("Invalid lobby ID input. Please enter a valid number.");
            }
        }
    }
}
