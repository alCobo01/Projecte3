using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsInputLocked { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void TogglePause() => SetPaused(!IsPaused);
    
    public void SetPaused(bool paused)
    {
        if (IsPaused == paused) return;
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
    }

    public void SetInputLock(bool locked)
    {
        IsInputLocked = locked;
    }
}