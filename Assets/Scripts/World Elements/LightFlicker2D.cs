using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Script para hacer que una luz 2D (Light2D) parpadee suavemente.
/// </summary>
[RequireComponent(typeof(Light2D))]
public class LightFlicker2D : MonoBehaviour
{
    private Light2D _luz;

    [Header("Ajustes de Intensidad")]
    [Tooltip("Intensidad mínima que alcanzará la luz.")]
    [SerializeField] private float intensidadMinima = 0.8f;
    
    [Tooltip("Intensidad máxima que alcanzará la luz.")]
    [SerializeField] private float intensidadMaxima = 1.2f;

    [Header("Ajustes de Velocidad")]
    [Tooltip("Velocidad del parpadeo (más alto = más rápido).")]
    [SerializeField] private float velocidad = 2f;

    [Tooltip("Desfase para que no todas las luces parpadeen igual.")]
    [SerializeField] private float offset;

    private void Awake()
    {
        _luz = GetComponent<Light2D>();
        
        // Asignar un offset aleatorio si no se ha definido para variar entre múltiples luces
        if (offset == 0)
        {
            offset = Random.Range(0f, 100f);
        }
    }

    private void Update()
    {
        if (_luz == null) return;

        // Usamos Perlin Noise para un parpadeo más natural y suave que un simple Sinusoidal
        float ruido = Mathf.PerlinNoise(Time.time * velocidad + offset, 0f);
        
        // Mapeamos el ruido (0 a 1) al rango de intensidad deseado
        _luz.intensity = Mathf.Lerp(intensidadMinima, intensidadMaxima, ruido);
    }
}
