using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimulationReport
{
    public int successes;
    public int failures;
    public int totalStepsInWins;
    public int bestStepsInWin;
    public int worstStepsInWin;

    public float AverageStepsInWins => successes > 0 ? (float)totalStepsInWins / successes : 0f;
}

public static class TileStacksSimulator
{
    public static SimulationReport RunSimulations(TilesStacksLevelData levelData, int iterations)
    {
        int success = 0;
        int failure = 0;
        int totalStepsInWins = 0;
        int bestStepsInWin = int.MaxValue;
        int worstStepsInWin = int.MinValue;

        for (int i = 0; i < iterations; i++)
        {
            bool win = SimulateOne(levelData, out int stepsUsed);
            if (win)
            {
                success++;
                totalStepsInWins += stepsUsed;

                if (stepsUsed < bestStepsInWin)
                    bestStepsInWin = stepsUsed;

                if (stepsUsed > worstStepsInWin)
                    worstStepsInWin = stepsUsed;
            }
            else
            {
                failure++;
            }
        }

        return new SimulationReport
        {
            successes = success,
            failures = failure,
            totalStepsInWins = totalStepsInWins,
            bestStepsInWin = success > 0 ? bestStepsInWin : 0,
            worstStepsInWin = success > 0 ? worstStepsInWin : 0
        };
    }


    private static bool SimulateOne(TilesStacksLevelData original, out int stepsUsed)
    {
        List<List<TileData>> stacks = new List<List<TileData>>();
        List<StackData> stackMetadata = new List<StackData>();

        foreach (var stack in original.stacks)
        {
            var tileCopy = new List<TileData>();
            foreach (var tile in stack.tiles)
            {
                tileCopy.Add(new TileData
                {
                    colorIndex = tile.colorIndex,
                    startHidden = tile.startHidden
                });
            }

            stacks.Add(tileCopy);

            stackMetadata.Add(new StackData
            {
                lockCount = stack.lockCount,
                lockColor = stack.lockColor,
                lockType = stack.lockType,
                isLocked = stack.lockCount > 0,
                tiles = tileCopy
            });
        }

        Dictionary<int, int> collectedColors = new();
        Dictionary<int, int> removedThisTurn = new();
        for (int i = 0; i < 9; i++)
        {
            collectedColors[i] = 0;
            removedThisTurn[i] = 0;
        }

        stepsUsed = 0;
        int turnsLeft = original.numTurns;


        while (turnsLeft > 0)
        {
            for (int i = 0; i < 9; i++) removedThisTurn[i] = 0;
                       

            HashSet<int> topColors = new();
            for (int i = 0; i < stackMetadata.Count; i++)
            {
                if (stackMetadata[i].isLocked || stackMetadata[i].tiles.Count == 0)
                    continue;

                topColors.Add(stackMetadata[i].tiles[^1].colorIndex);
            }

            if (topColors.Count == 0)
            {
                return false;
            }

            int colorToClick = PickRandomFromSet(topColors);

            bool removedAny = false;
            for (int i = 0; i < stackMetadata.Count; i++)
            {
                if (stackMetadata[i].isLocked) continue;

                var tileList = stackMetadata[i].tiles;
                while (tileList.Count > 0 && tileList[^1].colorIndex == colorToClick)
                {
                    tileList.RemoveAt(tileList.Count - 1);
                    collectedColors[colorToClick]++;
                    removedThisTurn[colorToClick]++;
                    removedAny = true;
                }
            }

            if (removedAny)
            {
                UpdateStackLocks(stackMetadata, collectedColors, removedThisTurn);
                stepsUsed++;
                turnsLeft--;
            }

            bool allEmpty = stackMetadata.All(s => s.tiles.Count == 0);
            if (allEmpty) return true;
        }

        return stackMetadata.All(s => s.tiles.Count == 0);
    }

    private static void UpdateStackLocks(List<StackData> stacks, Dictionary<int, int> collectedColors, Dictionary<int, int> removedThisTurn)
    {
        foreach (var stack in stacks)
        {
            if (stack.lockCount <= 0)
            {
                stack.isLocked = false;
                continue;
            }

            var data = (stack.lockType == LockType.SngPl) ? removedThisTurn : collectedColors;

            if (stack.lockColor == -1)
            {
                int total = data.Values.Sum();
                stack.isLocked = total < stack.lockCount;
            }
            else
            {
                stack.isLocked = data[stack.lockColor] < stack.lockCount;
            }
        }
    }


    private static int PickRandomFromSet(HashSet<int> set)
    {
        int index = Random.Range(0, set.Count);
        foreach (var val in set)
        {
            if (index == 0)
                return val;
            index--;
        }
        return -1;
    }
}