using UnityEngine;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    public static ScreenFader Instance;

    private void Awake()
    {
        Instance = this;
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        
        // Ensure it starts transparent
        if (canvasGroup != null) canvasGroup.alpha = 0;
    }

    public IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;
        yield return Fade(0, 1);
    }

    public IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        yield return Fade(1, 0);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
    }
}
