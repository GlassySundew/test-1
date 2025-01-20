using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ServerAdapter
{
    private readonly Server Server;
    private GameSessionState SurrentServerSession;
    private GameSessionState CurrentClientSession;

    public ServerAdapter()
    {
        Server = new Server();
    }

    public GameSessionState RequestNewGameSession()
    {
        SurrentServerSession = Server.CreateNewSession();

        // эмулируем репликацию состояния сессии по сети:
        CurrentClientSession = new GameSessionState();
        SurrentServerSession.SubscribeSessionForPropagation(CurrentClientSession);
        return CurrentClientSession;
    }

    /// <summary>
    /// возвращает игрока, который находится под контролем 'клиента'
    /// </summary>
    /// <returns></returns>
    public BattleEntity GetCurrentPlayer()
    {
        return CurrentClientSession.Team1.ElementAt(0);
    }

    /// <summary>
    /// возвращает первого попавшегося врага
    /// </summary>
    /// <returns></returns>
    public BattleEntity GetAnyEnemy()
    {
        return CurrentClientSession.Team2.ElementAt(0);
    }

    public bool UseAbility(AbilityType abilityType)
    {
        return Server.GameServer.UseAbility(SurrentServerSession, GetCurrentServerPlayer(), abilityType);
    }

    public void EndPlayerTurn()
    {
        Server.GameServer.EndTurn(SurrentServerSession);
    }

    /// <summary>
    /// возвращает серверный инстанс игрока, который соответствует этому клиентскому подключению
    /// </summary>
    /// <returns></returns>
    private BattleEntity GetCurrentServerPlayer()
    {
        return SurrentServerSession.Team1.ElementAt(0);
    }

}
