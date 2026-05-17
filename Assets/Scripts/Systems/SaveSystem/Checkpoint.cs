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
        }
    }

    private void OnValidate()
    {
        // Helpful for the editor
        if (string.IsNullOrEmpty(sceneName) && gameObject.scene.name != null)
        {
            sceneName = gameObject.scene.name;
        }
    }

    private void Start()
    {
        UpdateVisuals();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // El trigger ahora solo podría servir para mostrar un mensaje de "Pulsar E para interactuar"
        // pero ya no registra ni descubre nada automáticamente.
    }

    private void UpdateVisuals()
    {
        if (activeVisuals != null) activeVisuals.SetActive(_isDiscovered);
        if (inactiveVisuals != null) inactiveVisuals.SetActive(!_isDiscovered);
    }

    public Vector3 GetSpawnPosition() => spawnPoint.position;

    public void Interact()
    {
        if (CheckpointInteractionMenu.Instance != null)
        {
            CheckpointInteractionMenu.Instance.Open(this);
        }
        else
        {
            Debug.LogError("No se encuentra CheckpointInteractionMenu en la escena.");
        }
    }
}
