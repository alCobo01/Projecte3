using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MoveBehaviour : MonoBehaviour
{
    private Rigidbody2D _rb;
    [SerializeField] private float velocityMultiplier = 8f;

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    public void MoveCharacter(Vector2 direction)
    {
        var newVelocityX = direction.normalized.x * velocityMultiplier;
        var currentVelocityY = _rb.linearVelocity.y;

        _rb.linearVelocity = new Vector2(newVelocityX, currentVelocityY);
    }
}