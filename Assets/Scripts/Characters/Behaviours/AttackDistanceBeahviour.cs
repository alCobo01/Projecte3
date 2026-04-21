using System.Collections.Generic;
using UnityEngine;

public class AttackDistanceBeahviour : AttackBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private GameObject _gameObjecjtBullet;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private float speedProjectile = 5f;
    [SerializeField] private float timeSpawn = 2f;

    private float _bulletdirection;
    private float _nextSpawnTime = 0f;

    public Stack<GameObject> BulletStack = new Stack<GameObject>();

    private void Awake()
    {
       
    }

    private void TryShoot(float angle)
    {
        if (Time.time < _nextSpawnTime)
            return;

        if (BulletStack.Count == 0)
        {
            InstantiateBullets(angle);
        }
        else
        {
            Pop();
        }

        _nextSpawnTime = Time.time + timeSpawn;
    }

    public void Push(GameObject bullet)
    {
        BulletStack.Push(bullet);
        bullet.SetActive(false);
    }
    public override void Attack(Transform target)
    {
        if (target == null)
            return;
        _bulletdirection = target.transform.position.x - transform.position.x;
        float angle = Mathf.Atan2(target.transform.position.y - transform.position.y, target.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
        TryShoot(angle);
    }
    public GameObject Pop()
    {
        GameObject go = BulletStack.Pop();
        go.SetActive(true);
        go.GetComponent<Collider2D>().enabled = true;
        go.transform.position = _shootPoint.position;
        go.GetComponent<Rigidbody2D>().linearVelocityX = speedProjectile * _bulletdirection;
        return go;
    }

    public void InstantiateBullets(float angle)
    {
        GameObject bullet = Instantiate(_gameObjecjtBullet, _shootPoint.position, Quaternion.Euler(0, 0, angle));
        bullet.GetComponent<Bullet>().shooter = this;
        bullet.GetComponent<Rigidbody2D>().linearVelocityX = speedProjectile * _bulletdirection;
    }
}
