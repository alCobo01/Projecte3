using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheckpointUIElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button teleportButton;
    
    private string _checkpointId;

    public void Setup(Checkpoint checkpoint)
    {
        _checkpointId = checkpoint.checkpointId;
        nameText.text = checkpoint.displayName;
        teleportButton.onClick.RemoveAllListeners();
        teleportButton.onClick.AddListener(OnTeleportClicked);
    }

    private void OnTeleportClicked()
    {
        CheckpointManager.Instance.TeleportToCheckpoint(_checkpointId);
        // Opcionalmente cerrar el menú después de teletransportarse
        CheckpointMenu.Instance.CloseMenu();
    }
}
