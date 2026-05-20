using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : BaseMenu
{
    [SerializeField] private List<GameObject> uiToDisable;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private float fadeDuration = 0.3f;

    private CanvasGroup _canvasGroup;
    private Coroutine fadeRoutine;

    private void Start()
    {
        BattleManager.Instance.OnPlayerDied += Open;
        _canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
    }
    
    private void OnDestroy() => BattleManager.Instance.OnPlayerDied -= Open;

    public override void Open()
    {
        uiToDisable.ForEach(u => u.SetActive(false));
        gameOverPanel.SetActive(true);
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeIn());
        PauseManager.Instance.TogglePause();
        
        SoundManager.Instance.StopMusic();
        SoundManager.Instance.PlaySfx("GameOver", transform);
    }

    public void ExitToLastCheckpoint()
    {
        Close();
        if (CheckpointManager.Instance.HasCheckpoint())
        {
            CheckpointManager.Instance.RespawnAtLastCheckpoint();
            return;
        }

        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    private IEnumerator FadeIn()
    {
        _canvasGroup.alpha = 0f;
        var elapsed = 0f;
        var duration = Mathf.Max(0.01f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        _canvasGroup.alpha = 1f;
        fadeRoutine = null;
    }
}
