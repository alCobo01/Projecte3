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
        PauseManager.Instance.TogglePause();
        if (isActive)
        {
            RefreshUI();
        }
        else if (emptyInventoryText != null)
        {
            emptyInventoryText.gameObject.SetActive(false);
        }
    }
    
    private void RefreshUI()
    {
        foreach (Transform child in container) Destroy(child.gameObject);
        var hasItems = false;

        foreach (var stack in PlayerStatsManager.Instance.inventory.Where(stack => stack.quantity > 0))
        {
            hasItems = true;

            var obj = Instantiate(slotPrefab);
            var slotTransform = obj.transform as RectTransform;
            slotTransform.SetParent(container, false);
            slotTransform.localScale = Vector3.one;
            slotTransform.localRotation = Quaternion.identity;
            slotTransform.anchoredPosition3D = Vector3.zero;
            obj.GetComponent<InventorySlotUI>().SetData(stack, HandleUseItem);
        }
        
        emptyInventoryText.gameObject.SetActive(!hasItems);
        LayoutRebuilder.ForceRebuildLayoutImmediate(container);
    }
    
    private static void HandleUseItem(ItemData item) => PlayerStatsManager.Instance.ConsumeItem(item);
}
