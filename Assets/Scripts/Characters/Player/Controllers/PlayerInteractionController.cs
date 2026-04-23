using System;
using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private float interactionRange = 10f;
    [SerializeField] private float sphereCastRadius = 5f;
    [SerializeField] private LayerMask interactionLayer = ~0;
    [SerializeField] private Transform rayOrigin;
    
    private PlayerInputController _inputController;
    private IInteractable _currentInteractable;
    
    private void Awake()
    {
        _inputController = GetComponent<PlayerInputController>();
        _inputController.OnInteractEvent += HandleInteraction;
    }
    
    private void OnDisable() => _inputController.OnInteractEvent -= HandleInteraction;

    private void Update() => DetectInteractable();
    private void HandleInteraction() => _currentInteractable?.Interact();
    private void DetectInteractable()
    {
        var offsetOrigin = rayOrigin.position - (rayOrigin.forward * sphereCastRadius);
        var adjustedRange = interactionRange * sphereCastRadius;
        
        if (Physics.SphereCast(offsetOrigin, sphereCastRadius, rayOrigin.forward, out var hit, adjustedRange, interactionLayer))
            _currentInteractable = hit.collider.TryGetComponent(out IInteractable interactable) ? interactable : null;
        
        else _currentInteractable = null;
    }
}