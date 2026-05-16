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
    private static readonly int IsBattleHash = Animator.StringToHash("IsBattle");
    private static readonly int AboutToLandHash = Animator.StringToHash("AboutToLand");
    private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
    
    private AnimationBehaviour _animationBehaviour;
    private Rigidbody2D _rb;
    private GroundCheck _groundCheck;
    private bool _aboutToLandTriggered;

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
        _animationBehaviour.SetFloat(VerticalSpeedHash, verticalVelocity);

        if (!_groundCheck) return;
        
        bool isGrounded = _groundCheck.IsGrounded;
        _animationBehaviour.SetBool(IsGroundedHash, isGrounded);

        // Si tocamos el suelo, reseteamos todo el estado de aire
        if (isGrounded)
        {
            _animationBehaviour.SetBool(IsJumpingHash, false);
            _aboutToLandTriggered = false;
        }

        // AboutToLand: trigger predictivo. 
        // Si estamos bajando (velocidad < -0.1) y detectamos suelo cerca, disparamos.
        // Quitamos el umbral de -0.5 para que los saltos pequeños también lo activen.
        if (verticalVelocity < -0.1f && _groundCheck.IsNearGround && !isGrounded)
        {
            if (!_aboutToLandTriggered)
            {
                _animationBehaviour.Trigger(AboutToLandHash);
                _aboutToLandTriggered = true;
            }
        }
        
        // Si por alguna razón empezamos a subir (Doble Salto) mientras el trigger estaba activo, lo reseteamos
        if (verticalVelocity > 0.5f)
        {
            _aboutToLandTriggered = false;
        }
    }
    
    public void SetWalking(float speed) => _animationBehaviour.SetFloat(HorizontalSpeedHash, speed);
    public void TriggerJump() 
    {
        _animationBehaviour.Trigger(JumpHash);
        _animationBehaviour.SetBool(IsJumpingHash, true); // Marcamos que estamos saltando
        _aboutToLandTriggered = false;
    }
    public void TriggerDash() => _animationBehaviour.Trigger(DashHash);
    public void TriggerAttack() => _animationBehaviour.Trigger(AttackHash);
    public void TriggerHurt() => _animationBehaviour.Trigger(HurtHash);
    public void SetRunning(bool isRunning) => _animationBehaviour.SetBool(IsRunningHash, isRunning);
    public void SetBattle(bool isBattle) => _animationBehaviour.SetBool(IsBattleHash, isBattle);
    public void SetIsGrounded(bool isGrounded) => _animationBehaviour.SetBool(IsGroundedHash, isGrounded);    public void SetBool(bool value, int hash) => _animationBehaviour.SetBool(hash, value);
    public void TriggerHash(int hash) => _animationBehaviour.Trigger(hash);
}
