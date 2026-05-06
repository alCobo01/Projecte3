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
    
    public void SetData(ItemStack stack)
    {
        iconImage.sprite = stack.item.icon;
        quantityText.text = stack.quantity.ToString();
        itemNameText.text = stack.item.itemName;
        descriptionText.text = stack.item.description;
    }
}
