using System.Collections.Generic;
using UnityEngine;

public class AttackDistanceBeahviour : AttackBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private GameObject _gameObjecjtBullet;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private float speedProjectile = 5f;
    [SerializeField] private float timeSpawn = 2f;

    private Vector2 _shootDirection;
    private float _nextSpawnTime = 0f;

    public Stack<GameObject> BulletStack = new Stack<GameObject>();

    private void TryShoot(Vector2 direction, float angle)
    {
        if (Time.time < _nextSpawnTime)
            return;

        _shootDirection = direction;

        if (BulletStack.Count == 0)
        {
            InstantiateBullets(angle);
        }
        else
        {
            Pop(angle);
        }

        _nextSpawnTime = Time.time + timeSpawn;
    }

    public void Push(GameObject bullet)
    {
        if (BulletStack.Contains(bullet)) return;
        BulletStack.Push(bullet);
        bullet.SetActive(false);
    }

    public override void Attack(Transform target)
    {
        if (target == null || _shootPoint == null)
            return;

        Vector2 direction = (target.position - _shootPoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        TryShoot(direction, angle);
    }

    public GameObject Pop(float angle)
    {
        GameObject go = BulletStack.Pop();
        go.transform.position = _shootPoint.position;
        go.transform.rotation = Quaternion.Euler(0, 0, angle);
        go.SetActive(true);
        
        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; // Asegurar que no le afecte la gravedad
            rb.linearVelocity = _shootDirection * speedProjectile;
        }

        Collider2D col = go.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        return go;
    }

    public void InstantiateBullets(float angle)
    {
        GameObject bullet = Instantiate(_gameObjecjtBullet, _shootPoint.position, Quaternion.Euler(0, 0, angle));
        bullet.GetComponent<Bullet>().shooter = this;
        
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = _shootDirection * speedProjectile;
        }
    }
}
