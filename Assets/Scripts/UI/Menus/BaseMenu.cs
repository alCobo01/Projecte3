using UnityEngine;

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
        // Limpieza de seguridad: al cerrar cualquier menú, nos aseguramos de que no haya bloqueos de input
        PauseManager.Instance.SetInputLock(false);
    } 
    
    public void CloseGame() => Application.Quit();
}
   

   
