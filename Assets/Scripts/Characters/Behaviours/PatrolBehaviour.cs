using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PatrolBehaviour : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    
    private int _currentPointIndex = 0;
    private Rigidbody2D _rb;
    private float _waitTimer = 0f;
    private bool _isWaiting = false;
    private CharacterAnimationController _animController;
    private ObstacleAvoidance _obstacleAvoidance;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animController = GetComponent<CharacterAnimationController>();
        _obstacleAvoidance = GetComponent<ObstacleAvoidance>();
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
            if (_waitTimer <= 0)
            {
                _isWaiting = false;
                _currentPointIndex = (_currentPointIndex + 1) % patrolPoints.Length;
            }
            return;
        }

        Transform targetPoint = patrolPoints[_currentPointIndex];
        if (targetPoint == null) return;

        Vector2 targetPosition = targetPoint.position;
        Vector2 currentPosition = transform.position;

        // Check if we reached the point (using a small threshold)
        if (Vector2.Distance(new Vector2(currentPosition.x, 0), new Vector2(targetPosition.x, 0)) < 0.2f)
        {
            _isWaiting = true;
            _waitTimer = Random.Range(minWaitTime, maxWaitTime);
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
        }
        else
        {
            float directionX = Mathf.Sign(targetPosition.x - currentPosition.x);
            _animController.SetWalking(Mathf.Abs(directionX));

            _rb.linearVelocity = new Vector2(directionX * speed, _rb.linearVelocity.y);
            
            // Update rotation/flip based on direction
            if (directionX > 0) transform.rotation = Quaternion.Euler(0, 0, 0);
            else if (directionX < 0) transform.rotation = Quaternion.Euler(0, 180, 0);
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

            if (_waitTimer <= 0)
            {
                _isWaiting = false;
                _currentPointIndex = (_currentPointIndex + 1) % patrolPoints.Length;
            }
            return;
        }

        Transform targetPoint = patrolPoints[_currentPointIndex];
        if (targetPoint == null) return;

        Vector2 targetPos = targetPoint.position;
        Vector2 currentPos = transform.position;

        float distance = Vector2.Distance(currentPos, targetPos);

        // Si llegó al punto → esperar
        if (distance < 0.2f)
        {
            _isWaiting = true;
            _waitTimer = Random.Range(minWaitTime, maxWaitTime);
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        // Dirección en 2D
        Vector2 dir = (targetPos - currentPos).normalized;

        // Aplicar evitación de obstáculos si el componente existe
        if (_obstacleAvoidance != null)
        {
            dir = _obstacleAvoidance.GetAvoidanceDirection(dir);
        }

        // Movimiento volador
        _rb.linearVelocity = dir * speed;

        // Animación
        _animController.SetWalking(1f);

        // Rotación opcional (solo horizontal)
        if (dir.x > 0.1f) transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (dir.x < -0.1f) transform.rotation = Quaternion.Euler(0, 180, 0);
    }

    public void StopPatrol()
    {
        _isWaiting = false;
        if (_rb != null)
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] != null)
            {
                Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);
                
                // Draw line to next point
                int nextIndex = (i + 1) % patrolPoints.Length;
                if (patrolPoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
                }
            }
        }
    }
}
