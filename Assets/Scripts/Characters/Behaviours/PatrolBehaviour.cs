using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PatrolBehaviour : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float speed = 2f;
    
    [Header("Wait Settings")]
    [SerializeField] private float minWaitTime = 1f;
    [SerializeField] private float maxWaitTime = 3f;

    private int _currentPointIndex = 0;
    private Rigidbody2D _rb;
    private float _waitTimer = 0f;
    private bool _isWaiting = false;
    private CharacterAnimationController _animController;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animController = GetComponent<CharacterAnimationController>();
    }

    public void Patrol()
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
