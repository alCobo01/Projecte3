using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }
    public bool IsBattleRunning { get; private set; }
    public BattleUnit PlayerUnit { get; private set; }
    public int LastCoinReward { get; private set; }

    [Header("References")]
    [SerializeField] private BattleUI ui;
    [SerializeField] private GameObject statUpVfxPrefab;
    [SerializeField] private GameObject statDownVfxPrefab;
    
    [Header("Durations")]
    [SerializeField] private float enemyTurnDelay = 1f;
    [SerializeField] private float turnCadenceDelay = 0.35f;
    [SerializeField] private float lungeDuration = 0.25f;
    [SerializeField] private float returnDuration = 0.35f;
    
    [Header("Coin rewards")]
    [SerializeField] private int minCoinReward = 10;
    [SerializeField] private int maxCoinReward = 50;

    private readonly TurnStack _turnStack = new();
    private readonly List<BattleUnit> _allUnits = new();

    private Coroutine _battleLoop;
    private bool _endNotified;
    private int _round;
    
    public IReadOnlyList<BattleUnit> AllUnits => _allUnits;
    public IEnumerable<BattleUnit> GetUpcomingTurns(int count) => _turnStack.GetUpcomingTurns(count, _allUnits);

    public event UnityAction OnBattleStarted, OnBattleStateChanged;
    public event UnityAction<BattleUnit> OnTurnStarted, OnDamageTaken;
    public event UnityAction<bool> OnBattleEnded, OnFleeAttempted;
    public event UnityAction OnPlayerDied;
    public event UnityAction<string> OnCombatMessage;
    public event UnityAction<int> OnRoundStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartBattle(CharacterData playerData, CharacterData[] enemies, BattleInitiator initiator = BattleInitiator.Player, 
        CharacterAnimationController playerAnim = null, CharacterAnimationController enemyAnim = null,
        Transform playerTransform = null, Transform enemyTransform = null)
    {
        if (IsBattleRunning) return;
        if (playerData == null || enemies == null || PlayerStatsManager.Instance == null) return;

        var enemyData = enemies.Where(e => e).ToArray();
        if (enemyData.Length == 0) return;

        IsBattleRunning = true;
        LastCoinReward = 0;
        _endNotified = false;
        _round = 1;
        OnBattleStarted?.Invoke();

        if (_battleLoop != null) StopCoroutine(_battleLoop);

        var stats = PlayerStatsManager.Instance;
        if (stats == null) return;

        var savedHp = stats.currentHp <= 0 ? playerData.maxHp : stats.currentHp;
        var savedSp = Mathf.Clamp(stats.currentSp, 0, playerData.maxSp);

        PlayerUnit = new BattleUnit(playerData, isPlayer: true, startingSp: savedSp) 
        { 
            Animator = playerAnim,
            Transform = playerTransform,
            FeetVfxAnchor = playerAnim ? playerAnim.FeetVfxAnchor : playerTransform
        };
        PlayerUnit.SetHp(Mathf.Clamp(savedHp, 1, playerData.maxHp));
        PlayerUnit.SetSkills(stats.CharacterData.skills);

        _allUnits.Clear();
        _allUnits.Add(PlayerUnit);
        
        foreach (var e in enemyData)
        {
            var enemyUnit = new BattleUnit(e, isPlayer: false, startingSp: 0)
            {
                Animator = enemyAnim,
                Transform = enemyTransform,
                FeetVfxAnchor = enemyAnim ? enemyAnim.FeetVfxAnchor : enemyTransform
            };
            enemyUnit.SetSkills(e.skills);
            _allUnits.Add(enemyUnit);
        }

        var first = initiator == BattleInitiator.Enemy
            ? _allUnits.FirstOrDefault(u => !u.IsPlayer && !u.IsDead)
            : PlayerUnit;

        _turnStack.Build(_allUnits, first);
        ui.Initialize(this);

        SubscribeVfxEvents();

        StartCoroutine(BattleEntrySequence());
    }

    private IEnumerator PerformAttackMovement(BattleUnit attacker, BattleUnit target)
    {
        var startPos = attacker.Transform.position;
        var targetDir = (target.Transform.position - startPos).normalized;
        var peakPos = startPos + targetDir * 0.75f;

        var elapsed = 0f;

        while (elapsed < lungeDuration)
        {
            attacker.Transform.position = Vector3.Lerp(startPos, peakPos, elapsed / lungeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        attacker.Transform.position = peakPos;
        elapsed = 0f;

        while (elapsed < returnDuration)
        {
            attacker.Transform.position = Vector3.Lerp(peakPos, startPos, elapsed / returnDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        attacker.Transform.position = startPos;
    }

    private IEnumerator BattleEntrySequence()
    {
        // Desactivamos controllers del jugador en el mundo
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (player.TryGetComponent(out PlayerInputController input)) input.enabled = false;
            if (player.TryGetComponent(out PlayerDashController dash)) dash.enabled = false;
        }

        yield return BattleTransitionManager.Instance?.ExecuteBattleEntry();

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
            if (turnCadenceDelay > 0f)
                yield return new WaitForSeconds(turnCadenceDelay);
        }

        ForceCleanupIfNeeded();
    }

    private IEnumerator WaitForPlayerAction(BattleUnit player)
    {
        var done = false;
        var enemies = _allUnits.Where(u => !u.IsPlayer && !u.IsDead).ToList();
        
        ui.ShowActionMenu(player, enemies,
            onAttack: target =>
            {
                player.Animator.TriggerAttack();
                SoundManager.Instance.PlaySfx("Attack", transform);
                StartCoroutine(PerformAttackMovement(player, target));
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
                player.Animator.TriggerSpecialAttack();
                StartCoroutine(PerformAttackMovement(player, target));
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
                player.Animator.TriggerAttack();
                var before = CaptureHp();
                ActionResolver.ResolveItem(player, item);
                if (item.type == ItemType.Healing) SpawnVfx(statUpVfxPrefab, player);
                if (item.type == ItemType.Buff && item.boostedStat == StatType.Speed)
                    _turnStack.Refresh(_allUnits.Where(u => !u.IsDead));
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
        OnCombatMessage?.Invoke($"{enemy.Data.characterName} is going to attack.");
        yield return new WaitForSeconds(Mathf.Max(0f, enemyTurnDelay));

        enemy.Animator.TriggerAttack();
        SoundManager.Instance.PlaySfx("Attack", transform);
        StartCoroutine(PerformAttackMovement(enemy, PlayerUnit));

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
        if (!IsBattleRunning) return true;
        if (PlayerUnit == null) return false;

        if (PlayerUnit.IsDead)
        {
            EndBattle(playerWon: false);
            return true;
        }

        var enemiesAlive = _allUnits.Any(u => !u.IsPlayer && !u.IsDead);
        if (enemiesAlive) return false;

        EndBattle(playerWon: true);
        return true;
    }

    private void EndBattle(bool playerWon)
    {
        if (_endNotified) return;

        _endNotified = true;
        IsBattleRunning = false;

        var playerDied = PlayerUnit is { IsDead: true };
        PlayerStatsManager.Instance.currentHp = PlayerUnit.CurrentHp;
        PlayerStatsManager.Instance.currentSp = PlayerUnit.CurrentSp;
        
        if (playerWon)
        {
            var coinsToReward = Random.Range(minCoinReward, maxCoinReward + 1);
            PlayerStatsManager.Instance.AddCoins(coinsToReward);
            SoundManager.Instance.PlaySfx("GameOver", transform);
            LastCoinReward = coinsToReward;
            OnCombatMessage?.Invoke($"You won the battle and found {coinsToReward} coins!");
        }

        OnBattleStateChanged?.Invoke();
        OnBattleEnded?.Invoke(playerWon);

        if (_battleLoop != null) StopCoroutine(_battleLoop);
        _battleLoop = null;

        UnsubscribeVfxEvents();

        StartCoroutine(BattleExitSequence(playerWon, playerDied));
    }

    private IEnumerator BattleExitSequence(bool playerWon, bool playerDied)
    {
        yield return new WaitForSeconds(3f);
        if (playerDied) OnPlayerDied?.Invoke();
        ui.CloseBattleUi();
        yield return BattleTransitionManager.Instance?.ExecuteBattleExit(playerWon);

        // Re-activamos controllers al volver al mundo
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (player.TryGetComponent(out PlayerInputController input)) input.enabled = true;
            if (player.TryGetComponent(out PlayerDashController dash)) dash.enabled = true;
        }
    }

    private void SubscribeVfxEvents()
    {
        foreach (var unit in _allUnits)
        {
            unit.OnStatBoostApplied += HandleStatBoostVfx;
            unit.OnStatusEffectApplied += HandleStatusEffectVfx;
        }
    }

    private void UnsubscribeVfxEvents()
    {
        foreach (var unit in _allUnits)
        {
            unit.OnStatBoostApplied -= HandleStatBoostVfx;
            unit.OnStatusEffectApplied -= HandleStatusEffectVfx;
        }
    }

    private void HandleStatBoostVfx(BattleUnit unit, StatType stat, int amount)
    {
        if (unit == null || amount == 0) return;
        var prefab = amount > 0 ? statUpVfxPrefab : statDownVfxPrefab;
        SpawnVfx(prefab, unit);
    }

    private void HandleStatusEffectVfx(BattleUnit unit, StatusEffectData effectData)
    {
        if (unit == null || effectData == null) return;
        var isDebuff = effectData.type is StatusEffectType.Poison or StatusEffectType.Stun;
        SpawnVfx(isDebuff ? statDownVfxPrefab : statUpVfxPrefab, unit);
    }

    private void SpawnVfx(GameObject prefab, BattleUnit unit)
    {
        if (!prefab) return;
        var anchor = unit.FeetVfxAnchor != null ? unit.FeetVfxAnchor : unit.Transform;
        if (!anchor) return;
        var instance = Instantiate(prefab, anchor.position, Quaternion.identity, anchor);
        var particle = instance.GetComponent<ParticleSystem>();
        if (particle) particle.Play(true);
    }

    private void ForceCleanupIfNeeded() { if (!_endNotified) EndBattle(playerWon: false); }

    private Dictionary<BattleUnit, int> CaptureHp() => _allUnits.ToDictionary(u => u, u => u.CurrentHp);

    private void NotifyHpChanges(Dictionary<BattleUnit, int> before)
    {
        foreach (var unit in _allUnits)
            if (before.TryGetValue(unit, out var hp)) NotifyHpChange(unit, hp);
    }

    private void NotifyHpChange(BattleUnit unit, int hpBefore)
    {
        if (unit != null && hpBefore != unit.CurrentHp)
        {
            OnDamageTaken?.Invoke(unit);

            if (unit.IsDead && !unit.IsPlayer && unit.Transform != null)
            {
                var boss = unit.Transform.GetComponentInChildren<BossBehaviour>();
                if (boss != null) boss.NotifyBossDied();
            }
        }
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
