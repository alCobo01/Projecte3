using UnityEngine;

[RequireComponent(typeof(AnimationBehaviour))]
public class CharacterAnimationController : MonoBehaviour
{
    private static readonly int HorizontalSpeedHash = Animator.StringToHash("HorizontalSpeed");
    private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int DashHash = Animator.StringToHash("Dash");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int SpecialAttackHash = Animator.StringToHash("SpecialAttack");
    private static readonly int HurtHash = Animator.StringToHash("Hurt");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int IsBattleHash = Animator.StringToHash("IsBattle");
    private static readonly int AboutToLandHash = Animator.StringToHash("AboutToLand");
    private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
    
    private AnimationBehaviour _animationBehaviour;
    private Rigidbody2D _rb;
    private GroundCheck _groundCheck;
    private bool _aboutToLandTriggered;
    private float _airborneTime;

    [Header("VFX")]
    [SerializeField] private Transform feetVfxAnchor;

    public Transform FeetVfxAnchor => feetVfxAnchor;

    private const float MinAirTimeForLandAnticipation = 0.08f;
    private const float AboutToLandMinFallSpeed = -0.15f;

    protected virtual void Awake()
    {
        _animationBehaviour = GetComponent<AnimationBehaviour>();
        _rb = GetComponent<Rigidbody2D>();
        _groundCheck = GetComponent<GroundCheck>();
    }

    protected virtual void Update()
    {
        var speed = Mathf.Abs(_rb.linearVelocity.x);
        var verticalVelocity = _rb.linearVelocity.y;
        
        _animationBehaviour.SetFloat(HorizontalSpeedHash, speed);
        _animationBehaviour.SetFloatImmediate(VerticalSpeedHash, verticalVelocity);

        if (!_groundCheck) return;
        
        var isGrounded = _groundCheck.IsGrounded;
        _animationBehaviour.SetBool(IsGroundedHash, isGrounded);
        
        if (isGrounded)
        {
            _aboutToLandTriggered = false;
            _airborneTime = 0f;
        }
        else
        {
            _airborneTime += Time.deltaTime;
        }

        if (verticalVelocity <= AboutToLandMinFallSpeed &&
            _groundCheck.IsNearGround &&
            !isGrounded &&
            _airborneTime >= MinAirTimeForLandAnticipation)
        {
            if (!_aboutToLandTriggered)
            {
                _animationBehaviour.Trigger(AboutToLandHash);
                _aboutToLandTriggered = true;
            }
        }
        
        if (verticalVelocity > 0.5f) _aboutToLandTriggered = false;
    }
    
    public void SetWalking(float speed) => _animationBehaviour.SetFloat(HorizontalSpeedHash, speed);
    
    public void TriggerJump() 
    {
        _animationBehaviour.Trigger(JumpHash);
        _aboutToLandTriggered = false;
        _airborneTime = 0f;
    }
    
    public void TriggerDash() => _animationBehaviour.Trigger(DashHash);
    public void TriggerAttack() => _animationBehaviour.Trigger(AttackHash);
    public void TriggerSpecialAttack() => _animationBehaviour.Trigger(SpecialAttackHash);
    public void TriggerHurt() => _animationBehaviour.Trigger(HurtHash);
    public void SetRunning(bool isRunning) => _animationBehaviour.SetBool(IsRunningHash, isRunning);
    public void SetBattle(bool isBattle) => _animationBehaviour.SetBool(IsBattleHash, isBattle);
    public void SetIsGrounded(bool isGrounded) => _animationBehaviour.SetBool(IsGroundedHash, isGrounded);    public void SetBool(bool value, int hash) => _animationBehaviour.SetBool(hash, value);
    public void TriggerHash(int hash) => _animationBehaviour.Trigger(hash);
}
