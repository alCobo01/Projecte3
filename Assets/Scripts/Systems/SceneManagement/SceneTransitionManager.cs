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

        yield return SceneManager.LoadSceneAsync(sceneName);
        // OnSceneLoaded se encarga del resto
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
        if (existing != null) Destroy(existing);

        if (playerPrefab != null)
        {
            Debug.Log($"Instanciando Player en {spawnPos}");
            GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            CameraFollowHelper.AssignPlayerToCamera(player, this);
        }
        else
        {
            Debug.LogError("playerPrefab es null en SceneTransitionManager");
        }

        pendingSpawnPointId = null;

        if (ScreenFader.Instance != null)
            StartCoroutine(ScreenFader.Instance.FadeIn());
    }
}