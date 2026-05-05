using UnityEngine;

public class UnlockSkillUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    private void OnEnable()  => SkillEvents.OnSkillUnlocked += HandleSkillUnlocked;
    private void OnDisable() => SkillEvents.OnSkillUnlocked -= HandleSkillUnlocked;
    
    public void Toggle()
    {
        var isActive = !panel.activeSelf;
        panel.SetActive(isActive);
        PauseManager.Instance.TogglePause();
    }
    
    private void HandleSkillUnlocked(SkillData skill)
    {
        PlayerStatsManager.Instance.AddSkill(skill);
    }
}