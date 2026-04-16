using System.Collections.Generic;
using UnityEngine;

public class AttackDistanceBeahviour : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private GameObject _gameObjecjtBullet;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private float speedProjectile = 5f;
    [SerializeField] private float timeSpawn = 2f;

    [Header("Behaviour Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float attackDistance = 5f;

    private float _bulletdirection;
    private float _nextSpawnTime = 0f;

    public Stack<GameObject> BulletStack = new Stack<GameObject>();

    private void Awake()
    {
        _bulletdirection = -transform.localScale.x;
    }

    private void Update()
    {
        if (target == null)
            return;

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance <= attackDistance)
        {
            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (Time.time < _nextSpawnTime)
            return;

        if (BulletStack.Count == 0)
        {
            InstantiateBullets();
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

    public GameObject Pop()
    {
        GameObject go = BulletStack.Pop();
        go.SetActive(true);
        go.GetComponent<Collider2D>().enabled = true;
        go.transform.position = _shootPoint.position;
        go.GetComponent<Rigidbody2D>().linearVelocityX = speedProjectile * _bulletdirection;
        return go;
    }

    public void InstantiateBullets()
    {
        GameObject bullet = Instantiate(_gameObjecjtBullet, _shootPoint.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().shooter = this;
        bullet.GetComponent<Rigidbody2D>().linearVelocityX = speedProjectile * _bulletdirection;
    }
}
