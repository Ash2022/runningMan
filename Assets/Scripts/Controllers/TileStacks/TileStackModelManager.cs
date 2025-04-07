using System;
using System.Collections.Generic;
using UnityEngine;

public class TileStacksModelManager : MonoBehaviour
{
    const string LAST_PLAYED_LEVEL = "LastPlayedLevel";
    
    public static TileStacksModelManager Instance;

    [SerializeField] List<Material> tileColorMaterials;
    [SerializeField] Material tilehiddenMaterial;
    [SerializeField] List<Color> tilesColors;

    [SerializeField] List<Sprite> buttonsIdle = new List<Sprite>();
    [SerializeField] List<Sprite> buttonsDown = new List<Sprite>();
    [SerializeField] List<Sprite> locksColorIndications = new List<Sprite>();

    List<int> unlocksIndexList = new List<int>();
    [SerializeField] List<Sprite> unlockBGs = new List<Sprite>();
    [SerializeField] List<Sprite> unlockFills = new List<Sprite>();

    [SerializeField] private List<TextAsset> levelFiles;

    private List<TilesStacksLevelData> levels = new List<TilesStacksLevelData>();
    public List<int> UnlocksIndexList { get => unlocksIndexList; set => unlocksIndexList = value; }

    public void Init()
    {
        unlocksIndexList.Add(4);//color 4 
        unlocksIndexList.Add(9);//hidden tile
        unlocksIndexList.Add(14);//color 5
        unlocksIndexList.Add(19);//hidden stack
        unlocksIndexList.Add(24);//color 6
        unlocksIndexList.Add(32);//color 7 



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
            s.lockColor = stack.lockColor;
            s.lockCount = stack.lockCount;
            s.tiles = new List<TileData>();

            foreach (var tile in stack.tiles)
            {
                s.tiles.Add(new TileData
                {
                    colorIndex = tile.colorIndex,
                    startHidden = tile.startHidden
                });
            }

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

    public Material GetTileMaterial(int index)
    {
        return tileColorMaterials[index];
    }

    public Material GetHiddenMaterial()
    {
        return tilehiddenMaterial;
    }

    public Color GetTileColor(int index)
    {
        return tilesColors[index];
    }


    public Sprite GetButtonImage(int index, bool down)
    {
        if(down)
            return buttonsDown[index];
        else 
            return buttonsIdle[index];
    }

    public Sprite GetLocksIndication(int index)
    {
        return locksColorIndications[index];
    }

    public Sprite GetUnlockImage(int index, bool BGImage)
    {
        if (BGImage)
            return unlockBGs[index];
        else
            return unlockFills[index];
    }

    public int GetLastPlayedLevel()
    {
        return PlayerPrefs.GetInt(LAST_PLAYED_LEVEL, -1);
    }

    public void SetLastPlayedLevel(int level)
    {
        PlayerPrefs.SetInt(LAST_PLAYED_LEVEL, level);
    }

    internal int GetUnlock(int currLevelIndex)
    {
        int index = -1;

        if (unlocksIndexList.Contains(currLevelIndex))
            index = unlocksIndexList.FindIndex(x => x.Equals(currLevelIndex));

        return index;
    }
}
