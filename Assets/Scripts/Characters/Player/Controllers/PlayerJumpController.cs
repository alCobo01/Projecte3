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
    
    private void Awake()
    {
        _inputController = GetComponent<PlayerInputController>();
        _jumpBehaviour = GetComponent<JumpBehaviour>();
        _groundCheck = GetComponent<GroundCheck>();
    }
    
    private void Start() => _inputController.OnJumpEvent += HandleJump;
    private void OnDisable() => _inputController.OnJumpEvent -= HandleJump;

    private void Update() { if (_groundCheck.IsGrounded) _jumpsRemaining = maxJumps; } 
    
    private void HandleJump()
    {
        if (_jumpsRemaining <= 0) return;
        _jumpBehaviour.Jump();
        _jumpsRemaining--;
    }
}