using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public AttackDistanceBeahviour shooter;
    [SerializeField] private float lifeTime = 5f;
    private float _timer;

    private void OnEnable() => _timer = lifeTime;

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0)
            ReturnToPool();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != 6 && !collision.CompareTag("Player")) return;
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (!gameObject.activeSelf || !shooter) return;
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;
        shooter.Push(gameObject);
    }
}
