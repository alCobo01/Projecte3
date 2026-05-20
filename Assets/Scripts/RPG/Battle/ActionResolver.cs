using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ActionResolver
{
    public static void ResolveAttack(BattleUnit attacker, BattleUnit target)
    {
        var dmg = Mathf.Max(1, attacker.GetStat(StatType.Attack) - target.GetStat(StatType.Defense));
        target.TakeDamage(dmg);
        attacker.GainSp(attacker.Data.spGainPerHit);
    }

    public static void ResolveSkill(BattleUnit attacker, BattleUnit target,
        SkillData skill, List<BattleUnit> allEnemies)
    {
        var targets = skill.targetType switch
        {
            TargetType.Self        => new List<BattleUnit> { attacker },
            TargetType.AllEnemies  => allEnemies.Where(u => !u.IsDead).ToList(),
            _                      => new List<BattleUnit> { target }
        };

        foreach (var t in targets)
        {
            if (skill.basePower > 0)
                t.TakeDamage(skill.basePower);

            if (skill.appliedEffect && Random.value < skill.effectChance)
                t.ApplyEffect(skill.appliedEffect);
        }
    }

    public static void ResolveItem(BattleUnit user, ItemData item)
    {
        switch (item.type)
        {
            case ItemType.Healing:
                user.Heal(item.value);
                if (user.IsPlayer) PlayerStatsManager.Instance.currentHp = user.CurrentHp;
                break;
            case ItemType.Energy:
                user.GainSp(item.value);
                if (user.IsPlayer) PlayerStatsManager.Instance.currentSp = user.CurrentSp;
                break;
            case ItemType.Buff:
                user.ApplyStatBoost(item.boostedStat, item.value, item.boostDuration);
                break;
        }

        if (user.IsPlayer) PlayerStatsManager.Instance.ConsumeItem(item);
    }
}
