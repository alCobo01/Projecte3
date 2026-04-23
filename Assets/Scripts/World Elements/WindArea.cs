using UnityEngine;

public class WindArea : MonoBehaviour
{
    [SerializeField] private Vector2 windDirection = Vector2.up;
    [SerializeField] private float windStrength = 25f;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IWindAffected>(out var affectedObject))
            affectedObject.ApplyWindForce(windDirection * windStrength);
    }
}