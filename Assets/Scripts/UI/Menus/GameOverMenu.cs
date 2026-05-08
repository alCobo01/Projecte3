using System.Collections.Generic;
using UnityEngine;

public class GameOverMenu : BaseMenu
{
    [SerializeField] private List<GameObject> uiToDisable;
    
    public override void Open()
    {
        base.Open();
        uiToDisable.ForEach(u => u.SetActive(false));
    }
}
