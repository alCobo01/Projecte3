using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "SkillData", menuName = "RPG/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;
    
    [Header("Costs & Damage")]
    public int spCost;
    public int basePower;
    
    [Header("Targeting")]
    public TargetType targetType;
    
    [Header("Effects")]
    public StatusEffectData appliedEffect;
    [Range(0, 1)] public float effectChance = 1.0f;

    //public GameObject vfxPrefab;
}
