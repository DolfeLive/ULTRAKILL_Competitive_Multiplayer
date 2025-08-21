using Steamworks;
using Steamworks.Data;
using System.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace ULTRAKILL_Competitive_Multiplayer;

public struct scoreboardPlayer
{
    public string Name;
    public SteamId id;
    public uint Kills = 0;
    public uint Score = 0;
    public uint Deaths = 0;
    
    public scoreboardPlayer(string name, SteamId id)
    {
        this.Name = name;
        this.id = id;
    }
}

public class Scoreboard
{
    private static List<scoreboardPlayer> _players = new();
    public List<scoreboardPlayer> players => _players;

    public static List<(SteamId id, Texture2D? avatar)> playerAvatars = new();

    public async void addPlayer(scoreboardPlayer player)
    {
        playerAvatars.Add((player.id, await GetAvatar(player.id)));
        _players.Add(player);
    }
    

    private static async Task<Texture2D?> GetAvatar(SteamId user)
    {
        try
        {
            if (playerAvatars.Any(_ => _.id == user))
            {
                return playerAvatars.FirstOrDefault(_ => _.id == user).avatar;
            }

            Steamworks.Data.Image? avatar = await SteamFriends.GetMediumAvatarAsync(user);
            var texture = avatar?.Convert();
            playerAvatars.Add((user, texture));
            return texture;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return null;
        }
    }
}
public static class ImageExtensions
{
    public static Texture2D Convert(this Steamworks.Data.Image image)
    {
        var avatar = new Texture2D((int)image.Width, (int)image.Height, TextureFormat.ARGB32, false);
        avatar.filterMode = FilterMode.Trilinear;

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                var p = image.GetPixel(x, y);
                avatar.SetPixel(x, (int)image.Height - y, new UnityEngine.Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
            }
        }

        avatar.Apply();
        return avatar;
    }
}