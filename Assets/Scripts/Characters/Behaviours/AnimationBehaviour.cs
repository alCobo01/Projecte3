using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationBehaviour : MonoBehaviour
{
    private Animator _animator;

    private void Awake() 
    {
        _animator = GetComponent<Animator>();
        if (!_animator) Debug.LogError("¡ERROR! No hay Animator en " + gameObject.name);
    }

    // Set animator values
    public void Trigger(int triggerNameHash) 
    {
        if (_animator) _animator.SetTrigger(triggerNameHash);
    }
    
    public void SetBool(int parameterNameHash, bool value) 
    {
        if (_animator) _animator.SetBool(parameterNameHash, value);
    }
    
    public void SetFloat(int parameterNameHash, float value) 
    {
        if (_animator) _animator.SetFloat(parameterNameHash, value, 0.1f, Time.deltaTime);
    }

    public void SetFloatImmediate(int parameterNameHash, float value)
    {
        if (_animator) _animator.SetFloat(parameterNameHash, value);
    }
}
