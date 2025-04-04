using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StackData
{
    public List<TileData> tiles = new List<TileData>();
    public Vector2 position; // X = horizontal, Y = Z (depth)
    public int lockCount = 0;
    public int lockColor = 0;
}

[Serializable]
public class TilesStacksLevelData
{
    public int numTurns;
    public List<StackData> stacks = new List<StackData>();

    [NonSerialized]
    public int[] playedTiles = new int[9]; // runtime only – not serialized
}

[System.Serializable]
public class TileData
{
    public int colorIndex;
    public bool startHidden;
}
