using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackState", menuName = "Scriptable Objects/AttackState")]
public class AttackBehavior : Node
{
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
    }
    public override void OnUpdate(EnemyController ec)
    {
        base.OnUpdate(ec);
        Debug.Log("te reviento el ano");
    }
    public override void OnExit(EnemyController ec)
    {
    }
}
