using UnityEngine;

[RequireComponent(typeof(DashBehaviour))]
public class PlayerDashController : MonoBehaviour
{
    [SerializeField] private float dashCooldown = 2f;
    
    private PlayerInputController _inputController;
    private DashBehaviour _dashBehaviour;
    
    private Vector2 _currentMoveInput;
    private float _lastDashTime;
    private float _lastFacingDirectionX = 1f;
    
    private void Awake()
    {
        _inputController = GetComponent<PlayerInputController>();
        _dashBehaviour = GetComponent<DashBehaviour>();
    }

    private void Start()
    {
        _inputController.OnMoveEvent += UpdateMoveInput;
        _inputController.OnDashEvent += HandleDash;
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
    
    private void HandleDash()
    {
        if (!(Time.time >= _lastDashTime + dashCooldown) || _dashBehaviour.IsDashing) return;
        
        var targetDirectionX = _currentMoveInput.x != 0 ? Mathf.Sign(_currentMoveInput.x) : _lastFacingDirectionX;
        var dashDirection = new Vector2(targetDirectionX, 0f);

        _dashBehaviour.ExecuteDash(dashDirection);
        _lastDashTime = Time.time;
    }
}