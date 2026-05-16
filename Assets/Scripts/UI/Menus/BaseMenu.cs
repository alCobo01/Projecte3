using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class BaseMenu : MonoBehaviour
{
    public virtual void Open()
    {
        gameObject.SetActive(true);
        PauseManager.Instance.SetPaused(true);
    }

    public void Close() 
    {
        gameObject.SetActive(false);
        PauseManager.Instance.SetPaused(false);
    } 
    
    public void CloseGame()
    {
        Application.Quit(); 
    }
}
   

   
