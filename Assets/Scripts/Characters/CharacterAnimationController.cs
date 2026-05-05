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
    private static readonly int HurtHash = Animator.StringToHash("Hurt");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    
    private AnimationBehaviour _animationBehaviour;
    private Rigidbody2D _rb;
    private GroundCheck _groundCheck;

    protected virtual void Awake()
    {
        _animationBehaviour = GetComponent<AnimationBehaviour>();
        _rb = GetComponent<Rigidbody2D>();
        _groundCheck = GetComponent<GroundCheck>();
    }

    protected virtual void Update()
    {
        var speed = Mathf.Abs(_rb.linearVelocity.x);
        _animationBehaviour.SetFloat(HorizontalSpeedHash, speed);
        _animationBehaviour.SetFloat(VerticalSpeedHash, _rb.linearVelocity.y);
        
        if (_groundCheck)
            _animationBehaviour.SetBool(IsGroundedHash, _groundCheck.IsGrounded);
    }
    
    public void SetWalking(float speed) => _animationBehaviour.SetFloat(HorizontalSpeedHash, speed);
    public void TriggerJump() => _animationBehaviour.Trigger(JumpHash);
    public void TriggerDash() => _animationBehaviour.Trigger(DashHash);
    public void TriggerAttack() => _animationBehaviour.Trigger(AttackHash);
    public void TriggerHurt() => _animationBehaviour.Trigger(HurtHash);
    public void SetRunning(bool isRunning) => _animationBehaviour.SetBool(IsRunningHash, isRunning);
    public void TriggerHash(int hash) => _animationBehaviour.Trigger(hash);
}
