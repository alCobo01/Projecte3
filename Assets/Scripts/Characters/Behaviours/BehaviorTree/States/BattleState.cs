using UnityEngine;

[CreateAssetMenu(fileName = "BattleState", menuName = "Scriptable Objects/BattleState")]
public class BattleState : Node
{
    public override bool EnterCondition(EnemyController ec)
    {
        return ec.battle.check;
    }

    public override bool ExitCondition(EnemyController ec)
    {
        return !ec.battle.check;
    }

    public override void OnStart(EnemyController ec)
    {
        var anim = ec.GetComponent<CharacterAnimationController>();
        anim.SetRunning(false);
        anim.SetBattle(true);
        }

    public override void OnExit(EnemyController ec)
    {
        var anim = ec.GetComponent<CharacterAnimationController>();
        anim.SetBattle(false);
        
    }
}
