using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnlockSkillUI : MonoBehaviour
{
    [Header("UI Config")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private SkillSlotUI slotPrefab;
    [SerializeField] private float fadeDuration = 0.3f;
    
    [Header("Skill data")]
    [SerializeField] private AvailableSkills availableSkills;

    private CanvasGroup _canvasGroup;
    private Coroutine _fadeCoroutine;    
    
    private void OnEnable()
    {
        _canvasGroup = panel.GetComponent<CanvasGroup>();
        
        SkillEvents.OnSkillUnlocked += Toggle;
        PopulateGrid();
    } 
    
    private void OnDisable() => SkillEvents.OnSkillUnlocked -= Toggle;
    
    private void ChooseSkill(int index)
    {
        if (availableSkills == null || index < 0 || index >= availableSkills.skills.Count) return;
        var selectedSkill = availableSkills.skills[index];
        if (!selectedSkill) return;
        PlayerStatsManager.Instance.AddSkill(selectedSkill);
        availableSkills.skills.RemoveAt(index);
        Toggle();
    }
    
    private void PopulateGrid()
    {
        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);

        for (var i = 0; i < availableSkills.skills.Count; i++)
        {
            var index = i;
            if (!availableSkills.skills[index]) continue;
            var newSlot = Instantiate(slotPrefab, gridContainer);
            newSlot.Setup(availableSkills.skills[index], () => ChooseSkill(index));
        }
    }
    
    private void Toggle()
    {
        var isActive = !panel.activeSelf;
        PauseManager.Instance.SetPaused(isActive);
        
        if (_fadeCoroutine is not null) StopCoroutine(_fadeCoroutine);

        if (isActive)
        {
            PopulateGrid();
            panel.SetActive(true);
            _fadeCoroutine = StartCoroutine(Fade(0f, 1f));
        }
        else
            _fadeCoroutine = StartCoroutine(Fade(1f, 0f, onComplete: () => panel.SetActive(false)));
        
    }
    
    private IEnumerator Fade(float from, float to, UnityAction onComplete = null)
    {
        var elapsed = 0f;
        _canvasGroup.alpha = from;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = to;
        onComplete?.Invoke();
    }
}
