using UnityEngine;

public class SkillItemPickup : MonoBehaviour, IInteractable
{
    public void Interact(GameObject _)
    {
        SkillEvents.TriggerSkillUnlocked();
        Destroy(gameObject);
    } 
}
