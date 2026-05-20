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
    [SerializeField] private AvailableItems itemsToBuy;

    [Header("Feedback UI")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private float feedbackDisplayTime = 2f;

    private CanvasGroup _canvasGroup, _feedbackCanvasGroup;
    private Coroutine _fadeCoroutine, _feedbackCoroutine;
    private bool _hasTalkedOnce;
    private PlayerAttackController _playerAttackController;

    private void Awake() 
    {
        _canvasGroup = storePanel.GetComponent<CanvasGroup>();
        _feedbackCanvasGroup = feedbackPanel.GetComponent<CanvasGroup>();
        feedbackPanel.SetActive(false);
        SavePlayerAttackController();

        // Nos suscribimos al evento del DialogueManager
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;
        }
    }

    private void OnDestroy()
    {
        // Importante desuscribirse al destruir el objeto
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
        }
    }

    private void HandleDialogueEnd()
    {
        // Si acabamos de hablar por primera vez, marcamos que ya puede abrir la tienda
        if (!_hasTalkedOnce)
        {
            _hasTalkedOnce = true;
        }
    }

    public void ShowFeedback(string message)
    {
        if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
        _feedbackCoroutine = StartCoroutine(HandleFeedback(message));
    }

    private IEnumerator HandleFeedback(string message)
    {
        feedbackText.text = message;
        feedbackPanel.SetActive(true);
        yield return Fade(_feedbackCanvasGroup, 0f, 1f);
        yield return new WaitForSecondsRealtime(feedbackDisplayTime);
        yield return Fade(_feedbackCanvasGroup, 1f, 0f, onComplete: () => feedbackPanel.SetActive(false));
    }

    public override void Interact()
    {
        if (!_hasTalkedOnce) base.Interact();
        else
        {
            if (Time.time < _lastInteractionTime + interactionCooldown) return;
            _lastInteractionTime = Time.time;
            SoundManager.Instance.PlayMusicByIndex(2);
            ToggleStore();
        }
    }
    
    public void CloseStore() => ToggleStore();

    private void ToggleStore()
    {
        var isActive = !storePanel.activeSelf;
        if (isActive) 
        {
            storePanel.SetActive(true);
            RefreshUI();
            SetPlayerAttackEnabled(false);
        }
        else
        {
            SoundManager.Instance?.PlayMusicByIndex(1);
            SetPlayerAttackEnabled(true);
        }
        
        PauseManager.Instance.TogglePause();

        if (_fadeCoroutine is not null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(isActive ? Fade(_canvasGroup, 0f, 1f) : Fade(_canvasGroup, 1f, 0f, onComplete: () => storePanel.SetActive(false)));
    }
    
    public void RefreshUI()
    {
        //Coins
        coinAmountText.text = PlayerStatsManager.Instance.currentCoins.ToString();
        
        //Items
        foreach (Transform child in itemsContainer) Destroy(child.gameObject);
        foreach (var stack in itemsToBuy.items)
        {
            if (stack.quantity <= 0) continue;
            var obj = Instantiate(slotPrefab, itemsContainer, false);
            obj.GetComponent<StoreSlotUI>().SetData(stack, this);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(itemsContainer);
    }

    private IEnumerator Fade(CanvasGroup cg, float from, float to, UnityAction onComplete = null)
    {
        var elapsed = 0f;
        cg.alpha = from;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        cg.alpha = to;
        onComplete?.Invoke();
    }

    private void SetPlayerAttackEnabled(bool isEnabled)
    {
        SavePlayerAttackController();
        if (_playerAttackController != null) _playerAttackController.enabled = isEnabled;
    }

    private void SavePlayerAttackController()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player)
            _playerAttackController = player.GetComponentInChildren<PlayerAttackController>();
    }
}
