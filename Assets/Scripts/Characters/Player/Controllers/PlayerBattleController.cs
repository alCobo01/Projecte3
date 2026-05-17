using UnityEngine;

public class PlayerBattleController : MonoBehaviour
{
    private PlayerMovementController _movementController;
    private PlayerJumpController _jumpController;
    private PlayerDashController _dashController;
    private CharacterAnimationController _animationController;

    private void Awake()
    {
        _movementController = GetComponent<PlayerMovementController>();
        _jumpController = GetComponent<PlayerJumpController>();
        _dashController = GetComponent<PlayerDashController>();
        _animationController = GetComponent<CharacterAnimationController>();
    }

    private void OnEnable()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted += OnBattleStarted;
            BattleManager.Instance.OnBattleEnded += OnBattleEnded;
        }
    }

    private void OnDisable()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted -= OnBattleStarted;
            BattleManager.Instance.OnBattleEnded -= OnBattleEnded;
        }
    }
    
    private void OnBattleStarted()
    {
        SetMovement(false);
        _animationController?.SetBattle(true);
    }
    
    private void OnBattleEnded(bool _)
    {
        SetMovement(true);
        _animationController?.SetBattle(false);
    }

    private void SetMovement(bool canMove)
    {
        _movementController.CanMove = canMove;
        _jumpController.CanJump = canMove;
        _dashController.CanDash = canMove;
    }
}
