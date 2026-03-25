using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationBehaviour : MonoBehaviour
{
    private Animator _animator;

    private void Awake() => _animator = GetComponent<Animator>();

    // Set animator values
    public void Trigger(int triggerNameHash) => _animator.SetTrigger(triggerNameHash);
    public void SetBool(int parameterNameHash, bool value) => _animator.SetBool(parameterNameHash, value);
    public void SetFloat(int parameterNameHash, float value) => _animator.SetFloat(parameterNameHash, value, 0.1f, Time.deltaTime);
}