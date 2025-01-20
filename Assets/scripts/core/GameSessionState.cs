using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using UnityEngine;

public class GameSessionState
{
    public List<BattleEntity> Team1 { get; private set; }
    public List<BattleEntity> Team2 { get; private set; }
    public bool IsDisposed { get; private set; }

    public event Action OnChanged;
    public event Action Disposed;

    public GameSessionState()
    {
        RestartGameState();
    }

    public void RestartGameState()
    {
        BattleEntity player = CreateGenericBattleEntity("player");
        BattleEntity enemy = CreateGenericBattleEntity("enemy");
        Team1?.Clear();
        Team2?.Clear();
        Team1 = new List<BattleEntity>();
        Team2 = new List<BattleEntity>();
        Team1.Add(player);
        Team2.Add(enemy);
    }

    public void SubscribeSessionForPropagation(GameSessionState session)
    {
        void copyTeamInto(List<BattleEntity> from, List<BattleEntity> to)
        {
            foreach (var (entity, idx) in from.Select((value, i) => (value, i)))
            {
                if (idx >= to.Count())
                {
                    to.Add(entity.Clone());
                }
                else
                {
                    to.ElementAt(idx).CopyFrom(entity);
                }
            }
        }

        void propagateSession()
        {
            copyTeamInto(Team1, session.Team1);
            copyTeamInto(Team2, session.Team2);
            session.OnChanged?.Invoke();
        }

        Disposed += () => session.Disposed?.Invoke();
        OnChanged += propagateSession;
        propagateSession();
    }

    public BattleEntity GetAnyEnemyOf(BattleEntity entity)
    {
        if (Team1.Contains(entity))
        {
            return Team2.ElementAt(0);
        }
        if (Team2.Contains(entity))
        {
            return Team1.ElementAt(0);
        }

        throw new Exception("entity: " + entity + " does not belong to this session" + " (while getting any enemy)");
    }

    public void Updated()
    {
        OnChanged?.Invoke();
    }

    public void Dispose()
    {
        IsDisposed = true;
        Disposed?.Invoke();
    }

    BattleEntity CreateGenericBattleEntity(string name, int maxHp = -1)
    {
        if (maxHp <= 0) maxHp = GameConstants.DEFAULT_MAX_HP;

        BattleEntity entity = new BattleEntity(name, maxHp);
        entity.Abilities[AbilityType.Attack] = new Ability(AbilityType.Attack, GameConstants.ATTACK_CD);
        entity.Abilities[AbilityType.Barrier] = new Ability(AbilityType.Barrier, GameConstants.BARRIER_CD);
        entity.Abilities[AbilityType.Regeneration] = new Ability(AbilityType.Regeneration, GameConstants.REGENERATION_CD);
        entity.Abilities[AbilityType.Fireball] = new Ability(AbilityType.Fireball, GameConstants.FIREBALL_CD);
        entity.Abilities[AbilityType.Purify] = new Ability(AbilityType.Purify, GameConstants.PURIFY_CD);
        return entity;
    }
}
