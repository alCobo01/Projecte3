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
    [SerializeField] private Button battleEndButton;

    [Header("HUD")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerHpText;
    [SerializeField] private TMP_Text playerSpText;
    [SerializeField] private Transform enemyListRoot;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text battleEndText;
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

    private BattleManager _manager;

    private Action<BattleUnit> _onAttack;
    private Action<SkillData, BattleUnit> _onSkill;
    private Action<ItemData> _onItem;
    private Action _onFlee;

    private BattleUnit _player;
    private List<BattleUnit> _enemies = new();
    private SkillData _selectedSkill;

    private Mode _mode;
    private Mode _backFromTargetMode = Mode.Action;
    private bool _inputLockedByFeedback;
    private bool _fleeSuccess;
    private Coroutine _feedbackClearCoroutine;
    private Coroutine _feedbackUnlockCoroutine;

    private enum Mode { Hidden, Action, Skill, Item, Target, Ended }

    private void Awake()
    {
        attackButton?.onClick.AddListener(ShowAttackTargets);
        skillButton?.onClick.AddListener(ShowSkills);
        itemButton?.onClick.AddListener(ShowItems);
        fleeButton?.onClick.AddListener(RequestFlee);
        battleEndButton?.onClick.AddListener(CloseBattleUi);
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
        ClearFeedback();
        SetMode(Mode.Hidden);
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

    private void ShowSkills()
    {
        if (_mode != Mode.Action) return;
        BuildSkills();
        SetMode(Mode.Skill);
    }

    private void ShowItems()
    {
        if (_mode != Mode.Action) return;
        BuildItems();
        SetMode(Mode.Item);
    }

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
        if (!item || !PlayerStatsManager.Instance) return;
        if (!PlayerStatsManager.Instance.HasItem(item))
        {
            BuildItems();
            return;
        }

        HideInputPanels();
        _onItem?.Invoke(item);
    }

    private void BackFromTarget()
    {
        ClearButtons(targetListRoot);
        SetMode(_backFromTargetMode);
        if (_backFromTargetMode != Mode.Action) return;
        BuildSkills();
        BuildItems();
    }

    private void BuildSkills()
    {
        ClearButtons(skillListRoot);
        if (!skillListRoot || !listButtonPrefab || _player == null) return;

        var skills = _player.Data.skills ?? Array.Empty<SkillData>();
        if (skills.Length == 0)
        {
            CreateDisabledRow(skillListRoot, "No skills available");
            CreateButton(skillListRoot, "Back", () => SetMode(Mode.Action));
            return;
        }

        foreach (var s in skills.Where(s => s is not null))
        {
            var b = CreateButton(skillListRoot, $"{s.skillName} ({s.spCost} SP)", () => PickSkill(s));
            b.interactable = _player.CurrentSp >= s.spCost;
        }

        CreateButton(skillListRoot, "Back", () => SetMode(Mode.Action));
    }

    private void BuildItems()
    {
        ClearButtons(itemListRoot);
        if (itemListRoot|| listButtonPrefab)
            return;
        
        var inv = PlayerStatsManager.Instance.inventory;
        if (inv == null || inv.Count == 0)
        {
            CreateDisabledRow(itemListRoot, "No items available");
            CreateButton(itemListRoot, "Back", () => SetMode(Mode.Action));
            return;
        }

        foreach (var stack in inv.Where(x => x != null && x.item != null && x.quantity > 0))
            CreateButton(itemListRoot, $"{stack.item.itemName} x{stack.quantity}", () => PickItem(stack.item));

        CreateButton(itemListRoot, "Back", () => SetMode(Mode.Action));
    }

    private void BuildTargets(List<BattleUnit> targets, Action<BattleUnit> onPick)
    {
        ClearButtons(targetListRoot);
        if (targetListRoot == null || listButtonPrefab == null || targets == null)
            return;

        foreach (var t in targets.Where(t => t is { IsDead: false }))
            CreateButton(targetListRoot, $"{t.Data.characterName} HP {t.CurrentHp}/{t.Data.maxHp}", () => onPick?.Invoke(t));

        CreateButton(targetListRoot, "Back", BackFromTarget);
    }

    private Button CreateButton(Transform parent, string label, Action onClick)
    {
        var button = Instantiate(listButtonPrefab, parent);
        var text = button.GetComponentInChildren<TMP_Text>(true);
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
        for (var i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);
    }

    private void HandleTurnStarted(BattleUnit unit)
    {
        turnText.text = unit.IsPlayer ? "Your turn" : $"{unit.Data.characterName} turn";
        if (!unit.IsPlayer) HideInputPanels();
        RefreshHud();
    }

    private void HandleBattleEnded(bool playerWon)
    {
        HideInputPanels();
        SetMode(Mode.Ended);
        battleEndText.text = playerWon ? "Victory" : (_fleeSuccess ? "Fled" : "Defeat");
        turnText.text = string.Empty;

        ClearFeedback();
        StopFeedbackCoroutines();
        _inputLockedByFeedback = false;
        _fleeSuccess = false;

    }

    private void HandleDamageTaken(BattleUnit _) => RefreshHud();

    private void HandleRoundStarted(int round) => roundText.text = $"Round {Mathf.Max(1, round)}";

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
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;

        _inputLockedByFeedback = blockInput;
        ApplyActionInteractivity();

        StopFeedbackCoroutines();
        _feedbackClearCoroutine = StartCoroutine(ClearFeedbackAfter(Mathf.Max(0f, clearDelay)));
        if (blockInput)
            _feedbackUnlockCoroutine = StartCoroutine(UnlockInputAfter(Mathf.Max(0f, feedbackUiUnlockDelay)));
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

        if (_feedbackUnlockCoroutine == null) return;
        StopCoroutine(_feedbackUnlockCoroutine);
        _feedbackUnlockCoroutine = null;
    }

    private void CloseBattleUi() => SetMode(Mode.Hidden);

    private void ClearFeedback() => feedbackText.text = string.Empty;

    private void ApplyActionInteractivity()
    {
        var canUse = !_inputLockedByFeedback;
        attackButton.interactable = canUse && _enemies.Any(e => !e.IsDead);
        skillButton.interactable = canUse && _player != null && (_player.Data.skills?.Length ?? 0) > 0;
        itemButton.interactable = canUse && PlayerStatsManager.Instance != null;
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
        battleEndPanel.SetActive(mode == Mode.Ended);
    }

    private void HideInputPanels()
    {
        SetMode(Mode.Hidden);
        ClearButtons(skillListRoot);
        ClearButtons(itemListRoot);
        ClearButtons(targetListRoot);
    }

    private void RefreshHud()
    {
        var player = _manager.PlayerUnit;
        playerNameText.text = player.Data.characterName;
        playerHpText.text = $"HP {player.CurrentHp}/{player.Data.maxHp}";
        playerSpText.text = $"SP {player.CurrentSp}/{player.Data.maxSp}";
        
        ClearButtons(enemyListRoot);
        var enemies = _manager.AllUnits.Where(u => !u.IsPlayer).ToList();
        foreach (var enemy in enemies)
        {
            var row = CreateEnemyRow(enemyListRoot);
            row.text = enemy.IsDead
                ? $"{enemy.Data.characterName} - DEAD"
                : $"{enemy.Data.characterName} - HP {enemy.CurrentHp}/{enemy.Data.maxHp}";
        }
        
    }

    private TMP_Text CreateEnemyRow(Transform parent)
    {
        var go = new GameObject("EnemyRow", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var row = go.AddComponent<TextMeshProUGUI>();
        row.font = playerNameText.font;
        row.fontSharedMaterial = playerNameText.fontSharedMaterial;
        row.fontSize = playerNameText.fontSize;
        row.color = playerNameText.color;
        row.alignment = playerNameText.alignment;
        
        return row;
    }

    private void Unsubscribe()
    {
        if (_manager == null) return;

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
