using UnityEngine;

public class GroundEdgeDetector : MonoBehaviour
{
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float edgeCheckDistance = 0.5f;
    [SerializeField] private float groundDetectionDistance = 0.5f;

    public bool HasGroundAhead(float directionX)
    {
        var checkOrigin = (Vector2)transform.position + new Vector2(directionX * edgeCheckDistance, 0);
        var hit = Physics2D.Raycast(checkOrigin, Vector2.down, groundDetectionDistance, groundLayer);
        return hit.collider;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        
        // Detectar dirección basada en la rotación Y (0 es derecha, 180 es izquierda)
        var directionX = (Mathf.Abs(transform.eulerAngles.y - 180f) < 0.1f) ? -1f : 1f;
        var checkOrigin = (Vector2)transform.position + new Vector2(directionX * edgeCheckDistance, 0);
        Gizmos.DrawLine(checkOrigin, checkOrigin + Vector2.down * groundDetectionDistance);
        Gizmos.DrawWireSphere(checkOrigin + Vector2.down * groundDetectionDistance, 0.05f);
    }
}
