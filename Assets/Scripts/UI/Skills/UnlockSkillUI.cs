using System.Collections.Generic;
using UnityEngine;

public class UnlockSkillUI : MonoBehaviour
{
    [Header("UI Config")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private SkillSlotUI slotPrefab;
    
    [Header("Skill data")]
    [SerializeField] private List<SkillData> skills;

    private void OnEnable()
    {
        SkillEvents.OnSkillUnlocked += Toggle;
        PopulateGrid();
    } 
    
    private void OnDisable() => SkillEvents.OnSkillUnlocked -= Toggle;
    
    public void ChooseSkill(int index)
    {
        var selectedSkill = skills[index];
        PlayerStatsManager.Instance.AddSkill(selectedSkill);
        Toggle();
    }
    
    private void PopulateGrid()
    {
        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);

        for (var i = 0; i < skills.Count; i++)
        {
            var index = i; 
            var newSlot = Instantiate(slotPrefab, gridContainer);
            
            newSlot.Setup(skills[index], () => ChooseSkill(index));
        }
    }
    
    private void Toggle()
    {
        var isActive = !panel.activeSelf;
        panel.SetActive(isActive);
        PauseManager.Instance.TogglePause();
    }
}