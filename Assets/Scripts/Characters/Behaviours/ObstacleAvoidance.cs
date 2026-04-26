using UnityEngine;

public class ObstacleAvoidance : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float detectionRange = 1.5f;
    [SerializeField] private float avoidForce = 1.0f;
    [SerializeField] private int sensorCount = 5;
    [SerializeField] private float sensorAngle = 45f;

    public Vector2 GetAvoidanceDirection(Vector2 currentDirection)
    {
        if (currentDirection == Vector2.zero) return Vector2.zero;

        Vector2 resultDirection = currentDirection;
        bool obstacleDetected = false;

        // Lanzar rayos en abanico
        for (int i = 0; i < sensorCount; i++)
        {
            // Calcular el ángulo del sensor actual respecto a la dirección de movimiento
            float angleOffset = (i - (sensorCount - 1) / 2f) * (sensorAngle / (sensorCount - 1));
            float currentAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
            float finalAngle = (currentAngle + angleOffset) * Mathf.Deg2Rad;

            Vector2 sensorDir = new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle));
            
            RaycastHit2D hit = Physics2D.Raycast(transform.position, sensorDir, detectionRange, obstacleLayer);

            if (hit.collider != null)
            {
                obstacleDetected = true;
                // Cuanto más cerca esté el obstáculo, más fuerte es la repulsión
                float distanceWeight = 1f - (hit.distance / detectionRange);
                
                // La fuerza de repulsión es perpendicular al obstáculo o contraria al rayo
                Vector2 repulsion = (Vector2)transform.position - hit.point;
                resultDirection += repulsion.normalized * avoidForce * distanceWeight;
                
                Debug.DrawLine(transform.position, hit.point, Color.red);
            }
            else
            {
                Debug.DrawRay(transform.position, sensorDir * detectionRange, Color.green);
            }
        }

        return obstacleDetected ? resultDirection.normalized : currentDirection;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar los sensores en el editor
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
