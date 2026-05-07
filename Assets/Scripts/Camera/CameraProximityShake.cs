using Unity.Cinemachine;
using UnityEngine;

public class ProximityShake : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private NoiseSettings noiseProfile;
    [SerializeField] private Transform player;

    [Header("Distancias")]
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float minDistance = 1f;

    [Header("Shake")]
    [SerializeField] private float maxAmplitude = 2f;
    [SerializeField] private float maxFrequency = 3f;
    [SerializeField] private float smoothSpeed = 4f;

    private CinemachineBasicMultiChannelPerlin _noise;
    private float _currentAmplitude;

    private void Awake()
    {
        _noise = vcam.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();
        _noise.NoiseProfile = noiseProfile;
        _noise.AmplitudeGain = 0f;
        _noise.FrequencyGain = 0f;  
    } 
    
    private void Update()
    {
        var distance = Vector2.Distance(transform.position, player.position);
        var t = 1f - Mathf.InverseLerp(minDistance, maxDistance, distance);

        var targetAmplitude = Mathf.Lerp(0f, maxAmplitude, t);
        var targetFrequency  = Mathf.Lerp(0.5f, maxFrequency, t);
        
        _currentAmplitude = Mathf.Lerp(_currentAmplitude, targetAmplitude, Time.deltaTime * smoothSpeed);

        _noise.AmplitudeGain  = _currentAmplitude;
        _noise.FrequencyGain  = _currentAmplitude > 0.01f ? targetFrequency : 0f;
    }

    private void OnDestroy() => Destroy(_noise);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
        DrawCircle(transform.position, maxDistance, 40);
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        DrawCircle(transform.position, minDistance, 40);
    }

    private static void DrawCircle(Vector3 center, float radius, int segments)
    {
        var step = 360f / segments;
        var prev = center + new Vector3(radius, 0f, 0f);
        for (var i = 1; i <= segments; i++)
        {
            var rad = i * step * Mathf.Deg2Rad;
            var next = center + new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}