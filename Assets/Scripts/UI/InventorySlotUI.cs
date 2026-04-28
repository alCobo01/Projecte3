using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button useButton;

    private ItemData _itemData;
    private UnityAction<ItemData> _onUse;
    
    public void SetData(ItemStack stack, UnityAction<ItemData> onUse)
    {
        _itemData = stack.item;
        _onUse = onUse;
        
        iconImage.sprite = stack.item.icon;
        quantityText.text = stack.quantity > 1 ? stack.quantity.ToString() : "";
        itemNameText.text = stack.item.itemName;
        descriptionText.text = stack.item.description;
        
        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(UseItem);
    }

    private void UseItem()
    {
        if (_itemData == null) return;
        _onUse?.Invoke(_itemData);
    }
}
