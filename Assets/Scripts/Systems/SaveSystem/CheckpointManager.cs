using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Player")]
    public GameObject playerPrefab;

    [Header("Databases")]
    public List<ItemData> itemDatabase;
    public List<SkillData> skillDatabase;

    private string lastCheckpointId;
    private string lastCheckpointScene;
    public HashSet<string> discoveredIds = new();
    private List<Checkpoint> sceneCheckpoints = new();
    private Dictionary<string, string> checkpointSceneMap = new();

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneCheckpoints = Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None).ToList();

        foreach (var cp in sceneCheckpoints)
        {
            if (!string.IsNullOrEmpty(cp.checkpointId))
                checkpointSceneMap[cp.checkpointId] = scene.name;

            cp.IsDiscovered = discoveredIds.Contains(cp.checkpointId);
        }
    }

    // ─── Registro y descubrimiento ───────────────────────────────────────────

    public void RegisterCheckpoint(Checkpoint checkpoint)
    {
        RegisterDiscovery(checkpoint);
    }

    public void RegisterDiscovery(Checkpoint checkpoint)
    {
        if (!discoveredIds.Contains(checkpoint.checkpointId))
        {
            discoveredIds.Add(checkpoint.checkpointId);
            checkpoint.IsDiscovered = true;
        }

        lastCheckpointId = checkpoint.checkpointId;
        lastCheckpointScene = checkpoint.sceneName;
        checkpointSceneMap[checkpoint.checkpointId] = checkpoint.sceneName;

        Debug.Log($"Checkpoint activado: {checkpoint.displayName}");
    }

    public List<Checkpoint> GetDiscoveredCheckpoints()
    {
        return sceneCheckpoints.Where(c => c.IsDiscovered).ToList();
    }

    // ─── Teletransporte ──────────────────────────────────────────────────────

    public void TeleportToCheckpoint(string id)
    {
        if (!checkpointSceneMap.TryGetValue(id, out string sceneName))
        {
            Debug.LogWarning($"No se conoce la escena del checkpoint '{id}'. ¿Fue descubierto?");
            return;
        }

        StartCoroutine(TeleportRoutine(id, sceneName));
    }

    public void TeleportToCheckpoint(string id, string sceneName)
    {
        StartCoroutine(TeleportRoutine(id, sceneName));
    }

    private IEnumerator TeleportRoutine(string id, string sceneName)
    {
        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut();

        if (SceneManager.GetActiveScene().name != sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return null; // Espera a que OnSceneLoaded termine
        }

        PlacePlayerAtCheckpoint(id);

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeIn();
    }

    private void PlacePlayerAtCheckpoint(string id)
    {
        sceneCheckpoints = Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None).ToList();

        Checkpoint target = sceneCheckpoints.Find(c => c.checkpointId == id);
        if (target == null)
        {
            Debug.LogWarning($"Checkpoint '{id}' no encontrado en la escena actual.");
            return;
        }

        GameObject existing = GameObject.FindGameObjectWithTag("Player");
        if (existing != null) Destroy(existing);

        if (playerPrefab != null)
        {
            GameObject player = Instantiate(playerPrefab, target.GetSpawnPosition(), Quaternion.identity);
            StartCoroutine(AssignCameraNextFrame(player)); // ← igual aquí
        }
    }

    private IEnumerator AssignCameraNextFrame(GameObject player)
    {
        yield return null;
        CameraFollowHelper.AssignPlayerToCamera(player);
    }

    public void RespawnAtLastCheckpoint()
    {
        if (string.IsNullOrEmpty(lastCheckpointId))
        {
            Debug.LogWarning("No hay checkpoint activado.");
            return;
        }

        StartCoroutine(TeleportRoutine(lastCheckpointId, lastCheckpointScene));
    }

    public bool HasCheckpoint() => !string.IsNullOrEmpty(lastCheckpointId);

    public void SaveGame()
    {
        SaveData data = new SaveData();

        var stats = PlayerStatsManager.Instance;
        data.currentHp = stats.currentHp;
        data.currentSp = stats.currentSp;

        foreach (var itemStack in stats.inventory)
            data.inventory.Add(new SaveData.InventoryItemData
            {
                itemName = itemStack.item.itemName,
                quantity = itemStack.quantity
            });

        foreach (var skill in stats.CharacterData.skills)
            data.unlockedSkills.Add(skill.skillName);

        data.lastCheckpointId = lastCheckpointId;
        data.lastSceneName = lastCheckpointScene;
        data.discoveredCheckpointIds = discoveredIds.ToList();

        data.checkpointSceneMap = checkpointSceneMap
            .Select(kvp => new SaveData.CheckpointSceneEntry
            {
                checkpointId = kvp.Key,
                sceneName = kvp.Value
            }).ToList();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.playerPosition[0] = player.transform.position.x;
            data.playerPosition[1] = player.transform.position.y;
            data.playerPosition[2] = player.transform.position.z;
        }

        SaveSystem.Save(data);
        Debug.Log("Partida guardada.");
    }

    public void LoadGame()
    {
        StartCoroutine(LoadGameRoutine());
    }

    private IEnumerator LoadGameRoutine()
    {
        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut();

        yield return new WaitForSeconds(0.5f);

        SaveData data = SaveSystem.Load();
        if (data == null)
        {
            if (ScreenFader.Instance != null)
                yield return ScreenFader.Instance.FadeIn();
            yield break;
        }

        discoveredIds = new HashSet<string>(data.discoveredCheckpointIds);
        lastCheckpointId = data.lastCheckpointId;
        lastCheckpointScene = data.lastSceneName;

        checkpointSceneMap.Clear();
        if (data.checkpointSceneMap != null)
            foreach (var entry in data.checkpointSceneMap)
                checkpointSceneMap[entry.checkpointId] = entry.sceneName;

        var stats = PlayerStatsManager.Instance;
        stats.currentHp = data.currentHp;
        stats.currentSp = data.currentSp;

        stats.inventory.Clear();
        foreach (var itemData in data.inventory)
        {
            ItemData asset = itemDatabase.Find(i => i.itemName == itemData.itemName);
            if (asset != null)
                stats.inventory.Add(new ItemStack { item = asset, quantity = itemData.quantity });
        }

        stats.CharacterData.skills.Clear();
        foreach (var skillName in data.unlockedSkills)
        {
            SkillData asset = skillDatabase.Find(s => s.skillName == skillName);
            if (asset != null)
                stats.CharacterData.skills.Add(asset);
        }

        if (!string.IsNullOrEmpty(lastCheckpointId))
            yield return StartCoroutine(TeleportRoutine(lastCheckpointId, lastCheckpointScene));
        else if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeIn();
    }
}