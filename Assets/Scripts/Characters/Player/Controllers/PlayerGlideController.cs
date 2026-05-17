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

    private void HandleGlide(bool isButtonPressed)
    {
        _animController.SetBool(isButtonPressed, IsGlidingHash);
        _glideBehaviour.IsGliding = isButtonPressed && !_groundCheck.IsGrounded;
    }
}