using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnStack
{
    private readonly Dictionary<BattleUnit, int> _turnPoints = new();
    private readonly Queue<BattleUnit> _currentBatch = new();
    private const int ActionThreshold = 100;

    public bool IsEmpty => _currentBatch.Count == 0;

    public void Build(IEnumerable<BattleUnit> units, BattleUnit firstUnit = null)
    {
        _turnPoints.Clear();
        _currentBatch.Clear();

        var battleUnits = units.ToList();
        foreach (var unit in battleUnits.Where(u => !u.IsDead))
            _turnPoints[unit] = 0;

        // Give the first unit an immediate turn by filling their points
        if (firstUnit is { IsDead: false })
            _turnPoints[firstUnit] = ActionThreshold;

        GenerateNextBatch(battleUnits);
    }

    public void Rebuild(IEnumerable<BattleUnit> livingUnits) => GenerateNextBatch(livingUnits);
    
    public void Refresh(IEnumerable<BattleUnit> livingUnits)
    {
        _currentBatch.Clear();
        GenerateNextBatch(livingUnits);
    }
    
    public BattleUnit PopCurrent() => _currentBatch.Count > 0 ? _currentBatch.Dequeue() : null;

    private void GenerateNextBatch(IEnumerable<BattleUnit> units)
    {
        var living = units.Where(u => !u.IsDead).ToList();
        if (living.Count == 0) return;

        // A round consists of 'living.Count' number of actions
        var turnsToGenerate = living.Count; 
        
        for (var i = 0; i < turnsToGenerate; i++)
        {
            var nextUnit = SimulateUntilNextTurn(living, _turnPoints);
            if (nextUnit == null) break;

            _currentBatch.Enqueue(nextUnit);
            _turnPoints[nextUnit] -= ActionThreshold;
        }
    }

    private static BattleUnit SimulateUntilNextTurn(List<BattleUnit> living, Dictionary<BattleUnit, int> pointsDict)
    {
        var watchdog = 1000; // Prevent infinite loops if speed is 0
        
        while (watchdog > 0)
        {
            watchdog--;
            BattleUnit readyUnit = null;

            // Check if anyone already reached the threshold
            foreach (var unit in living)
            {
                pointsDict.TryAdd(unit, 0);

                if (pointsDict[unit] < ActionThreshold) continue;
                if (readyUnit == null || pointsDict[unit] > pointsDict[readyUnit])
                    readyUnit = unit;
                
                else if (pointsDict[unit] == pointsDict[readyUnit])
                {
                    // tiebreaker: higher effective speed wins
                    if (unit.GetStat(StatType.Speed) > readyUnit.GetStat(StatType.Speed))
                        readyUnit = unit;
                }
            }

            if (readyUnit != null) return readyUnit;

            // If no one is ready, everyone gains points equal to their speed
            foreach (var unit in living)
                pointsDict[unit] += Mathf.Max(1, unit.GetStat(StatType.Speed));
        }

        return living.FirstOrDefault();
    }

    public IEnumerable<BattleUnit> GetUpcomingTurns(int count, IEnumerable<BattleUnit> allLivingUnits)
    {
        var result = new List<BattleUnit>();
        var living = allLivingUnits.Where(u => !u.IsDead).ToList();
        
        if (living.Count == 0) return result;

        // Add already generated turns from the queue
        foreach (var unit in _currentBatch)
        {
            if (!unit.IsDead) result.Add(unit);
            if (result.Count >= count) return result;
        }

        // Clone the current points dictionary for future simulation
        var simPoints = new Dictionary<BattleUnit, int>(_turnPoints);
        while (result.Count < count)
        {
            var nextUnit = SimulateUntilNextTurn(living, simPoints);
            if (nextUnit == null) break;

            result.Add(nextUnit);
            simPoints[nextUnit] -= ActionThreshold;
        }

        return result;
    }
}
