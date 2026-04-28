using System;

public class StatusEffect
{
    public StatusEffectData Data { get; private set; }
    public int RemainingTurns { get; private set; }
    public bool IsExpired => RemainingTurns <= 0;

    public StatType? BoostedStat { get; }
    public int BoostAmount { get; }
    
    public StatusEffect(StatusEffectData data)
    {
        Data = data;
        RemainingTurns = data.duration;
    }
    
    public StatusEffect(StatType stat, int amount, int duration)
    {
        BoostedStat = stat;
        BoostAmount = amount;
        RemainingTurns = duration;
    }

    public void Tick(BattleUnit unit)
    {
        switch (Data.type)
        {
            case StatusEffectType.Poison: unit.TakeDamage(Data.damagePerTurn); break;
            case StatusEffectType.Regen:  unit.Heal(Data.healPerTurn);         break;
            case StatusEffectType.Stun:   unit.SkipNextAction = true;          break;
            default:
                break;
        }
        
        RemainingTurns--;
    }
}
