using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(JumpBehaviour))]
[RequireComponent(typeof(GroundCheck))]
public class PlayerJumpController : MonoBehaviour
{
    public bool CanJump { get; set; }
    public bool CanDoubleJump { get; set; }
    
    [SerializeField] private int maxJumps = 2;
    
    private PlayerInputController _inputController;
    private CharacterAnimationController _animController;
    private JumpBehaviour _jumpBehaviour;
    private GroundCheck _groundCheck;
    private int _jumpsRemaining;
    private bool _isJumpHeld, _wasGrounded;
    
    private void Awake()
    {
        _inputController = GetComponent<PlayerInputController>();
        _animController = GetComponent<CharacterAnimationController>();
        _jumpBehaviour = GetComponent<JumpBehaviour>();
        _groundCheck = GetComponent<GroundCheck>();
    }

    private void Start()
    {
        _wasGrounded = _groundCheck.IsGrounded;
        _jumpsRemaining = _wasGrounded ? maxJumps : 0;
        _inputController.OnJumpEvent += HandleJump;

        CanJump = true;
    } 
    
    private void OnDisable() => _inputController.OnJumpEvent -= HandleJump;

    private void Update()
    {
        var isGrounded = _groundCheck.IsGrounded;
        if (isGrounded && !_wasGrounded) _jumpsRemaining = maxJumps;
        _wasGrounded = isGrounded;
    } 
    
    private void FixedUpdate()
    {
        if (_isJumpHeld) _jumpBehaviour.HoldJump(Time.fixedDeltaTime);
    }
    
    private void HandleJump(bool isPressed)
    {
        if (!CanJump) return;
        _isJumpHeld = isPressed;
        if (isPressed)
        {
            if (_jumpsRemaining <= 0) return;
            SoundManager.Instance.PlaySfx("Jump", transform);
            _animController?.TriggerJump();
            _jumpBehaviour.Jump();
            _jumpsRemaining--;
        }
        else
        {
            _jumpBehaviour.CancelJump();
        }
    }
}
