using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private RectTransform container;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TMP_Text emptyInventoryText;
    
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
        PauseManager.Instance.SetPaused(isActive);
        
        if (isActive)
            RefreshUI();
        else
            emptyInventoryText.gameObject.SetActive(false);
    }
    
    private void RefreshUI()
    {
        foreach (Transform child in container) Destroy(child.gameObject);
        var hasItems = false;

        foreach (var stack in PlayerStatsManager.Instance.inventory.Where(stack => stack.quantity > 0))
        {
            hasItems = true;

            var obj = Instantiate(slotPrefab, container, false);
            obj.GetComponent<InventorySlotUI>().SetData(stack);
        }
        
        emptyInventoryText.gameObject.SetActive(!hasItems);
        LayoutRebuilder.ForceRebuildLayoutImmediate(container);
    }
}
