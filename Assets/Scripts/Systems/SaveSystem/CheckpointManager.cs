using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Databases (Assets for loading)")]
    public List<ItemData> itemDatabase;
    public List<SkillData> skillDatabase;

    [Header("Runtime Data")]
    public List<Checkpoint> allCheckpoints = new();
    public HashSet<string> discoveredIds = new();
    public string lastCheckpointId;
    public string lastSceneName; // Track scene of last checkpoint

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAllCheckpoints();
        UpdateCheckpointsState();
        
        // Auto-teleport on load if we just loaded a save
        // (This could be handled by a specific Menu-to-Game flow)
    }

    private void FindAllCheckpoints()
    {
        allCheckpoints = Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None).ToList();
    }

    public void RegisterDiscovery(Checkpoint checkpoint)
    {
        bool isNew = !discoveredIds.Contains(checkpoint.checkpointId);
        if (isNew)
        {
            discoveredIds.Add(checkpoint.checkpointId);
            checkpoint.IsDiscovered = true;
            Debug.Log($"Checkpoint {checkpoint.displayName} discovered!");
        }

        lastCheckpointId = checkpoint.checkpointId;
        lastSceneName = checkpoint.sceneName; // Update current scene name
    }

    private void UpdateCheckpointsState()
    {
        foreach (var cp in allCheckpoints)
        {
            cp.IsDiscovered = discoveredIds.Contains(cp.checkpointId);
        }
    }

    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        SaveData data = new SaveData();
        
        // Player Stats
        var stats = PlayerStatsManager.Instance;
        data.currentHp = stats.currentHp;
        data.currentSp = stats.currentSp;

        // Inventory
        foreach (var itemStack in stats.inventory)
        {
            data.inventory.Add(new SaveData.InventoryItemData 
            { 
                itemName = itemStack.item.itemName, 
                quantity = itemStack.quantity 
            });
        }

        // Skills
        foreach (var skill in stats.CharacterData.skills)
        {
            data.unlockedSkills.Add(skill.skillName);
        }

        // World Data
        data.lastCheckpointId = lastCheckpointId;
        data.lastSceneName = lastSceneName;
        data.discoveredCheckpointIds = discoveredIds.ToList();
        
        // Position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.playerPosition[0] = player.transform.position.x;
            data.playerPosition[1] = player.transform.position.y;
            data.playerPosition[2] = player.transform.position.z;
        }

        SaveSystem.Save(data);
    }

    [ContextMenu("Load Game")]
    public void LoadGame()
    {
        StartCoroutine(LoadGameRoutine());
    }

    private IEnumerator LoadGameRoutine()
    {
        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut();

        yield return new WaitForSeconds(0.5f); // 0.5s delay before loading

        SaveData data = SaveSystem.Load();
        if (data != null)
        {
            discoveredIds = new HashSet<string>(data.discoveredCheckpointIds);
            lastCheckpointId = data.lastCheckpointId;
            lastSceneName = data.lastSceneName;

            // Restore Player Stats
            var stats = PlayerStatsManager.Instance;
            stats.currentHp = data.currentHp;
            stats.currentSp = data.currentSp;

            // Restore Inventory
            stats.inventory.Clear();
            foreach (var itemData in data.inventory)
            {
                ItemData asset = itemDatabase.Find(i => i.itemName == itemData.itemName);
                if (asset != null)
                {
                    stats.inventory.Add(new ItemStack { item = asset, quantity = itemData.quantity });
                }
            }

            // Restore Skills
            stats.CharacterData.skills.Clear();
            foreach (var skillName in data.unlockedSkills)
            {
                SkillData asset = skillDatabase.Find(s => s.skillName == skillName);
                if (asset != null)
                {
                    stats.CharacterData.skills.Add(asset);
                }
            }
            
            UpdateCheckpointsState();
            
            if (!string.IsNullOrEmpty(lastCheckpointId))
            {
                // Use a modified teleport that handles scenes
                yield return StartCoroutine(TeleportInternal(lastCheckpointId, lastSceneName));
            }

            // Re-enable player collider in case it was disabled (e.g., by Killer script)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Collider2D col = player.GetComponent<Collider2D>();
                if (col != null) col.enabled = true;
            }
        }

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeIn();
    }

    public void TeleportToCheckpoint(string id)
    {
        // For menu teleportation, we might not have the scene name immediately.
        // We'd need a database of all checkpoints in the project.
        // For now, assume it's in the current scene if not specified, 
        // OR pass the scene name from the UI element.
        StartCoroutine(TeleportRoutine(id, "")); 
    }

    public void TeleportToCheckpoint(string id, string sceneName)
    {
        StartCoroutine(TeleportRoutine(id, sceneName));
    }

    private IEnumerator TeleportRoutine(string id, string sceneName)
    {
        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut();

        yield return new WaitForSeconds(0.5f); 

        yield return StartCoroutine(TeleportInternal(id, sceneName));

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeIn();
    }

    private IEnumerator TeleportInternal(string id, string sceneName)
    {
        // If sceneName is specified and different from current, load it
        if (!string.IsNullOrEmpty(sceneName) && SceneManager.GetActiveScene().name != sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
            // After scene load, allCheckpoints will be updated via OnSceneLoaded
        }

        ExecuteTeleport(id);
    }

    private void ExecuteTeleport(string id)
    {
        Checkpoint target = allCheckpoints.Find(c => c.checkpointId == id);
        if (target != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = target.GetSpawnPosition();
                Debug.Log($"Teleported to {target.displayName}");
            }
        }
        else
        {
            Debug.LogWarning($"Checkpoint {id} not found in scene {SceneManager.GetActiveScene().name}!");
        }
    }

    public void TeleportToLastCheckpoint()
    {
        if (!string.IsNullOrEmpty(lastCheckpointId))
        {
            TeleportToCheckpoint(lastCheckpointId, lastSceneName);
        }
    }

    public List<Checkpoint> GetDiscoveredCheckpoints()
    {
        // This includes checkpoints from other scenes if they were discovered
        // But we only have access to the ones currently in the scene through 'allCheckpoints'.
        // For a full system, you might want a SO database of ALL checkpoints in the game.
        return allCheckpoints.Where(c => c.IsDiscovered).ToList();
    }
}
