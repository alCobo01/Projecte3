using System;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool IsGrounded { get; private set; }
    
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float rayLength = 2f;

    private void Update()
    {
        var hit = Physics2D.Raycast(transform.position, -transform.up, rayLength, groundLayer);
        IsGrounded = hit;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (-transform.up * rayLength));
    }
}