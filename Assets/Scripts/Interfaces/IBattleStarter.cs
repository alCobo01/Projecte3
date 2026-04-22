public interface IBattleStarter
{
    BattleManager.BattleInitiator BattleInitiator { get; }
    CharacterData[] BattleParty { get; }
}
