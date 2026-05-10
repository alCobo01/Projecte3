using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AvailableSkills", menuName = "RPG/AvailableSkills")]
public class AvailableSkills : ScriptableObject
{
    public List<SkillData> skills;
}
