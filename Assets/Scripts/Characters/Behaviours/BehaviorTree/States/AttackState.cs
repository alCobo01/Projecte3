using UnityEngine;

[CreateAssetMenu(fileName = "AttackState", menuName = "Scriptable Objects/AttackState")]
public class AttackState : Node
{
    [SerializeField] private float attackCooldown = 1f;
    private float _lastAttackTime = 0f;

    public override bool EnterCondition(EnemyController ec)
    {
        return ec.attack.check;
    }
    public override bool ExitCondition(EnemyController ec)
    {
        return !ec.attack.check;
    }
    public override void OnStart(EnemyController ec)
    {
        _lastAttackTime = 0f;
    }
    public override void OnUpdate(EnemyController ec)
    {
        base.OnUpdate(ec);
        
        if (Time.time >= _lastAttackTime + attackCooldown)
        {
            var anim = ec.GetComponent<CharacterAnimationController>();
            if (anim != null) anim.TriggerAttack();
            Debug.Log(ec.GetComponent<AttackBehaviour>());
            ec.GetComponent<AttackBehaviour>().Attack(ec.target.transform);
            _lastAttackTime = Time.time;
            Debug.Log("Enemigo atacando!");
        }
    }
    public override void OnExit(EnemyController ec)
    {
    }
}
