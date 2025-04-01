using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
    public List<List<int>> peopleQueues;
    public List<ObstacleData> obstacles;
    public int horizon;
}


[System.Serializable]
public class ObstacleData
{
    public List<int> units;
    public int gapToNext = 1;
    public int turnsToHitQueue; // new field
    public List<bool> unitsResolved;

    public void InitializeResolvedFlags()
    {
        unitsResolved = new List<bool>();
        for (int i = 0; i < units.Count; i++)
        {
            unitsResolved.Add(false);
        }
    }
}