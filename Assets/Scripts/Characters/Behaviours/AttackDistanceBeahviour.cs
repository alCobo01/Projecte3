using System.Collections.Generic;
using UnityEngine;

public class AttackDistanceBeahviour : AttackBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float speedProjectile = 5f;
    [SerializeField] private float timeSpawn = 2f;

    private Vector2 _shootDirection;
    private float _nextSpawnTime = 0f;
    private readonly Stack<GameObject> bulletStack = new();

    private void TryShoot(Vector2 direction, float angle)
    {
        if (Time.time < _nextSpawnTime)
            return;

        _shootDirection = direction;

        if (bulletStack.Count == 0)
            InstantiateBullets(angle);
        else
            Pop(angle);

        _nextSpawnTime = Time.time + timeSpawn;
    }

    public void Push(GameObject bullet)
    {
        if (bulletStack.Contains(bullet)) return;
        bulletStack.Push(bullet);
        bullet.SetActive(false);
    }

    public override void Attack(Transform target)
    {
        if (!target || !shootPoint) return;

        Vector2 direction = (target.position - shootPoint.position).normalized;
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        TryShoot(direction, angle);
    }

    private GameObject Pop(float angle)
    {
        var go = bulletStack.Pop();
        go.transform.position = shootPoint.position;
        go.transform.rotation = Quaternion.Euler(0, 0, angle);
        go.SetActive(true);
        
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; // Asegurar que no le afecte la gravedad
            rb.linearVelocity = _shootDirection * speedProjectile;
        }

        var col = go.GetComponent<Collider2D>();
        if (col) col.enabled = true;

        return go;
    }

    private void InstantiateBullets(float angle)
    {
        var bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.Euler(0, 0, angle));
        bullet.GetComponent<Bullet>().shooter = this;
        
        var bulletStarter = bullet.GetComponent<BattleStarter>();
        var ownerStarter = GetComponentInParent<BattleStarter>();
        if (bulletStarter && ownerStarter)
        {
            bulletStarter.SetProjectileOwner(ownerStarter);
        }
        
        if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = _shootDirection * speedProjectile;
        }
    }
}
