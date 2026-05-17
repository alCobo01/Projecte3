using UnityEngine;
using UnityEngine.SceneManagement;

public class TestSceneLoad : MonoBehaviour
{
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 50), "Cargar ParkourLevel"))
        {
            Debug.Log("Cargando ParkourLevel...");
            SceneManager.LoadScene("ParkourLevel");
        }
    }
}