using UnityEngine;

[RequireComponent(typeof(DashBehaviour))]
public class PlayerDashController : MonoBehaviour
{
    private static readonly int DashEndHash = Animator.StringToHash("DashEnd");
    
    [SerializeField] private float dashCooldown = 2f;

    private CharacterAnimationController _animController;
    private PlayerInputController _inputController;
    private DashBehaviour _dashBehaviour;
    
    private Vector2 _currentMoveInput;
    private float _lastDashTime;
    private float _lastFacingDirectionX = 1f;
    private bool _canDash = true;
    
    private void Awake()
    {
        _animController = GetComponent<CharacterAnimationController>();
        _inputController = GetComponent<PlayerInputController>();
        _dashBehaviour = GetComponent<DashBehaviour>();
    }

    private void Start()
    {
        _inputController.OnMoveEvent += UpdateMoveInput;
        _inputController.OnDashEvent += HandleDash;
        _dashBehaviour.OnDashFinishedEvent += HandleDashFinished;
    }

    private void OnDisable()
    {
        _inputController.OnMoveEvent -= UpdateMoveInput;
        _inputController.OnDashEvent -= HandleDash;
        _dashBehaviour.OnDashFinishedEvent -= HandleDashFinished;
    } 
    
    private void UpdateMoveInput(Vector2 moveInput)
    {
        _currentMoveInput = moveInput;
        if (moveInput.x != 0) _lastFacingDirectionX = Mathf.Sign(moveInput.x);
    }
    
    private void HandleDash()
    {
        if (!CanDash || !IsDashUnlocked) return;
        if (!(Time.time >= _lastDashTime + dashCooldown) || _dashBehaviour.IsDashing) return;
        
        var targetDirectionX = _currentMoveInput.x != 0 ? Mathf.Sign(_currentMoveInput.x) : _lastFacingDirectionX;
        var dashDirection = new Vector2(targetDirectionX, 0f);

        SoundManager.Instance.PlaySfx("Dash", transform);
        _animController?.TriggerDash();
        _dashBehaviour.ExecuteDash(dashDirection);
        _lastDashTime = Time.time;
    }

    private void HandleDashFinished()
    {
        _animController?.TriggerHash(DashEndHash);
    }

    public bool CanDash
    {
        get => _canDash;
        set => _canDash = value;
    }

    private bool IsDashUnlocked => PlayerStatsManager.Instance != null && PlayerStatsManager.Instance.dashUnlocked;
}
