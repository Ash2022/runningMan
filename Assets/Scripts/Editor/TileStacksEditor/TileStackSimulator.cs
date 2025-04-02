using System.Collections.Generic;
using UnityEngine;

public class SimulationReport
{
    public int successes;
    public int failures;
    public int totalStepsInWins;

    public float AverageStepsInWins => successes > 0 ? (float)totalStepsInWins / successes : 0f;
}

public static class TileStacksSimulator
{
    public static SimulationReport RunSimulations(TilesStacksLevelData levelData, int iterations)
    {
        int success = 0;
        int failure = 0;
        int totalStepsInWins = 0;

        for (int i = 0; i < iterations; i++)
        {
            bool win = SimulateOne(levelData, out int stepsUsed);
            if (win)
            {
                success++;
                totalStepsInWins += stepsUsed;
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
            totalStepsInWins = totalStepsInWins
        };
    }


    private static bool SimulateOne(TilesStacksLevelData original, out int stepsUsed)
    {
        List<List<int>> stacks = new List<List<int>>();
        foreach (var stack in original.stacks)
        {
            stacks.Add(new List<int>(stack.tiles));
        }

        int turnsLeft = original.numTurns;
        stepsUsed = 0;

        while (turnsLeft > 0)
        {
            HashSet<int> topColors = new HashSet<int>();
            foreach (var stack in stacks)
            {
                if (stack.Count > 0)
                    topColors.Add(stack[stack.Count - 1]);
            }

            if (topColors.Count == 0)
                return true; // Win

            int colorToClick = PickRandomFromSet(topColors);

            bool removedAny;
            do
            {
                removedAny = false;
                for (int i = 0; i < stacks.Count; i++)
                {
                    if (stacks[i].Count > 0 && stacks[i][stacks[i].Count - 1] == colorToClick)
                    {
                        stacks[i].RemoveAt(stacks[i].Count - 1);
                        removedAny = true;
                    }
                }
            } while (removedAny);

            stepsUsed++;
            turnsLeft--;
        }

        // Check if any tiles remain
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
        return -1; // should never happen
    }
}
