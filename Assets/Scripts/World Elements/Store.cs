using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Store : NPC
{
    [Header("Store settings")]
    [SerializeField] private GameObject storePanel;
    [SerializeField] private RectTransform itemsContainer;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private TMP_Text coinAmountText;
    [SerializeField] private float fadeDuration = 2f;
    
    [Header("Items to buy data")]
    [SerializeField] private List<ItemStack> itemsToBuy;

    private CanvasGroup _canvasGroup;
    private Coroutine _fadeCoroutine;
    private bool _hasTalkedOnce;

    private void Awake() => _canvasGroup = storePanel.GetComponent<CanvasGroup>();

    public override void Interact()
    {
        if (!_hasTalkedOnce) base.Interact();
        else
        {
            if (Time.time < _lastInteractionTime + interactionCooldown) return;
            _lastInteractionTime = Time.time;
            ToggleStore();
        }
    }
    
    public void CloseStore() => ToggleStore();

    protected override void EndDialogue(bool completed)
    {
        base.EndDialogue(completed);
        if (completed) _hasTalkedOnce = true;
    }
    
    private void ToggleStore()
    {
        var isActive = !storePanel.activeSelf;
        storePanel.SetActive(isActive);
        PauseManager.Instance.TogglePause();
        
        if (isActive) RefreshUI();
        if (_fadeCoroutine is not null) StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(isActive ? Fade(0f, 1f) : Fade(1f, 0f, onComplete: () => storePanel.SetActive(false)));
    }
    
    private void RefreshUI()
    {
        //Coins
        coinAmountText.text = PlayerStatsManager.Instance.currentCoins.ToString();
        
        //Items
        foreach (Transform child in itemsContainer) Destroy(child.gameObject);
        foreach (var stack in itemsToBuy)
        {
            var obj = Instantiate(slotPrefab, itemsContainer, false);
            obj.GetComponent<InventorySlotUI>().SetData(stack);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(itemsContainer);
    }

    private IEnumerator Fade(float from, float to, UnityAction onComplete = null)
    {
        var elapsed = 0f;
        _canvasGroup.alpha = from;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = to;
        onComplete?.Invoke();
    }
}
