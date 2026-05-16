using UnityEngine;
using UnityEngine.UI;

public class CheckpointInteractionMenu : BaseMenu
{
    public static CheckpointInteractionMenu Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        gameObject.SetActive(false);
    }

    private Checkpoint _currentCheckpoint;

    public void Open(Checkpoint checkpoint)
    {
        _currentCheckpoint = checkpoint;
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

    public void OnTeleportClicked()
    {
        Close(); // Cerramos este (despausará brevemente)
        CheckpointMenu.Instance.OpenMenu(); // Abrimos el otro (volverá a pausar)
    }

    public void OnCancelClicked()
    {
        Close();
    }
}
