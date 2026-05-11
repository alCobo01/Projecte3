using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : BaseMenu
{
    public void Play()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }
}
