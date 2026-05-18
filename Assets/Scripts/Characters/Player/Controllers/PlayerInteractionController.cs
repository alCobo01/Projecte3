using System;
using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    public static PlayerInteractionController Instance { get; private set; }

    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private LayerMask interactionLayer = ~0;
    
    private PlayerInputController _inputController;
    private IInteractable _currentInteractable;

    public event Action<IInteractable> OnInteractableTargeted;
    public event Action<IInteractable> OnInteractableUntargeted;
    
    private void Awake()
    {
        Instance = this; // El nuevo jugador siempre toma el control

        _inputController = GetComponent<PlayerInputController>();
        _inputController.OnInteractEvent += HandleInteraction;
    }
    
    private void OnDestroy() 
    {
        if (Instance == this) Instance = null;
        if (_inputController != null) _inputController.OnInteractEvent -= HandleInteraction;
    }

    private void Update() => DetectInteractable();
    private void HandleInteraction() => _currentInteractable?.Interact();
    private void DetectInteractable()
    {
        Vector2 origin = transform.position;
        var hits = Physics2D.OverlapCircleAll(origin, interactionRadius, interactionLayer);

        IInteractable newInteractable = null;
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IInteractable interactable))
            {
                newInteractable = interactable;
                break;
            }
        }

        if (newInteractable != _currentInteractable)
        {
            if (_currentInteractable != null) OnInteractableUntargeted?.Invoke(_currentInteractable);
            _currentInteractable = newInteractable;
            if (_currentInteractable != null) OnInteractableTargeted?.Invoke(_currentInteractable);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        var originTransform = transform;
        var origin = originTransform.position;
    
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, interactionRadius);
    }
    
}
