using UnityEngine;

public class PauseMenu : BaseMenu
{
    [SerializeField] private GameObject pausePanel;

    private void OnEnable() => PlayerInputController.OnPauseMenuEvent += TogglePauseMenu;
    private void OnDisable() => PlayerInputController.OnPauseMenuEvent -= TogglePauseMenu;

    public void TogglePauseMenu()
    {
        var isActive = !pausePanel.activeSelf;
        pausePanel.SetActive(isActive);
        PauseManager.Instance.SetPaused(isActive);
    }
}