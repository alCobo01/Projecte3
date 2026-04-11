using UnityEngine;

[CreateAssetMenu(fileName = "EnterCombatState", menuName = "Scriptable Objects/EnterCombatState")]
public class EnterCombatState : Node
{
    public override bool EnterCondition(EnemyController ec)
    {
        return ec.combat.check;
    }
    public override bool ExitCondition(EnemyController ec)
    {
        return !ec.combat.check;
    }
    public override void OnStart(EnemyController ec)
    {
 
    }
    public override void OnUpdate(EnemyController ec)
    {
        base.OnUpdate(ec);
        Debug.Log("Starting Combat");

    }
    public override void OnExit(EnemyController ec)
    {

    }
}
