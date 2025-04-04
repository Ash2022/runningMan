using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileStacksGameManager : MonoBehaviour
{
    const float TILES_FLY_DELAY = 0.15f;
    const float TILES_FLY_TIME = 0.75f;
    const float TILES_VERTICAL_OFFSET = 0.075f;

    public static TileStacksGameManager Instance;

    [SerializeField] RectTransform canvasRect;
    [SerializeField] private TileStacksLevelVisualizer levelVisualizer;
    [SerializeField] private TileStacksUIManager uiManager;

    private List<List<TileStacksTileView>> activeTiles;
    private TilesStacksLevelData activeLevel;
    private List<TileStacksStackView> stackViews;

    int levelIndex=0;
    bool gameActive = false;
    int numColorInLevel = 0;

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

    private void Start()
    {
        Init();
        BuildLevel();
    }

    private void Init()
    {
        TileStacksModelManager.Instance.Init();

        float baselineAspect = 9f / 16f; // your ideal aspect
        float baselineVerticalFOV = 50f;
        float targetAspect = Camera.main.aspect;

        // Convert baseline vertical FOV to radians
        float baselineVertFOVRad = baselineVerticalFOV * Mathf.Deg2Rad;

        // Compute horizontal FOV from vertical FOV and baseline aspect
        float baselineHorzFOVRad = 2f * Mathf.Atan(Mathf.Tan(baselineVertFOVRad / 2f) * baselineAspect);

        // Now compute the new vertical FOV that gives the same horizontal FOV at the new aspect
        float newVertFOVRad = 2f * Mathf.Atan(Mathf.Tan(baselineHorzFOVRad / 2f) / targetAspect);

        // Set the camera's vertical FOV in degrees
        Camera.main.fieldOfView = newVertFOVRad * Mathf.Rad2Deg;

        Debug.Log($"[FOV Adjust] Aspect: {targetAspect:F3}, Adjusted FOV: {Camera.main.fieldOfView:F2}");

    }


    public void BuildLevel()
    {
        activeLevel = TileStacksModelManager.Instance.GetLevel(levelIndex);
        (activeTiles, stackViews,numColorInLevel) = levelVisualizer.BuildLevel(activeLevel, TILES_VERTICAL_OFFSET);
        uiManager.SetTurns(activeLevel.numTurns);
        gameActive = true;
    }

    public void OnColorButtonClicked(int colorID, int buttonIndex, TileStacksColorButtonView clickedButton)
    {
        if (!gameActive)
            return;

        bool hasMatchingTopTile = false;

        for (int i = 0; i < activeLevel.stacks.Count; i++)
        {
            var stack = activeLevel.stacks[i];
            var tiles = stack.tiles;

            if (tiles.Count == 0) continue;

            // Skip locked stacks
            if (stack.lockCount > 0 && activeLevel.playedTiles[stack.lockColor] < stack.lockCount)
                continue;

            if (tiles[tiles.Count - 1].colorIndex == colorID)
            {
                hasMatchingTopTile = true;
                break;
            }
        }

        if (!hasMatchingTopTile)
        {
            Debug.Log("No matching top tiles for color " + colorID + " — skipping.");
            clickedButton.ShowButton();
            return;
        }

        Debug.Log("Color button clicked: " + colorID);

        bool removedAny = false;
        int flyingTiles = 0;

        for (int i = 0; i < activeLevel.stacks.Count; i++)
        {
            var stack = activeLevel.stacks[i];
            var tiles = stack.tiles;

            // Skip locked stacks
            if (stack.lockCount > 0 && activeLevel.playedTiles[stack.lockColor] < stack.lockCount)
                continue;

            float startDelay = 0;

            while (tiles.Count > 0 && tiles[tiles.Count - 1].colorIndex == colorID)
            {
                tiles.RemoveAt(tiles.Count - 1);               

                var view = activeTiles[i][activeTiles[i].Count - 1];
                activeTiles[i].RemoveAt(activeTiles[i].Count - 1);

                // After removing a tile, check if new top is a hidden tile that just became revealed
                if (tiles.Count > 0)
                {
                    var newTopTile = tiles[tiles.Count - 1];
                    var newTopView = activeTiles[i][activeTiles[i].Count - 1];

                    if (newTopTile.startHidden)
                    {
                        Debug.Log($"Hidden tile on stack {i} just got revealed — color: {newTopTile.colorIndex}");

                        // You now have both the data and the view:
                        // newTopTile  → TileData
                        // newTopView  → TileStacksTileView

                        // Optional reveal effect:
                        newTopView.RevealTileColor();
                    }
                }

                flyingTiles++;

                view.FlyTo(
                    new Vector3(TileStacksUtils.GetButtonWorldX(numColorInLevel, buttonIndex), flyingTiles*TILES_VERTICAL_OFFSET, -9.5f),
                    startDelay,
                    TILES_FLY_TIME,
                    () =>
                    {
                        view.PlayDestroyParticles();
                        flyingTiles--;

                        // ✅ Track played color count
                        activeLevel.playedTiles[colorID]++;
                        clickedButton.UpdateCounter(activeLevel.playedTiles[colorID]);

                        // ✅ Check if any locked stack is now unlocked (and only trigger once)
                        for (int si = 0; si < activeLevel.stacks.Count; si++)
                        {
                            var s = activeLevel.stacks[si];

                            if (s.lockCount > 0 &&
                                s.lockColor == colorID &&
                                activeLevel.playedTiles[colorID] == s.lockCount)
                            {
                                // ✅ This stack is now officially unlocked (first time)
                                Debug.Log($"Stack {si} unlocked! Required {s.lockCount} of color {s.lockColor}.");
                                // Optionally: animate / mark / notify

                                stackViews[si].UnlockStackCover();
                            }
                        }

                        if (flyingTiles == 0)
                        {
                            Debug.Log("Last tile animation finished.");
                            clickedButton.ShowButton();
                        }
                    });

                removedAny = true;
                startDelay += TILES_FLY_DELAY;
            }
        }

        if (removedAny)
        {
            

            activeLevel.numTurns--;
            uiManager.SetTurns(activeLevel.numTurns);
        }

        CheckGameOver();
    }



    private void CheckGameOver()
    {
        bool allCleared = true;
        foreach (var stack in activeLevel.stacks)
        {
            if (stack.tiles.Count > 0)
            {
                allCleared = false;
                break;
            }
        }

        if (allCleared)
        {
            Debug.Log("Game Over: You Win!");
            uiManager.ShowGameOver(true);
            gameActive = false;
            // Handle win state here
        }
        else if (activeLevel.numTurns <= 0)
        {
            Debug.Log("Game Over: You Lose!");
            uiManager.ShowGameOver(false);
            gameActive = false;
            // Handle lose state here
        }
    }

    public void EndGameScreenClicked(bool lastLevelWon)
    {
        StartCoroutine(ResetScene(()=>
        {
            if (lastLevelWon)
                // Advance to next level (or loop)
                levelIndex = (levelIndex + 1) % TileStacksModelManager.Instance.GetNumLevels();

            BuildLevel();
        }));

        
    }

    IEnumerator ResetScene(Action done)
    {
        // Cleanup old visuals

        int numItems = levelVisualizer.LevelRoot.childCount - 1;

        for (int i = numItems; i >= 0; i--)
            Destroy(levelVisualizer.LevelRoot.GetChild(i).gameObject);

        int numItems2 = levelVisualizer.UiRoot.childCount - 1;

        for (int i = numItems2; i >= 0; i--)
            Destroy(levelVisualizer.UiRoot.GetChild(i).gameObject);

      activeTiles.Clear();

        yield return new WaitForEndOfFrame();

        done?.Invoke();
    }

    public Vector2 WorldToRect(Vector3 world)
    {
        Vector2 myCurrentHeightWorld = Camera.main.WorldToScreenPoint(world);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, myCurrentHeightWorld, Camera.main, out var localPos);
        return localPos;
    }

    public Vector3 RectToWorld(Vector2 localRectPos)
    {
        Vector2 screenPos = RectTransformUtility.PixelAdjustPoint(localRectPos, canvasRect, canvasRect.GetComponentInParent<Canvas>());
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane));
        return worldPos;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.UpArrow))
            EndGameScreenClicked(true);

        if (Input.GetKeyUp(KeyCode.DownArrow))
            EndGameScreenClicked(false);


    }
}
