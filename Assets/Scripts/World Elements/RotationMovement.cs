using UnityEngine;

public class RotationMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float amplitud = 2f;      
    public float velocidadMovimiento = 2f; 

    [Header("Rotación")]
    public float velocidadRotacion = 90f; // Grados por segundo

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
       
        float offsetY = Mathf.Sin(Time.time * velocidadMovimiento) * amplitud;
        transform.position = posicionInicial + new Vector3(0f, offsetY, 0f);

        // Rotación constante hacia la derecha (en 2D = eje Z negativo)
        transform.Rotate(0f, 0f, -velocidadRotacion * Time.deltaTime);
    }
}
