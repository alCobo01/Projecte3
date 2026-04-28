using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private GameObject inventoryPanel;
    
    private void OnEnable()
    {
        PlayerStatsManager.OnInventoryChanged += RefreshUI;
        PlayerInputController.OnOpenInventoryEvent += ToggleInventory;
    }

    private void OnDisable()
    {
        PlayerStatsManager.OnInventoryChanged -= RefreshUI;
        PlayerInputController.OnOpenInventoryEvent -= ToggleInventory;
    } 
    
    private void ToggleInventory()
    {
        var isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);
        PauseManager.Instance.TogglePause();
        if (isActive) RefreshUI();
    }
    
    private void RefreshUI()
    {
        foreach (Transform child in container) Destroy(child.gameObject);
        foreach (var stack in PlayerStatsManager.Instance.inventory)
        {
            if (stack.quantity <= 0) continue;

            var obj = Instantiate(slotPrefab, container, false);
            obj.transform.localScale = Vector3.one;
            obj.GetComponent<InventorySlotUI>().SetData(stack);
        }
    }
}
