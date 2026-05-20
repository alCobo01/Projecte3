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
            bool isPaused = PauseManager.Instance.IsPaused || PauseManager.Instance.IsInputLocked;
            bool dialogueFreeze = DialogueManager.Instance != null && DialogueManager.Instance.ShouldFreezePlayer;

            // Si es pausa total, desactivamos todo el mapa
            if (isPaused)
            {
                if (_inputActions.Player.enabled) _inputActions.Player.Disable();
            }
            else
            {
                // Si el mapa estaba desactivado por pausa, lo activamos
                if (!_inputActions.Player.enabled) _inputActions.Player.Enable();

                // Si estamos en un diálogo que congela, desactivamos solo lo que impide moverse/atacar
                if (dialogueFreeze)
                {
                    _inputActions.Player.Move.Disable();
                    _inputActions.Player.Attack.Disable();
                    _inputActions.Player.Jump.Disable();
                    _inputActions.Player.Dash.Disable();
                    _inputActions.Player.Fly.Disable();
                    // Mantenemos Interact, OpenInventory y PauseMenu habilitados
                }
                else
                {
                    // Si no hay freeze, nos aseguramos de que todo esté habilitado
                    _inputActions.Player.Move.Enable();
                    _inputActions.Player.Attack.Enable();
                    _inputActions.Player.Jump.Enable();
                    _inputActions.Player.Dash.Enable();
                    _inputActions.Player.Fly.Enable();
                }
            }
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
