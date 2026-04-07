using System.Linq;
using UnityEngine;

public class BattleStarter : MonoBehaviour, IBattleStarter
{
    [SerializeField] private BattleManager.BattleInitiator battleInitiator = BattleManager.BattleInitiator.Player;
    [SerializeField] private CharacterData[] battleParty;

    public BattleManager.BattleInitiator BattleInitiator => battleInitiator;
    public CharacterData[] BattleParty => battleParty;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryStartBattleWith(other);
    }

    private bool TryStartBattleWith(Collider2D other)
    {
        if (!other) return false;
        if (BattleManager.Instance == null) return false;
        if (BattleManager.Instance.IsBattleRunning) return false;
        
        var target = other.GetComponentInParent<IBattleStarter>();
        if (target == null || (BattleStarter)target == this) return false;

        var ownParty = GetAliveParty(BattleParty);
        var targetParty = GetAliveParty(target.BattleParty);
        
        CharacterData playerData;
        CharacterData[] enemies;

        if (BattleInitiator == BattleManager.BattleInitiator.Player)
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
        return true;
    }

    private static CharacterData[] GetAliveParty(CharacterData[] party)
    {
        if (party == null || party.Length == 0)
            return System.Array.Empty<CharacterData>();

        return party.Where(member => member).ToArray();
    }
}
