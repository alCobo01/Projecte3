using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text priceText;
    
    private Store _store;
    private ItemStack _stack;

    public void SetData(ItemStack stack, Store store)
    {
        _stack = stack;
        _store = store;

        iconImage.sprite = stack.item.icon;
        quantityText.text = stack.quantity.ToString();
        itemNameText.text = stack.item.itemName;
        descriptionText.text = stack.item.description;
        priceText.text = stack.item.value.ToString();
        
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
        
        // El botón solo se desactiva si no hay stock. 
        // Si no hay monedas, lo dejamos activo para mostrar el mensaje de error.
        buyButton.interactable = stack.quantity > 0;
    }

    private void OnBuyClicked()
    {
        var price = _stack.item.value;
        if (PlayerStatsManager.Instance.currentCoins < price)
        {
            _store.ShowFeedback("You don't have enough coins!");
            return;
        }
        
        // Buy one item
        PlayerStatsManager.Instance.SubstractCoins(price);
        PlayerStatsManager.Instance.AddItem(new ItemStack { item = _stack.item, quantity = 1 });
            
        // Substract from store stock and refresh UI
        _stack.quantity--;
        _store.RefreshUI();

        _store.ShowFeedback($"You just bought {_stack.item.itemName} for {price} coins!");
            
        if (_stack.quantity <= 0)
            buyButton.interactable = false;
    }
}
