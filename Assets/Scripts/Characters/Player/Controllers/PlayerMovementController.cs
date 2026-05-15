using System;
using UnityEngine;

[RequireComponent(typeof(MoveBehaviour))]
public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private float runMultiplier = 1.75f;
    [SerializeField] private bool canRun;

    public bool CanMove 
    { 
        get => _canMove;
        set 
        {
            _canMove = value;
            if (!_canMove) _moveInput = Vector2.zero;
        }
    }
    private bool _canMove;
    
    public bool HasToRun { get; set; }
    
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
    
    private void FixedUpdate()
    {
        if (!CanMove)
        {
            _moveBehaviour.MoveCharacter(Vector2.zero);
            return;
        }

        HasToRun = canRun;
        var moveInput = HasToRun ? _moveInput * runMultiplier : _moveInput;
        _moveBehaviour.MoveCharacter(moveInput);
    }
    
    private void HandleMovement(Vector2 moveInput)
    {
        if (CanMove) _moveInput = moveInput;
    } 
}
