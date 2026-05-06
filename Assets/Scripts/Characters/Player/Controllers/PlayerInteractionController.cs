using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private LayerMask interactionLayer = ~0;
    
    private PlayerInputController _inputController;
    private IInteractable _currentInteractable;
    
    private void Awake()
    {
        _inputController = GetComponent<PlayerInputController>();
        _inputController.OnInteractEvent += HandleInteraction;
    }
    
    private void OnDisable() => _inputController.OnInteractEvent -= HandleInteraction;

    private void Update() => DetectInteractable();
    private void HandleInteraction() => _currentInteractable?.Interact(gameObject);
    private void DetectInteractable()
    {
        Vector2 origin = transform.position;
        var hits = Physics2D.OverlapCircleAll(origin, interactionRadius, interactionLayer);

        _currentInteractable = null;
        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent(out IInteractable interactable)) continue;
            _currentInteractable = interactable;
            break;
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
