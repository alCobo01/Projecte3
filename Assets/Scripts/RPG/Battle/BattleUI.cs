using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private GameObject actionPanel;
    [SerializeField] private GameObject skillPanel;
    [SerializeField] private GameObject itemPanel;
    [SerializeField] private GameObject targetPanel;

    [Header("Action Buttons")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button skillButton;
    [SerializeField] private Button itemButton;
    [SerializeField] private Button fleeButton;

    [Header("HUD")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerHpText;
    [SerializeField] private TMP_Text playerSpText;
    [SerializeField] private Transform enemyListRoot;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Feedback")]
    [SerializeField] private float feedbackClearDelay = 2f;
    [SerializeField] private float feedbackUiUnlockDelay = 0.8f;
    [SerializeField] private float postBattleFeedbackDelay = 1.2f;

    [Header("Dynamic Lists")]
    [SerializeField] private Transform skillListRoot;
    [SerializeField] private Transform itemListRoot;
    [SerializeField] private Transform targetListRoot;
    [SerializeField] private Button listButtonPrefab;

    [Header("Turn Stack")]
    [SerializeField] private Transform turnStackRoot; 
    [SerializeField] private TurnOrderSlot turnSlotPrefab;
    
    private BattleManager _manager;
    private Action<BattleUnit> _onAttack;
    private Action<SkillData, BattleUnit> _onSkill;
    private Action<ItemData> _onItem;
    private Action _onFlee;

    private BattleUnit _player, _currentUnit;
    private List<BattleUnit> _enemies = new();
    private SkillData _selectedSkill;
    private bool _isPlayerTurn, _inputLockedByFeedback, _fleeSuccess;
    
    private Mode _backFromTargetMode = Mode.Action, _mode;
    private Coroutine _feedbackClearCoroutine, _feedbackUnlockCoroutine;

    private enum Mode { Hidden, Action, Skill, Item, Target }

    private readonly struct ListOption
    {
        public readonly string label;
        public readonly Action onClick;
        public readonly bool interactable;

        public ListOption(string label, Action onClick, bool interactable = true)
        {
            this.label = label;
            this.onClick = onClick;
            this.interactable = interactable;
        }
    }

    private void Awake()
    {
        attackButton.onClick.AddListener(ShowAttackTargets);
        skillButton.onClick.AddListener(ShowSkills);
        itemButton.onClick.AddListener(ShowItems);
        fleeButton.onClick.AddListener(RequestFlee);
        
        feedbackPanel.SetActive(false);
        SetMode(Mode.Hidden);
    }

    private void OnDisable()
    {
        Unsubscribe();
        StopFeedbackCoroutines();
    }

    public void Initialize(BattleManager manager)
    {
        if (_manager == manager)
        {
            RefreshHud();
            return;
        }

        Unsubscribe();
        _manager = manager;

        _manager.OnTurnStarted += HandleTurnStarted;
        _manager.OnDamageTaken += HandleDamageTaken;
        _manager.OnBattleStateChanged += RefreshHud;
        _manager.OnBattleEnded += HandleBattleEnded;
        _manager.OnFleeAttempted += HandleFleeAttempted;
        _manager.OnCombatMessage += HandleCombatMessage;
        _manager.OnRoundStarted += HandleRoundStarted;

        _fleeSuccess = false;
        _inputLockedByFeedback = false;
        _isPlayerTurn = false;
        ClearFeedback();
        SetMode(Mode.Action);
        ApplyActionInteractivity();
        RefreshHud();
    }

    public void ShowActionMenu(BattleUnit player, List<BattleUnit> enemies, Action<BattleUnit> onAttack,
        Action<SkillData, BattleUnit> onSkill, Action<ItemData> onItem, Action onFlee)
    {
        _player = player;
        _enemies = enemies ?? new List<BattleUnit>();
        _onAttack = onAttack;
        _onSkill = onSkill;
        _onItem = onItem;
        _onFlee = onFlee;
        _selectedSkill = null;

        SetMode(Mode.Action);
        BuildSkills();
        BuildItems();
        ApplyActionInteractivity();
        RefreshHud();
    }

    private void ShowAttackTargets()
    {
        if (_mode != Mode.Action || _enemies.Count == 0) return;

        _selectedSkill = null;
        _backFromTargetMode = Mode.Action;
        BuildTargets(_enemies, PickAttackTarget);
        SetMode(Mode.Target);
    }

    private void ShowSkills() { if (_mode == Mode.Action) { BuildSkills(); SetMode(Mode.Skill); } }
    private void ShowItems() { if (_mode == Mode.Action) { BuildItems(); SetMode(Mode.Item); } }

    private void RequestFlee()
    {
        if (_mode != Mode.Action) return;
        HideInputPanels();
        _onFlee?.Invoke();
    }

    private void PickAttackTarget(BattleUnit target)
    {
        if (target == null || target.IsDead) return;
        HideInputPanels();
        _onAttack?.Invoke(target);
    }

    private void PickSkill(SkillData skill)
    {
        if (!skill || _player == null || _player.CurrentSp < skill.spCost) return;

        _selectedSkill = skill;
        switch (skill.targetType)
        {
            case TargetType.Self:
                HideInputPanels();
                _onSkill?.Invoke(skill, _player);
                return;
            case TargetType.AllEnemies:
            {
                var firstAlive = _enemies.FirstOrDefault(e => !e.IsDead);
                if (firstAlive == null) return;
                HideInputPanels();
                _onSkill?.Invoke(skill, firstAlive);
                return;
            }
        }

        _backFromTargetMode = Mode.Skill;
        BuildTargets(_enemies, PickSkillTarget);
        SetMode(Mode.Target);
    }

    private void PickSkillTarget(BattleUnit target)
    {
        if (target == null || target.IsDead || !_selectedSkill) return;
        HideInputPanels();
        _onSkill?.Invoke(_selectedSkill, target);
    }

    private void PickItem(ItemData item)
    {
        var stats = PlayerStatsManager.Instance;
        if (!item || !stats) return;
        if (!stats.HasItem(item)) { BuildItems(); return; }

        HideInputPanels();
        _onItem?.Invoke(item);
    }

    private void BackFromTarget()
    {
        ClearButtons(targetListRoot);
        SetMode(_backFromTargetMode);
        if (_backFromTargetMode == Mode.Action) { BuildSkills(); BuildItems(); }
    }

    private void BuildSkills()
    {
        var skills = (_player?.Skills ?? new List<SkillData>())
            .Where(s => s)
            .Select(s => new ListOption($"{s.skillName} ({s.spCost} SP)", () => PickSkill(s), _player.CurrentSp >= s.spCost));

        BuildList(skillListRoot, skills, "No skills available", () => SetMode(Mode.Action));
    }

    private void BuildItems()
    {
        var inventory = PlayerStatsManager.Instance?.inventory;
        var items = (inventory ?? new List<ItemStack>())
            .Where(stack => stack != null && stack.item != null && stack.quantity > 0)
            .Select(stack => new ListOption($"{stack.item.itemName} x{stack.quantity}", () => PickItem(stack.item)));

        BuildList(itemListRoot, items, "No items available", () => SetMode(Mode.Action));
    }

    private void BuildTargets(List<BattleUnit> targets, Action<BattleUnit> onPick)
    {
        var targetOptions = (targets ?? new List<BattleUnit>())
            .Where(t => t is { IsDead: false })
            .Select(t => new ListOption($"{t.Data.characterName}", () => onPick?.Invoke(t)));

        BuildList(targetListRoot, targetOptions, emptyMessage: null, onBack: BackFromTarget);
    }

    private void BuildList(Transform root, IEnumerable<ListOption> options, string emptyMessage, Action onBack)
    {
        ClearButtons(root);
        var hasAny = false;
        foreach (var option in options ?? Enumerable.Empty<ListOption>())
        {
            hasAny = true;
            var button = CreateButton(root, option.label, option.onClick);
            button.interactable = option.interactable;
        }

        if (!hasAny && !string.IsNullOrEmpty(emptyMessage)) CreateDisabledRow(root, emptyMessage);
        CreateButton(root, "Back", onBack);
    }

    private Button CreateButton(Transform parent, string label, Action onClick)
    {
        var button = Instantiate(listButtonPrefab, parent);
        var rect = button.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.anchoredPosition = Vector2.zero;
        var actionRect = attackButton.GetComponent<RectTransform>();
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, actionRect.rect.height);

        var layoutElement = button.GetComponent<LayoutElement>();
        if (!layoutElement) layoutElement = button.gameObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = actionRect.rect.height;
        layoutElement.preferredHeight = actionRect.rect.height;
        layoutElement.flexibleWidth = 1f;
        layoutElement.flexibleHeight = 0f;

        var text = button.GetComponentInChildren<TMP_Text>(true);
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.text = label;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
        return button;
    }

    private void CreateDisabledRow(Transform parent, string text)
    {
        var row = CreateButton(parent, text, null);
        row.interactable = false;
    }

    private static void ClearButtons(Transform root)
    {
        for (var i = root.childCount - 1; i >= 0; i--) Destroy(root.GetChild(i).gameObject);
    }

    private void HandleTurnStarted(BattleUnit unit)
    {
        _currentUnit = unit;
        _isPlayerTurn = unit.IsPlayer;
        SetMode(Mode.Action);
        ApplyActionInteractivity();
        RefreshHud();
    }

    private void HandleBattleEnded(bool playerWon)
    {
        HideInputPanels();
        ShowFeedbackOnly();

        ClearFeedback();
        StopFeedbackCoroutines();
        var victoryMessage = playerWon && _manager != null && _manager.LastCoinReward > 0
            ? $"Victory! +{_manager.LastCoinReward} coins"
            : "Victory!";
        ShowFeedback(playerWon ? victoryMessage : (_fleeSuccess ? "Fled successfully! Run" : "Defeat"), 3f, blockInput: false);
        _fleeSuccess = false;
    }

    private void HandleDamageTaken(BattleUnit _) => RefreshHud();
    private void HandleRoundStarted(int round) { roundText.text = $"Round {Mathf.Max(1, round)}"; }

    private void HandleFleeAttempted(bool success)
    {
        _fleeSuccess = success;
        ShowFeedback(success ? "Escaped successfully." : "Could not escape! The enemy attacks.",
            success ? postBattleFeedbackDelay : feedbackClearDelay,
            blockInput: !success);
    }

    private void HandleCombatMessage(string message) => ShowFeedback(message, feedbackClearDelay, blockInput: true);

    private void ShowFeedback(string message, float clearDelay, bool blockInput)
    {
        feedbackPanel.SetActive(true);
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;

        _inputLockedByFeedback = blockInput;
        ApplyActionInteractivity();

        StopFeedbackCoroutines();
        _feedbackClearCoroutine = StartCoroutine(ClearFeedbackAfter(Mathf.Max(0f, clearDelay)));
        if (blockInput) _feedbackUnlockCoroutine = StartCoroutine(UnlockInputAfter(Mathf.Max(0f, feedbackUiUnlockDelay)));
    }

    private IEnumerator ClearFeedbackAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearFeedback();
        _feedbackClearCoroutine = null;
    }

    private IEnumerator UnlockInputAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        _inputLockedByFeedback = false;
        _feedbackUnlockCoroutine = null;
        ApplyActionInteractivity();
    }

    private void StopFeedbackCoroutines()
    {
        if (_feedbackClearCoroutine != null)
        {
            StopCoroutine(_feedbackClearCoroutine);
            _feedbackClearCoroutine = null;
        }

        if (_feedbackUnlockCoroutine != null)
        {
            StopCoroutine(_feedbackUnlockCoroutine);
            _feedbackUnlockCoroutine = null;
        }
    }

    public void CloseBattleUi() => SetMode(Mode.Hidden);

    private void ClearFeedback()
    {
        feedbackText.text = string.Empty;
        feedbackText.gameObject.SetActive(false);
        feedbackPanel.SetActive(false);
    }

    private void ApplyActionInteractivity()
    {
        var canUse = !_inputLockedByFeedback && _isPlayerTurn;
        attackButton.interactable = canUse && _enemies.Any(e => !e.IsDead);
        skillButton.interactable = canUse && _player != null && (_player.Skills?.Count ?? 0) > 0;
        itemButton.interactable = canUse;
        fleeButton.interactable = canUse;
    }

    private void SetMode(Mode mode)
    {
        _mode = mode;
        rootPanel.SetActive(mode != Mode.Hidden);
        actionPanel.SetActive(mode == Mode.Action);
        skillPanel.SetActive(mode == Mode.Skill);
        itemPanel.SetActive(mode == Mode.Item);
        targetPanel.SetActive(mode == Mode.Target);
    }

    private void HideInputPanels()
    {
        SetMode(Mode.Action);
        ClearButtons(skillListRoot);
        ClearButtons(itemListRoot);
        ClearButtons(targetListRoot);
    }


    private void ShowFeedbackOnly()
    {
        _mode = Mode.Hidden;
        rootPanel.SetActive(true);
        actionPanel.SetActive(false);
        skillPanel.SetActive(false);
        itemPanel.SetActive(false);
        targetPanel.SetActive(false);
        feedbackPanel.SetActive(true);
    }

    private void RefreshTurnStack()
    {
        if (!turnStackRoot || !turnSlotPrefab) return;
        ClearButtons(turnStackRoot);
        
        if (_currentUnit is { IsDead: false })
        {
            var currentSlot = Instantiate(turnSlotPrefab, turnStackRoot);
            currentSlot.Setup(_currentUnit, true);
        }

        var upcoming = _manager.GetUpcomingTurns(4);
        foreach (var unit in upcoming)
        {
            var slot = Instantiate(turnSlotPrefab, turnStackRoot);
            slot.Setup(unit, false);
        }
    }

    private void RefreshHud()
    {
        var player = _manager.PlayerUnit;
        playerNameText.text = player.Data.characterName;
        playerHpText.text = $"HP {player.CurrentHp}/{player.Data.maxHp}";
        playerSpText.text = $"SP {player.CurrentSp}/{player.Data.maxSp}";

        ClearButtons(enemyListRoot);
        foreach (var enemy in _manager.AllUnits.Where(u => !u.IsPlayer))
        {
            var row = CreateEnemyRow(enemyListRoot);
            row.text = enemy.IsDead
                ? $"{enemy.Data.characterName} - HP 0/{enemy.Data.maxHp} - DEAD"
                : $"{enemy.Data.characterName} - HP {enemy.CurrentHp}/{enemy.Data.maxHp}";
        }

        RefreshTurnStack();
    }

    private TMP_Text CreateEnemyRow(Transform parent)
    {
        var go = new GameObject("EnemyRow", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var row = go.AddComponent<TextMeshProUGUI>();
        row.font = playerNameText.font;
        row.autoSizeTextContainer = false;
        row.fontSharedMaterial = playerNameText.fontSharedMaterial;
        row.fontSize = playerNameText.fontSize;
        row.color = playerNameText.color;
        row.characterHorizontalScale = 1.65f;
        row.alignment = playerNameText.alignment;
        row.textWrappingMode = TextWrappingModes.Normal;
        row.fontSize = playerNameText.fontSize;
        row.overflowMode = TextOverflowModes.Ellipsis;
        return row;
    }

    private void Unsubscribe()
    {
        if (!_manager) return;

        _manager.OnTurnStarted -= HandleTurnStarted;
        _manager.OnDamageTaken -= HandleDamageTaken;
        _manager.OnBattleStateChanged -= RefreshHud;
        _manager.OnBattleEnded -= HandleBattleEnded;
        _manager.OnFleeAttempted -= HandleFleeAttempted;
        _manager.OnCombatMessage -= HandleCombatMessage;
        _manager.OnRoundStarted -= HandleRoundStarted;
        _manager = null;
    }
}
