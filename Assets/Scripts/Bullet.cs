using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public AttackDistanceBeahviour shooter;

    [SerializeField] private Rigidbody2D _rb;

    private void Awake()
    {
        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si choca con el entorno (layer 6)
        if (collision.gameObject.layer == 6)
        {
            // Desactivar collider antes de devolver al pool
            GetComponent<Collider2D>().enabled = false;

            // Devolver al pool
            shooter.Push(gameObject);
        }
    }
}
