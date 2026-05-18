using System.Collections;
using UnityEngine;

public class AppearOnDiscovery : MonoBehaviour
{
    [Header("Appearance Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private bool useScaleAnimation = true;
    [SerializeField] private Vector3 targetScale = Vector3.one;

    private SpriteRenderer _spriteRenderer;
    private Coroutine _activeRoutine;

    public void SetInteracting(bool interacting)
    {
        if (_activeRoutine != null) StopCoroutine(_activeRoutine);

        if (interacting)
        {
            _activeRoutine = StartCoroutine(AppearRoutine());
        }
        else
        {
            HideInstantly();
        }
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        HideInstantly();
    }

    private void HideInstantly()
    {
        if (useScaleAnimation) transform.localScale = Vector3.zero;
        if (_spriteRenderer != null)
        {
            Color c = _spriteRenderer.color;
            c.a = 0;
            _spriteRenderer.color = c;
        }
    }

    private IEnumerator AppearRoutine()
    {
        float elapsed = 0;
        Vector3 startScale = useScaleAnimation ? Vector3.zero : targetScale;

        // Asegurarnos de que sea visible al empezar
        if (_spriteRenderer != null)
        {
            Color c = _spriteRenderer.color;
            if (c.a < 0.01f && !useScaleAnimation) c.a = 0; // Por si acaso
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            if (useScaleAnimation)
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            if (_spriteRenderer != null)
            {
                Color c = _spriteRenderer.color;
                c.a = Mathf.Lerp(0, 1, t);
                _spriteRenderer.color = c;
            }

            yield return null;
        }

        transform.localScale = targetScale;
        if (_spriteRenderer != null)
        {
            Color c = _spriteRenderer.color;
            c.a = 1;
            _spriteRenderer.color = c;
        }
    }
}
