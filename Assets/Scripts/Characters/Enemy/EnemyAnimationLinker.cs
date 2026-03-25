
public class EnemyAnimationLinker : CharacterAnimationController
{
    private JumpBehaviour _jumpBehaviour;
    private DashBehaviour _dashBehaviour;

    protected override void Awake()
    {
        base.Awake();
        _jumpBehaviour = GetComponent<JumpBehaviour>();
        _dashBehaviour = GetComponent<DashBehaviour>();
    }

    private void OnEnable()
    {
        if (_jumpBehaviour != null) _jumpBehaviour.OnJumpEvent += TriggerJump;
        if (_dashBehaviour != null) _dashBehaviour.OnDashEvent += TriggerDash;
    }

    private void OnDisable()
    {
        if (_jumpBehaviour != null) _jumpBehaviour.OnJumpEvent -= TriggerJump;
        if (_dashBehaviour != null) _dashBehaviour.OnDashEvent -= TriggerDash;
    } 
}