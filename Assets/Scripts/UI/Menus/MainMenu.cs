using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : BaseMenu
{
    private void Start() => SoundManager.Instance.PlayMusicByIndex(0);

    public void Play()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
        SoundManager.Instance.PlayMusicByIndex(1);
    }
}
