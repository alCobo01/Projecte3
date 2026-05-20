using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "savegame.json");

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Game Saved to {SavePath}");
    }

    public static SaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("No save file found.");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        return data;
    }

    public static bool SaveExists() => File.Exists(SavePath);
    
    public static void DeleteSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }
}
