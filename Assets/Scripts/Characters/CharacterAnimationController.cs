using UnityEngine;

[RequireComponent(typeof(AnimationBehaviour))]
public class CharacterAnimationController : MonoBehaviour
{
    protected static readonly int HorizontalSpeedHash = Animator.StringToHash("HorizontalSpeed");
    protected static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
    protected static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    protected static readonly int JumpHash = Animator.StringToHash("Jump");
    protected static readonly int DashHash = Animator.StringToHash("Dash");
    protected static readonly int AttackHash = Animator.StringToHash("Attack");
    protected static readonly int HurtHash = Animator.StringToHash("Hurt");
    protected static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    
    protected AnimationBehaviour _animationBehaviour;
    protected Rigidbody2D _rb;
    protected GroundCheck _groundCheck;

    protected virtual void Awake()
    {
        _animationBehaviour = GetComponent<AnimationBehaviour>();
        _rb = GetComponent<Rigidbody2D>();
        _groundCheck = GetComponent<GroundCheck>();
    }

    protected virtual void Update()
    {
        float speed = Mathf.Abs(_rb.linearVelocity.x);
        //_animationBehaviour.SetFloat(HorizontalSpeedHash, speed);
        _animationBehaviour.SetFloat(VerticalSpeedHash, _rb.linearVelocity.y);
        
        if (_groundCheck != null)
            _animationBehaviour.SetBool(IsGroundedHash, _groundCheck.IsGrounded);

        // Log para ver si el script detecta movimiento
        if (speed > 0.01f) {
            // Debug.Log(gameObject.name + " moviéndose a velocidad: " + speed);
        }
    }
    public void SetWalking(float speed) => _animationBehaviour.SetFloat(HorizontalSpeedHash, speed);
    public void TriggerJump() => _animationBehaviour.Trigger(JumpHash);
    public void TriggerDash() => _animationBehaviour.Trigger(DashHash);
    public void TriggerAttack() => _animationBehaviour.Trigger(AttackHash);
    public void TriggerHurt() => _animationBehaviour.Trigger(HurtHash);
    public void SetRunning(bool isRunning) => _animationBehaviour.SetBool(IsRunningHash, isRunning);
    public void TriggerHash(int hash) => _animationBehaviour.Trigger(hash);
}
