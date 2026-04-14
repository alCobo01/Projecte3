using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BattleManager : MonoBehaviour
{
    public enum BattleInitiator { Player, Enemy }

    public static BattleManager Instance { get; private set; }

    [SerializeField] private BattleUI ui;

    private readonly TurnStack _turnStack = new();
    private readonly List<BattleUnit> _allUnits = new();

    private Coroutine _battleLoop;
    private bool _endNotified;
    private int _round;

    public BattleUnit PlayerUnit { get; private set; }

    public IReadOnlyList<BattleUnit> AllUnits => _allUnits;
    public bool IsBattleRunning { get; private set; }

    public event Action<BattleUnit> OnTurnStarted;
    public event Action<BattleUnit> OnDamageTaken;
    public event Action<bool> OnBattleEnded;
    public event Action OnBattleStateChanged;
    public event Action<bool> OnFleeAttempted;
    public event Action<string> OnCombatMessage;
    public event Action<int> OnRoundStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartBattle(CharacterData playerData, CharacterData[] enemies, BattleInitiator initiator = BattleInitiator.Player)
    {
        if (IsBattleRunning) return;
        if (playerData == null || enemies == null || PlayerStatsManager.Instance == null) return;

        var enemyData = enemies.Where(e => e != null).ToArray();
        if (enemyData.Length == 0) return;

        IsBattleRunning = true;
        _endNotified = false;
        _round = 1;

        if (_battleLoop != null) StopCoroutine(_battleLoop);

        var savedHp = PlayerStatsManager.Instance.currentHp <= 0 ? playerData.maxHp : PlayerStatsManager.Instance.currentHp;
        var savedSp = Mathf.Clamp(PlayerStatsManager.Instance.currentSp, 0, playerData.maxSp);

        PlayerUnit = new BattleUnit(playerData, isPlayer: true, startingSp: savedSp);
        PlayerUnit.SetHp(Mathf.Clamp(savedHp, 1, playerData.maxHp));

        _allUnits.Clear();
        _allUnits.Add(PlayerUnit);
        _allUnits.AddRange(enemyData.Select(e => new BattleUnit(e, isPlayer: false, startingSp: 0)));

        var first = initiator == BattleInitiator.Enemy
            ? _allUnits.FirstOrDefault(u => !u.IsPlayer && !u.IsDead)
            : PlayerUnit;

        _turnStack.Build(_allUnits, first);
        ui.Initialize(this);

        OnRoundStarted?.Invoke(_round);
        OnBattleStateChanged?.Invoke();
        _battleLoop = StartCoroutine(BattleLoop());
    }

    private IEnumerator BattleLoop()
    {
        while (IsBattleRunning)
        {
            if (TryEndIfFinished())
                yield break;

            if (_turnStack.IsEmpty)
            {
                _round++;
                OnRoundStarted?.Invoke(_round);
                _turnStack.Rebuild(_allUnits.Where(u => !u.IsDead));
            }

            var current = _turnStack.PopCurrent();
            if (current == null)
                continue;

            var hpBeforeTick = current.CurrentHp;
            current.TickEffects();
            NotifyHpChange(current, hpBeforeTick);

            if (TryEndIfFinished()) yield break;
            if (current.IsDead) continue;

            if (current.SkipNextAction)
            {
                current.SkipNextAction = false;
                OnBattleStateChanged?.Invoke();
                continue;
            }

            OnTurnStarted?.Invoke(current);

            if (current.IsPlayer) yield return WaitForPlayerAction(current);
            else yield return ExecuteEnemyAction(current);

            if (TryEndIfFinished()) yield break;
        }

        ForceCleanupIfNeeded();
    }

    private IEnumerator WaitForPlayerAction(BattleUnit player)
    {
        var done = false;
        var enemies = _allUnits.Where(u => !u.IsPlayer && !u.IsDead).ToList();

        if (ui == null)
        {
            if (enemies.Count > 0)
            {
                var hp = enemies[0].CurrentHp;
                ActionResolver.ResolveAttack(player, enemies[0]);
                NotifyHpChange(enemies[0], hp);
                EmitHpDeltaMessage(player.Data.characterName, "Attack", enemies[0], hp);
                OnBattleStateChanged?.Invoke();
            }

            yield break;
        }

        ui.ShowActionMenu(
            player,
            enemies,
            onAttack: target =>
            {
                var hp = target.CurrentHp;
                ActionResolver.ResolveAttack(player, target);
                NotifyHpChange(target, hp);
                EmitHpDeltaMessage(player.Data.characterName, "Attack", target, hp);
                OnBattleStateChanged?.Invoke();
                done = true;
            },
            onSkill: (skill, target) =>
            {
                if (!player.SpendSp(skill.spCost)) return;
                var before = CaptureHp();
                ActionResolver.ResolveSkill(player, target, skill, enemies);
                NotifyHpChanges(before);
                EmitGroupHpDeltaMessages(player.Data.characterName, skill.skillName, before);
                OnBattleStateChanged?.Invoke();
                done = true;
            },
            onItem: item =>
            {
                if (!PlayerStatsManager.Instance.HasItem(item)) return;
                var before = CaptureHp();
                ActionResolver.ResolveItem(player, item);
                NotifyHpChanges(before);
                EmitGroupHpDeltaMessages(player.Data.characterName, item.itemName, before);
                OnBattleStateChanged?.Invoke();
                done = true;
            },
            onFlee: () => StartCoroutine(AttemptFlee(enemies, () => done = true))
        );

        yield return new WaitUntil(() => done || !IsBattleRunning);
    }

    private IEnumerator ExecuteEnemyAction(BattleUnit enemy)
    {
        yield return new WaitForSeconds(1f);

        var affordable = enemy.Data.skills.Where(s => s.spCost > 0 && enemy.CurrentSp >= s.spCost).ToList();
        var useSkill = affordable.Count > 0 && Random.value < 0.6f;

        if (useSkill)
        {
            var skill = affordable[Random.Range(0, affordable.Count)];
            enemy.SpendSp(skill.spCost);
            var before = CaptureHp();
            ActionResolver.ResolveSkill(enemy, PlayerUnit, skill, new List<BattleUnit> { PlayerUnit });
            NotifyHpChanges(before);
            EmitGroupHpDeltaMessages(enemy.Data.characterName, skill.skillName, before);
        }
        else
        {
            var hp = PlayerUnit.CurrentHp;
            ActionResolver.ResolveAttack(enemy, PlayerUnit);
            NotifyHpChange(PlayerUnit, hp);
            EmitHpDeltaMessage(enemy.Data.characterName, "Attack", PlayerUnit, hp);
        }

        OnBattleStateChanged?.Invoke();
    }

    private IEnumerator AttemptFlee(List<BattleUnit> enemies, Action onDone)
    {
        if (enemies == null || enemies.Count == 0)
        {
            onDone?.Invoke();
            yield break;
        }

        var avgSpeed = enemies.Average(e => (float)e.Data.speed);
        var chance = Mathf.Clamp01(PlayerUnit.Data.speed / (avgSpeed * 1.5f));

        if (Random.value < chance)
        {
            OnFleeAttempted?.Invoke(true);
            OnCombatMessage?.Invoke("Escaped successfully.");
            EndBattle(playerWon: false);
        }
        else
        {
            OnFleeAttempted?.Invoke(false);
            var punisher = enemies.OrderByDescending(e => e.Data.speed).First();
            yield return new WaitForSeconds(0.8f);
            var hp = PlayerUnit.CurrentHp;
            ActionResolver.ResolveAttack(punisher, PlayerUnit);
            NotifyHpChange(PlayerUnit, hp);
            EmitHpDeltaMessage(punisher.Data.characterName, "Punish", PlayerUnit, hp);
            OnBattleStateChanged?.Invoke();
        }

        onDone?.Invoke();
    }

    private bool TryEndIfFinished()
    {
        if (!IsBattleRunning)
            return true;
        if (PlayerUnit == null)
            return false;

        if (PlayerUnit.IsDead)
        {
            EndBattle(playerWon: false);
            return true;
        }

        var enemiesAlive = _allUnits.Any(u => !u.IsPlayer && !u.IsDead);
        if (enemiesAlive)
            return false;

        EndBattle(playerWon: true);
        return true;
    }

    private void EndBattle(bool playerWon)
    {
        if (_endNotified) return;

        _endNotified = true;
        IsBattleRunning = false;

        if (PlayerUnit != null && PlayerStatsManager.Instance != null)
        {
            PlayerStatsManager.Instance.currentHp = PlayerUnit.CurrentHp;
            PlayerStatsManager.Instance.currentSp = PlayerUnit.CurrentSp;
        }

        OnBattleStateChanged?.Invoke();
        OnBattleEnded?.Invoke(playerWon);

        if (_battleLoop != null) StopCoroutine(_battleLoop);
        _battleLoop = null;
    }

    private void ForceCleanupIfNeeded()
    {
        if (!_endNotified) EndBattle(playerWon: false);
    }

    private Dictionary<BattleUnit, int> CaptureHp() => _allUnits.ToDictionary(u => u, u => u.CurrentHp);

    private void NotifyHpChanges(Dictionary<BattleUnit, int> before)
    {
        foreach (var unit in _allUnits)
        {
            if (before.TryGetValue(unit, out var hp)) NotifyHpChange(unit, hp);
        }
    }

    private void NotifyHpChange(BattleUnit unit, int hpBefore)
    {
        if (unit != null && hpBefore != unit.CurrentHp) OnDamageTaken?.Invoke(unit);
    }

    private void EmitGroupHpDeltaMessages(string actorName, string actionName, Dictionary<BattleUnit, int> before)
    {
        foreach (var unit in _allUnits)
        {
            if (before.TryGetValue(unit, out var hp))
                EmitHpDeltaMessage(actorName, actionName, unit, hp);
        }
    }

    private void EmitHpDeltaMessage(string actorName, string actionName, BattleUnit target, int hpBefore)
    {
        var delta = hpBefore - target.CurrentHp;
        switch (delta)
        {
            case > 0:
                OnCombatMessage?.Invoke($"{actorName} dealt {delta} damage to {target.Data.characterName} ({actionName}).");
                break;
            case < 0:
                OnCombatMessage?.Invoke($"{actorName} healed {-delta} HP on {target.Data.characterName} ({actionName}).");
                break;
        }
    }
}
