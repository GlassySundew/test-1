using System.Collections.Generic;
using System.Linq;

public class BattleEntity
{
    public string Name;
    public int CurrentHp;
    public int MaxHp;
    public List<Effect> ActiveEffects;
    public Dictionary<AbilityType, Ability> Abilities;

    public BattleEntity(string name, int maxHp)
    {
        this.Name = name;
        this.MaxHp = maxHp;
        CurrentHp = maxHp;
        ActiveEffects = new List<Effect>();
        Abilities = new Dictionary<AbilityType, Ability>();
    }

    public BattleEntity Clone()
    {
        var entity = new BattleEntity(Name, MaxHp);
        entity.CopyFrom(this);
        return entity;
    }

    public void CopyFrom(BattleEntity other)
    {
        this.Name = other.Name;
        this.MaxHp = other.MaxHp;
        this.CurrentHp = other.CurrentHp;

        ActiveEffects.Clear();
        foreach (var item in other.ActiveEffects)
        {
            ActiveEffects.Add(item.Clone());
        }

        Abilities.Clear();
        foreach (var item in other.Abilities)
        {
            Abilities[item.Key] = item.Value.Clone();
        }
    }

    public void TakeDamage(int damage)
    {
        var barrierMaybe = ActiveEffects.Where(item => item.Type == EffectType.Barrier);
        if (barrierMaybe.Count() > 0)
        {
            var barrier = barrierMaybe.First();
            var damageWas = damage;
            damage -= barrier.Charges;
            if (damage > 0)
            {
                RemoveEffect(EffectType.Barrier);
            }
            else
            {
                barrier.Charges -= damageWas;
                if (barrier.Charges <= 0)
                {
                    RemoveEffect(EffectType.Barrier);
                }
                return;
            }
        }
        CurrentHp -= damage;
    }

    public void Heal(int amount)
    {
        CurrentHp += amount;
        if (CurrentHp > MaxHp) CurrentHp = MaxHp;
    }

    public void ApplyEffect(Effect effect)
    {
        ActiveEffects.Add(effect);
    }

    public void RemoveEffect(EffectType type)
    {
        foreach (var item in ActiveEffects)
        {
            if (item.Type != type) continue;
            item.IsRemoved = true;
        }
        ActiveEffects.RemoveAll(e => e.Type == type);
    }

    public void RemoveAllEffects()
    {
        foreach (var effect in ActiveEffects)
        {
            effect.IsRemoved = true;
        }
        ActiveEffects.Clear();
    }

    public void UpdateEffects()
    {
        foreach (var item in ActiveEffects.ToList())
        {
            item.Update(this);
            item.RemainingTurns--;
            if (item.RemainingTurns < 0)
            {
                item.IsRemoved = true;
                ActiveEffects.Remove(item);
            }
        }
    }

    public bool CanUseAbility(AbilityType ability)
    {
        return Abilities.ContainsKey(ability) && Abilities[ability].Cooldown == 0;
    }

    public void UpdateCooldowns()
    {
        foreach (var ability in Abilities.Values)
        {
            if (
                ability.Cooldown > 0
                && (
                    ability.CurrentlyAppliedEffect == null
                    || ability.CurrentlyAppliedEffect.IsRemoved
                )
            )
            {
                ability.Cooldown--;
            }
        }
    }
}
