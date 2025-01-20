using System;

public enum AbilityType
{
    Attack,
    Barrier,
    Regeneration,
    Fireball,
    Purify
}

public class Ability
{
    public readonly AbilityType Type;
    public readonly int MaxCooldown;
    public int Cooldown;
    public Effect CurrentlyAppliedEffect;

    public Ability(AbilityType type, int maxCooldown)
    {
        this.Type = type;
        this.MaxCooldown = maxCooldown;
        Cooldown = 0;
    }

    public bool CanBeUsed()
    {
        return Cooldown == 0;
    }

    public void IsAppliedWithEffect(Effect effect)
    {
        CurrentlyAppliedEffect = effect;
    }

    public Ability Clone()
    {
        var ability = new Ability(Type, MaxCooldown);
        ability.Cooldown = Cooldown;
        return ability;
    }
}