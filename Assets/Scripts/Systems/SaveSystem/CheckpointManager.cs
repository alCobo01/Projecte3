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

    public void RegisterAndSave(Checkpoint checkpoint)
    {
        bool isNew = !discoveredIds.Contains(checkpoint.checkpointId);
        if (isNew)
        {
            discoveredIds.Add(checkpoint.checkpointId);
            checkpoint.IsDiscovered = true;
            Debug.Log($"Checkpoint {checkpoint.displayName} discovered!");
        }

        lastCheckpointId = checkpoint.checkpointId;
        SaveGame();
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
        SaveData data = SaveSystem.Load();
        if (data == null) return;

        discoveredIds = new HashSet<string>(data.discoveredCheckpointIds);
        lastCheckpointId = data.lastCheckpointId;

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
        TeleportToLastCheckpoint();
    }

    public void TeleportToCheckpoint(string id)
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
            Debug.LogWarning($"Checkpoint {id} not found in this scene!");
        }
    }

    public void TeleportToLastCheckpoint()
    {
        if (!string.IsNullOrEmpty(lastCheckpointId))
        {
            TeleportToCheckpoint(lastCheckpointId);
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
