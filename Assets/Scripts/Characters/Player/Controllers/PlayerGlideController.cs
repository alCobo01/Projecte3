using System;
using UnityEngine;

[RequireComponent(typeof(GlideBehaviour))]
public class PlayerGlideController : MonoBehaviour
{
    public bool CanGlide { get; set; }
    
    private PlayerInputController _inputController;
    private GlideBehaviour glideBehaviour;
    private GroundCheck _groundCheck;

    private void Awake()
    {
        glideBehaviour = GetComponent<GlideBehaviour>();
        _groundCheck = GetComponent<GroundCheck>();
    }

    private void Start()
    {
        _inputController.OnFlyEvent += HandleGlide;
        CanGlide = true;
    }
    
    private void OnDisable() => _inputController.OnFlyEvent -= HandleGlide;

    private void HandleGlide(bool isGliding)
    {
        glideBehaviour.IsGliding = !_groundCheck.IsGrounded;
    }
}