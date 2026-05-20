using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "RPG/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    
    [Header("Base stats")]
    public int maxHp;
    public int maxSp;
    public int attack;
    public int defense;
    public int speed;

    [Header("Combat mechanics")] 
    public int spGainPerHit = 10;
    public List<SkillData> skills;
    public Sprite portrait;
}
