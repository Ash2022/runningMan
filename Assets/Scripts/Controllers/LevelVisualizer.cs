using UnityEngine;
using System.Collections.Generic;

public class LevelVisualizer : MonoBehaviour
{
    [SerializeField] private Transform uiRoot;
    [SerializeField] private GameObject queueButtonPrefab;
    [SerializeField] private Transform levelRoot;
    [SerializeField] private GameObject personPrefab;
    [SerializeField] private GameObject obstacleUnitPrefab;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private GameObject frameIndicationDividerPrefab;

    float queueXSpacing;
    float queueZSpacing;

    public Transform UiRoot { get => uiRoot; set => uiRoot = value; }
    public Transform LevelRoot { get => levelRoot; set => levelRoot = value; }
    public float QueueXSpacing { get => queueXSpacing; set => queueXSpacing = value; }
    public float QueueZSpacing { get => queueZSpacing; set => queueZSpacing = value; }

    public (List<List<PersonView>>, List<ObstacleView>) BuildLevel(
        ORG_LevelData levelData,
        float personOffsetZ = 0f,
        float obstacleOffsetZ = 20f)
    {
        var peopleViews = new List<List<PersonView>>();
        var obstacleViews = new List<ObstacleView>();

        int numQueues = levelData.peopleQueues.Count;
        float frameWidth = 6f; // X from -3 to +3

        float maxWidthPerQueue = frameWidth / numQueues;
        float personScale = Mathf.Min(maxWidthPerQueue * 0.7f, 1f);
        queueXSpacing = maxWidthPerQueue;
        queueZSpacing = personScale * 1.1f;
        float startX = -frameWidth / 2f + queueXSpacing / 2f;

        // Instantiate people queues
        for (int q = 0; q < numQueues; q++)
        {
            var queueViews = new List<PersonView>();
            List<int> queueData = levelData.peopleQueues[q];

            float baseX = startX + q * queueXSpacing;

            for (int i = queueData.Count - 1; i >= 0; i--)
            {
                float z = 15f - (queueData.Count - 1 - i) * queueZSpacing;
                Vector3 pos = new Vector3(baseX, 0, z);
                GameObject go = Instantiate(personPrefab, pos, Quaternion.identity, levelRoot);
                go.transform.localScale = new Vector3(personScale, 1, personScale);

                go.name = $"Unit_Color_{queueData[i]}_{Utils.GetColorNameFromId(queueData[i])}";

                PersonView view = go.GetComponent<PersonView>();
                view.SetColor(queueData[i]);
                queueViews.Add(view);
            }

            peopleViews.Add(queueViews);
        }

        // Instantiate obstacles
        float obstacleZGap = 1;
        float obstacleStartZ = obstacleOffsetZ;

        for (int o = 0; o < levelData.obstacles.Count; o++)
        {
            var obstacleData = levelData.obstacles[o];
            GameObject obstacleGO = new GameObject($"Obstacle_{o}");
            obstacleGO.transform.SetParent(levelRoot);

            float obstacleZ = obstacleStartZ + o * obstacleZGap;
            obstacleGO.transform.localPosition = new Vector3(0, 0, obstacleZ);

            ObstacleView obstacleView = obstacleGO.AddComponent<ObstacleView>();

            // ❗️NEW: build obstacle using center-aligned X
            obstacleView.Build(obstacleData.units,unitPrefab);

            obstacleViews.Add(obstacleView);
        }

        float buttonSize = Mathf.Clamp(375f / numQueues, 65f, 125f);

        for (int q = 0; q < numQueues; q++)
        {
            float baseX = startX + q * queueXSpacing;
            Vector3 worldPos = new Vector3(baseX, 0, 0);
            Vector3 screenPos = GameManager.Instance.WorldToRect(worldPos);

            GameObject button = Instantiate(queueButtonPrefab, uiRoot);
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchoredPosition = screenPos;
            rect.sizeDelta = new Vector2(buttonSize, buttonSize);

            int index = q;
            button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => GameManager.Instance.OnQueueClicked(index));
        }

        //set the frame divider
        GameObject frameDivider = Instantiate(frameIndicationDividerPrefab, levelRoot);
        //place in the middle between the person that is matchSize and +1
        int dividerPosition = levelData.obstacles[0].units.Count;

        float zPosition = (peopleViews[0][dividerPosition].transform.position.z + peopleViews[0][dividerPosition-1].transform.position.z)/2f;

        frameDivider.transform.position = new Vector3(0,0.5f,zPosition);

        return (peopleViews, obstacleViews);
    }

}
