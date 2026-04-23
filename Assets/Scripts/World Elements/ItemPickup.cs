using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemStack stack;
    
    public void Interact()
    {
        if (TryGetComponent<PlayerInputController>(out _))
            PlayerStatsManager.Instance.AddItem(stack);
    }
}