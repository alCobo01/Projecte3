using UnityEngine;

public class ScenePortal : MonoBehaviour
{
    [Header("Destino")]
    public string targetScene;
    public string targetSpawnPointId; // Debe coincidir con el SpawnPoint de la escena destino

    private bool _used = false; // Evita doble activación

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"OnTriggerEnter2D: {other.name} | tag: {other.tag} | _used: {_used}");

        if (_used) return;
        if (!other.CompareTag("Player")) return;

        var movement = other.GetComponent<PlayerMovementController>();
        if (movement != null) movement.CanMove = false;

        _used = true;
        Debug.Log($"Yendo a escena: '{targetScene}' spawnId: '{targetSpawnPointId}'");
        SceneTransitionManager.Instance.GoToScene(targetScene, targetSpawnPointId);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}