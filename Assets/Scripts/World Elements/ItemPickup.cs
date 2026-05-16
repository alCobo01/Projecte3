using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemStack stack;
    
    public void Interact()
    {
        if (stack == null || !stack.item || stack.quantity <= 0) return;
        PlayerStatsManager.Instance.AddItem(stack);
        Destroy(gameObject);
    }
}
