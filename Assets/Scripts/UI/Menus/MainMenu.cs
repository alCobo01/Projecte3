using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : BaseMenu
{
    private void Start()
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.MaximizedWindow);
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        SoundManager.Instance.PlayMusicByIndex(0);
    } 

    public void Play()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
        SoundManager.Instance.PlayMusicByIndex(1);
    }
}
