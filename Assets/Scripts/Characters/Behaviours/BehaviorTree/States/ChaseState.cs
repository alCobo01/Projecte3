using UnityEngine;

[CreateAssetMenu(fileName = "ChaseState", menuName = "Scriptable Objects/ChaseState")]
public class ChaseState : Node
{
    public override bool EnterCondition(EnemyController ec)
    {
        return ec.chase.check;
    }
    public override bool ExitCondition(EnemyController ec)
    {
        return !ec.chase.check || ec.attack.check;
    }
    public override void OnStart(EnemyController ec)
    {
        var anim = ec.GetComponent<CharacterAnimationController>();
        if (anim != null) anim.SetRunning(true);
    }
    public override void OnUpdate(EnemyController ec)
    {
        base.OnUpdate(ec);
        
        // Comprobamos que el objetivo existe antes de intentar perseguirlo
        if (ec.target != null)
        {
            ChaseBehaviour chase = ec.GetComponent<ChaseBehaviour>();
            if (chase != null)
            {
                chase.Chase(ec.target.transform);
            }
        }
    }
    public override void OnExit(EnemyController ec)
    {
        ec.GetComponent<ChaseBehaviour>().StopChasing();
        Debug.Log("NO CHASE");
    }
}
