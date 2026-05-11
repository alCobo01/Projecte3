public interface IBattleStarter
{
    BattleInitiator BattleInitiator { get; }
    CharacterData[] BattleParty { get; }
}
