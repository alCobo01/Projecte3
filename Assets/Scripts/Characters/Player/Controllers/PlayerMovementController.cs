using System;
using UnityEngine;

[RequireComponent(typeof(MoveBehaviour))]
public class PlayerMovementController : MonoBehaviour
{
    public bool CanMove { get; set; }
    
    private PlayerInputController _inputController;
    private MoveBehaviour _moveBehaviour;
    private Vector2 _moveInput;

    private void Awake()
    {
        _moveBehaviour = GetComponent<MoveBehaviour>();
        _inputController = GetComponent<PlayerInputController>();
        CanMove = true;
    }

    private void Start() => _inputController.OnMoveEvent += HandleMovement;
    private void OnDisable() => _inputController.OnMoveEvent -= HandleMovement;
    private void FixedUpdate() => _moveBehaviour.MoveCharacter(_moveInput);
    
    private void HandleMovement(Vector2 moveInput)
    {
        if (CanMove) _moveInput = moveInput;
    } 
}