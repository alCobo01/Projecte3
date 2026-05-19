using UnityEngine;

public class Checkpoint : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public string checkpointId;
    public string displayName;
    public string sceneName; // Added to handle cross-scene teleportation
    public Transform spawnPoint;

    [Header("Visuals")]
    [SerializeField] private GameObject activeVisuals;
    [SerializeField] private GameObject inactiveVisuals;

    private bool _isDiscovered;
    private bool _isInteracting;

    public bool IsDiscovered
    {
        get => _isDiscovered;
        set
        {
            _isDiscovered = value;
            UpdateVisuals();
        }
    }

    private void Awake()
    {
        if (spawnPoint == null) spawnPoint = transform;
        
        // Auto-assign scene name if empty
        if (string.IsNullOrEmpty(sceneName))
        {
            sceneName = gameObject.scene.name;
            Debug.Log($"[Checkpoint] ID '{checkpointId}' auto-asignado a escena '{sceneName}'");
        }
    }

    private void OnValidate()
    {
        // Helpful for the editor
        if (string.IsNullOrEmpty(sceneName) && !string.IsNullOrEmpty(gameObject.scene.name))
        {
            sceneName = gameObject.scene.name;
        }
    }

    private void Start()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (activeVisuals != null) activeVisuals.SetActive(_isDiscovered);
        if (inactiveVisuals != null) inactiveVisuals.SetActive(!_isDiscovered);
    }

    public Vector3 GetSpawnPosition() => spawnPoint.position;

    public void Interact()
    {
        if (_isInteracting) return;
        
        // Verificamos si los teletransportes están desbloqueados globalmente
        if (CheckpointManager.Instance != null && !CheckpointManager.Instance.TeleportUnlocked)
        {
            Debug.Log("[Checkpoint] Los teletransportes aún no han sido desbloqueados.");
            return;
        }

        _isInteracting = true;

        // MOSTRAMOS el prompt (máscara) al interactuar
        var discovery = GetComponentInChildren<AppearOnDiscovery>();
        if (discovery != null) discovery.SetInteracting(true);

        // Bloqueamos input sin congelar el tiempo
        if (PauseManager.Instance != null) PauseManager.Instance.SetInputLock(true);

        // Marcamos como descubierto inmediatamente para que los visuales reaccionen
        IsDiscovered = true; 
        StartCoroutine(DelayedInteract());
    }

    private System.Collections.IEnumerator DelayedInteract()
    {
        yield return new WaitForSeconds(1.0f);

        if (CheckpointInteractionMenu.Instance != null)
        {
            CheckpointInteractionMenu.Instance.Open(this);
        }
        else
        {
            Debug.LogError("No se encuentra CheckpointInteractionMenu en la escena.");
        }

        // Esperamos a que el menú se cierre
        while (CheckpointInteractionMenu.Instance != null && CheckpointInteractionMenu.Instance.gameObject.activeSelf)
        {
            yield return null;
        }

        // OCULTAMOS el prompt (máscara) al terminar
        var discovery = GetComponentInChildren<AppearOnDiscovery>();
        if (discovery != null) discovery.SetInteracting(false);

        if (PauseManager.Instance != null) PauseManager.Instance.SetInputLock(false);
        
        _isInteracting = false;
    }
}
