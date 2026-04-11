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
        Debug.Log("PATROL START");
    }
    public override void OnUpdate(EnemyController ec)
    {
        base.OnUpdate(ec);
        ec.GetComponent<PatrolBehaviour>().Patrol();
    }
    public override void OnExit(EnemyController ec)
    {
        Debug.Log("PATROL EXIT - Reason: Chase=" + ec.chase.check + " Attack=" + ec.attack.check);
        ec.GetComponent<PatrolBehaviour>().StopPatrol();
    }
}
