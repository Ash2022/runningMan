using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

public class LevelEditorWindow : EditorWindow
{
    private int numQueues = 3;
    private int numObstacles = 5;
    private int obstacleSize = 3;
    private int peoplePerSet = 2;
    private int numColors = 4;
    private int horizon = 5;

    private List<List<int>> peopleQueues = new();
    private List<ObstacleData> obstacles = new();

    private const int cellSize = 20;
    private int simulationIterations = 100;

    [MenuItem("Tools/Level Editor - Queues and Obstacles")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Queue Level Editor");
    }

    private void OnGUI()
    {
        DrawConfigPanel();

        GUILayout.Space(10);

        if (peopleQueues.Count > 0 && obstacles.Count > 0)
        {
            Rect gridRect = GUILayoutUtility.GetRect(position.width, position.height - 150);
            DrawLevelVisualization(gridRect);
        }
    }

    private void DrawConfigPanel()
    {
        GUILayout.Label("Level Configuration", EditorStyles.boldLabel);

        numQueues = EditorGUILayout.IntPopup("Number of Queues", numQueues, new[] { "3", "4", "5", "6", "7", "8" }, new[] { 3, 4, 5, 6, 7, 8 });
        obstacleSize = EditorGUILayout.IntPopup("Obstacle Size", obstacleSize, new[] { "3", "4", "5", "6", "7", "8" }, new[] { 3, 4, 5, 6, 7, 8 });
        peoplePerSet = EditorGUILayout.IntPopup("People per Queue (x ObstacleSize)", peoplePerSet, Enumerable.Range(1, 10).Select(x => x.ToString()).ToArray(), Enumerable.Range(1, 10).ToArray());
        numColors = EditorGUILayout.IntPopup("Number of Colors", numColors, new[] { "3", "4", "5", "6", "7", "8" }, new[] { 3, 4, 5, 6, 7, 8 });
        numObstacles = EditorGUILayout.IntPopup("Number of Obstacles", numObstacles, Enumerable.Range(1, 25).Select(x => x.ToString()).ToArray(), Enumerable.Range(1, 25).ToArray());
        horizon = EditorGUILayout.IntPopup("Horizon (Visible Obstacles)", horizon, Enumerable.Range(2, 7).Select(x => x.ToString()).ToArray(), Enumerable.Range(2, 7).ToArray());

        GUILayout.Space(10);
        if (GUILayout.Button("Generate"))
        {
            GenerateLevel();
        }

        if (GUILayout.Button("Save Level"))
        {
            SaveLevel();
        }

        if (GUILayout.Button("Load Level"))
        {
            LoadLevel();
        }

        GUILayout.Space(10);
        GUILayout.Label("Simulation", EditorStyles.boldLabel);
        simulationIterations = EditorGUILayout.IntSlider("Iterations", simulationIterations, 1, 1000);
        if (GUILayout.Button("Run Solver Simulation"))
        {
            var data = new LevelData
            {
                peopleQueues = peopleQueues,
                obstacles = obstacles,
                horizon = horizon
            };
            ObstacleSolver.RunSimulation(data, simulationIterations);
        }
    }

    private void GenerateLevel()
    {
        int peoplePerQueue = obstacleSize * peoplePerSet;

        // Generate people queues
        peopleQueues.Clear();
        for (int i = 0; i < numQueues; i++)
        {
            var queue = new List<int>();
            for (int j = 0; j < peoplePerQueue; j++)
                queue.Add(Random.Range(0, numColors));
            peopleQueues.Add(queue);
        }

        // Generate obstacles
        obstacles.Clear();
        for (int i = 0; i < numObstacles; i++)
        {
            var obs = new ObstacleData
            {
                units = new List<int>(),
                gapToNext = 1
            };
            for (int j = 0; j < obstacleSize; j++)
                obs.units.Add(Random.Range(0, numColors));
            obstacles.Add(obs);
        }
    }

    private void DrawLevelVisualization(Rect area)
    {
        Handles.BeginGUI();

        float spacing = 5f;
        float startX = area.xMin + 10;
        float startY = area.yMin + 10;

        // Draw people queues
        for (int q = 0; q < peopleQueues.Count; q++)
        {
            List<int> queue = peopleQueues[q];
            for (int i = 0; i < queue.Count; i++)
            {
                int colorId = queue[i];
                float x = startX + (i * (cellSize + spacing));
                float y = startY + (q * (cellSize + spacing));

                Rect rect = new Rect(x, y, cellSize, cellSize);
                EditorGUI.DrawRect(rect, GetColorFromID(colorId));

                if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    queue[i] = (queue[i] + 1) % numColors;
                    Event.current.Use();
                    Repaint();
                }
            }
        }

        // Draw obstacles (right to left)
        float obsStartX = area.xMax - (obstacles.Count * (cellSize + spacing)) - 10;
        float obsStartY = startY + peopleQueues.Count * (cellSize + spacing) + 40;

        for (int o = 0; o < obstacles.Count; o++)
        {
            ObstacleData obs = obstacles[o];
            float baseX = obsStartX + o * (cellSize + spacing);

            // Draw dropdown above obstacle for gapToNext
            Rect dropdownRect = new Rect(baseX, obsStartY - 25, cellSize, 20);
            obs.gapToNext = EditorGUI.IntField(dropdownRect, obs.gapToNext);

            for (int i = 0; i < obs.units.Count; i++)
            {
                int colorId = obs.units[i];
                float x = baseX;
                float y = obsStartY + i * (cellSize + spacing);

                Rect rect = new Rect(x, y, cellSize, cellSize);
                EditorGUI.DrawRect(rect, GetColorFromID(colorId));

                if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    obs.units[i] = (obs.units[i] + 1) % numColors;
                    Event.current.Use();
                    Repaint();
                }
            }
        }

        Handles.EndGUI();
    }

    private Color GetColorFromID(int id)
    {
        Color[] palette = {
            Color.red, Color.green, Color.blue, Color.yellow, Color.cyan,
            new Color(1f, 0.5f, 0f), Color.magenta, Color.gray
        };

        return palette[id % palette.Length];
    }

    private void SaveLevel()
    {
        var path = EditorUtility.SaveFilePanel("Save Level", "", "QueueLevel.json", "json");
        if (string.IsNullOrEmpty(path)) return;

        LevelData data = new LevelData
        {
            peopleQueues = peopleQueues,
            obstacles = obstacles,
            horizon = horizon
        };

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(path, json);
        Debug.Log($"[LevelEditor] Level saved to: {path}");
    }

    private void LoadLevel()
    {
        var path = EditorUtility.OpenFilePanel("Load Level", "", "json");
        if (string.IsNullOrEmpty(path)) return;

        string json = File.ReadAllText(path);
        LevelData data = JsonConvert.DeserializeObject<LevelData>(json);
        peopleQueues = data.peopleQueues;
        obstacles = data.obstacles;
        horizon = data.horizon;

        Repaint();
        Debug.Log($"[LevelEditor] Level loaded from: {path}");
    }
}
