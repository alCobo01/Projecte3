using UnityEngine;

public class ChaseBehaviour : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] public GameObject target;
    [SerializeField] private GroundEdgeDetector edgeDetector;
    
    private Rigidbody2D _rb;
    private CharacterAnimationController _animController;
    private ObstacleAvoidance _obstacleAvoidance;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animController = GetComponent<CharacterAnimationController>();
        _obstacleAvoidance = GetComponent<ObstacleAvoidance>();
        if (!edgeDetector) edgeDetector = GetComponent<GroundEdgeDetector>();
    }

    public void Chase(Transform target,float speed)
    {
        Vector2 dir = (target.position - transform.position).normalized;
        var directionX = Mathf.Sign(dir.x);
        _animController.SetWalking(Mathf.Abs(dir.x));

        if (!edgeDetector || edgeDetector.HasGroundAhead(directionX))
        {
            _rb.linearVelocity = new Vector2(directionX * speed, _rb.linearVelocity.y);

            transform.rotation = directionX switch
            {
                // Flip character
                > 0 => Quaternion.Euler(0, 0, 0),
                < 0 => Quaternion.Euler(0, 180, 0),
                _ => transform.rotation
            };
        }
        else
        {
            // Stop if there's no ground immediately ahead
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
        } 
    }
    public void FlyingChase(Transform flyTarget, float speed)
    {
        Vector2 dir = (flyTarget.position - transform.position).normalized;
        
        // Aplicar evitación de obstáculos si el componente existe
        if (_obstacleAvoidance)
            dir = _obstacleAvoidance.GetAvoidanceDirection(dir);

        _animController.SetWalking(Mathf.Abs(dir.x));
        _rb.linearVelocity = dir * speed;

        transform.rotation = dir.x switch
        {
            // Rotación para mirar hacia el objetivo (basada en la dirección final)
            > 0.1f => Quaternion.Euler(0, 0, 0),
            < -0.1f => Quaternion.Euler(0, 180, 0),
            _ => transform.rotation
        };
    }

    public void StopChasing() => _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
}
