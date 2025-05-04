﻿using MultiplayerUtil;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clogger = ULTRAKILL_Competitive_Multiplayer.Logger;
using ULTRAKILL;
using GameConsole;
using Steamworks.Data;

namespace ULTRAKILL_Competitive_Multiplayer;

public class Command
{
    static bool registered = false;
    public static void Register()
    {
        if (registered) return;

        registered = true;
        var nestedTypes = typeof(Commands).GetNestedTypes();

        GameConsole.Console.Instance.RegisterCommands(nestedTypes.Where(_ => typeof(ICommand).IsAssignableFrom(_)).
            Select(_ => Activator.CreateInstance(_) as GameConsole.ICommand).
            Where(_ => _ != null)
            .ToList()
        );
    }
}

public class Commands
{
    /// <summary>
    /// Send Message
    /// </summary>
    public class SM : GameConsole.ICommand
    {
        public string Name => "SendMessage";

        public string Description => "";

        public string Command => "SM";

        public async void Execute(GameConsole.Console con, string[] args)
        {
            MultiplayerUtil.LobbyManager.SendMessage(string.Join(" ", args));
        }
    }

    /// <summary>
    /// Join Lobby
    /// </summary>
    public class JoinLobby : GameConsole.ICommand
    {
        public string Name => "JoinLobby";

        public string Description => "";

        public string Command => "JL";

        public async void Execute(GameConsole.Console con, string[] args)
        {
            MultiplayerUtil.LobbyManager.JoinLobbyWithID(ulong.Parse(args[0]));
        }
    }

    /// <summary>
    /// Create Lobby
    /// </summary>
    public class CreateLobby : GameConsole.ICommand
    {
        public string Name => "CreateLobby";

        public string Description => "";

        public string Command => "CL";

        public async void Execute(GameConsole.Console con, string[] args)
        {
            MultiplayerUtil.LobbyManager.CreateLobby("New lobby", 3, true, true, false, false, ("Idk", "idk"));
        }
    }

    /// <summary>
    /// List Lobbies
    /// </summary>
    public class ListLobbys : GameConsole.ICommand
    {
        public string Name => "ListLobbys";

        public string Description => "";

        public string Command => "LL";

        public async void Execute(GameConsole.Console con, string[] args)
        {
            Clogger.Log("Retriving all open lobbies");
            List<Lobby> getthingy = getthingy = await MultiplayerUtil.LobbyManager.FetchLobbies(("Idk", "idk"));


            foreach (Lobby lob in getthingy)
            {
                Clogger.Log("-------------------");

                Clogger.Log($"Lobby name: {lob.Data.Where(kvp => kvp.Key == "name" && !string.IsNullOrEmpty(kvp.Value))
                             .Select(kvp => kvp.Value)
                             .FirstOrDefault()} ");

                Clogger.Log($"Members: {lob.Data.Where(kvp => kvp.Key == "members" && !string.IsNullOrEmpty(kvp.Value))
                             .Select(kvp => kvp.Value)
                             .FirstOrDefault()} ");

                Clogger.Log($"Id: {lob.Id}");
                Clogger.Log($"Owner:{lob.Data.Where(kvp => kvp.Key == "Owner" && !string.IsNullOrEmpty(kvp.Value))
                            .Select(kvp => kvp.Value)
                            .FirstOrDefault()}");
            }
        }
    }

    /// <summary>
    /// Disconect from current lobby
    /// </summary>
    public class Disconect : GameConsole.ICommand
    {
        public string Name => "Disconect";

        public string Description => "";

        public string Command => "DC";

        public async void Execute(GameConsole.Console con, string[] args)
        {
            MultiplayerUtil.LobbyManager.Disconnect();
        }
    }

    public class P2PSend : GameConsole.ICommand
    {
        public string Name => "P2PSend";

        public string Description => "";

        public string Command => "P2PSend";

        public async void Execute(GameConsole.Console con, string[] args)
        {
            TestMessage tm = new();
            tm.msg = string.Join(" ", args);
            Clogger.Log($"Sending: {string.Join(" ", args)} with p2p");
            MultiplayerUtil.LobbyManager.SendData(tm);
        }
    }

    public class CreateCustomLobby : GameConsole.ICommand
    {
        public string Name => "CreateCustomLobby ";

        public string Description => "";

        public string Command => "CCL";

        public async void Execute(GameConsole.Console con, string[] args)
        {
            MultiplayerUtil.LobbyManager.CreateLobby(args[0], int.Parse(args[1]), bool.Parse(args[2]), true, false, false, ("Idk", "idk"));
        }
    }

    public class SelfP2P : GameConsole.ICommand
    {
        public string Name => "SelfP2P";

        public string Description => "";

        public string Command => "SELFP2P";

        public async void Execute(GameConsole.Console con, string[] args)
        {
            Clogger.Log($"Current steam id: {SteamManager.instance.selfID}");
            MultiplayerUtil.SteamManager.instance.EstablishP2P(SteamManager.instance.selfID);
            MultiplayerUtil.SteamManager.instance.server.besties.Add(SteamManager.instance.selfID);
        }
    }

}
[System.Serializable]
class TestMessage
{
    public string msg = "";
}