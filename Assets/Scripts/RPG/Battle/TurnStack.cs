using System.Collections.Generic;
using System.Linq;

public class TurnStack
{
    private readonly Stack<BattleUnit> _stack = new();
    public bool IsEmpty => _stack.Count == 0;

    public void Build(IEnumerable<BattleUnit> units, BattleUnit firstUnit = null)
    {
        _stack.Clear();
        foreach (var unit in units.Where(u => !u.IsDead && u != firstUnit).OrderBy(u => u.Data.speed))
            _stack.Push(unit);

        if (firstUnit is { IsDead: false }) _stack.Push(firstUnit);
    }

    public void Rebuild(IEnumerable<BattleUnit> livingUnits) => Build(livingUnits);
    
    public BattleUnit PopCurrent() => _stack.Count > 0 ? _stack.Pop() : null;

    public IEnumerable<BattleUnit> GetUpcomingTurns(int count, IEnumerable<BattleUnit> allLivingUnits)
    {
        var result = new List<BattleUnit>();
        result.AddRange(_stack.Where(u => !u.IsDead));

        var living = allLivingUnits.Where(u => !u.IsDead).ToList();
        if (living.Count > 0)
        {
            var nextRound = living.OrderByDescending(u => u.Data.speed).ToList();
            while (result.Count < count)
            {
                result.AddRange(nextRound);
            }
        }

        return result.Take(count);
    }
}
