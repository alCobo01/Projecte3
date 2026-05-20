using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheckpointUIElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button teleportButton;
    
    private string _checkpointId;

    public void Setup(CheckpointManager.DiscoveredCheckpointData checkpointData)
    {
        _checkpointId = checkpointData.id;
        nameText.text = checkpointData.displayName;
        teleportButton.onClick.RemoveAllListeners();
        teleportButton.onClick.AddListener(OnTeleportClicked);
    }

    private void OnTeleportClicked()
    {
        CheckpointManager.Instance.TeleportToCheckpoint(_checkpointId);
        CheckpointMenu.Instance.CloseMenu();
    }
}
