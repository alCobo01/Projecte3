using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    // Datos del Jugador
    public int currentHp;
    public int currentSp;
    public List<InventoryItemData> inventory = new();
    public List<string> skills = new();

    // Datos del Mundo/Checkpoints
    public string lastCheckpointId;
    public string lastSceneName;
    public List<string> discoveredCheckpointIds = new();
    
    // Posición por si acaso (aunque los checkpoints suelen ser el punto de spawn)
    public float[] playerPosition = new float[3];
    public List<CheckpointSceneEntry> checkpointSceneMap = new();

    [System.Serializable]
    public class CheckpointSceneEntry
    {
        public string checkpointId;
        public string sceneName;
    }

    [Serializable]
    public class InventoryItemData
    {
        public string itemName;
        public int quantity;
    }
}
