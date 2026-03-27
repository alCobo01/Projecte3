using System;
using static InputSystem_Actions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour, IPlayerActions
{
    // Events
    public event UnityAction OnAttackEvent, OnJumpEvent, OnInteractEvent, OnDashEvent;
    public event UnityAction<Vector2> OnMoveEvent;
    public event UnityAction<bool> OnFlyEvent;

    // Init input system
    private InputSystem_Actions _inputActions;
    
    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Player.SetCallbacks(this);
    }
    
    private void Start() => _inputActions.Enable();
    private void OnEnable() => _inputActions.Enable();
    private void OnDisable() => _inputActions.Disable();

    // Methods
    public void OnMove(InputAction.CallbackContext context) => OnMoveEvent?.Invoke(context.ReadValue<Vector2>());
    public void OnAttack(InputAction.CallbackContext context) => OnAttackEvent?.Invoke();
    public void OnInteract(InputAction.CallbackContext context) => OnInteractEvent?.Invoke();
    public void OnJump(InputAction.CallbackContext context) => OnJumpEvent?.Invoke();
    public void OnDash(InputAction.CallbackContext context) => OnDashEvent?.Invoke();
    public void OnFly(InputAction.CallbackContext context)
    {
        if (context.performed) OnFlyEvent?.Invoke(true);
        if (context.canceled) OnFlyEvent?.Invoke(false);
    }
}

