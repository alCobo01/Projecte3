using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button unlockButton;

    public void Setup(SkillData data, UnityAction onUnlock)
    {
        skillNameText.text = data.skillName;
        iconImage.sprite = data.icon;
        descriptionText.text = data.description;
        
        unlockButton.onClick.RemoveAllListeners();
        unlockButton.onClick.AddListener(() => onUnlock?.Invoke());
    }
}