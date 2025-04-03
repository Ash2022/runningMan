using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StackData
{
    public List<int> tiles = new List<int>();
    public Vector2 position; // X = horizontal, Y = Z (depth)
    public int lockCount = 0;
    public int lockColor = 0;
}

[System.Serializable]
public class TilesStacksLevelData
{
    public int numTurns;
    public List<StackData> stacks = new List<StackData>();
}

