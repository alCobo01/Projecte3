using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Player")]
    public GameObject playerPrefab;

    private string pendingSpawnPointId;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void GoToScene(string sceneName, string spawnPointId)
    {
        StartCoroutine(GoToSceneRoutine(sceneName, spawnPointId));
    }

    private IEnumerator GoToSceneRoutine(string sceneName, string spawnPointId)
    {
        pendingSpawnPointId = spawnPointId;

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut();

        Debug.Log("Cargando escena...");
        SceneManager.LoadScene(sceneName); 
        Debug.Log("Escena cargada");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"OnSceneLoaded: {scene.name} | pendingSpawnPointId: '{pendingSpawnPointId}'");

        if (string.IsNullOrEmpty(pendingSpawnPointId)) return;

        Vector3 spawnPos = Vector3.zero;
        bool found = false;

        foreach (var sp in Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None))
        {
            Debug.Log($"SpawnPoint encontrado: '{sp.spawnPointId}'");
            if (sp.spawnPointId == pendingSpawnPointId)
            {
                spawnPos = sp.transform.position;
                found = true;
                break;
            }
        }

        if (!found)
            Debug.LogWarning($"SpawnPoint '{pendingSpawnPointId}' no encontrado en {scene.name}");

        GameObject existing = GameObject.FindGameObjectWithTag("Player");
        Debug.Log($"Player existente antes de instanciar: {existing}");
        if (existing != null) Destroy(existing);

        Debug.Log($"playerPrefab: {playerPrefab}");
        if (playerPrefab != null)
        {
            GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            Debug.Log($"Player instanciado en {spawnPos}");
            StartCoroutine(AssignCameraNextFrame(player));
        }

        pendingSpawnPointId = null;

        if (ScreenFader.Instance != null)
            StartCoroutine(ScreenFader.Instance.FadeIn());
    }

    private IEnumerator AssignCameraNextFrame(GameObject player)
    {
        yield return null; // Espera un frame a que la escena termine de inicializarse
        CameraFollowHelper.AssignPlayerToCamera(player);
    }
}