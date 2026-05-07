using Unity.Cinemachine;
using UnityEngine;

using UnityEngine;
using Unity.Cinemachine;

public class ProximityShake : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CinemachineCamera vcam;
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
        _noise = vcam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        
        if (_noise == null)
            Debug.LogError("Agrega CinemachineBasicMultiChannelPerlin a la vcam");
    }

    private void Update()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        float t = 1f - Mathf.InverseLerp(minDistance, maxDistance, distance);

        float targetAmplitude = Mathf.Lerp(0f, maxAmplitude, t);
        // Frecuencia también sube con la proximidad (más frenético)
        float targetFrequency  = Mathf.Lerp(0.5f, maxFrequency, t);

        // Suavizado para que no corte de golpe
        _currentAmplitude = Mathf.Lerp(_currentAmplitude, targetAmplitude, Time.deltaTime * smoothSpeed);

        _noise.AmplitudeGain  = _currentAmplitude;
        _noise.FrequencyGain  = _currentAmplitude > 0.01f ? targetFrequency : 0f;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
        DrawCircle(transform.position, maxDistance, 40);
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        DrawCircle(transform.position, minDistance, 40);
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float step = 360f / segments;
        Vector3 prev = center + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float rad = i * step * Mathf.Deg2Rad;
            Vector3 next = center + new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}