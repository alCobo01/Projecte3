using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PatrolBehaviour : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private GroundEdgeDetector edgeDetector;
    
    private int _currentPointIndex;
    private Rigidbody2D _rb;
    private float _waitTimer;
    private bool _isWaiting;
    private CharacterAnimationController _animController;
    private ObstacleAvoidance _obstacleAvoidance;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animController = GetComponent<CharacterAnimationController>();
        _obstacleAvoidance = GetComponent<ObstacleAvoidance>();
        if (!edgeDetector) edgeDetector = GetComponent<GroundEdgeDetector>();
    }

    public void Patrol(float speed, float minWaitTime, float maxWaitTime)
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("No patrol points assigned to PatrolBehaviour on " + gameObject.name);
            return;
        }

        if (_isWaiting)
        {
            _waitTimer -= Time.deltaTime;
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
            _animController.SetWalking(0f);
            if (!(_waitTimer <= 0)) return;
            _isWaiting = false;
            _currentPointIndex = (_currentPointIndex + 1) % patrolPoints.Length;
            return;
        }

        var targetPoint = patrolPoints[_currentPointIndex];
        if (!targetPoint) return;

        Vector2 targetPosition = targetPoint.position;
        Vector2 currentPosition = transform.position;

        // Check if we reached the point (using a small threshold)
        if (Vector2.Distance(new Vector2(currentPosition.x, 0), new Vector2(targetPosition.x, 0)) < 0.2f)
        {
            _isWaiting = true;
            _waitTimer = Random.Range(minWaitTime, maxWaitTime);
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
            _animController.SetWalking(0f);
        }
        else
        {
            var directionX = Mathf.Sign(targetPosition.x - currentPosition.x);
            
            if (!edgeDetector || edgeDetector.HasGroundAhead(directionX))
            {
                _animController.SetWalking(Mathf.Abs(directionX));
                _rb.linearVelocity = new Vector2(directionX * speed, _rb.linearVelocity.y);

                transform.rotation = directionX switch
                {
                    // Update rotation/flip based on direction
                    > 0 => Quaternion.Euler(0, 0, 0),
                    < 0 => Quaternion.Euler(0, 180, 0),
                    _ => transform.rotation
                };
            }
            else
            {
                // No hay suelo adelante, actuar como si hubiera llegado al punto
                _isWaiting = true;
                _waitTimer = Random.Range(minWaitTime, maxWaitTime);
                _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                _animController.SetWalking(0f);
            }
        }
    }
    
    public void FlyingPatrol(float speed, float minWaitTime, float maxWaitTime)
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("No patrol points assigned to PatrolBehaviour on " + gameObject.name);
            return;
        }

        if (_isWaiting)
        {
            _waitTimer -= Time.deltaTime;
            _rb.linearVelocity = Vector2.zero;
            _animController.SetWalking(0f);

            if (!(_waitTimer <= 0)) return;
            _isWaiting = false;
            _currentPointIndex = (_currentPointIndex + 1) % patrolPoints.Length;
            return;
        }

        var targetPoint = patrolPoints[_currentPointIndex];
        if (!targetPoint) return;

        Vector2 targetPos = targetPoint.position;
        Vector2 currentPos = transform.position;

        var distance = Vector2.Distance(currentPos, targetPos);

        // Si llegó al punto → esperar
        if (distance < 0.2f)
        {
            _isWaiting = true;
            _waitTimer = Random.Range(minWaitTime, maxWaitTime);
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        // Dirección en 2D
        var dir = (targetPos - currentPos).normalized;

        // Aplicar evitación de obstáculos si el componente existe
        if (_obstacleAvoidance)
        {
            dir = _obstacleAvoidance.GetAvoidanceDirection(dir);
        }

        // Movimiento volador
        _rb.linearVelocity = dir * speed;

        // Animación
        _animController.SetWalking(1f);
        transform.rotation = dir.x switch
        {
            // Rotación opcional (solo horizontal)
            > 0.1f => Quaternion.Euler(0, 0, 0),
            < -0.1f => Quaternion.Euler(0, 180, 0),
            _ => transform.rotation
        };
    }

    public void StopPatrol()
    {
        _isWaiting = false;
        _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
        
    }

    private void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Gizmos.color = Color.yellow;
        for (var i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;
            Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);
                
            // Draw line to next point
            var nextIndex = (i + 1) % patrolPoints.Length;
            if (patrolPoints[nextIndex] != null)
            {
                Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
            }
        }
    }
}
