using UnityEngine;

[RequireComponent(typeof(MoveBehaviour))]
public class PlayerMovementController : MonoBehaviour
{
    private PlayerInputController _inputController;
    private MoveBehaviour _moveBehaviour;

    private void Awake()
    {
        _moveBehaviour = GetComponent<MoveBehaviour>();
        _inputController = GetComponent<PlayerInputController>();
    }

    private void Start() => _inputController.OnMoveEvent += HandleMovement;
    private void OnDisable() => _inputController.OnMoveEvent -= HandleMovement;

    private void HandleMovement(Vector2 moveInput)
    {
        _moveBehaviour.MoveCharacter(moveInput);
    }
}