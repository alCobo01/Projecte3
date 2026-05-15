using UnityEngine;

public class GlideBehaviour : MonoBehaviour, IWindAffected
{
    public bool IsGliding { get; set; }
    
    [SerializeField] private float glidingGravityScale = 0.5f;
    [SerializeField] private float defaultGravityScale = 1f;
    [SerializeField] private float maxFallSpeed = -2f;
    [SerializeField] private float maxRiseSpeed = 4f;

    private Rigidbody2D _rb;
    private bool _inWind;

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    private void FixedUpdate()
    {
        if (IsGliding && (_rb.linearVelocityY <= 0f || _inWind))
        {
            _rb.gravityScale = glidingGravityScale;
            
            // Limit both falling and rising speeds while gliding
            var velY = Mathf.Clamp(_rb.linearVelocityY, maxFallSpeed, _inWind ? maxRiseSpeed : _rb.linearVelocityY);
            _rb.linearVelocity = new Vector2(_rb.linearVelocityX, velY);
        }
        else
        {
            _rb.gravityScale = defaultGravityScale;
        }
        
        _inWind = false; 
    }
    
    public void ApplyWindForce(Vector2 force)
    {
        if (!IsGliding) return;
        _inWind = true;
        _rb.AddForce(force, ForceMode2D.Force);
    }
}