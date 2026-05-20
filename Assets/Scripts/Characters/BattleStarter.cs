using System;
using System.Linq;
using UnityEngine;

public class BattleStarter : MonoBehaviour, IBattleStarter
{
    public BattleInitiator BattleInitiator => battleInitiator;
    public CharacterData[] BattleParty => ownerType == OwnerType.Projectile
        ? _owner.BattleParty
        : battleParty;    
    
    [Header("Batlle config")]
    [SerializeField] private BattleInitiator battleInitiator = BattleInitiator.Player;
    [SerializeField] private CharacterData[] battleParty;
    [SerializeField] private OwnerType ownerType = OwnerType.Self;
    
    [Header("Runtime")]
    [SerializeField] private float reenterCooldown = 0.2f;
    
    private BattleStarter _owner;
    private int _playerLayer, _enemyLayer;
    private float _nextAllowedBattleTime;
    private enum OwnerType { Self, Projectile }
    
    public void SetProjectileOwner(BattleStarter owner) => _owner = owner;

    private void Awake()
    {
        _playerLayer = LayerMask.NameToLayer("Player");
        _enemyLayer = LayerMask.NameToLayer("Enemy");
        _nextAllowedBattleTime = 0f;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!enabled) return;
        if (Time.time < _nextAllowedBattleTime) return;
        
        var otherLayer = other.gameObject.layer;
        var validTargetLayer = battleInitiator == BattleInitiator.Player
            ? otherLayer == _enemyLayer
            : otherLayer == _playerLayer;

        if (!validTargetLayer)
            return;

        TryStartBattleWith(other.gameObject);
    } 

    public void TryStartBattleWithTarget(IBattleStarter target)
    {
        var targetMb = target as MonoBehaviour;
        if (targetMb == null) return;
        TryStartBattleWith(targetMb.gameObject);
    }

    private void TryStartBattleWith(GameObject other)
    {
        if (Time.time < _nextAllowedBattleTime) return;
        if (BattleManager.Instance.IsBattleRunning) return;
        
        var target = other.GetComponentInParent<IBattleStarter>();
        if (target == null) return;
        
        //In case that the collided battlestarter scripts is disabled, dont start the battle
        if (target is MonoBehaviour mb)
            if (!mb.enabled || !mb.gameObject.activeInHierarchy) return;
        
        var actualSelf = ownerType == OwnerType.Projectile && _owner
            ? _owner
            : this;
        
        var ownParty = GetAliveParty(actualSelf.BattleParty);
        var targetParty = GetAliveParty(target.BattleParty);
        if (ownParty.Length == 0 || targetParty.Length == 0) return;
        
        CharacterData playerData;
        CharacterData[] enemies;

        if (BattleInitiator == BattleInitiator.Player)
        {
            playerData = ownParty[0];
            enemies = targetParty;
            var playerTransform = actualSelf.transform.root;
            var enemyTransform = other.transform.root;
            BattleTransitionManager.Instance?.PrepareBattle(playerTransform, enemyTransform);
            
            var playerAnim = playerTransform.GetComponentInChildren<CharacterAnimationController>();
            var enemyAnim = enemyTransform.GetComponentInChildren<CharacterAnimationController>();
            BattleManager.Instance.StartBattle(playerData, enemies, BattleInitiator, playerAnim, enemyAnim, playerTransform, enemyTransform);
            
            // Notify enemies to enter BattleState
            playerTransform.GetComponentInChildren<EnemyController>()?.SetBattleMode(true);
            enemyTransform.GetComponentInChildren<EnemyController>()?.SetBattleMode(true);
        }
        else
        {
            playerData = targetParty[0];
            enemies = ownParty;
            var playerTransform = other.transform.root;
            var enemyTransform = actualSelf.transform.root;
            BattleTransitionManager.Instance?.PrepareBattle(playerTransform, enemyTransform);
            
            var playerAnim = playerTransform.GetComponentInChildren<CharacterAnimationController>();
            var enemyAnim = enemyTransform.GetComponentInChildren<CharacterAnimationController>();
            BattleManager.Instance.StartBattle(playerData, enemies, BattleInitiator, playerAnim, enemyAnim, playerTransform, enemyTransform);

            // Notify enemies to enter BattleState
            playerTransform.GetComponentInChildren<EnemyController>()?.SetBattleMode(true);
            enemyTransform.GetComponentInChildren<EnemyController>()?.SetBattleMode(true);
        }
        
        actualSelf._nextAllowedBattleTime = Time.time + Mathf.Max(0.01f, reenterCooldown);
    }

    private static CharacterData[] GetAliveParty(CharacterData[] party)
    {
        if (party == null || party.Length == 0)
            return Array.Empty<CharacterData>();

        return party.Where(member => member != null).ToArray();
    }
}
