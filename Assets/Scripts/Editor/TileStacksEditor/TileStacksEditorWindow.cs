using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class TileStacksEditorWindow : EditorWindow
{
    private int numStacks = 10;
    private int tilesPerStack = 10;
    private int numTurns = 20;
    private int numColors = 6;
    private int numCols = 5;
    private float zOffset = 1.0f;
    private float editorYOffset = 0f;

    private List<StackData> stacks = new List<StackData>();
    private Vector2 scroll;

    private const float totalWidth = 600f;
    private const float tileWidth = 20f;
    private const float tileHeightFactor = 0.6f;
    private const float tilePadding = 2f;
    private const int maxStacks = 25;

    private float fragmentation = 0.5f;
    private int simulationIterations = 500;
    private SimulationReport simulationReport;

    [MenuItem("Tools/Tile Stacks Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<TileStacksEditorWindow>("Tile Stacks Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);

        numStacks = EditorGUILayout.IntSlider("Number of Stacks", numStacks, 1, maxStacks);
        tilesPerStack = EditorGUILayout.IntSlider("Tiles per Stack", tilesPerStack, 5, 25);
        numTurns = EditorGUILayout.IntSlider("Number of Turns", numTurns, 10, 30);
        numColors = EditorGUILayout.IntSlider("Number of Colors", numColors, 3, 9);
        numCols = EditorGUILayout.IntSlider("Number of Columns", numCols, 1, maxStacks);
        zOffset = EditorGUILayout.Slider("Z Offset per Row", zOffset, 0.5f, 2.0f);
        editorYOffset = EditorGUILayout.Slider("Editor Y Offset", editorYOffset, -500f, 500f);

        if (GUILayout.Button("Generate Stacks"))
        {
            GenerateStacks();
        }

        if (GUILayout.Button("Save Level to JSON"))
        {
            SaveLevelToJson();
        }

        if (GUILayout.Button("Load Level from JSON"))
        {
            LoadLevelFromJson();
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Color Fill Tools", EditorStyles.boldLabel);

        fragmentation = EditorGUILayout.Slider("Fragmentation", fragmentation, 0f, 1f);

        if (GUILayout.Button("Randomize Colors"))
        {
            ApplyRandomColors();
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Simulation", EditorStyles.boldLabel);

        simulationIterations = EditorGUILayout.IntSlider("Iterations", simulationIterations, 100, 1000);

        if (GUILayout.Button("Run Simulation"))
        {
            TilesStacksLevelData level = new TilesStacksLevelData
            {
                numTurns = numTurns,
                stacks = stacks
            };

            simulationReport = TileStacksSimulator.RunSimulations(level, simulationIterations);
            Debug.Log($"Simulation Results: Successes = {simulationReport.successes}, Failures = {simulationReport.failures}, Avg Steps in Wins = {simulationReport.AverageStepsInWins:F2}");
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Edit Tiles", EditorStyles.boldLabel);
        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(600));
        DrawAllStacksFlipped();
        EditorGUILayout.EndScrollView();
    }

    private void GenerateStacks()
    {
        stacks.Clear();
        float colSpacing = (totalWidth * 0.75f) / numCols;

        for (int i = 0; i < numStacks; i++)
        {
            int row = i / numCols;
            int col = i % numCols;

            float x = col * colSpacing;
            if (row % 2 == 1)
                x += colSpacing / 2f;

            float z = row * zOffset;

            StackData stack = new StackData();
            stack.position = new Vector2(x, z);

            for (int j = 0; j < tilesPerStack; j++)
            {
                stack.tiles.Add(0);
            }

            stacks.Add(stack);
        }
    }

    private void DrawAllStacksFlipped()
    {
        float tileHeight = tileWidth * tileHeightFactor;

        // Find max Z (for visual Y inversion)
        float maxZ = 0f;
        foreach (var stack in stacks)
        {
            if (stack.position.y > maxZ)
                maxZ = stack.position.y;
        }

        foreach (var stack in stacks)
        {
            float x = stack.position.x;
            float visualYBase = (maxZ - stack.position.y) * 100f + editorYOffset;

            for (int i = 0; i < stack.tiles.Count; i++)
            {
                float drawX = x;
                float drawY = visualYBase - (i * (tileHeight + tilePadding));

                Rect rect = new Rect(drawX, drawY, tileWidth, tileHeight);
                int colorID = stack.tiles[i];

                Color prev = GUI.color;
                GUI.color = TileStacksUtils.GetColorFromID(colorID);

                if (GUI.Button(rect, ""))
                {
                    stack.tiles[i] = (colorID + 1) % numColors;
                }

                GUI.color = prev;
            }
        }

        GUILayout.Space(1000); // scroll buffer
    }



    private void SaveLevelToJson()
    {
        if (simulationReport == null)
        {
            Debug.LogWarning("Please run simulation before saving to get proper metadata in filename.");
            return;
        }

        var data = new TilesStacksLevelData
        {
            numTurns = numTurns,
            stacks = stacks
        };

        // Calculate tile and color metadata
        int totalTiles = 0;
        HashSet<int> colorSet = new HashSet<int>();
        foreach (var stack in stacks)
        {
            totalTiles += stack.tiles.Count;
            foreach (int color in stack.tiles)
            {
                colorSet.Add(color);
            }
        }

        int numStacks = stacks.Count;
        int numColorsUsed = colorSet.Count;
        float winRate = (float)simulationReport.successes / (simulationReport.successes + simulationReport.failures) * 100f;
        float avgMoves = simulationReport.AverageStepsInWins;

        string fileName = $"T_{totalTiles}_S_{numStacks}_C_{numColorsUsed}_W_{winRate:F1}_AVGM_{avgMoves:F1}.json";

        var path = EditorUtility.SaveFilePanel("Save Level JSON", "", fileName, "json");
        if (string.IsNullOrEmpty(path)) return;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log("Saved level to: " + path);
    }

    private void LoadLevelFromJson()
    {
        var path = EditorUtility.OpenFilePanel("Load Level JSON", "", "json");
        if (string.IsNullOrEmpty(path)) return;

        string json = File.ReadAllText(path);
        TilesStacksLevelData data = JsonUtility.FromJson<TilesStacksLevelData>(json);

        if (data != null)
        {
            numTurns = data.numTurns;
            stacks = data.stacks ?? new List<StackData>();
            numStacks = stacks.Count;
            tilesPerStack = stacks.Count > 0 ? stacks[0].tiles.Count : 10;
        }

        Debug.Log("Loaded level from: " + path);
    }

    private void ApplyRandomColors()
    {
        foreach (var stack in stacks)
        {
            int currentColor = Random.Range(0, numColors);

            for (int i = 0; i < stack.tiles.Count; i++)
            {
                if (Random.value < fragmentation)
                {
                    currentColor = Random.Range(0, numColors);
                }

                stack.tiles[i] = currentColor;
            }
        }

        Repaint();
    }


}
