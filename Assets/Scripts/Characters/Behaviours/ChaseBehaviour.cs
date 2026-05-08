using UnityEngine;

public class ChaseBehaviour : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Detection")]
    [SerializeField] public GameObject target;
    [SerializeField] private GroundEdgeDetector edgeDetector;
    private CharacterAnimationController animController;
    private ObstacleAvoidance obstacleAvoidance;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animController = GetComponent<CharacterAnimationController>();
        obstacleAvoidance = GetComponent<ObstacleAvoidance>();
        if (edgeDetector == null) edgeDetector = GetComponent<GroundEdgeDetector>();
    }

    public void Chase(Transform target,float speed)
    {
        Vector2 dir = (target.position - transform.position).normalized;
        float directionX = Mathf.Sign(dir.x);
        animController.SetWalking(Mathf.Abs(dir.x));

        if (edgeDetector == null || edgeDetector.HasGroundAhead(directionX))
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
}
