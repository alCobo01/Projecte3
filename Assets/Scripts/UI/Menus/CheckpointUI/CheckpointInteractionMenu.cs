using UnityEngine;
using UnityEngine.UI;

public class CheckpointInteractionMenu : BaseMenu
{
    public static CheckpointInteractionMenu Instance { get; private set; }
    
    [SerializeField] private Button teleportButton;
    private Checkpoint _currentCheckpoint;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        gameObject.SetActive(false);
    }
    
    public void Open(Checkpoint checkpoint)
    {
        _currentCheckpoint = checkpoint;
        
        if (teleportButton != null)
        {
            teleportButton.interactable = CheckpointManager.Instance.discoveredIds.Count > 0;
        }
        
        Open();
    }

    public void OnSaveClicked()
    {
        if (_currentCheckpoint != null)
        {
            CheckpointManager.Instance.RegisterDiscovery(_currentCheckpoint);
        }
        
        CheckpointManager.Instance.SaveGame();
        Debug.Log("Game Saved via Checkpoint!");
        Close();
    }
    
    public void OnLoadClicked()
    {
        CheckpointManager.Instance.LoadGame();
        Close();
    }

    public void OnTeleportClicked()
    {
        Close(); 
        CheckpointMenu.Instance.OpenMenu(); 
    }

    public void OnCancelClicked()
    {
        Close();
    }
}
