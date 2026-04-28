using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "RPG/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public ItemType type;
    public int value;
    public int boostDuration = 1;
    public StatType boostedStat;
    public Sprite icon;
    public string description;
}
