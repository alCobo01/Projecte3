using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public AttackDistanceBeahviour shooter;
    [SerializeField] private float lifeTime = 5f;
    private float _timer;

    private void OnEnable()
    {
        _timer = lifeTime;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si choca con el entorno (layer 6) o con el Jugador (puedes ajustar el nombre de la capa)
        if (collision.gameObject.layer == 6 || collision.CompareTag("Player"))
        {
            if (collision.CompareTag("Player"))
            {
                // Aquí podrías llamar a una función de daño en el jugador
                Debug.Log("¡Bala impactó al jugador!");
            }
            
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (gameObject.activeSelf && shooter != null)
        {
            // Desactivar collider antes de devolver al pool para evitar dobles colisiones
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            shooter.Push(gameObject);
        }
    }
}
