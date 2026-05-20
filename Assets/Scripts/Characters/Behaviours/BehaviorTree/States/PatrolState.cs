using UnityEngine;

[CreateAssetMenu(fileName = "PatrolState", menuName = "Scriptable Objects/PatrolState")]
public class PatrolState : Node
{
    public override bool EnterCondition(EnemyController ec)
    {
        return true;
    }
    
    public override bool ExitCondition(EnemyController ec)
    {
        return ec.chase.check || ec.attack.check;
    }
    
    public override void OnStart(EnemyController ec)
    {
        var anim = ec.GetComponent<CharacterAnimationController>();
        if (anim) anim.SetRunning(false);
    }
    
    public override void OnUpdate(EnemyController ec)
    {
        base.OnUpdate(ec);
        var patrol = ec.GetComponent<PatrolBehaviour>();
        ec.enemyData.Patrol(patrol);
    }
    
    public override void OnExit(EnemyController ec)
    {
        var patrol = ec.GetComponent<PatrolBehaviour>();
        if (patrol) patrol.StopPatrol();
    }
}
