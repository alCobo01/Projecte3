using UnityEngine;

public class ChaseBehaviour : MonoBehaviour
{
    public float Speed;
    private Rigidbody2D rb;

    [Header("Ground Detection")]
    [SerializeField] private GroundCheck groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float edgeCheckDistance = 0.5f;
    [SerializeField] private float groundDetectionDistance = 0.5f;
    private CharacterAnimationController animController;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animController = GetComponent<CharacterAnimationController>();
        if (groundCheck == null) groundCheck = GetComponentInChildren<GroundCheck>();
    }

    public void Chase(Transform target)
    {
        Vector2 dir = (target.position - transform.position).normalized;
        float directionX = Mathf.Sign(dir.x);
        animController.SetWalking(Mathf.Abs(dir.x));
        // Check for ground ahead with a much shorter ray
        Vector2 checkOrigin = (Vector2)transform.position + new Vector2(directionX * edgeCheckDistance, 0);
        RaycastHit2D hit = Physics2D.Raycast(checkOrigin, Vector2.down, groundDetectionDistance, groundLayer);

        if (hit.collider != null)
        {
            rb.linearVelocity = new Vector2(directionX * Speed, rb.linearVelocity.y);
            // Debug.Log("CHASE: Moviéndose. Velocidad: " + (directionX * Speed));
            
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

    public void Run(Transform target, Transform self)
    {
        Vector2 dir = (self.position - target.position).normalized;
        Vector2 escapePoint = (Vector2)self.position + dir * 10f;

        Vector2 runDir = (escapePoint - (Vector2)self.position).normalized;
        rb.linearVelocity = runDir * Speed;
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
