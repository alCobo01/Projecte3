using UnityEngine;

[RequireComponent(typeof(DashBehaviour))]
[RequireComponent(typeof(GroundCheck))]
public class PlayerDashController : MonoBehaviour
{
    public bool CanDash { get; set; }
    
    [SerializeField] private float landingDashCooldown = 0.2f;
    
    private PlayerInputController _inputController;
    private DashBehaviour _dashBehaviour;
    private GroundCheck _groundCheck;
    
    private Vector2 _currentMoveInput;
    private float _lastFacingDirectionX = -1f;
    private bool _hasDashAvailable;
    private bool _wasGrounded;
    private float _dashReadyTime;
    
    private void Awake()
    {
        _inputController = GetComponent<PlayerInputController>();
        _dashBehaviour = GetComponent<DashBehaviour>();
        _groundCheck = GetComponent<GroundCheck>();
    }

    private void Start()
    {
        _inputController.OnMoveEvent += UpdateMoveInput;
        _inputController.OnDashEvent += HandleDash;

        CanDash = true;
        _wasGrounded = _groundCheck.IsGrounded;
        _hasDashAvailable = _wasGrounded;
        _dashReadyTime = _wasGrounded ? Time.time + landingDashCooldown : float.MaxValue;
    }

    private void OnDisable()
    {
        _inputController.OnMoveEvent -= UpdateMoveInput;
        _inputController.OnDashEvent -= HandleDash;
    } 
    
    private void UpdateMoveInput(Vector2 moveInput)
    {
        _currentMoveInput = moveInput;
        if (moveInput.x != 0) _lastFacingDirectionX = Mathf.Sign(moveInput.x);
    }

    private void Update()
    {
        var isGrounded = _groundCheck.IsGrounded;
        if (isGrounded && !_wasGrounded)
        {
            _hasDashAvailable = true;
            _dashReadyTime = Time.time + landingDashCooldown;
        }

        _wasGrounded = isGrounded;
    }
    
    private void HandleDash()
    {
        if (!CanDash) return;
        if (!_hasDashAvailable || Time.time < _dashReadyTime || _dashBehaviour.IsDashing) return;
        
        var targetDirectionX = _currentMoveInput.x != 0 ? Mathf.Sign(_currentMoveInput.x) : _lastFacingDirectionX;
        var dashDirection = new Vector2(targetDirectionX, 0f);

        _dashBehaviour.ExecuteDash(dashDirection);
        _hasDashAvailable = false;

        if (_groundCheck.IsGrounded)
        {
            _hasDashAvailable = true;
            _dashReadyTime = Time.time + landingDashCooldown;
        }
    }
}
