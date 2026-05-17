using System;
using UnityEngine;

[RequireComponent(typeof(GlideBehaviour))]
public class PlayerGlideController : MonoBehaviour
{
    private static readonly int IsGlidingHash = Animator.StringToHash("IsGliding"); 
    
    public bool CanGlide { get; set; }
    
    private PlayerInputController _inputController;
    private CharacterAnimationController _animController;
    private GlideBehaviour _glideBehaviour;
    private GroundCheck _groundCheck;
    private bool _isGlideButtonHeld;

    private void Awake()
    {
        _inputController = GetComponent<PlayerInputController>();
        _animController = GetComponent<CharacterAnimationController>();
        _glideBehaviour = GetComponent<GlideBehaviour>();
        _groundCheck = GetComponent<GroundCheck>();
    }

    private void Start()
    {
        _inputController.OnFlyEvent += HandleGlide;
        CanGlide = true;
    }
    
    private void OnDisable() => _inputController.OnFlyEvent -= HandleGlide;

    private void Update()
    {
        if (_groundCheck.IsGrounded)
        {
            SetGlideState(false);
            return;
        }

        SetGlideState(CanGlide && _isGlideButtonHeld);
    }

    private void HandleGlide(bool isButtonPressed)
    {
        _isGlideButtonHeld = isButtonPressed;
        if (_groundCheck.IsGrounded)
        {
            SetGlideState(false);
            return;
        }

        SetGlideState(CanGlide && _isGlideButtonHeld);
    }

    private void SetGlideState(bool isGliding)
    {
        _animController.SetBool(isGliding, IsGlidingHash);
        _glideBehaviour.IsGliding = isGliding;
    }
}
