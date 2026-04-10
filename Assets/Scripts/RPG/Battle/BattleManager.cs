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
    private BattleUnit _playerUnit;
    private Coroutine _battleLoopCoroutine;
    private bool _battleEnded;
    private bool _battleStartingOrRunning;
    private bool _battleEndNotified;

    public event Action<BattleUnit> OnTurnStarted;
    public event Action<BattleUnit> OnDamageTaken;
    public event Action<bool> OnBattleEnded;
    public event Action OnBattleStateChanged;
    public event Action<bool> OnFleeAttempted;
    public event Action<string> OnCombatMessage;
    public event Action<int> OnRoundStarted;

    private int _roundNumber;

    public BattleUnit PlayerUnit => _playerUnit;
    public IReadOnlyList<BattleUnit> AllUnits => _allUnits;
    public bool IsBattleRunning => _battleStartingOrRunning;

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
        if (_battleStartingOrRunning)
        {
            Debug.Log("BattleManager: battle already in progress, new start request ignored.", this);
            return;
        }

        if (playerData == null || enemies == null)
        {
            Debug.LogError("BattleManager.StartBattle received null playerData or enemies.", this);
            return;
        }

        if (PlayerStatsManager.Instance == null)
        {
            Debug.LogError("BattleManager.StartBattle requires PlayerStatsManager.Instance in scene.", this);
            return;
        }

        _battleStartingOrRunning = true;
        _battleEndNotified = false;

        if (_battleLoopCoroutine != null)
            StopCoroutine(_battleLoopCoroutine);

        _battleEnded = false;

        var persistedSp = Mathf.Clamp(PlayerStatsManager.Instance.currentSp, 0, playerData.maxSp);
        var persistedHp = PlayerStatsManager.Instance.currentHp;
        if (persistedHp <= 0)
            persistedHp = playerData.maxHp;

        _playerUnit = new BattleUnit(playerData, isPlayer: true, startingSp: persistedSp);
        _playerUnit.SetHp(Mathf.Clamp(persistedHp, 1, playerData.maxHp));

        _allUnits.Clear();
        _allUnits.Add(_playerUnit);
        foreach (var e in enemies)
        {
            if (e == null) continue;
            _allUnits.Add(new BattleUnit(e, isPlayer: false, startingSp: 0));
        }

        if (_allUnits.Count <= 1)
        {
            Debug.LogError("BattleManager.StartBattle needs at least one valid enemy CharacterData.", this);
            _battleStartingOrRunning = false;
            return;
        }

        _roundNumber = 1;
        var firstTurnUnit = GetFirstTurnUnit(initiator);
        _turnStack.Build(_allUnits, firstTurnUnit);
        if (ui != null)
            ui.Initialize(this);
        else
            Debug.LogWarning("BattleManager UI reference is null. Battle will run without menu UI.", this);

        OnRoundStarted?.Invoke(_roundNumber);
        OnBattleStateChanged?.Invoke();
        _battleLoopCoroutine = StartCoroutine(BattleLoop());
    }

    private IEnumerator BattleLoop()
    {
        while (!_battleEnded)
        {
            if (CheckBattleEnd(out var earlyWinner))
            {
                EndBattle(earlyWinner);
                yield break;
            }

            if (_turnStack.IsEmpty)
            {
                _roundNumber++;
                OnRoundStarted?.Invoke(_roundNumber);
                _turnStack.Rebuild(_allUnits.Where(u => !u.IsDead));
            }

            var current = _turnStack.PopCurrent();
            if (current == null)
                continue;

            var hpBeforeTick = current.CurrentHp;
            current.TickEffects();
            NotifyHpChange(current, hpBeforeTick);

            if (CheckBattleEnd(out var winnerAfterTick))
            {
                EndBattle(winnerAfterTick);
                yield break;
            }

            if (current.IsDead) continue;

            if (current.SkipNextAction)
            {
                current.SkipNextAction = false;
                OnBattleStateChanged?.Invoke();
                continue;
            }

            OnTurnStarted?.Invoke(current);

            if (current.IsPlayer)
                yield return WaitForPlayerAction(current);
            else
                yield return ExecuteEnemyAction(current);

            if (_battleEnded)
                yield break;

            if (!CheckBattleEnd(out var playerWon)) continue;
            EndBattle(playerWon);
            yield break;
        }

        if (!_battleEndNotified)
        {
            if (CheckBattleEnd(out var winner))
                EndBattle(winner);
            else
            {
                Debug.LogWarning("BattleLoop exited without terminal state. Forcing cleanup.", this);
                _battleEnded = true;
                _battleStartingOrRunning = false;
            }
        }

        _battleLoopCoroutine = null;
    }

    private IEnumerator WaitForPlayerAction(BattleUnit player)
    {
        var done = false;
        var enemies = _allUnits.Where(u => !u.IsPlayer && !u.IsDead).ToList();

        if (ui == null)
        {
            if (enemies.Count > 0)
            {
                var hpBefore = enemies[0].CurrentHp;
                ActionResolver.ResolveAttack(player, enemies[0]);
                NotifyHpChange(enemies[0], hpBefore);
                EmitHpDeltaMessage(player.Data.characterName, "Attack", enemies[0], hpBefore);
                OnBattleStateChanged?.Invoke();
            }

            yield return null;
            yield break;
        }

        ui.ShowActionMenu(
            player,
            enemies,
            onAttack: (target) =>
            {
                var hpBefore = target.CurrentHp;
                ActionResolver.ResolveAttack(player, target);
                NotifyHpChange(target, hpBefore);
                EmitHpDeltaMessage(player.Data.characterName, "Attack", target, hpBefore);
                OnBattleStateChanged?.Invoke();
                TryEndBattleFromAction();
                done = true;
            },
            onSkill: (skill, target) =>
            {
                if (!player.SpendSp(skill.spCost)) return;
                var hpBefore = CaptureHp();
                ActionResolver.ResolveSkill(player, target, skill, enemies);
                NotifyHpChanges(hpBefore);
                EmitGroupHpDeltaMessages(player.Data.characterName, skill.skillName, hpBefore);
                OnBattleStateChanged?.Invoke();
                TryEndBattleFromAction();
                done = true;
            },
            onItem: (item) =>
            {
                if (!PlayerStatsManager.Instance.HasItem(item)) return;
                var hpBefore = CaptureHp();
                ActionResolver.ResolveItem(player, item);
                NotifyHpChanges(hpBefore);
                EmitGroupHpDeltaMessages(player.Data.characterName, item.itemName, hpBefore);
                OnBattleStateChanged?.Invoke();
                TryEndBattleFromAction();
                done = true;
            },
            onFlee: () => StartCoroutine(AttemptFlee(enemies, () => done = true))
        );

        yield return new WaitUntil(() => done);
    }

    private IEnumerator ExecuteEnemyAction(BattleUnit enemy)
    {
        yield return new WaitForSeconds(1f);

        var affordableSkills = enemy.Data.skills
            .Where(s => s.spCost > 0 && enemy.CurrentSp >= s.spCost)
            .ToList();

        var useSkill = affordableSkills.Count > 0 && Random.value < 0.6f;

        if (useSkill)
        {
            var skill = affordableSkills[Random.Range(0, affordableSkills.Count)];
            enemy.SpendSp(skill.spCost);
            var hpBefore = CaptureHp();
            ActionResolver.ResolveSkill(enemy, _playerUnit, skill,
                                        new List<BattleUnit> { _playerUnit });
            NotifyHpChanges(hpBefore);
            EmitGroupHpDeltaMessages(enemy.Data.characterName, skill.skillName, hpBefore);
        }
        else
        {
            var hpBefore = _playerUnit.CurrentHp;
            ActionResolver.ResolveAttack(enemy, _playerUnit);
            NotifyHpChange(_playerUnit, hpBefore);
            EmitHpDeltaMessage(enemy.Data.characterName, "Attack", _playerUnit, hpBefore);
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

        var avgEnemySpeed = enemies.Average(e => (float)e.Data.speed);
        var fleeChance = Mathf.Clamp01(_playerUnit.Data.speed / (avgEnemySpeed * 1.5f));

        if (Random.value < fleeChance)
        {
            OnFleeAttempted?.Invoke(true);
            _battleEnded = true;
            EndBattle(false);
        }
        else
        {
            OnFleeAttempted?.Invoke(false);
            var punisher = enemies.OrderByDescending(e => e.Data.speed).First();
            yield return new WaitForSeconds(0.8f);
            var hpBefore = _playerUnit.CurrentHp;
            ActionResolver.ResolveAttack(punisher, _playerUnit);
            NotifyHpChange(_playerUnit, hpBefore);
            EmitHpDeltaMessage(punisher.Data.characterName, "Punish", _playerUnit, hpBefore);
            OnBattleStateChanged?.Invoke();
        }

        onDone?.Invoke();
    }

    private bool CheckBattleEnd(out bool playerWon)
    {
        playerWon = false;
        if (_playerUnit.IsDead) return true;
        if (!_allUnits.Where(u => !u.IsPlayer).All(u => u.IsDead)) return false;
        playerWon = true;
        return true;
    }

    private void PersistPlayerState()
    {
        PlayerStatsManager.Instance.currentHp = _playerUnit.CurrentHp;
        PlayerStatsManager.Instance.currentSp = _playerUnit.CurrentSp;
    }

    private Dictionary<BattleUnit, int> CaptureHp()
        => _allUnits.ToDictionary(u => u, u => u.CurrentHp);

    private void NotifyHpChanges(Dictionary<BattleUnit, int> beforeHp)
    {
        if (beforeHp == null) return;

        foreach (var unit in _allUnits)
        {
            if (!beforeHp.TryGetValue(unit, out var previousHp)) continue;
            NotifyHpChange(unit, previousHp);
        }
    }

    private void NotifyHpChange(BattleUnit unit, int hpBefore)
    {
        if (unit == null || hpBefore == unit.CurrentHp) return;
        OnDamageTaken?.Invoke(unit);
    }

    private void EndBattle(bool playerWon)
    {
        if (_battleEndNotified)
            return;

        _battleEnded = true;
        _battleEndNotified = true;
        PersistPlayerState();
        OnBattleStateChanged?.Invoke();
        OnBattleEnded?.Invoke(playerWon);
        if (_battleLoopCoroutine != null)
            StopCoroutine(_battleLoopCoroutine);
        _battleLoopCoroutine = null;
        _battleStartingOrRunning = false;
    }

    private BattleUnit GetFirstTurnUnit(BattleInitiator initiator)
    {
        return initiator switch
        {
            BattleInitiator.Enemy => _allUnits.FirstOrDefault(u => !u.IsPlayer && !u.IsDead),
            BattleInitiator.Player => _playerUnit
        };
    }

    private void EmitGroupHpDeltaMessages(string actorName, string actionName, Dictionary<BattleUnit, int> beforeHp)
    {
        foreach (var unit in _allUnits)
        {
            if (!beforeHp.TryGetValue(unit, out var hpBefore))
                continue;

            EmitHpDeltaMessage(actorName, actionName, unit, hpBefore);
        }
    }

    private void EmitHpDeltaMessage(string actorName, string actionName, BattleUnit target, int hpBefore)
    {
        if (target == null)
            return;

        var delta = hpBefore - target.CurrentHp;
        if (delta > 0)
            OnCombatMessage?.Invoke($"{actorName} has dealt {delta} damage to {target.Data.characterName} ({actionName}).");
        else if (delta < 0)
            OnCombatMessage?.Invoke($"{actorName} has healed {-delta} HP on {target.Data.characterName} ({actionName}).");
    }

    private void TryEndBattleFromAction()
    {
        if (!CheckBattleEnd(out var playerWon))
            return;

        EndBattle(playerWon);
    }
}
