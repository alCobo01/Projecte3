using System.Collections.Generic;
using UnityEngine;

public class GameOverMenu : BaseMenu
{
    [SerializeField] private List<GameObject> uiToDisable;

    private void OnEnable() => BattleManager.Instance.OnPlayerDied += Open;
    private void OnDisable() => BattleManager.Instance.OnPlayerDied -= Open;

    public override void Open()
    {
        base.Open();
        uiToDisable.ForEach(u => u.SetActive(false));
    }
}
