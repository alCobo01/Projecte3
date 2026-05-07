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
    [SerializeField] private List<SkillData> skills;

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
        var selectedSkill = skills[index];
        PlayerStatsManager.Instance.AddSkill(selectedSkill);
        skills.Remove(selectedSkill);
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
        PauseManager.Instance.TogglePause();
        
        if (_fadeCoroutine is not null) StopCoroutine(_fadeCoroutine);

        if (isActive)
        {
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