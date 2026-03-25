using UnityEngine;
using UnityEngine.Events;

public class JumpBehaviour : MonoBehaviour
{
    public event UnityAction OnJumpEvent;
    
    [SerializeField] private float jumpForce = 2f;
    private Rigidbody2D _rb;
    
    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    public void Jump()
    {
        _rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
        OnJumpEvent?.Invoke();
    } 
    
}