using static InputSystem_Actions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour, IPlayerActions
{
    // Events
    public static event UnityAction OnOpenInventoryEvent, OnPauseMenuEvent;
    public event UnityAction OnAttackEvent, OnInteractEvent, OnDashEvent;
    public event UnityAction<Vector2> OnMoveEvent;
    public event UnityAction<bool> OnJumpEvent, OnFlyEvent;

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

    private void Update()
    {
        if (PauseManager.Instance != null)
        {
            bool shouldDisable = PauseManager.Instance.IsInputLocked;

            if (shouldDisable && _inputActions.Player.enabled)
                _inputActions.Player.Disable();
            else if (!shouldDisable && !_inputActions.Player.enabled)
                _inputActions.Player.Enable();
        }
    }

    // Methods
    public void OnMove(InputAction.CallbackContext context) => OnMoveEvent?.Invoke(context.ReadValue<Vector2>());
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnAttackEvent?.Invoke();
    }
    public void OnInteract(InputAction.CallbackContext context) => OnInteractEvent?.Invoke();
    public void OnDash(InputAction.CallbackContext context) => OnDashEvent?.Invoke();
    public void OnOpenInventory(InputAction.CallbackContext context) => OnOpenInventoryEvent?.Invoke();
    public void OnPauseMenu(InputAction.CallbackContext context) => OnPauseMenuEvent?.Invoke();

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started) OnJumpEvent?.Invoke(true);
        else if (context.canceled) OnJumpEvent?.Invoke(false);
    } 
    
    public void OnFly(InputAction.CallbackContext context)
    {
        if (context.performed) OnFlyEvent?.Invoke(true);
        else if (context.canceled) OnFlyEvent?.Invoke(false);
    }

    
}
