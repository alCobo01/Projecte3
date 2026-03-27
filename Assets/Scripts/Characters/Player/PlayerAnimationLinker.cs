public class PlayerAnimationLinker : CharacterAnimationController
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
        _jumpBehaviour.OnJumpEvent += TriggerJump;
        _dashBehaviour.OnDashEvent += TriggerDash;
    }

    private void OnDisable()
    {
        _jumpBehaviour.OnJumpEvent -= TriggerJump;
        _dashBehaviour.OnDashEvent -= TriggerDash;
    } 
}