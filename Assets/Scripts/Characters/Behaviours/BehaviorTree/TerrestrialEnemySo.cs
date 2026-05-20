using UnityEngine;

[CreateAssetMenu(fileName = "TerrestrialEnemy", menuName = "Scriptable Objects/TerrestrialEnemy")]
public class TerrestrialEnemySoEnemy : EnemySO
{
    public override void Chase(ChaseBehaviour chaseBehaviour, GameObject target)
    {
        chaseBehaviour.Chase(target.transform, chaseSpeed);
    }

    public override void Patrol(PatrolBehaviour patrolBehaviour)
    {
        patrolBehaviour.Patrol(walkingSpeed, minWaitTime, maxWaitTime);
    }
}
