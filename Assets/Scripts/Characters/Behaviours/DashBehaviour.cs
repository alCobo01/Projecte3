using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DashBehaviour : MonoBehaviour
{
    public bool IsDashing { get; private set; }
    public event UnityAction OnDashEvent;
    public event UnityAction OnDashFinishedEvent;
    
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;

    private Rigidbody2D _rb;
    
    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    public void ExecuteDash(Vector2 direction)
    {
        if (IsDashing) return;
        StartCoroutine(DashCoroutine(direction));
    }
    
    private IEnumerator DashCoroutine(Vector2 direction)
    {
        IsDashing = true;
        OnDashEvent?.Invoke();
        
        var originalGravity = _rb.gravityScale;
        _rb.gravityScale = 0f;
        
        _rb.linearVelocity = direction.normalized * dashSpeed;
        
        yield return new WaitForSeconds(dashDuration);
        
        _rb.gravityScale = originalGravity;
        _rb.linearVelocity = Vector2.zero;
        
        IsDashing = false;
        OnDashFinishedEvent?.Invoke();
    }
}
