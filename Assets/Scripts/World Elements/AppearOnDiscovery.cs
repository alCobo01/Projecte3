using System.Collections;
using UnityEngine;

public class AppearOnDiscovery : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Checkpoint targetCheckpoint;

    [Header("Appearance Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private bool useScaleAnimation = true;
    [SerializeField] private Vector3 targetScale = Vector3.one;

    private SpriteRenderer _spriteRenderer;
    private bool _hasAppeared;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Si el checkpoint ya fue descubierto en una partida anterior, aparecer directamente
        if (targetCheckpoint != null && targetCheckpoint.IsDiscovered)
        {
            ShowInstantly();
        }
        else
        {
            HideInstantly();
        }
    }

    private void Update()
    {
        // Vigilamos si el checkpoint cambia a descubierto (cuando interactúas)
        if (!_hasAppeared && targetCheckpoint != null && targetCheckpoint.IsDiscovered)
        {
            _hasAppeared = true;
            StartCoroutine(AppearRoutine());
        }
    }

    private void ShowInstantly()
    {
        _hasAppeared = true;
        transform.localScale = targetScale;
        if (_spriteRenderer != null)
        {
            Color c = _spriteRenderer.color;
            c.a = 1;
            _spriteRenderer.color = c;
        }
        gameObject.SetActive(true);
    }

    private void HideInstantly()
    {
        _hasAppeared = false;
        if (useScaleAnimation) transform.localScale = Vector3.zero;
        if (_spriteRenderer != null)
        {
            Color c = _spriteRenderer.color;
            c.a = 0;
            _spriteRenderer.color = c;
        }
        // No desactivamos el GameObject para que el script pueda seguir escuchando
    }

    private IEnumerator AppearRoutine()
    {
        float elapsed = 0;
        Vector3 startScale = useScaleAnimation ? Vector3.zero : targetScale;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            // Animación de escala
            if (useScaleAnimation)
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            // Animación de Alpha (transparencia)
            if (_spriteRenderer != null)
            {
                Color c = _spriteRenderer.color;
                c.a = Mathf.Lerp(0, 1, t);
                _spriteRenderer.color = c;
            }

            yield return null;
        }

        transform.localScale = targetScale;
    }
}
