using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using static UnityEngine.UI.Image;
using Newtonsoft.Json;

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
    private const float tileWidth = 25f;
    private const float tileHeightFactor = 0.9f;
    private const float tilePadding = 2f;
    private const int maxStacks = 25;

    private float fragmentation = 0.5f;
    private int simulationIterations = 500;
    private SimulationReport simulationReport;

    private int contextStackIndex = -1;
    private int contextTileIndex = -1;

    private enum ViewMode { Data, Arrange }
    private ViewMode currentViewMode = ViewMode.Data;

    private bool[,] selectedGrid = new bool[4, 5];

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
        zOffset = EditorGUILayout.Slider("Z Offset per Row", zOffset, 0.5f, 2.0f);
        editorYOffset = EditorGUILayout.Slider("Editor Y Offset", editorYOffset, -500f, 500f);

        if (GUILayout.Button("Generate Stacks"))
        {
            GenerateStacks();

            bool[,] newGrid = new bool[4, 5]; // reset grid

            List<int> layoutOrder = new List<int>();

            // Define custom layout index pattern
            switch (numStacks)
            {
                case 3: layoutOrder.AddRange(new[] { 1, 2, 3 }); break;
                case 4: layoutOrder.AddRange(new[] { 1, 2, 3, 7 }); break;
                case 5: layoutOrder.AddRange(new[] { 1, 2, 3, 6, 8 }); break;
                case 6: layoutOrder.AddRange(new[] { 0, 1, 2, 3, 4, 7 }); break;
                case 7: layoutOrder.AddRange(new[] { 0, 1, 2, 3, 4, 6, 8 }); break;
                case 8: layoutOrder.AddRange(new[] { 0, 1, 2, 3, 4, 6, 7, 8 }); break;
                case 9: layoutOrder.AddRange(new[] { 0, 1, 2, 3, 4, 6, 7, 8, 12 }); break;
                case 10: layoutOrder.AddRange(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }); break;
                case 11: layoutOrder.AddRange(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 12 }); break;
                case 12: layoutOrder.AddRange(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 13 }); break;
                case 13: layoutOrder.AddRange(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 12, 13 }); break;
                case 14: layoutOrder.AddRange(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }); break;
                case 15: layoutOrder.AddRange(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }); break;
                case 16: layoutOrder.AddRange(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }); break;
                case 17: layoutOrder.AddRange(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }); break;
                case 18: layoutOrder.AddRange(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }); break;
                case 19: layoutOrder.AddRange(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }); break;
                case 20: layoutOrder.AddRange(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }); break;
                default:
                    //Debug.LogError(\"No layout defined for stack count: \" + numStacks);
                    return;
            }

            // Apply to selectedGrid (grid index to row/col)
            foreach (int index in layoutOrder)
            {
                int row = index / 5;
                int col = index % 5;

                int visualRow = 3 - row; // flip row vertically so 0 is bottom
                if (visualRow >= 0 && visualRow < 4)
                {
                    newGrid[visualRow, col] = true;
                }
            }

            selectedGrid = newGrid;

            Repaint();


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
            Debug.Log($"Simulation Results: Successes = {simulationReport.successes}, Failures = {simulationReport.failures}," +
                $" Avg Steps in Wins = {simulationReport.AverageStepsInWins:F2},Best = {simulationReport.bestStepsInWin}");
        }

        EditorGUILayout.LabelField("Edit Tiles", EditorStyles.boldLabel);
        currentViewMode = (ViewMode)EditorGUILayout.EnumPopup("View Mode", currentViewMode);

        if (currentViewMode == ViewMode.Data)
        {
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(600));
            DrawAllStacksFlipped();
            EditorGUILayout.EndScrollView();
        }
        else
        {
            
            DrawStackArrangeView();
        }
        
    }

    private void DrawStackArrangeView()
    {
        float gridCellSize = 40f;
        float startX = 20f;
        float startY = 500f;

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                Rect cell = new Rect(startX + col * gridCellSize, startY + row * gridCellSize, gridCellSize, gridCellSize);
                Color prevColor = GUI.color;
                GUI.color = selectedGrid[row, col] ? Color.cyan : new Color(1f, 1f, 1f, 0.2f);
                if (GUI.Button(cell, ""))
                {
                    selectedGrid[row, col] = !selectedGrid[row, col];
                }
                GUI.color = prevColor;
            }
        }
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
            if (row % 2 == 1) x += colSpacing / 2f;
            float z = row * zOffset;

            StackData stack = new StackData
            {
                position = new Vector2(x, z),
                tiles = new List<TileData>()
            };

            for (int j = 0; j < tilesPerStack; j++)
            {
                stack.tiles.Add(new TileData { colorIndex = 0, startHidden = false });
            }

            stacks.Add(stack);
        }
    }

    private void DrawAllStacksFlipped()
    {
        float tileHeight = tileWidth * tileHeightFactor;
        float maxZ = 0f;
        foreach (var stack in stacks) maxZ = Mathf.Max(maxZ, stack.position.y);

        for (int s = 0; s < stacks.Count; s++)
        {
            StackData stack = stacks[s];
            float x = stack.position.x;
            float visualYBase = (maxZ - stack.position.y) * 100f + editorYOffset;

            for (int i = 0; i < stack.tiles.Count; i++)
            {
                float drawX = x;
                float drawY = visualYBase - (i * (tileHeight + tilePadding));

                Rect rect = new Rect(drawX, drawY, tileWidth, tileHeight);
                TileData tile = stack.tiles[i];

                Color prev = GUI.color;
                GUI.color = TileStacksUtils.GetColorFromID(tile.colorIndex);

                if (GUI.Button(rect, tile.startHidden ? "?" : ""))
                {
                    if (Event.current.button == 0)
                    {
                        tile.colorIndex = (tile.colorIndex + 1) % numColors;
                    }
                    else if (Event.current.button == 1)
                    {
                        contextStackIndex = s;
                        contextTileIndex = i;
                        ShowContextMenu();
                        Event.current.Use();
                    }
                }

                GUI.color = prev;
            }

            // After drawing all the tile buttons in the stack
            Rect lockRect = new Rect(x, visualYBase + 20f, tileWidth * 1.5f, 16f);
            string newText = EditorGUI.TextField(lockRect, stack.lockCount.ToString());

            if (int.TryParse(newText, out int newVal))
            {
                stack.lockCount = newVal;
            }

            // Color cycle box (square next to the lock textbox)
            Rect colorBox = new Rect(x + tileWidth * 1.6f, visualYBase + 20f, tileWidth, tileWidth * 0.6f);
            Color prevColor = GUI.color;
            GUI.color = TileStacksUtils.GetColorFromID(stack.lockColor);

            if (GUI.Button(colorBox, ""))
            {
                stack.lockColor = (stack.lockColor + 1) % numColors;
            }

            GUI.color = prevColor;

        }

        GUILayout.Space(1000);
    }

    private void SaveLevelToJson()
    {
        if (simulationReport == null)
        {
            Debug.LogWarning("Please run simulation before saving to get proper metadata in filename.");
            return;
        }

        List<Vector2Int> selectedPositions = GetSelectedArrangePositions();
        int selectedCount = selectedPositions.Count;

        if (selectedCount != stacks.Count)
        {
            Debug.LogError($"Grid selection mismatch! You selected {selectedCount} positions but have {stacks.Count} stacks.");
            return;
        }

        List<StackData> clonedStacks = new List<StackData>();

        for (int i = 0; i < stacks.Count; i++)
        {
            StackData original = stacks[i];
            Vector2Int gridPos = selectedPositions[i]; // bottom-left first

            Debug.Log($"Assigning Stack[{i}] to Grid Pos: {gridPos}");

            StackData clone = new StackData
            {
                position = new Vector2(gridPos.x, gridPos.y),
                lockColor = original.lockColor,
                lockCount = original.lockCount,
                tiles = new List<TileData>()
            };

            foreach (var tile in original.tiles)
            {
                clone.tiles.Add(new TileData
                {
                    colorIndex = tile.colorIndex,
                    startHidden = tile.startHidden
                });
            }

            clonedStacks.Add(clone);
        }

        var data = new TilesStacksLevelData
        {
            numTurns = numTurns,
            stacks = clonedStacks
        };

        // Metadata for filename
        int totalTiles = 0;
        HashSet<int> colorSet = new HashSet<int>();
        foreach (var stack in clonedStacks)
        {
            totalTiles += stack.tiles.Count;
            foreach (var tile in stack.tiles)
            {
                colorSet.Add(tile.colorIndex);
            }
        }

        int numStacks = clonedStacks.Count;
        int numColorsUsed = colorSet.Count;
        float winRate = (float)simulationReport.successes / (simulationReport.successes + simulationReport.failures) * 100f;
        float avgMoves = simulationReport.AverageStepsInWins;
        int best = simulationReport.bestStepsInWin;

        string fileName = $"T_{totalTiles}_S_{numStacks}_C_{numColorsUsed}_W_{winRate:F1}_AVGM_{avgMoves:F1}_Best{best}_.json";

        var path = EditorUtility.SaveFilePanel("Save Level JSON", "", fileName, "json");
        if (string.IsNullOrEmpty(path)) return;

        string json = JsonConvert.SerializeObject(data, Formatting.Indented,
        new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        File.WriteAllText(path, json);
        Debug.Log("Saved level to: " + path);
    }



    private void ShowContextMenu()
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Toggle Hidden At Start"), false, ToggleHiddenAtStart);
        menu.ShowAsContext();
    }

    private void ToggleHiddenAtStart()
    {
        if (contextStackIndex >= 0 && contextTileIndex >= 0 && contextStackIndex < stacks.Count && contextTileIndex < stacks[contextStackIndex].tiles.Count)
        {
            TileData tile = stacks[contextStackIndex].tiles[contextTileIndex];
            tile.startHidden = !tile.startHidden;
            Repaint();
        }
    }


    private int GetNumSelectedStackArrange()
    {
        int selectedCount = 0;
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                if (selectedGrid[row, col]) selectedCount++;
            }
        }

        return selectedCount;
    }

    private void LoadLevelFromJson()
    {
        var path = EditorUtility.OpenFilePanel("Load Level JSON", "", "json");
        if (string.IsNullOrEmpty(path)) return;

        string json = File.ReadAllText(path);
        TilesStacksLevelData data = JsonConvert.DeserializeObject<TilesStacksLevelData>(json);

        if (data != null)
        {
            numTurns = data.numTurns;
            stacks = data.stacks ?? new List<StackData>();
            numStacks = stacks.Count;
            tilesPerStack = stacks.Count > 0 ? stacks[0].tiles.Count : 10;
        }


        selectedGrid = new bool[4, 5];
        foreach (var stack in stacks)
        {
            Vector2Int gridPos = Vector2Int.RoundToInt(stack.position);
            if (gridPos.y >= 0 && gridPos.y < 4 && gridPos.x >= 0 && gridPos.x < 5)
            {
                int visualRow = 3 - gridPos.y;
                if (visualRow >= 0 && visualRow < 4 && gridPos.x >= 0 && gridPos.x < 5)
                {
                    selectedGrid[visualRow, gridPos.x] = true;
                }
            }

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

                stack.tiles[i].colorIndex = currentColor;
            }
        }

        Repaint();
    }


    private List<Vector2Int> GetSelectedArrangePositions()
    {
        List<Vector2Int> selected = new List<Vector2Int>();

        for (int row = 0; row < 4; row++)         // bottom to top
        {
            for (int col = 0; col < 5; col++)     // left to right
            {
                if (selectedGrid[3 - row, col])   // flip vertical so bottom row = index 0
                {
                    selected.Add(new Vector2Int(col, row));
                }
            }
        }

        return selected;
    }


}
