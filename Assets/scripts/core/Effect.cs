using System;

public enum EffectType
{
    Barrier,
    Regeneration,
    Burn
}

public class Effect
{
    public EffectType Type;
    public int RemainingTurns;
    public bool IsRemoved = false;
    public int Charges;

    public Effect(EffectType type, int duration, int charges = 1)
    {
        this.Type = type;
        this.RemainingTurns = duration;
        this.Charges = charges;
    }

    public Effect Clone()
    {
        var effect = new Effect(Type, RemainingTurns);
        return effect;
    }

    public void Update(BattleEntity effector)
    {
        if (Type == EffectType.Burn)
        {
            effector.TakeDamage(1);
        }
        if (Type == EffectType.Regeneration)
        {
            effector.Heal(2);
        }
    }
}