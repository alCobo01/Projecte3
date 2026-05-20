using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BattleUnit
{
    public CharacterData Data { get; }
    public List<SkillData> Skills { get; private set; }
    public CharacterAnimationController Animator { get; set; }
    public Transform Transform { get; set; }
    public Transform FeetVfxAnchor { get; set; }
    public int CurrentHp { get; private set; }
    public int CurrentSp { get; private set; }
    public bool SkipNextAction { get; set; }
    public bool IsPlayer { get; }

    public event UnityAction<BattleUnit, StatusEffectData> OnStatusEffectApplied;
    public event UnityAction<BattleUnit, StatType, int> OnStatBoostApplied;
    
    public bool IsDead => CurrentHp <= 0;
    public bool IsStunned => _activeEffects.Any(e => e.Data != null && e.Data.type == StatusEffectType.Stun);
    
    private readonly List<StatusEffect> _activeEffects = new();
    private readonly Dictionary<StatType, int> _tempBoosts = new();

    public BattleUnit(CharacterData data, bool isPlayer, int startingSp = 0)
    {
        Data = data;
        CurrentHp = data.maxHp;
        IsPlayer = isPlayer;
        CurrentSp = startingSp;
        Skills = new List<SkillData>();
    }

    public void SetSkills(List<SkillData> skills)
    {
        Skills = skills != null ? new List<SkillData>(skills) : new List<SkillData>();
    }

    public void SetHp(int hp)
        => CurrentHp = Mathf.Clamp(hp, 0, Data.maxHp);

    public void TakeDamage(int amount)
    {
        CurrentHp = Mathf.Max(0, CurrentHp - Mathf.Max(1, amount));
        Animator?.TriggerHurt();
    }

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
    {
        _activeEffects.Add(new StatusEffect(effectData));
        OnStatusEffectApplied?.Invoke(this, effectData);
    }

    public void ApplyStatBoost(StatType stat, int amount, int duration)
    {
        _tempBoosts[stat] = _tempBoosts.GetValueOrDefault(stat) + amount;
        _activeEffects.Add(new StatusEffect(stat, amount, duration));
        OnStatBoostApplied?.Invoke(this, stat, amount);
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
