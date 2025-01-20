using System.Collections.Generic;
using UnityEngine;

public class Server
{
    public readonly GameServer GameServer;

    public Server()
    {
        GameServer = new GameServer();
    }


    public GameSessionState CreateNewSession()
    {
        GameSessionState session = new();
        return session;
    }
}
