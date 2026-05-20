using System.Collections;
using UnityEngine;

public class InteractionPrompt : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;      // Estrella pequeña
    [SerializeField] private Sprite targetedSprite;  // Estrella grande

    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float transitionSpeed = 0.15f; 
    [SerializeField] private Vector3 smallScale = new Vector3(0.8f, 0.8f, 1f);
    [SerializeField] private Vector3 largeScale = new Vector3(1.2f, 1.2f, 1f);

    private Coroutine _transitionCoroutine;
    private bool _isHidden;

    public void Show()
    {
        _isHidden = false;
        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }

    public void Hide()
    {
        _isHidden = true;
        if (spriteRenderer != null) spriteRenderer.enabled = false;
    }

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Estado inicial
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = idleSprite;
            transform.localScale = smallScale;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            StopAndStart(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            StopAndStart(false);
        }
    }

    private void StopAndStart(bool reachingTarget)
    {
        if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
        
        // Evitamos error si el objeto se desactiva o si está oculto intencionadamente
        if (gameObject.activeInHierarchy && !_isHidden)
        {
            _transitionCoroutine = StartCoroutine(TransitionRoutine(reachingTarget));
        }
    }

    private IEnumerator TransitionRoutine(bool reachingTarget)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = reachingTarget ? largeScale : smallScale;
        Sprite targetSprite = reachingTarget ? targetedSprite : idleSprite;

        float elapsed = 0;

        // Primero encogemos un poco y cambiamos el sprite a mitad de camino para que sea fluido
        while (elapsed < transitionSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionSpeed;
            
            // Usamos una curva suave (SmoothStep)
            float smoothT = Mathf.SmoothStep(0, 1, t);
            
            transform.localScale = Vector3.Lerp(startScale, endScale, smoothT);

            // Cambiamos el sprite justo a la mitad del cambio de tamaño
            if (t >= 0.5f && spriteRenderer.sprite != targetSprite)
            {
                spriteRenderer.sprite = targetSprite;
            }

            yield return null;
        }

        transform.localScale = endScale;
        spriteRenderer.sprite = targetSprite;
    }
}
