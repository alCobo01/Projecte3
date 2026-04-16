using System;
using static BattleManager;
using System.Linq;
using UnityEngine;

public class BattleStarter : MonoBehaviour, IBattleStarter
{
    [Header("Batlle config")]
    [SerializeField] private BattleInitiator battleInitiator = BattleInitiator.Player;
    [SerializeField] private CharacterData[] battleParty;
    
    [Header("Runtime")]
    [SerializeField] private float reenterCooldown = 0.2f;

    public BattleInitiator BattleInitiator => battleInitiator;
    public CharacterData[] BattleParty => battleParty;

    private int _playerLayer;
    private int _enemyLayer;
    private float _nextAllowedBattleTime;
    
    private void Awake()
    {
        _playerLayer = LayerMask.NameToLayer("Player");
        _enemyLayer = LayerMask.NameToLayer("Enemy");
        _nextAllowedBattleTime = 0f;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (Time.time < _nextAllowedBattleTime) return;

        var otherLayer = other.gameObject.layer;
        var validTargetLayer = battleInitiator == BattleInitiator.Player
            ? otherLayer == _enemyLayer
            : otherLayer == _playerLayer;

        if (!validTargetLayer)
            return;

        TryStartBattleWith(other.gameObject);
    } 

    private bool TryStartBattleWith(GameObject other)
    {
        if (Time.time < _nextAllowedBattleTime)
            return false;

        if (BattleManager.Instance.IsBattleRunning) return false;
        
        var target = other.GetComponentInParent<IBattleStarter>();
        if (target == null || ReferenceEquals(target, this)) return false;

        var ownParty = GetAliveParty(BattleParty);
        var targetParty = GetAliveParty(target.BattleParty);
        if (ownParty.Length == 0 || targetParty.Length == 0) return false;
        
        CharacterData playerData;
        CharacterData[] enemies;

        if (BattleInitiator == BattleInitiator.Player)
        {
            playerData = ownParty[0];
            enemies = targetParty;
        }
        else
        {
            playerData = targetParty[0];
            enemies = ownParty;
        }

        BattleManager.Instance.StartBattle(playerData, enemies, BattleInitiator);
        _nextAllowedBattleTime = Time.time + Mathf.Max(0.01f, reenterCooldown);
        return true;
    }

    private static CharacterData[] GetAliveParty(CharacterData[] party)
    {
        if (party == null || party.Length == 0)
            return Array.Empty<CharacterData>();

        return party.Where(member => member != null).ToArray();
    }
}
