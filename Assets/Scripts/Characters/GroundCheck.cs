using System;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool IsGrounded { get; private set; }
    public bool IsNearGround { get; private set; }
    
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float rayLength = 0.2f;
    [SerializeField] private float nearGroundThreshold = 2.25f;

    private void Update()
    {
        var origin = transform.position;
        var direction = -transform.up;

        // Check if actually touching the ground
        var hitGround = Physics2D.Raycast(origin, direction, rayLength, groundLayer);
        IsGrounded = hitGround.collider;

        // Check if about to land (near the ground)
        var hitNear = Physics2D.Raycast(origin, direction, nearGroundThreshold, groundLayer);
        IsNearGround = hitNear.collider;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (-transform.up * rayLength));
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + (-transform.up * nearGroundThreshold));
    }
}
