using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileStacksGameManager : MonoBehaviour
{
    const float BASE_DESTROY_DELAY = 0.075f;
    const float DESTORY_DELAY_FALLOUT_RATE = 0.95f;

    
    const float TILES_FLY_DELAY = 0.1f;
    const float TILES_FLY_TIME = 0.5f;
    const float FLY_DELAY_FALLOUT_RATE = 0.95f;


    public const float TILES_VERTICAL_OFFSET = 0.075f;

    public static TileStacksGameManager Instance;

    [SerializeField] RectTransform canvasRect;
    [SerializeField] private TileStacksLevelVisualizer levelVisualizer;
    [SerializeField] private TileStacksUIManager uiManager;

    public GameOverView gameOverView;

    private List<List<TileStacksTileView>> activeTiles;
    private TilesStacksLevelData activeLevel;
    private List<TileStacksStackView> stackViews;

    public int levelIndex=0;
    bool gameActive = false;
    int numColorInLevel = 0;
    int levelStartTotalTiles = 0;//keeping this for progress bar - because when playing the model is removing the played tiles - so cant know how many were there.

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

        if (levelIndex == -1)
        {
            levelIndex = TileStacksModelManager.Instance.GetLastPlayedLevel();

            levelIndex++;

        }

        if(levelIndex == 0)
        {
            uiManager.ShowTutorialImage(true, levelIndex);            
        }
        else
            BuildLevel();
    }

    private void Init()
    {
        Application.targetFrameRate = 60;

        TileStacksModelManager.Instance.Init();

        float baselineAspect = 9f / 16f; // Target aspect
        float baselineOrthoSize = 5f;    // Desired ortho size at 9:16

        float targetAspect = Camera.main.aspect;

        // Adjust orthographic size based on aspect
        float adjustedOrthoSize = baselineOrthoSize * (baselineAspect / targetAspect);

        Camera.main.orthographicSize = adjustedOrthoSize;

        Debug.Log($"[Ortho Adjust] Aspect: {targetAspect:F3}, Adjusted OrthoSize: {adjustedOrthoSize:F2}");

    }


    public void BuildLevel()
    {
        activeLevel = TileStacksModelManager.Instance.GetLevel(levelIndex);
        (activeTiles, stackViews,numColorInLevel) = levelVisualizer.BuildLevel(activeLevel, TILES_VERTICAL_OFFSET);

        levelStartTotalTiles = activeLevel.stacks.Count * activeLevel.stacks[0].tiles.Count;

        uiManager.InitLevel(activeLevel,levelIndex);
        gameActive = true;
    }

    public void HideTutorialImage()
    {
        uiManager.ShowTutorialImage(false, 0);
        BuildLevel();

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
            SoundsManager.Instance.ButtonClick(false);
            activeLevel.numTurns--;
            uiManager.SetTurns(activeLevel.numTurns);
            CheckGameOver();
            return;
        }

        Debug.Log("Color button clicked: " + colorID);

        SoundsManager.Instance.ButtonClick(true);

        List<(TileStacksTileView view, float delay)> flights = CollectTileFlights(colorID, out bool removedAny);

        if (removedAny)
        {
            activeLevel.numTurns--;
            uiManager.SetTurns(activeLevel.numTurns);
        }

        AnimateTileFlights(flights, colorID, buttonIndex, clickedButton);
    }


    private void CheckGameOver()
    {

        if(!gameActive)
            return;

        bool allCleared = true;
        foreach (var stack in activeLevel.stacks)
        {
            if (stack.tiles.Count > 0)
            {
                allCleared = false;
                break;
            }
        }

        if(allCleared || activeLevel.numTurns<=0)
        {
            gameActive = false;

            bool win = allCleared;

            if (activeLevel.numTurns <= 0)
                win = false;

            gameOverView.InitEndScreen(win, levelIndex, () =>
            {
                StartCoroutine(ResetScene(() =>
                {
                    if (win)
                    {
                        TileStacksModelManager.Instance.SetLastPlayedLevel(levelIndex);
                        // Advance to next level (or loop)
                        levelIndex = (levelIndex + 1) % TileStacksModelManager.Instance.GetNumLevels();
                    }

                    int unlockIndex = TileStacksModelManager.Instance.GetUnlock(levelIndex);

                    if (unlockIndex != -1)
                    {
                        uiManager.ShowTutorialImage(true, unlockIndex + 1);

                    }
                    else
                        BuildLevel();


                }));

            });
        }

    }

    public float GetPlayedPercentage()
    {
        int totalPlayed = 0;
        foreach (int count in activeLevel.playedTiles)
        {
            totalPlayed += count;
        }

        return (float)totalPlayed / levelStartTotalTiles;
    }

    public void EndGameScreenClicked(bool lastLevelWon)
    {
        StartCoroutine(ResetScene(()=>
        {
            if (lastLevelWon)
                // Advance to next level (or loop)
                levelIndex = (levelIndex + 1) % TileStacksModelManager.Instance.GetNumLevels();

                int unlockIndex = TileStacksModelManager.Instance.GetUnlock(levelIndex);

                if (unlockIndex != -1)
                {
                    uiManager.ShowTutorialImage(true, unlockIndex + 1);

                }
                else
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

    internal int GetButtonsCollected(int colorID)
    {
        return activeLevel.playedTiles[colorID];
    }

    #region clickVisuals

    private List<(TileStacksTileView view, float startDelay)> CollectTileFlights(int colorID, out bool removedAny)
    {
        removedAny = false;
        List<(TileStacksTileView view, float startDelay)> flights = new();
        List<int> matchingStacks = new();

        for (int i = 0; i < activeLevel.stacks.Count; i++)
        {
            var stack = activeLevel.stacks[i];
            var tiles = stack.tiles;

            if (stack.lockCount > 0 && activeLevel.playedTiles[stack.lockColor] < stack.lockCount)
                continue;

            if (tiles.Count > 0 && tiles[tiles.Count - 1].colorIndex == colorID)
            {
                matchingStacks.Add(i);
            }
        }

        float baseDelay = TILES_FLY_DELAY;
        int totalStacks = matchingStacks.Count;

        for (int si = 0; si < totalStacks; si++)
        {
            int stackIndex = matchingStacks[si];
            var stack = activeLevel.stacks[stackIndex];
            var tiles = stack.tiles;

            float perStackOffset = baseDelay * (si / (float)totalStacks);
            float localStartDelay = perStackOffset;

            while (tiles.Count > 0 && tiles[tiles.Count - 1].colorIndex == colorID)
            {
                var tileData = tiles[^1];
                tiles.RemoveAt(tiles.Count - 1);

                var view = activeTiles[stackIndex][^1];
                activeTiles[stackIndex].RemoveAt(activeTiles[stackIndex].Count - 1);

                if (tiles.Count > 0)
                {
                    var newTopTile = tiles[^1];
                    var newTopView = activeTiles[stackIndex][^1];

                    if (newTopTile.startHidden)
                    {
                        Debug.Log($"Hidden tile on stack {stackIndex} just got revealed — color: {newTopTile.colorIndex}");
                        newTopView.RevealTileColor();
                    }
                }

                flights.Add((view, localStartDelay));
                localStartDelay += baseDelay;
                removedAny = true;
            }
        }

        flights.Sort((a, b) => (a.startDelay + TILES_FLY_TIME).CompareTo(b.startDelay + TILES_FLY_TIME));
        return flights;
    }

    private void AnimateTileFlights(
        List<(TileStacksTileView view, float startDelay)> flights,
        int colorID,
        int buttonIndex,
        TileStacksColorButtonView clickedButton)
    {
        int pendingCallbacks = 0;
        int animCompleteCounter = 0;

        float baseIncrement = TILES_FLY_DELAY;
        float decayRate = FLY_DELAY_FALLOUT_RATE;
        float cumulativeDelay = 0f;

        for (int i = 0; i < flights.Count; i++)
        {
            var flight = flights[i];
            var view = flight.view;
            float yPos = (i + 1) * TILES_VERTICAL_OFFSET;

            float increment = baseIncrement * Mathf.Pow(decayRate, i);
            cumulativeDelay += increment;

            pendingCallbacks++;
            animCompleteCounter++;

            view.FlyTo(new Vector3(levelVisualizer.GetButtonPositionX(buttonIndex), yPos, GetButtonRowZ()+0.1f), cumulativeDelay, TILES_FLY_TIME, () =>
            {
                activeLevel.playedTiles[colorID]++;

                uiManager.UpdateProgressBar(GetPlayedPercentage());

                clickedButton.UpdateStackingCounter(activeLevel.playedTiles[colorID]);

                for (int si = 0; si < activeLevel.stacks.Count; si++)
                {
                    var s = activeLevel.stacks[si];
                    if (s.lockCount > 0 && s.lockColor == colorID && activeLevel.playedTiles[colorID] == s.lockCount)
                    {
                        Debug.Log($"Stack {si} unlocked! Required {s.lockCount} of color {s.lockColor}.");
                        stackViews[si].UnlockStackCover();
                    }
                }

                pendingCallbacks--;
                if (pendingCallbacks == 0)
                {
                    Debug.Log("All tile animations complete.");
                    clickedButton.ShowButton();

                    AnimateDestroyParticles(flights, clickedButton, animCompleteCounter);
                }
            });
        }
    }

    private void AnimateDestroyParticles(
        List<(TileStacksTileView view, float startDelay)> flights,
        TileStacksColorButtonView clickedButton,
        int animCompleteCounter)
    {
        float baseIncrement = BASE_DESTROY_DELAY;
        float decayRate = DESTORY_DELAY_FALLOUT_RATE;
        float delay = 0f;

        for (int i = 0; i < flights.Count; i++)
        {
            var viewToDestroy = flights[i].view;
            float increment = baseIncrement * Mathf.Pow(decayRate, i);
            delay += increment;

            viewToDestroy.PlayDestroyParticles(delay, () =>
            {
                animCompleteCounter--;
                if (animCompleteCounter == 0)
                {
                    clickedButton.UpdateButtonToModel();
                    CheckGameOver();
                }
            });
        }
    }

    #endregion

    public float GetButtonRowZ()
    {
        float baselineAspect = 9f / 16f;
        float baselineZ = -9.25f;

        float targetAspect = Camera.main.aspect;

        // For each step of aspect ratio increase (from 9:16 to 9:20), the Z offset goes -2 units.
        // So we compute how much wider we are relative to baseline, and scale that change.
        float zPerAspectUnit = (-11.25f - baselineZ) / ((9f / 20f) - baselineAspect); // -2 over delta
        float aspectDelta = targetAspect - baselineAspect;

        float adjustedZ = baselineZ + (zPerAspectUnit * aspectDelta);
        return adjustedZ;
    }

}
