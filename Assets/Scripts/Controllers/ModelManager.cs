using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ModelManager : MonoBehaviour
{
    [SerializeField] private List<TextAsset> levelJsonFiles;
    private List<ORG_LevelData> allLevels = new List<ORG_LevelData>();

    public void Init()
    {
        allLevels.Clear();

        foreach (var json in levelJsonFiles)
        {
            if (json != null)
            {
                ORG_LevelData level = JsonConvert.DeserializeObject<ORG_LevelData>(json.text);
                allLevels.Add(level);
            }
        }

        Debug.Log($"[ModelManager] Loaded {allLevels.Count} levels.");
    }

    public ORG_LevelData GetLevel(int index)
    {
        if (index >= 0 && index < allLevels.Count)
        {
            // Deep copy using Newtonsoft.Json
            string json = JsonConvert.SerializeObject(allLevels[index]);
            return JsonConvert.DeserializeObject<ORG_LevelData>(json);
        }

        Debug.LogWarning("[ModelManager] Invalid level index requested.");
        return null;
    }

    public int GetNumLevels()
    {
        return allLevels.Count;
    }
}
