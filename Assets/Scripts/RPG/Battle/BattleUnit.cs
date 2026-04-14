using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleUnit
{
    public CharacterData Data { get; }
    public int CurrentHp { get; private set; }
    public int CurrentSp { get; private set; }
    public bool SkipNextAction { get; set; }
    public bool IsPlayer { get; }
    
    public bool IsDead => CurrentHp <= 0;
    public bool IsStunned => _activeEffects.Any(e => e.Data != null && e.Data.type == StatusEffectType.Stun);
    
    private List<StatusEffect> _activeEffects = new();
    private Dictionary<StatType, int> _tempBoosts = new();

    public BattleUnit(CharacterData data, bool isPlayer, int startingSp = 0)
    {
        Data = data;
        CurrentHp = data.maxHp;
        IsPlayer = isPlayer;
        CurrentSp = startingSp;
    }

    public void SetHp(int hp)
        => CurrentHp = Mathf.Clamp(hp, 0, Data.maxHp);

    public void TakeDamage(int amount)
        => CurrentHp = Mathf.Max(0, CurrentHp - Mathf.Max(1, amount));

    public void Heal(int amount)
        => CurrentHp = Mathf.Min(Data.maxHp, CurrentHp + amount);

    public void GainSp(int amount)
        => CurrentSp = Mathf.Min(Data.maxSp, CurrentSp + amount);

    public bool SpendSp(int amount)
    {
        if (CurrentSp < amount) return false;
        CurrentSp -= amount;
        return true;
    }

    public int GetStat(StatType stat)
    {
        var baseVal = stat switch
        {
            StatType.Attack       => Data.attack,
            StatType.Defense      => Data.defense,
            StatType.Speed        => Data.speed,
            _                     => 0
        };
        _tempBoosts.TryGetValue(stat, out var bonus);
        return baseVal + bonus;
    }

    public void ApplyEffect(StatusEffectData effectData)
        => _activeEffects.Add(new StatusEffect(effectData));

    public void ApplyStatBoost(StatType stat, int amount, int duration)
    {
        _tempBoosts[stat] = _tempBoosts.GetValueOrDefault(stat) + amount;
        _activeEffects.Add(new StatusEffect(stat, amount, duration));
    }

    public void TickEffects()
    {
        foreach (var e in _activeEffects)
        {
            e.Tick(this);
            if (e.IsExpired && e.BoostedStat.HasValue)
                _tempBoosts[e.BoostedStat.Value] -= e.BoostAmount;
        }
        _activeEffects.RemoveAll(e => e.IsExpired);
    }
}
