using UnityEngine;
using UnityEngine.Events;

public class SkillEvents : MonoBehaviour
{
    public static event UnityAction OnSkillUnlocked;
    public static void TriggerSkillUnlocked()
    {
        OnSkillUnlocked?.Invoke();
    }
}