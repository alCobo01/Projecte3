using UnityEngine;

public abstract class EnemySO : ScriptableObject
{
    public float attackDistance;
    public float walkingSpeed;
    public float chaseSpeed;
    public float attackSpeed;

    [Header("Wait Settings")]
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;
    public abstract void Chase(ChaseBehaviour chaseBehaviour, GameObject target);
    public abstract void Patrol(PatrolBehaviour patrolBehaviour);
}
