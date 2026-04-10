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
    [SerializeField] private GameObject battleEndPanel;

    [Header("Action Buttons")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button skillButton;
    [SerializeField] private Button itemButton;
    [SerializeField] private Button fleeButton;

    [Header("Combatant HUD")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerHpText;
    [SerializeField] private TMP_Text playerSpText;
    [SerializeField] private TMP_Text enemyNameText;
    [SerializeField] private TMP_Text enemyHpText;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text battleEndText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private float feedbackClearDelay = 2f;
    [SerializeField] private float feedbackUiUnlockDelay = 0.8f;
    [SerializeField] private float postBattleFeedbackDelay = 1.2f;

    [Header("Dynamic Lists")]
    [SerializeField] private Transform skillListRoot;
    [SerializeField] private Transform itemListRoot;
    [SerializeField] private Transform targetListRoot;
    [SerializeField] private Button listButtonPrefab;

    private BattleManager _battleManager;

    private Action<BattleUnit> _onAttack;
    private Action<SkillData, BattleUnit> _onSkill;
    private Action<ItemData> _onItem;
    private Action _onFlee;

    private BattleUnit _currentPlayer;
    private List<BattleUnit> _currentEnemies = new();
    private SkillData _selectedSkill;
    private UiMode _targetBackMode = UiMode.Action;
    private bool _fleeSucceeded;
    private Coroutine _feedbackClearCoroutine;
    private Coroutine _feedbackUnlockCoroutine;
    private bool _isFeedbackBlockingInput;

    private enum UiMode { Hidden, Action, Skill, Item, Target, Ended }
    private UiMode _mode;

    private void Awake()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);

        attackButton?.onClick.AddListener(HandleAttackPressed);
        skillButton?.onClick.AddListener(HandleSkillPressed);
        itemButton?.onClick.AddListener(HandleItemPressed);
        fleeButton?.onClick.AddListener(HandleFleePressed);

        SetMode(UiMode.Hidden);
    }

    private void OnDisable()
    {
        if (_battleManager == null) return;

        _battleManager.OnTurnStarted -= HandleTurnStarted;
        _battleManager.OnDamageTaken -= HandleDamageTaken;
        _battleManager.OnBattleEnded -= HandleBattleEnded;
        _battleManager.OnBattleStateChanged -= HandleBattleStateChanged;
        _battleManager.OnFleeAttempted -= HandleFleeAttempted;
        _battleManager.OnCombatMessage -= HandleCombatMessage;
        _battleManager.OnRoundStarted -= HandleRoundStarted;
        _battleManager = null;

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

        _isFeedbackBlockingInput = false;
    }

    public void Initialize(BattleManager manager)
    {
        if (_battleManager == manager)
        {
            RefreshHud();
            return;
        }

        OnDisable();
        _battleManager = manager;

        _battleManager.OnTurnStarted += HandleTurnStarted;
        _battleManager.OnDamageTaken += HandleDamageTaken;
        _battleManager.OnBattleEnded += HandleBattleEnded;
        _battleManager.OnBattleStateChanged += HandleBattleStateChanged;
        _battleManager.OnFleeAttempted += HandleFleeAttempted;
        _battleManager.OnCombatMessage += HandleCombatMessage;
        _battleManager.OnRoundStarted += HandleRoundStarted;

        if (rootPanel != null)
            rootPanel.SetActive(true);

        _fleeSucceeded = false;
        SetMode(UiMode.Hidden);
        ClearFeedbackImmediate();
        RefreshHud();
    }

    public void ShowActionMenu(BattleUnit player, List<BattleUnit> enemies, Action<BattleUnit> onAttack,
        Action<SkillData, BattleUnit> onSkill, Action<ItemData> onItem, Action onFlee)
    {
        _currentPlayer = player;
        _currentEnemies = enemies ?? new List<BattleUnit>();
        _onAttack = onAttack;
        _onSkill = onSkill;
        _onItem = onItem;
        _onFlee = onFlee;
        _selectedSkill = null;

        RefreshHud();
        SetMode(UiMode.Action);

        BuildSkillButtons();
        BuildItemButtons();
        ApplyActionButtonInteractivity();
    }

    private void HandleAttackPressed()
    {
        if (_mode != UiMode.Action || _currentEnemies.Count == 0) return;

        _selectedSkill = null;
        _targetBackMode = UiMode.Action;
        BuildTargetButtons(_currentEnemies, HandleAttackTargetPicked, HandleTargetBackPressed);
        SetMode(UiMode.Target);
    }

    private void HandleSkillPressed()
    {
        if (_mode != UiMode.Action) return;

        BuildSkillButtons();
        SetMode(UiMode.Skill);
    }

    private void HandleItemPressed()
    {
        if (_mode != UiMode.Action) return;

        BuildItemButtons();
        SetMode(UiMode.Item);
    }

    private void HandleFleePressed()
    {
        if (_mode != UiMode.Action) return;

        LockInputUntilTurnEnds();
        _onFlee?.Invoke();
    }

    private void HandleAttackTargetPicked(BattleUnit target)
    {
        if (target == null || target.IsDead) return;

        LockInputUntilTurnEnds();
        _onAttack?.Invoke(target);
    }

    private void HandleSkillSelected(SkillData skill)
    {
        if (skill == null || _currentPlayer == null) return;
        if (_currentPlayer.CurrentSp < skill.spCost) return;

        _selectedSkill = skill;

        switch (skill.targetType)
        {
            case TargetType.Self:
                LockInputUntilTurnEnds();
                _onSkill?.Invoke(skill, _currentPlayer);
                return;

            case TargetType.AllEnemies:
            {
                var firstAlive = _currentEnemies.FirstOrDefault(e => !e.IsDead);
                if (firstAlive == null)
                    return;
                
                _onSkill?.Invoke(skill, firstAlive);
                return;
            }

            case TargetType.SingleEnemy:
            default:
                _targetBackMode = UiMode.Skill;
                BuildTargetButtons(_currentEnemies, HandleSkillTargetPicked, HandleTargetBackPressed);
                SetMode(UiMode.Target);
                return;
        }
    }

    private void HandleSkillTargetPicked(BattleUnit target)
    {
        if (target == null || _selectedSkill == null || target.IsDead)
            return;

        LockInputUntilTurnEnds();
        _onSkill?.Invoke(_selectedSkill, target);
    }

    private void HandleItemSelected(ItemData item)
    {
        if (item == null || PlayerStatsManager.Instance == null)
            return;

        if (!PlayerStatsManager.Instance.HasItem(item))
        {
            BuildItemButtons();
            return;
        }

        LockInputUntilTurnEnds();
        _onItem?.Invoke(item);
    }

    private void BuildSkillButtons()
    {
        ClearButtons(skillListRoot);
        if (skillListRoot == null || listButtonPrefab == null || _currentPlayer == null)
            return;

        var skills = _currentPlayer.Data.skills ?? Array.Empty<SkillData>();
        if (skills.Length == 0)
        {
            AddTextOnlyRow(skillListRoot, "No skills available");
            CreateButton(skillListRoot, "Back", () => SetMode(UiMode.Action));
            return;
        }

        foreach (var skill in skills)
        {
            if (skill == null)
                continue;

            var button = CreateButton(skillListRoot, $"{skill.skillName} ({skill.spCost} SP)", () => HandleSkillSelected(skill));
            if (button != null)
                button.interactable = _currentPlayer.CurrentSp >= skill.spCost;
        }

        CreateButton(skillListRoot, "Back", () => SetMode(UiMode.Action));
    }

    private void BuildItemButtons()
    {
        ClearButtons(itemListRoot);
        if (itemListRoot == null || listButtonPrefab == null)
            return;

        if (PlayerStatsManager.Instance == null)
        {
            AddTextOnlyRow(itemListRoot, "Player stats unavailable");
            CreateButton(itemListRoot, "Back", () => SetMode(UiMode.Action));
            return;
        }

        var inventory = PlayerStatsManager.Instance.inventory;
        if (inventory == null || inventory.Count == 0)
        {
            AddTextOnlyRow(itemListRoot, "No items available");
            CreateButton(itemListRoot, "Back", () => SetMode(UiMode.Action));
            return;
        }

        foreach (var stack in inventory)
        {
            if (stack == null || stack.item == null || stack.quantity <= 0)
                continue;

            CreateButton(itemListRoot, $"{stack.item.itemName} x{stack.quantity}", () => HandleItemSelected(stack.item));
        }

        CreateButton(itemListRoot, "Back", () => SetMode(UiMode.Action));
    }

    private void BuildTargetButtons(List<BattleUnit> targets, Action<BattleUnit> onPick, Action onBack)
    {
        ClearButtons(targetListRoot);
        if (targetListRoot == null || listButtonPrefab == null || targets == null)
            return;

        foreach (var target in targets.Where(target => target is { IsDead: false }))
        {
            CreateButton(targetListRoot,
                $"{target.Data.characterName} HP {target.CurrentHp}/{target.Data.maxHp}",
                () => onPick?.Invoke(target));
        }

        CreateButton(targetListRoot, "Back", () => onBack?.Invoke());
    }

    private void HandleTargetBackPressed()
    {
        ClearButtons(targetListRoot);
        SetMode(_targetBackMode);

        if (_targetBackMode == UiMode.Action)
        {
            BuildSkillButtons();
            BuildItemButtons();
        }
    }

    private Button CreateButton(Transform parent, string label, Action onClick)
    {
        var button = Instantiate(listButtonPrefab, parent);
        var text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
            text.text = label;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
        return button;
    }

    private void AddTextOnlyRow(Transform parent, string content)
    {
        if (parent == null || listButtonPrefab == null)
            return;

        var row = Instantiate(listButtonPrefab, parent);
        row.interactable = false;

        var text = row.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
            text.text = content;
    }

    private static void ClearButtons(Transform root)
    {
        if (root == null)
            return;

        for (var i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);
    }

    private void SetMode(UiMode mode)
    {
        _mode = mode;

        if (rootPanel != null)
            rootPanel.SetActive(mode != UiMode.Hidden);

        if (actionPanel != null) actionPanel.SetActive(mode == UiMode.Action);
        if (skillPanel != null) skillPanel.SetActive(mode == UiMode.Skill);
        if (itemPanel != null) itemPanel.SetActive(mode == UiMode.Item);
        if (targetPanel != null) targetPanel.SetActive(mode == UiMode.Target);
        if (battleEndPanel != null) battleEndPanel.SetActive(mode == UiMode.Ended);
    }

    private void ApplyActionButtonInteractivity()
    {
        var canInteract = !_isFeedbackBlockingInput;

        if (attackButton != null)
            attackButton.interactable = canInteract && _currentEnemies.Any(e => !e.IsDead);
        if (skillButton != null)
            skillButton.interactable = canInteract && _currentPlayer != null && (_currentPlayer.Data.skills?.Length ?? 0) > 0;
        if (itemButton != null)
            itemButton.interactable = canInteract && PlayerStatsManager.Instance != null;
        if (fleeButton != null)
            fleeButton.interactable = canInteract;
    }

    private void LockInputUntilTurnEnds()
    {
        SetMode(UiMode.Hidden);
        ClearButtons(skillListRoot);
        ClearButtons(itemListRoot);
        ClearButtons(targetListRoot);
    }

    private void HandleTurnStarted(BattleUnit unit)
    {
        if (unit == null)
            return;

        if (turnText != null)
            turnText.text = unit.IsPlayer ? "Your turn" : $"{unit.Data.characterName} turn";

        if (!unit.IsPlayer)
            LockInputUntilTurnEnds();

        RefreshHud();
    }

    private void HandleDamageTaken(BattleUnit _)
    {
        RefreshHud();
    }

    private void HandleBattleStateChanged()
    {
        RefreshHud();
    }

    private void HandleBattleEnded(bool playerWon)
    {
        if (_fleeSucceeded)
        {
            LockInputUntilTurnEnds();
            if (rootPanel != null)
                rootPanel.SetActive(false);
            SetFeedback("Escaped successfully.", postBattleFeedbackDelay, blockInput: false);
            _fleeSucceeded = false;
            return;
        }

        LockInputUntilTurnEnds();
        SetMode(UiMode.Ended);

        if (battleEndText != null)
            battleEndText.text = playerWon ? "Victory" : "Defeat / Fled";
        if (turnText != null)
            turnText.text = string.Empty;

        RefreshHud();
    }

    private void HandleRoundStarted(int roundNumber)
    {
        if (roundText != null)
            roundText.text = $"Round {Mathf.Max(1, roundNumber)}";
    }

    private void HandleCombatMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        SetFeedback(message, feedbackClearDelay, blockInput: true);

        if (_mode == UiMode.Action)
            ApplyActionButtonInteractivity();
    }

    private void HandleFleeAttempted(bool success)
    {
        _fleeSucceeded = success;

        var message = success
            ? "Escaped successfully."
            : "Could not escape! The enemy attacks.";

        SetFeedback(message, success ? postBattleFeedbackDelay : feedbackClearDelay, blockInput: !success);

        if (_mode == UiMode.Action)
            ApplyActionButtonInteractivity();
    }

    private void SetFeedback(string message, float clearDelay, bool blockInput)
    {
        if (feedbackText == null)
            return;

        if (!feedbackText.gameObject.activeSelf)
            feedbackText.gameObject.SetActive(true);

        feedbackText.text = message;

        _isFeedbackBlockingInput = blockInput;

        if (_feedbackClearCoroutine != null)
            StopCoroutine(_feedbackClearCoroutine);

        if (_feedbackUnlockCoroutine != null)
            StopCoroutine(_feedbackUnlockCoroutine);

        _feedbackClearCoroutine = StartCoroutine(ClearFeedbackAfterDelay(Mathf.Max(0f, clearDelay)));
        if (blockInput)
            _feedbackUnlockCoroutine = StartCoroutine(UnlockInputAfterDelay(Mathf.Max(0f, feedbackUiUnlockDelay)));
        else
            _isFeedbackBlockingInput = false;
    }

    private IEnumerator ClearFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearFeedbackImmediate();
        _feedbackClearCoroutine = null;
    }

    private IEnumerator UnlockInputAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _isFeedbackBlockingInput = false;
        _feedbackUnlockCoroutine = null;

        if (_mode == UiMode.Action) ApplyActionButtonInteractivity();
    }

    private void ClearFeedbackImmediate()
    {
        if (feedbackText != null)
            feedbackText.text = string.Empty;
    }

    private void RefreshHud()
    {

        var player = _battleManager.PlayerUnit;
        if (player != null)
        {
            if (playerNameText != null) playerNameText.text = player.Data.characterName;
            if (playerHpText != null) playerHpText.text = $"HP {player.CurrentHp}/{player.Data.maxHp}";
            if (playerSpText != null) playerSpText.text = $"SP {player.CurrentSp}/{player.Data.maxSp}";
        }

        var enemy = _battleManager.AllUnits.FirstOrDefault(u => !u.IsPlayer && !u.IsDead);
        if (enemy != null)
        {
            if (enemyNameText != null) enemyNameText.text = enemy.Data.characterName;
            if (enemyHpText != null) enemyHpText.text = $"HP {enemy.CurrentHp}/{enemy.Data.maxHp}";
        }
        else
        {
            if (enemyNameText != null) enemyNameText.text = "-";
            if (enemyHpText != null) enemyHpText.text = "HP 0/0";
        }
    }
}
