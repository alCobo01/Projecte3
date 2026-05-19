using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInputController))]
public class PlayerAttackController : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackRadius = 0.75f;
    [SerializeField] private LayerMask targetLayer = ~0;
    
    private PlayerInputController _inputController;
    private CharacterAnimationController _animController;
    private PlayerBattleController _battleController;
    private BattleStarter _battleStarter;
    
    private void Awake()
    {
        _inputController = GetComponent<PlayerInputController>();
        _animController = GetComponent<CharacterAnimationController>();
        _battleController = GetComponent<PlayerBattleController>();
        _battleStarter = GetComponent<BattleStarter>();
        
        _inputController.OnAttackEvent += HandleAttack;
    }

    private void OnDisable() => _inputController.OnAttackEvent -= HandleAttack;
    
    private void HandleAttack()
    {
        if (!_battleController.CanAttack) return;
        SoundManager.Instance.PlaySfx("Attack", transform);
        _animController.TriggerAttack();
        TryStartBattle();
    }
    
    private void TryStartBattle()
    {
        var pivot = attackPoint != null ? attackPoint : transform;
        var origin = pivot.position + pivot.right * attackRange;
        var hitColliders = Physics2D.OverlapCircleAll(origin, attackRadius, targetLayer);
        var damagedTargets = new HashSet<IBattleStarter>();

        foreach (var hit in hitColliders)
        {
            var damageable = hit.GetComponentInParent<IBattleStarter>();
            if (damageable == null || !damagedTargets.Add(damageable)) continue;

            if (!_battleStarter) continue;
            _battleStarter.TryStartBattleWithTarget(damageable);
        }
    }

    private void OnDrawGizmosSelected()
    {
        var pivot = attackPoint != null ? attackPoint : transform;
        var origin = pivot.position + pivot.right * attackRange;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, attackRadius);
    }


}
