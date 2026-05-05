using UnityEngine;
using UnityEngine.Events;

public class SkillEvents : MonoBehaviour
{
    public static event UnityAction<SkillData> OnSkillUnlocked;
    public static void TriggerSkillUnlocked(SkillData skill)
    {
        OnSkillUnlocked?.Invoke(skill);
    }
}