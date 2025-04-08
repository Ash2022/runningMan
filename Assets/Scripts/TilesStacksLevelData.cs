using System;
using System.Collections.Generic;
using UnityEngine;

public enum LockType
{
    Accum,
    SngPl
}

[System.Serializable]
public class StackData
{
    public List<TileData> tiles = new List<TileData>();
    public Vector2 position;
    public int lockColor;
    public int lockCount;
    public LockType lockType = LockType.Accum;
    public bool isLocked=false;
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
