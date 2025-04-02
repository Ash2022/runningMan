using System;
using System.Collections.Generic;
using UnityEngine;

public class TileStacksModelManager : MonoBehaviour
{
    public static TileStacksModelManager Instance;

    [SerializeField] private List<TextAsset> levelFiles;

    private List<TilesStacksLevelData> levels = new List<TilesStacksLevelData>();

    public void Init()
    {
        levels.Clear();
        foreach (var file in levelFiles)
        {
            TilesStacksLevelData level = JsonUtility.FromJson<TilesStacksLevelData>(file.text);
            levels.Add(level);
        }
    }

    public TilesStacksLevelData GetLevel(int index)
    {
        TilesStacksLevelData original = levels[index];
        TilesStacksLevelData copy = new TilesStacksLevelData();
        copy.numTurns = original.numTurns;
        copy.stacks = new List<StackData>();

        foreach (var stack in original.stacks)
        {
            StackData s = new StackData();
            s.position = stack.position;
            s.tiles = new List<int>(stack.tiles);
            copy.stacks.Add(s);
        }

        return copy;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    internal int GetNumLevels()
    {
        return levels.Count;
    }
}
