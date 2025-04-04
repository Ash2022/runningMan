using System.Collections.Generic;
using UnityEngine;

public class SimulationReport
{
    public int successes;
    public int failures;
    public int totalStepsInWins;
    public int bestStepsInWin = int.MaxValue;

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

        for (int i = 0; i < iterations; i++)
        {
            bool win = SimulateOne(levelData, out int stepsUsed);
            if (win)
            {
                success++;
                totalStepsInWins += stepsUsed;
                if (stepsUsed < bestStepsInWin)
                    bestStepsInWin = stepsUsed;
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
            bestStepsInWin = success > 0 ? bestStepsInWin : 0
        };
    }

    private static bool SimulateOne(TilesStacksLevelData original, out int stepsUsed)
    {
        List<List<TileData>> stacks = new List<List<TileData>>();
        List<int> lockCounts = new List<int>();
        List<int> lockColors = new List<int>();

        foreach (var stack in original.stacks)
        {
            List<TileData> tileCopy = new List<TileData>();
            foreach (var tile in stack.tiles)
            {
                tileCopy.Add(new TileData
                {
                    colorIndex = tile.colorIndex,
                    startHidden = tile.startHidden
                });
            }
            stacks.Add(tileCopy);
            lockCounts.Add(stack.lockCount);
            lockColors.Add(stack.lockColor);
        }

        Dictionary<int, int> collectedColors = new Dictionary<int, int>();
        for (int i = 0; i < 9; i++) collectedColors[i] = 0;

        int turnsLeft = original.numTurns;
        stepsUsed = 0;

        int counter = 0;

        while (turnsLeft > 0 && counter < 1000)
        {
            counter++;
            HashSet<int> topColors = new HashSet<int>();

            for (int i = 0; i < stacks.Count; i++)
            {
                if (stacks[i].Count == 0) continue;

                int lockColor = lockColors[i];
                int lockCount = lockCounts[i];
                if (lockCount > 0 && collectedColors[lockColor] < lockCount)
                    continue; // skip locked stack's top tile

                topColors.Add(stacks[i][stacks[i].Count - 1].colorIndex);
            }

            if (topColors.Count == 0)
                return false;

            int colorToClick = PickRandomFromSet(topColors);

            bool turnRemovedAny = false;
            bool removedAny;
            do
            {
                removedAny = false;
                for (int i = 0; i < stacks.Count; i++)
                {
                    if (stacks[i].Count == 0) continue;

                    int lockColor = lockColors[i];
                    int lockCount = lockCounts[i];

                    if (lockCount > 0 && collectedColors[lockColor] < lockCount)
                        continue;

                    TileData topTile = stacks[i][stacks[i].Count - 1];

                    if (topTile.colorIndex == colorToClick)
                    {
                        stacks[i].RemoveAt(stacks[i].Count - 1);
                        collectedColors[colorToClick]++;
                        removedAny = true;
                        turnRemovedAny = true;
                    }
                }
            } while (removedAny);

            if (turnRemovedAny)
            {
                stepsUsed++;
                turnsLeft--;
            }

            bool allEmpty = true;
            foreach (var s in stacks)
            {
                if (s.Count > 0)
                {
                    allEmpty = false;
                    break;
                }
            }

            if (allEmpty)
            {
                stepsUsed++; // final move counts
                return true;
            }
        }

        foreach (var stack in stacks)
        {
            if (stack.Count > 0)
                return false;
        }

        return true;
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