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
        if (!PauseManager.Instance) return;

        var isPaused = PauseManager.Instance.IsInputLocked;
        var dialogueFreeze = DialogueManager.Instance && DialogueManager.Instance.ShouldFreezePlayer;

        if (isPaused)
        {
            if (_inputActions.Player.enabled) _inputActions.Player.Disable();
            return;
        }

        if (!_inputActions.Player.enabled) _inputActions.Player.Enable();

        if (dialogueFreeze)
        {
            _inputActions.Player.Move.Disable();
            _inputActions.Player.Attack.Disable();
            _inputActions.Player.Jump.Disable();
            _inputActions.Player.Dash.Disable();
            _inputActions.Player.Fly.Disable();
        }
        else
        {
            _inputActions.Player.Move.Enable();
            _inputActions.Player.Attack.Enable();
            _inputActions.Player.Jump.Enable();
            _inputActions.Player.Dash.Enable();
            _inputActions.Player.Fly.Enable();
        }
    }

    // Methods
    public void OnMove(InputAction.CallbackContext context) => OnMoveEvent?.Invoke(context.ReadValue<Vector2>());
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnAttackEvent?.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnInteractEvent?.Invoke();
    }
    
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
