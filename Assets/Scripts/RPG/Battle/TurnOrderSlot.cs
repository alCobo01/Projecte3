using UnityEngine;
using UnityEngine.UI;

public class TurnOrderSlot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image teamColorStrip;
    [SerializeField] private GameObject currentTurnIndicator;
    
    [Header("Colors")]
    [SerializeField] private Color playerColor = new(0.2f, 0.6f, 1f, 1f);
    [SerializeField] private Color enemyColor = new(1f, 0.3f, 0.3f, 1f);

    public void Setup(BattleUnit unit, bool isCurrentTurn)
    {
        portraitImage.sprite = unit.Data.portrait;
        portraitImage.enabled = true;
        teamColorStrip.color = unit.IsPlayer ? playerColor : enemyColor;
        currentTurnIndicator.SetActive(isCurrentTurn);
    }
}
