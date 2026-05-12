using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Settings")]
    public string checkpointId;
    public string displayName;
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
    }

    private void Start()
    {
        UpdateVisuals();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CheckpointManager.Instance.RegisterAndSave(this);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            CheckpointMenu.Instance.OpenMenu();
        }
    }

    private void UpdateVisuals()
    {
        if (activeVisuals != null) activeVisuals.SetActive(_isDiscovered);
        if (inactiveVisuals != null) inactiveVisuals.SetActive(!_isDiscovered);
    }

    public Vector3 GetSpawnPosition() => spawnPoint.position;
}
