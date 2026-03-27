using UnityEngine;

[RequireComponent(typeof(AnimationBehaviour))]
public class CharacterAnimationController : MonoBehaviour
{
    private static readonly int HorizontalSpeedHash = Animator.StringToHash("HorizontalSpeed");
    private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int DashHash = Animator.StringToHash("Dash");
    
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
        _animationBehaviour.SetFloat(HorizontalSpeedHash, Mathf.Abs(_rb.linearVelocityX));
        _animationBehaviour.SetFloat(VerticalSpeedHash, _rb.linearVelocityY);
        _animationBehaviour.SetBool(IsGroundedHash, _groundCheck.IsGrounded);
    }

    protected void TriggerJump() => _animationBehaviour.Trigger(JumpHash);
    protected void TriggerDash() => _animationBehaviour.Trigger(DashHash);
    protected void TriggerHash(int hash) => _animationBehaviour.Trigger(hash);
}