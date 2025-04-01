using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ModelManager : MonoBehaviour
{
    [SerializeField] private List<TextAsset> levelJsonFiles;
    private List<LevelData> allLevels = new List<LevelData>();

    public void Init()
    {
        allLevels.Clear();

        foreach (var json in levelJsonFiles)
        {
            if (json != null)
            {
                LevelData level = JsonConvert.DeserializeObject<LevelData>(json.text);
                allLevels.Add(level);
            }
        }

        Debug.Log($"[ModelManager] Loaded {allLevels.Count} levels.");
    }

    public LevelData GetLevel(int index)
    {
        if (index >= 0 && index < allLevels.Count)
        {
            // Deep copy using Newtonsoft.Json
            string json = JsonConvert.SerializeObject(allLevels[index]);
            return JsonConvert.DeserializeObject<LevelData>(json);
        }

        Debug.LogWarning("[ModelManager] Invalid level index requested.");
        return null;
    }

    public int GetNumLevels()
    {
        return allLevels.Count;
    }
}
