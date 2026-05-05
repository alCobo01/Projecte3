using UnityEngine;

public class SkillItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private SkillData skillToUnlock;

    public void Interact()
    {
        SkillEvents.TriggerSkillUnlocked(skillToUnlock);
        Destroy(gameObject);
    } 
}
