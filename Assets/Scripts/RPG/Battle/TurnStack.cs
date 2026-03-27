using System.Collections.Generic;
using System.Linq;

public class TurnStack
{
    private readonly Stack<BattleUnit> _stack = new();
    public bool IsEmpty => _stack.Count == 0;

    public void Build(IEnumerable<BattleUnit> units)
    {
        _stack.Clear();
        foreach (var unit in units.Where(u => !u.IsDead).OrderBy(u => u.Data.speed))
            _stack.Push(unit);
    }

    public void Rebuild(IEnumerable<BattleUnit> livingUnits) => Build(livingUnits);

    public BattleUnit PopCurrent()
        => _stack.Count > 0 ? _stack.Pop() : null;
}