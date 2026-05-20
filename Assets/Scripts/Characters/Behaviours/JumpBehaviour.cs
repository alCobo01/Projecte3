using UnityEngine;
using UnityEngine.Events;

public class JumpBehaviour : MonoBehaviour
{
    public event UnityAction OnJumpEvent;
    
    [SerializeField] private float jumpForce = 2f;
    [SerializeField] private float holdJumpForce = 6f;
    [SerializeField] private float maxHoldTime = 0.2f;
    [Range(0f, 1f)] [SerializeField] private float cutJumpModifier = 0.5f;
    
    private Rigidbody2D _rb;
    private float _holdTimeRemaining;
    
    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    public void Jump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        _holdTimeRemaining = maxHoldTime;
        OnJumpEvent?.Invoke();
    }

    public void HoldJump(float deltaTime)
    {
        if (_holdTimeRemaining <= 0f) return;
        if (_rb.linearVelocity.y <= 0f)
        {
            _holdTimeRemaining = 0f;
            return;
        }

        _rb.AddForce(Vector2.up * (holdJumpForce * deltaTime), ForceMode2D.Force);
        _holdTimeRemaining -= deltaTime;
    }
    
    public void CancelJump()
    {
        if (_rb.linearVelocity.y > 0)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * cutJumpModifier);
        }
        _holdTimeRemaining = 0f;
    }
}
