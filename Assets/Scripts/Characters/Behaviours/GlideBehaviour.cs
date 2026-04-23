using UnityEngine;

public class GlideBehaviour : MonoBehaviour, IWindAffected
{
    public bool IsGliding { get; set; }
    
    [SerializeField] private float glidingGravityScale = 0.5f;
    [SerializeField] private float defaultGravityScale = 3f;
    [SerializeField] private float maxFallSpeed = -2f;

    private Rigidbody2D _rb;

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    private void FixedUpdate()
    {
        if (IsGliding && _rb.linearVelocityY < 0)
        {
            _rb.gravityScale = glidingGravityScale;
            var velY = Mathf.Max(_rb.linearVelocityY, maxFallSpeed);
            _rb.linearVelocity = new Vector2(_rb.linearVelocityX, velY);
        }
        else
            _rb.gravityScale = defaultGravityScale;
    }
    
    public void ApplyWindForce(Vector2 force)
    {
        if (IsGliding) _rb.AddForce(force, ForceMode2D.Force);
    }
}