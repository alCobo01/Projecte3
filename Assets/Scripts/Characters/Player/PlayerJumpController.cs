using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(JumpBehaviour))]
[RequireComponent(typeof(GroundCheck))]
public class PlayerJumpController : MonoBehaviour
{
    public bool CanDoubleJump { get; set; }
    
    [SerializeField] private int maxJumps = 2;
    
    private PlayerInputController _inputController;
    private JumpBehaviour _jumpBehaviour;
    private GroundCheck _groundCheck;
    private int _jumpsRemaining;
    private bool _isJumpHeld;
    
    private void Awake()
    {
        _inputController = GetComponent<PlayerInputController>();
        _jumpBehaviour = GetComponent<JumpBehaviour>();
        _groundCheck = GetComponent<GroundCheck>();
    }
    
    private void Start() => _inputController.OnJumpEvent += HandleJump;
    private void OnDisable() => _inputController.OnJumpEvent -= HandleJump;

    private void Update() { if (_groundCheck.IsGrounded) _jumpsRemaining = maxJumps; } 
    private void FixedUpdate()
    {
        if (_isJumpHeld) _jumpBehaviour.HoldJump(Time.fixedDeltaTime);
    }
    
    private void HandleJump(bool isPressed)
    {
        _isJumpHeld = isPressed;
        if (isPressed)
        {
            if (_jumpsRemaining <= 0) return;
            _jumpBehaviour.Jump();
            _jumpsRemaining--;
        }
        else
        {
            _jumpBehaviour.CancelJump();
        }
    }
}
