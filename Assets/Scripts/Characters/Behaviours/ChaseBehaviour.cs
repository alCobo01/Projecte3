using UnityEngine;

public class ChaseBehaviour : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Ground Detection")]
    [SerializeField] public GameObject target;
    [SerializeField] private GroundCheck groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float edgeCheckDistance = 0.5f;
    [SerializeField] private float groundDetectionDistance = 0.5f;
    private CharacterAnimationController animController;
    private ObstacleAvoidance obstacleAvoidance;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animController = GetComponent<CharacterAnimationController>();
        obstacleAvoidance = GetComponent<ObstacleAvoidance>();
        if (groundCheck == null) groundCheck = GetComponentInChildren<GroundCheck>();
    }

    public void Chase(Transform target,float speed)
    {
        Vector2 dir = (target.position - transform.position).normalized;
        float directionX = Mathf.Sign(dir.x);
        animController.SetWalking(Mathf.Abs(dir.x));

        // Check for ground ahead with a much shorter ray
        Vector2 checkOrigin = (Vector2)transform.position + new Vector2(directionX * edgeCheckDistance, 0);
        RaycastHit2D hit = Physics2D.Raycast(checkOrigin, Vector2.down, groundDetectionDistance, groundLayer);

        if (hit.collider != null)
        {
            rb.linearVelocity = new Vector2(directionX * speed, rb.linearVelocity.y);
            
            // Flip character
            if (directionX > 0) transform.rotation = Quaternion.Euler(0, 0, 0);
            else if (directionX < 0) transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            // Stop if there's no ground immediately ahead
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        } 
    }
    public void FlyingChase(Transform target, float speed)
    {
        Vector2 dir = (target.position - transform.position).normalized;
        
        // Aplicar evitación de obstáculos si el componente existe
        if (obstacleAvoidance != null)
        {
            dir = obstacleAvoidance.GetAvoidanceDirection(dir);
        }

        animController.SetWalking(Mathf.Abs(dir.x));
        rb.linearVelocity = dir * speed;

        // Rotación para mirar hacia el objetivo (basada en la dirección final)
        if (dir.x > 0.1f) transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (dir.x < -0.1f) transform.rotation = Quaternion.Euler(0, 180, 0);
    }

    public void StopChasing()
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // Detectar dirección basada en la rotación Y (0 es derecha, 180 es izquierda)
        float directionX = (Mathf.Abs(transform.eulerAngles.y - 180f) < 0.1f) ? -1f : 1f;
        
        Vector2 checkOrigin = (Vector2)transform.position + new Vector2(directionX * edgeCheckDistance, 0);
        // Dibujar la línea con la distancia exacta de detección
        Gizmos.DrawLine(checkOrigin, checkOrigin + Vector2.down * groundDetectionDistance);
        
        // Dibujar una pequeña esfera al final para marcar el límite
        Gizmos.DrawWireSphere(checkOrigin + Vector2.down * groundDetectionDistance, 0.05f);
    }
}
