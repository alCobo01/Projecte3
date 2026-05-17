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
        if (string.IsNullOrEmpty(pendingSpawnPointId)) return;

        Vector3 spawnPos = Vector3.zero;
        bool found = false;

        foreach (var sp in Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None))
        {
            if (sp.spawnPointId == pendingSpawnPointId)
            {
                spawnPos = sp.transform.position;
                found = true;
                break;
            }
        }

        GameObject existing = GameObject.FindGameObjectWithTag("Player");
        if (existing != null) Destroy(existing);

        if (playerPrefab != null)
        {
            GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            
            var vcams = Object.FindObjectsByType<Unity.Cinemachine.CinemachineCamera>(FindObjectsSortMode.None);
            foreach (var vcam in vcams)
            {
                if (vcam.name == "ExplorationCamera")
                {
                    vcam.Follow = player.transform;
                    vcam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, vcam.transform.position.z);
                    vcam.OnTargetObjectWarped(player.transform, player.transform.position - vcam.transform.position);
                    break;
                }
            }
        }

        pendingSpawnPointId = null;

        if (ScreenFader.Instance != null)
            StartCoroutine(ScreenFader.Instance.FadeIn());
    }
}