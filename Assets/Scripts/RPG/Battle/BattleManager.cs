using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [SerializeField] private BattleUI _ui;

    private TurnStack _turnStack = new();
    private List<BattleUnit> _allUnits = new();
    private BattleUnit _playerUnit;
    private Coroutine _battleLoopCoroutine;
    private bool _battleEnded;

    public event Action<BattleUnit> OnTurnStarted;
    public event Action<BattleUnit> OnDamageTaken;
    public event Action<bool> OnBattleEnded;
    public event Action OnBattleStateChanged;

    public BattleUnit PlayerUnit => _playerUnit;
    public IReadOnlyList<BattleUnit> AllUnits => _allUnits;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartBattle(CharacterData playerData, CharacterData[] enemies)
    {
        if (_battleLoopCoroutine != null)
            StopCoroutine(_battleLoopCoroutine);

        _battleEnded = false;

        _playerUnit = new BattleUnit(playerData, isPlayer: true,
                                     startingSp: PlayerStats.Instance.currentSp);
        _playerUnit.SetHp(PlayerStats.Instance.currentHp);

        _allUnits.Clear();
        _allUnits.Add(_playerUnit);
        foreach (var e in enemies)
            _allUnits.Add(new BattleUnit(e, isPlayer: false, startingSp: 0));

        _turnStack.Build(_allUnits);
        if (_ui != null)
            _ui.Initialize(this);

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
                _turnStack.Rebuild(_allUnits.Where(u => !u.IsDead));

            var current = _turnStack.PopCurrent();
            if (current == null) break;

            var hpBeforeTick = current.CurrentHp;
            current.TickEffects();
            NotifyHpChange(current, hpBeforeTick);

            if (CheckBattleEnd(out var winnerAfterTick))
            {
                EndBattle(winnerAfterTick);
                yield break;
            }

            if (current.IsDead)
                continue;

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

            if (!CheckBattleEnd(out var playerWon)) continue;
            EndBattle(playerWon);
            yield break;
        }

        _battleLoopCoroutine = null;
    }

    private IEnumerator WaitForPlayerAction(BattleUnit player)
    {
        var done = false;
        var enemies = _allUnits.Where(u => !u.IsPlayer && !u.IsDead).ToList();

        _ui.ShowActionMenu(
            player,
            enemies,
            onAttack: (target) =>
            {
                var hpBefore = target.CurrentHp;
                ActionResolver.ResolveAttack(player, target);
                NotifyHpChange(target, hpBefore);
                OnBattleStateChanged?.Invoke();
                done = true;
            },
            onSkill: (skill, target) =>
            {
                if (!player.SpendSp(skill.spCost)) return;
                var hpBefore = CaptureHp();
                ActionResolver.ResolveSkill(player, target, skill, enemies);
                NotifyHpChanges(hpBefore);
                OnBattleStateChanged?.Invoke();
                done = true;
            },
            onItem: (item) =>
            {
                if (!PlayerStats.Instance.HasItem(item)) return;
                var hpBefore = CaptureHp();
                ActionResolver.ResolveItem(player, item);
                NotifyHpChanges(hpBefore);
                OnBattleStateChanged?.Invoke();
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
        }
        else
        {
            var hpBefore = _playerUnit.CurrentHp;
            ActionResolver.ResolveAttack(enemy, _playerUnit);
            NotifyHpChange(_playerUnit, hpBefore);
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
            _battleEnded = true;
            EndBattle(false);
        }
        else
        {
            var punisher = enemies.OrderByDescending(e => e.Data.speed).First();
            yield return new WaitForSeconds(0.8f);
            var hpBefore = _playerUnit.CurrentHp;
            ActionResolver.ResolveAttack(punisher, _playerUnit);
            NotifyHpChange(_playerUnit, hpBefore);
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
        PlayerStats.Instance.currentHp = _playerUnit.CurrentHp;
        PlayerStats.Instance.currentSp = _playerUnit.CurrentSp;
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
        if (_battleEnded && _battleLoopCoroutine == null)
            return;

        _battleEnded = true;
        PersistPlayerState();
        OnBattleStateChanged?.Invoke();
        OnBattleEnded?.Invoke(playerWon);
        _battleLoopCoroutine = null;
    }
}
