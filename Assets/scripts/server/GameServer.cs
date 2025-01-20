using System.Collections.Generic;
using UnityEngine;

public class GameServer
{

    public GameServer()
    {
    }

    public bool UseAbility(
        GameSessionState session,
        BattleEntity user,
        AbilityType abilityType
    )
    {
        if (user.Abilities[abilityType].Cooldown == 0)
        {
            switch (abilityType)
            {
                case AbilityType.Attack:
                    BattleEntity battleEntity = session.GetAnyEnemyOf(user);
                    battleEntity.TakeDamage(8);
                    break;
                case AbilityType.Fireball:
                    battleEntity = session.GetAnyEnemyOf(user);
                    battleEntity.TakeDamage(GameConstants.REGENERATION_TICK_HEAL);
                    var effect = new Effect(EffectType.Burn, GameConstants.FIREBALL_DURATION);
                    battleEntity.ApplyEffect(effect);
                    user.Abilities[AbilityType.Fireball].IsAppliedWithEffect(effect);
                    break;
                case AbilityType.Regeneration:
                    effect = new Effect(EffectType.Regeneration, GameConstants.REGENERATION_DURATION);
                    user.ApplyEffect(effect);
                    user.Abilities[AbilityType.Regeneration].IsAppliedWithEffect(effect);
                    break;
                case AbilityType.Barrier:
                    effect = new Effect(EffectType.Barrier, GameConstants.BARRIER_DURATION, GameConstants.BARRIER_STRENGTH);
                    user.ApplyEffect(effect);
                    user.Abilities[AbilityType.Barrier].IsAppliedWithEffect(effect);
                    break;
                case AbilityType.Purify:
                    user.RemoveEffect(EffectType.Burn);
                    break;

            }

            user.Abilities[abilityType].Cooldown = user.Abilities[abilityType].MaxCooldown;
            session.Updated();
            return true;
        }

        return false;
    }

    public void EndTurn(GameSessionState session)
    {
        void UpdateTeam(List<BattleEntity> team)
        {
            foreach (var item in team)
            {
                item.UpdateEffects();
                item.UpdateCooldowns();
            }
        }

        UpdateTeam(session.Team1);
        UpdateTeam(session.Team2);

        foreach (var enemy in session.Team2)
        {
            UseAbility(
                session,
                enemy,
                GetRandomAbility(enemy)
            );
        }

        void CheckForNegativeHealth(List<BattleEntity> team)
        {
            foreach (var item in team)
            {
                if (item.CurrentHp <= 0)
                {
                    Debug.Log("session disposed");
                    session.Dispose();
                    return;
                }
            }
        }
        CheckForNegativeHealth(session.Team1);
        CheckForNegativeHealth(session.Team2);
    }

    AbilityType GetRandomAbility(
        BattleEntity user
    )
    {
        List<AbilityType> availableAbilities = new List<AbilityType>();

        foreach (var ability in user.Abilities)
        {
            if (ability.Value.CanBeUsed()) // Проверяем, можно ли использовать способность
            {
                availableAbilities.Add(ability.Key);
            }
        }

        // Если нет доступных способностей
        if (availableAbilities.Count == 0)
        {
            Debug.LogWarning("Нет доступных способностей!");
            return default;
        }

        // Выбираем случайную способность из доступных
        int randomIndex = UnityEngine.Random.Range(0, availableAbilities.Count);
        return availableAbilities[randomIndex];
    }
}
