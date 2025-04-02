using System;
using System.Collections.Generic;
using UnityEngine;

public static class ObstacleSolver
{
    public static void RunSimulation(ORG_LevelData levelData, int numIterations)
    {
        int wins = 0;
        int losses = 0;
        System.Random rng = new System.Random();

        for (int run = 0; run < numIterations; run++)
        {
            var log = $"=== Simulation {run + 1}/{numIterations} ===\n";

            var queues = new List<List<int>>();
            foreach (var q in levelData.peopleQueues)
                queues.Add(new List<int>(q));

            var activeObstacles = new List<(int distance, ORG_ObstacleData data, int originalIndex, List<int> originalUnits)>();
            int spawnPointer = 0;
            int turnsSinceLastSpawn = 0;

            if (levelData.obstacles.Count > 0)
            {
                var first = CloneObstacle(levelData.obstacles[0]);
                activeObstacles.Add((levelData.horizon, first, 0, new List<int>(first.units)));
                spawnPointer = 1;
            }

            int turn = 0;

            while (true)
            {
                turn++;
                log += $"--- Turn {turn} ---\n";

                int queueIndex = rng.Next(queues.Count);
                List<int> queue = queues[queueIndex];

                int peopleToPlay = levelData.obstacles[0].units.Count;
                List<int> currentPeople = new();
                for (int i = 0; i < peopleToPlay && queue.Count > 0; i++)
                {
                    int person = queue[^1];
                    queue.RemoveAt(queue.Count - 1);
                    currentPeople.Add(person);
                }

                log += $"Clicked Queue {queueIndex}:\n";

                foreach (int person in currentPeople)
                {
                    bool resolved = false;
                    for (int o = 0; o < activeObstacles.Count; o++)
                    {
                        var (dist, data, originalIndex, originalUnits) = activeObstacles[o];
                        for (int u = 0; u < data.units.Count; u++)
                        {
                            if (data.units[u] == person)
                            {
                                int absoluteIndex = originalUnits.IndexOf(data.units[u]);
                                log += $"  Person color {ColorName(person)} matched Obstacle {originalIndex}, Unit {absoluteIndex}";
                                data.units.RemoveAt(u);
                                resolved = true;
                                break;
                            }
                        }
                        if (resolved) break;
                    }
                    if (!resolved)
                        log += $"  Person color {ColorName(person)} matched nothing (destroyed)";
                }

                for (int i = 0; i < activeObstacles.Count; i++)
                    activeObstacles[i] = (activeObstacles[i].distance - 1, activeObstacles[i].data, activeObstacles[i].originalIndex, activeObstacles[i].originalUnits);

                if (activeObstacles.Count > 0)
                {
                    string activeInfo = "Active Obstacles: ";
                    foreach (var (dist, _, originalIndex, _) in activeObstacles)
                        activeInfo += $"[Obstacle {originalIndex} at distance {dist}] ";
                    log += activeInfo + "\n";
                }

                for (int i = activeObstacles.Count - 1; i >= 0; i--)
                {
                    if (activeObstacles[i].data.units.Count == 0)
                    {
                        log += $"  ✅ Obstacle {activeObstacles[i].originalIndex} completed\n";
                        activeObstacles.RemoveAt(i);
                    }
                }

                if (activeObstacles.Count > 0 && activeObstacles[0].distance <= 0)
                {
                    log += "Result: ❌ Lose - Obstacle hit the queue.\n";
                    losses++;
                    break;
                }

                if (spawnPointer >= levelData.obstacles.Count && activeObstacles.Count == 0)
                {
                    log += "Result: ✅ Win - All obstacles cleared.\n";
                    wins++;
                    break;
                }

                turnsSinceLastSpawn++;
                if (spawnPointer < levelData.obstacles.Count)
                {
                    int requiredGap = levelData.obstacles[spawnPointer - 1].gapToNext;
                    if (turnsSinceLastSpawn >= requiredGap)
                    {
                        var next = CloneObstacle(levelData.obstacles[spawnPointer]);
                        activeObstacles.Add((levelData.horizon, next, spawnPointer, new List<int>(next.units)));
                        spawnPointer++;
                        turnsSinceLastSpawn = 0;
                    }
                }

                if (activeObstacles.Count > 0)
                    log += $"End of Turn {turn} → Closest Obstacle at distance {activeObstacles[0].distance}\n";
            }

            Debug.Log(log);
        }

        Debug.Log($"=== Simulation Complete ===");
        Debug.Log($"Wins: {wins}, Losses: {losses}");
    }

    private static string ColorName(int id)
    {
        string[] names = { "red", "green", "blue", "yellow", "cyan", "orange", "magenta", "gray" };
        return names[id % names.Length];
    }

    private static ORG_ObstacleData CloneObstacle(ORG_ObstacleData original)
    {
        return new ORG_ObstacleData
        {
            units = new List<int>(original.units),
            gapToNext = original.gapToNext
        };
    }
}