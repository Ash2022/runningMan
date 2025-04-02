using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Person
{
    public int Color;
}

[System.Serializable]
public class PersonQueue
{
    public List<Person> People;
}

[System.Serializable]
public class ObstacleUnit
{
    public int Color;
}

[System.Serializable]
public class Obstacle
{
    public List<ObstacleUnit> Units;
}

[System.Serializable]
public class LevelData
{
    public List<PersonQueue> Queues;
    public List<Obstacle> Obstacles;
}

public class GeminiLevelEditor : EditorWindow
{
    private int numQueues = 3;
    private int obstacleSize = 3;
    private int numPeoplePerQueue = 1;
    private int numColors = 3;
    private int numObstacles = 1;

    private LevelData currentLevelData;

    private float personSize = 0.5f;
    private float personSpacing = 0.1f;
    private float obstacleSpacing = 0.5f;
    private float unitVerticalSpacing = 0.2f;

    private List<Rect> personRects = new List<Rect>();
    private List<Tuple<int, int>> personIndices = new List<Tuple<int, int>>(); // QueueIndex, PersonIndex

    private List<Rect> obstacleUnitRects = new List<Rect>();
    private List<Tuple<int, int>> obstacleUnitIndices = new List<Tuple<int, int>>(); // ObstacleIndex, UnitIndex

    [MenuItem("Window/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<GeminiLevelEditor>("Level Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Editor Settings", EditorStyles.boldLabel);

        numQueues = EditorGUILayout.IntSlider("Number of Queues", numQueues, 3, 8);
        obstacleSize = EditorGUILayout.IntSlider("Obstacle Size", obstacleSize, 3, 8);
        numPeoplePerQueue = EditorGUILayout.IntSlider("People per Queue", numPeoplePerQueue, 1, 10);
        numColors = EditorGUILayout.IntSlider("Number of Colors", numColors, 3, 8);
        numObstacles = EditorGUILayout.IntSlider("Number of Obstacles", numObstacles, 1, 25);

        if (GUILayout.Button("Generate"))
        {
            GenerateLevelData();
            SceneView.RepaintAll();
        }

        if (currentLevelData != null)
        {
            GUILayout.Label("Current Level Data:", EditorStyles.boldLabel);
            // You could add more UI here to display or modify the data directly if needed
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (currentLevelData != null)
        {
            ClearVisualizationData();
            DrawQueues();
            DrawObstacles();
            HandleInput(sceneView);
        }
    }

    private void ClearVisualizationData()
    {
        personRects.Clear();
        personIndices.Clear();
        obstacleUnitRects.Clear();
        obstacleUnitIndices.Clear();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void GenerateLevelData()
    {
        currentLevelData = new LevelData
        {
            Queues = new List<PersonQueue>(),
            Obstacles = new List<Obstacle>()
        };

        for (int i = 0; i < numQueues; i++)
        {
            PersonQueue queue = new PersonQueue { People = new List<Person>() };
            for (int j = 0; j < numPeoplePerQueue; j++)
            {
                queue.People.Add(new Person { Color = UnityEngine.Random.Range(0, numColors) });
            }
            currentLevelData.Queues.Add(queue);
        }

        for (int i = 0; i < numObstacles; i++)
        {
            Obstacle obstacle = new Obstacle { Units = new List<ObstacleUnit>() };
            for (int j = 0; j < obstacleSize; j++)
            {
                obstacle.Units.Add(new ObstacleUnit { Color = UnityEngine.Random.Range(0, numColors) });
            }
            currentLevelData.Obstacles.Add(obstacle);
        }
    }

    void DrawQueues()
    {
        if (currentLevelData == null || currentLevelData.Queues == null) return;

        float startX = 0f;
        float yOffset = 0f;

        for (int i = 0; i < currentLevelData.Queues.Count; i++)
        {
            PersonQueue queue = currentLevelData.Queues[i];
            float currentX = startX;

            for (int j = queue.People.Count - 1; j >= 0; j--)
            {
                Vector3 position = new Vector3(currentX, yOffset, 0f);
                Color color = GetColorFromInt(queue.People[j].Color, numColors);
                Handles.color = color;
                float size = personSize / 2f;
                Handles.DrawSolidDisc(position, Vector3.forward, size);

                // Store the bounds and indices for click detection
                Rect personRect = new Rect(position.x - size, position.y - size, personSize, personSize);
                personRects.Add(personRect);
                personIndices.Add(Tuple.Create(i, queue.People.Count - 1 - j));

                currentX += personSize + personSpacing;
            }
            yOffset -= personSize + personSpacing;
            startX = 0f;
        }
    }

    void DrawObstacles()
    {
        if (currentLevelData == null || currentLevelData.Obstacles == null) return;

        float startX = 5f;
        float currentObstacleX = startX;

        for (int i = currentLevelData.Obstacles.Count - 1; i >= 0; i--)
        {
            Obstacle obstacle = currentLevelData.Obstacles[i];
            float currentY = 0f;

            for (int j = 0; j < obstacle.Units.Count; j++)
            {
                Vector3 position = new Vector3(currentObstacleX, currentY, 0f);
                Color color = GetColorFromInt(obstacle.Units[j].Color, numColors);
                Handles.color = color;
                float size = personSize / 2f;
                Handles.DrawSolidDisc(position, Vector3.forward, size);

                // Store bounds and indices for obstacle units
                Rect unitRect = new Rect(position.x - size, position.y - size, personSize, personSize);
                obstacleUnitRects.Add(unitRect);
                obstacleUnitIndices.Add(Tuple.Create(currentLevelData.Obstacles.Count - 1 - i, j));

                currentY += personSize + unitVerticalSpacing;
            }
            currentObstacleX += personSize + obstacleSpacing;
        }
    }

    Color GetColorFromInt(int colorIndex, int numColors)
    {
        switch (colorIndex % numColors)
        {
            case 0: return Color.red;
            case 1: return Color.green;
            case 2: return Color.blue;
            case 3: return Color.yellow;
            case 4: return Color.cyan;
            case 5: return Color.magenta;
            case 6: return new Color(1f, 0.5f, 0f); // Orange
            case 7: return Color.gray;
            default: return Color.white;
        }
    }

    void HandleInput(SceneView sceneView)
    {
        Event guiEvent = Event.current;
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
        {
            Vector2 mousePosition = guiEvent.mousePosition;
            // Convert mouse position to world space
            Ray worldRay = HandleUtility.GUIPointToWorldRay(mousePosition);
            Plane plane = new Plane(Vector3.forward, Vector3.zero); // Assuming visualization is on the XY plane
            if (plane.Raycast(worldRay, out float distance))
            {
                Vector3 worldPoint = worldRay.GetPoint(distance);

                // Check for clicks on people
                for (int i = 0; i < personRects.Count; i++)
                {
                    if (personRects[i].Contains(worldPoint))
                    {
                        int queueIndex = personIndices[i].Item1;
                        int personIndex = personIndices[i].Item2;
                        CyclePersonColor(queueIndex, personIndex);
                        guiEvent.Use(); // Consume the event
                        return;
                    }
                }

                // Check for clicks on obstacle units
                for (int i = 0; i < obstacleUnitRects.Count; i++)
                {
                    if (obstacleUnitRects[i].Contains(worldPoint))
                    {
                        int obstacleIndex = obstacleUnitIndices[i].Item1;
                        int unitIndex = obstacleUnitIndices[i].Item2;
                        CycleObstacleUnitColor(obstacleIndex, unitIndex);
                        guiEvent.Use(); // Consume the event
                        return;
                    }
                }
            }
        }
    }

    void CyclePersonColor(int queueIndex, int personIndex)
    {
        if (currentLevelData != null && queueIndex < currentLevelData.Queues.Count && personIndex < currentLevelData.Queues[queueIndex].People.Count)
        {
            currentLevelData.Queues[queueIndex].People[personIndex].Color = (currentLevelData.Queues[queueIndex].People[personIndex].Color + 1) % numColors;
            SceneView.RepaintAll();
        }
    }

    void CycleObstacleUnitColor(int obstacleIndex, int unitIndex)
    {
        if (currentLevelData != null && obstacleIndex < currentLevelData.Obstacles.Count && unitIndex < currentLevelData.Obstacles[obstacleIndex].Units.Count)
        {
            currentLevelData.Obstacles[obstacleIndex].Units[unitIndex].Color = (currentLevelData.Obstacles[obstacleIndex].Units[unitIndex].Color + 1) % numColors;
            SceneView.RepaintAll();
        }
    }
}