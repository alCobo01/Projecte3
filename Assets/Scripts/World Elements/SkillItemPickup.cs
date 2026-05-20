using UnityEngine;

public class SkillItemPickup : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        SkillEvents.TriggerSkillUnlocked();
        Destroy(gameObject);
    } 
}
