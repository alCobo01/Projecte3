using UnityEngine;

public class PlayerBattleController : MonoBehaviour
{
    private PlayerMovementController _movementController;
    private PlayerJumpController _jumpController;
    private PlayerDashController _dashController;

    private bool _valueToSet;

    private void Awake()
    {
        _movementController = GetComponent<PlayerMovementController>();
        _jumpController = GetComponent<PlayerJumpController>();
        _dashController = GetComponent<PlayerDashController>();
    }

    private void OnEnable()
    {
        BattleManager.Instance.OnBattleStarted += HandleStart;
        BattleManager.Instance.OnBattleEnded += HandleEnd;
    }

    private void OnDisable()
    {
        BattleManager.Instance.OnBattleStarted -= HandleStart;
        BattleManager.Instance.OnBattleEnded -= HandleEnd;
    }

    private void HandleStart()
    {
        _valueToSet = false;
        TogglePlayerMovement();
    }

    private void HandleEnd(bool _)
    {
        _valueToSet = true;
        TogglePlayerMovement();
    }

    private void TogglePlayerMovement()
    {
        _movementController.CanMove = _valueToSet;
        _jumpController.CanJump = _valueToSet;
        _dashController.CanDash = _valueToSet;
    }
}
