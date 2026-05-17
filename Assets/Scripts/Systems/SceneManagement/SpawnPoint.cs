using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public string spawnPointId; // Ej: "entrada_derecha", "entrada_izquierda"

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
        Gizmos.DrawIcon(transform.position, "sv_label_1"); // icono en el editor
    }
}