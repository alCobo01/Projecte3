using UnityEngine;

public class PlayerBattleController : MonoBehaviour
{
    private PlayerMovementController _movementController;
    private PlayerJumpController _jumpController;
    private PlayerDashController _dashController;

    private void Awake()
    {
        _movementController = GetComponent<PlayerMovementController>();
        _jumpController = GetComponent<PlayerJumpController>();
        _dashController = GetComponent<PlayerDashController>();
    }

    private void Update()
    {
        BattleManager.Instance.OnBattleStarted += OnBattleStarted;
        BattleManager.Instance.OnBattleEnded += OnBattleEnded;
    }
    
    private void OnBattleStarted() => SetMovement(false);
    private void OnBattleEnded(bool _) => SetMovement(true);

    private void SetMovement(bool canMove)
    {
        _movementController.CanMove = canMove;
        _jumpController.CanJump = canMove;
        _dashController.CanDash = canMove;
    }
}
