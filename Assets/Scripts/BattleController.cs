using System;
using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public BattleManager Instance { get; private set; }

    private Stack _turnOrder;
    
    private void Awake()
    {
        Instance = this;
    }

    public void StartBattle()
    {
        
    }
}
