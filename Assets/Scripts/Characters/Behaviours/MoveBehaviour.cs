using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MoveBehaviour : MonoBehaviour
{
    private Rigidbody2D _rb;
    [SerializeField] private float maxSpeed = 2f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 25f;

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    public void MoveCharacter(Vector2 direction)
    {
        var targetVelocityX = direction.x * maxSpeed;
        var currentVelocityX = _rb.linearVelocity.x;
        
        var rate = Mathf.Abs(direction.x) > 0.01f ? acceleration : deceleration;
        var newVelocityX = Mathf.MoveTowards(currentVelocityX, targetVelocityX, rate * Time.fixedDeltaTime);

        _rb.linearVelocity = new Vector2(newVelocityX, _rb.linearVelocity.y);
    }
}