using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraShake : MonoBehaviour
{
    [SerializeField] [Range(0f, 2f)] private float shakeForce;
    private CinemachineImpulseSource _source;

    private void Awake() => _source = GetComponent<CinemachineImpulseSource>();
    public void TriggerShake() => _source.GenerateImpulse(shakeForce);
}