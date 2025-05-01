using Steamworks;
using Steamworks.Data;
using System.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ULTRAKILL_Competitive_Multiplayer;

public struct scoreboardPlayer
{
    public string Name;
    public SteamId id;
}

public class Scoreboard
{
    public List<scoreboardPlayer> players = new();

    void Start()
    {

    }

    private static async Task<Steamworks.Data.Image?> GetAvatar(SteamId user)
    {
        try
        {
            return await SteamFriends.GetMediumAvatarAsync(user);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return null;
        }
    }
}
