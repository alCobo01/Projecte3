using UnityEngine;
using UnityEngine.Events;

public static class SkillEvents
{
    public static event UnityAction OnSkillUnlocked;
    public static void TriggerSkillUnlocked()
    {
        OnSkillUnlocked?.Invoke();
    }
}