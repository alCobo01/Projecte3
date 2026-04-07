using System;
using System.Collections.Generic;
using System.Linq;
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

    [Header("Action Labels")]
    [SerializeField] private Text attackButtonLabel;
    [SerializeField] private Text skillButtonLabel;
    [SerializeField] private Text itemButtonLabel;
    [SerializeField] private Text fleeButtonLabel;

    [Header("Combatant HUD")]
    [SerializeField] private Text playerNameText;
    [SerializeField] private Text playerHpText;
    [SerializeField] private Text playerSpText;
    [SerializeField] private Text enemyNameText;
    [SerializeField] private Text enemyHpText;
    [SerializeField] private Text turnText;
    [SerializeField] private Text battleEndText;

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

    private enum UiMode { Hidden, Action, Skill, Item, TargetForAttack, TargetForSkill, Ended }
    private UiMode _mode = UiMode.Hidden;
    private SkillData _selectedSkill;

    private void Awake()
    {
        WireStaticButtons();
        ApplyStaticLabels();
        SetMode(UiMode.Hidden);
    }

    private void OnDisable() => Unsubscribe();
    public void Initialize(BattleManager manager)
    {
        if (_battleManager == manager)
        {
            RefreshHud();
            return;
        }

        Unsubscribe();
        _battleManager = manager;

        _battleManager.OnTurnStarted += HandleTurnStarted;
        _battleManager.OnDamageTaken += HandleDamageTaken;
        _battleManager.OnBattleEnded += HandleBattleEnded;
        _battleManager.OnBattleStateChanged += HandleBattleStateChanged;

        if (rootPanel != null) rootPanel.SetActive(true);

        SetMode(UiMode.Hidden);
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

        attackButton.interactable = _currentEnemies.Any(e => !e.IsDead);
        skillButton.interactable = _currentPlayer != null && (_currentPlayer.Data.skills?.Length ?? 0) > 0;
        itemButton.interactable = PlayerStatsManager.Instance;

        BuildSkillButtons();
        BuildItemButtons();
    }

    private void WireStaticButtons()
    {
        BindButton(attackButton, HandleAttackPressed);
        BindButton(skillButton, HandleSkillPressed);
        BindButton(itemButton, HandleItemPressed);
        BindButton(fleeButton, HandleFleePressed);
    }

    private void ApplyStaticLabels()
    {
        SetText(attackButtonLabel, "Attack");
        SetText(skillButtonLabel, "Skills");
        SetText(itemButtonLabel, "Items");
        SetText(fleeButtonLabel, "Flee");
    }

    private void HandleAttackPressed()
    {
        if (_mode != UiMode.Action || _currentEnemies.Count == 0) return;
        BuildTargetButtons(_currentEnemies, HandleAttackTargetPicked);
        SetMode(UiMode.TargetForAttack);
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
        if (_currentPlayer.CurrentSp < skill.spCost) return;
        _selectedSkill = skill;

        switch (skill.targetType)
        {
            case TargetType.Self:
                LockInputUntilTurnEnds();
                _onSkill?.Invoke(skill, _currentPlayer);
                return;

            case TargetType.SingleEnemy:
                BuildTargetButtons(_currentEnemies, HandleSkillTargetPicked);
                SetMode(UiMode.TargetForSkill);
                return;

            case TargetType.AllEnemies:
            {
                var firstAlive = _currentEnemies.FirstOrDefault(e => !e.IsDead);
                if (firstAlive == null) return;
                LockInputUntilTurnEnds();
                _onSkill?.Invoke(skill, firstAlive);
                return;
            }

            default:
                BuildTargetButtons(_currentEnemies, HandleSkillTargetPicked);
                SetMode(UiMode.TargetForSkill);
                return;
        }
    }

    private void HandleSkillTargetPicked(BattleUnit target)
    {
        if (target.IsDead || !_selectedSkill) return;
        LockInputUntilTurnEnds();
        _onSkill?.Invoke(_selectedSkill, target);
    }

    private void HandleItemSelected(ItemData item)
    {
        if (!item) return;
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
        if (!skillListRoot || !listButtonPrefab || _currentPlayer == null) return;

        var skills = _currentPlayer.Data.skills ?? Array.Empty<SkillData>();
        if (skills.Length == 0)
        {
            AddTextOnlyRow(skillListRoot, "No skills available");
            CreateButton(skillListRoot, "Back", () => SetMode(UiMode.Action));
            return;
        }

        foreach (var skill in skills)
        {
            if (!skill) continue;

            var label = $"{skill.skillName} ({skill.spCost} SP)";
            var canUse = _currentPlayer.CurrentSp >= skill.spCost;

            var button = CreateButton(skillListRoot, label, () => HandleSkillSelected(skill));
            button.interactable = canUse;
        }

        CreateButton(skillListRoot, "Back", () => SetMode(UiMode.Action));
    }

    private void BuildItemButtons()
    {
        ClearButtons(itemListRoot);
        if (itemListRoot || listButtonPrefab)
            return;
        
        var inventory = PlayerStatsManager.Instance.inventory;
        if (inventory == null || inventory.Count == 0)
        {
            AddTextOnlyRow(itemListRoot, "No items available");
            CreateButton(itemListRoot, "Back", () => SetMode(UiMode.Action));
            return;
        }

        foreach (var stack in inventory)
        {
            if (stack == null || stack.item || stack.quantity <= 0) continue;
            var label = $"{stack.item.itemName} x{stack.quantity}";
            CreateButton(itemListRoot, label, () => HandleItemSelected(stack.item));
        }

        CreateButton(itemListRoot, "Back", () => SetMode(UiMode.Action));
    }

    private void BuildTargetButtons(List<BattleUnit> targets, Action<BattleUnit> onPick)
    {
        ClearButtons(targetListRoot);

        foreach (var target in targets)
        {
            if (target == null || target.IsDead) continue;

            var label = $"{target.Data.characterName} HP {target.CurrentHp}/{target.Data.maxHp}";
            CreateButton(targetListRoot, label, () => onPick(target));
        }

        CreateButton(targetListRoot, "Back", () => SetMode(UiMode.Action));
    }

    private Button CreateButton(Transform parent, string label, Action onClick)
    {
        if (!parent || !listButtonPrefab) return null;

        var button = Instantiate(listButtonPrefab, parent);
        var text = button.GetComponentInChildren<Text>(true);
        text.text = label;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
        return button;
    }

    private void AddTextOnlyRow(Transform parent, string content)
    {
        if (parent || listButtonPrefab) return;
        var row = Instantiate(listButtonPrefab, parent);
        row.interactable = false;
        var text = row.GetComponentInChildren<Text>(true);
        text.text = content;
    }

    private static void ClearButtons(Transform root)
        { for (var i = root.childCount - 1; i >= 0; i--) Destroy(root.GetChild(i).gameObject); }
    
    private void SetMode(UiMode mode)
    {
        _mode = mode;

        SetPanel(actionPanel, mode == UiMode.Action);
        SetPanel(skillPanel, mode == UiMode.Skill);
        SetPanel(itemPanel, mode == UiMode.Item);
        SetPanel(targetPanel, mode == UiMode.TargetForAttack || mode == UiMode.TargetForSkill);
        SetPanel(battleEndPanel, mode == UiMode.Ended);
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
        if (unit.IsPlayer) SetText(turnText, "Your turn");
        else
        {
            SetText(turnText, $"{unit.Data.characterName} turn");
            LockInputUntilTurnEnds();
        }
        RefreshHud();
    }

    private void HandleDamageTaken(BattleUnit _) => RefreshHud();
    private void HandleBattleStateChanged() => RefreshHud();
    
    private void HandleBattleEnded(bool playerWon)
    {
        LockInputUntilTurnEnds();
        SetMode(UiMode.Ended);
        SetText(battleEndText, playerWon ? "Victory" : "Defeat / Fled");
        SetText(turnText, string.Empty);
        RefreshHud();
    }

    private void RefreshHud()
    {
        var player = _battleManager.PlayerUnit;
        SetText(playerNameText, player.Data.characterName);
        SetText(playerHpText, $"HP {player.CurrentHp}/{player.Data.maxHp}");
        SetText(playerSpText, $"SP {player.CurrentSp}/{player.Data.maxSp}");

        var enemy = _battleManager.AllUnits.FirstOrDefault(u => !u.IsPlayer && !u.IsDead);
        if (enemy != null)
        {
            SetText(enemyNameText, enemy.Data.characterName);
            SetText(enemyHpText, $"HP {enemy.CurrentHp}/{enemy.Data.maxHp}");
        }
        else
        {
            SetText(enemyNameText, "-");
            SetText(enemyHpText, "HP 0/0");
        }
    }

    private void SetPanel(GameObject panel, bool active) => panel.SetActive(active);
    private void SetText(Text text, string content) => text.text = content;

    private void BindButton(Button button, Action action)
    {
        if (button == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => action?.Invoke());
    }

    private void Unsubscribe()
    {
        _battleManager.OnTurnStarted -= HandleTurnStarted;
        _battleManager.OnDamageTaken -= HandleDamageTaken;
        _battleManager.OnBattleEnded -= HandleBattleEnded;
        _battleManager.OnBattleStateChanged -= HandleBattleStateChanged;
        _battleManager = null;
    }
}
