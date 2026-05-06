using Unity.Cinemachine;
using UnityEngine;

public class ProximityShake : MonoBehaviour
{
    [Header("Referencias")] [SerializeField]
    private CinemachineImpulseSource _source;
    [SerializeField] private Transform _player;

    [Header("Distancias")] [SerializeField]
    private float maxDistance = 10f;

    [SerializeField] private float minDistance = 1f;

    [Header("Shake")] [SerializeField] private float maxForce = 1f;
    [SerializeField] private float impulseRate = 0.05f;

    private float _timer;

    private void Update()
    {
        float distance = Vector2.Distance(transform.position, _player.position);
        float t = 1f - Mathf.InverseLerp(minDistance, maxDistance, distance);

        if (t <= 0f) return;

        _timer += Time.deltaTime;

        if (_timer >= impulseRate)
        {
            _timer = 0f;

        }
    }
}