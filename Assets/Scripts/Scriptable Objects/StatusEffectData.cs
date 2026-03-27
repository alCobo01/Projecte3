using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectData", menuName = "RPG/StatusEffectData")]
public class StatusEffectData : ScriptableObject
{
    public string effectName;
    public StatusEffectType type;
    public int duration;
    public int damagePerTurn;
    public int healPerTurn;
    public Sprite icon;
}
