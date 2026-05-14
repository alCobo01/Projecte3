using UnityEngine;

public class CharacterFlipBehaviour : MonoBehaviour
{
    public bool IsFacingRight { get; private set; }

    private void Awake()
    {
        // Initialize based on the current localScale
        // If it starts with negative scale, it's facing right
        IsFacingRight = transform.localScale.x < 0;
    }

    public void SetOrientation(bool faceRight)
    {
        if (IsFacingRight == faceRight) return;
        Flip();
    }

    public void ForceOrientation(bool faceRight)
    {
        IsFacingRight = faceRight;
        var localScale = transform.localScale;
        // Inverting: faceRight = true -> negative scale, faceRight = false -> positive scale
        localScale.x = faceRight ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
        transform.localScale = localScale;
    }

    public void Flip()
    {
        IsFacingRight = !IsFacingRight;
        var localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }
}
