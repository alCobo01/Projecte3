using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private void OnEnable() => PlayerInputController.OnPauseMenuEvent += TogglePauseMenu;
    private void OnDisable() => PlayerInputController.OnPauseMenuEvent -= TogglePauseMenu;

    private void TogglePauseMenu()
    {
        var isActive = !pausePanel.activeSelf;
        pausePanel.SetActive(isActive);
        PauseManager.Instance.TogglePause();
    }
}