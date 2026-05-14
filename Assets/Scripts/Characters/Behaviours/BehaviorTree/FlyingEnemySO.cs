using UnityEngine;

[CreateAssetMenu(fileName = "FlyingEnemySO", menuName = "Scriptable Objects/FlyingEnemySO")]
public class FlyingEnemySO : EnemySO
{
    public override void Chase(ChaseBehaviour chaseBehaviour, GameObject target)
    {
        chaseBehaviour.FlyingChase(target.transform, chaseSpeed);
    }
    
    public override void Patrol(PatrolBehaviour patrolBehaviour)
    {
        patrolBehaviour.FlyingPatrol(walkingSpeed, minWaitTime, maxWaitTime);
    }
}
