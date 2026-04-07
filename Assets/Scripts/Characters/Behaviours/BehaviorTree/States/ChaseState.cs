using UnityEngine;

[CreateAssetMenu(fileName = "ChaseState", menuName = "Scriptable Objects/ChaseState")]
public class ChaseState : Node
{
    public override bool EnterCondition(EnemyController ec)
    {
        Debug.Log("CHASE ENTER" + ec.chase.check);
        return ec.chase.check;
    }
    public override bool ExitCondition(EnemyController ec)
    {
        return !ec.chase.check || ec.run.check || ec.attack.check;
    }
    public override void OnStart(EnemyController ec)
    {

    }
    public override void OnUpdate(EnemyController ec)
    {
        base.OnUpdate(ec);
        ec.GetComponent<ChaseBehaviour>().Chase(ec.target.transform);
        Debug.Log("Chasing");
        Debug.Log("updateChase");
    }
    public override void OnExit(EnemyController ec)
    {
        ec.GetComponent<ChaseBehaviour>().StopChasing();
        Debug.Log("NO CHASE");
    }
}
