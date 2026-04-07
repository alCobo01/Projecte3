using UnityEngine;
public class ChaseBehaviour : MonoBehaviour
{
    public float Speed;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Chase(Transform target)
    {
        Vector2 dir = (target.position - transform.position).normalized;
        rb.linearVelocity = dir * Speed;
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
        rb.linearVelocity = Vector2.zero;
    }
}
