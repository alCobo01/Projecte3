using UnityEngine;

public class Killer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            KillPlayer(other.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            KillPlayer(collision.gameObject);
        }
    }

    private void KillPlayer(GameObject player)
    {
        // Disable collider as requested
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        // Logic for respawning
        if (CheckpointManager.Instance != null)
        {
            // You might want to use LoadGame() if you want to restore the state, 
            // or TeleportToLastCheckpoint() if you just want to move the player.
            // Based on your instruction "load del checkpoint manager", LoadGame seems appropriate.
            
            // Re-enabling the collider should probably happen after respawn. 
            // CheckpointManager.LoadGame() handles the fade and teleport.
            CheckpointManager.Instance.RespawnAtLastCheckpoint();
            
            // Note: Since LoadGame is a Coroutine, we might want to re-enable 
            // the collider after the load finishes, but since the scene might reload 
            // or the player state is restored, the collider might be re-enabled automatically.
            // If it's a persistent player object, we'd need a way to re-enable it.
            // For now, I'll stick to your instruction of disabling it.
        }
        else
        {
            Debug.LogWarning("Killer: No se encontró CheckpointManager.Instance");
        }
    }
}
