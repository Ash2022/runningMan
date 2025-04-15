using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TileStacksGameManager : MonoBehaviour
{
    public const int LOOP_SIZE = 50;

    const float BASE_DESTROY_DELAY = 0.075f;
    const float DESTORY_DELAY_FALLOUT_RATE = 0.95f;

    
    public const float TILES_FLY_DELAY = 0.1f;
    public const float TILES_FLY_TIME = 0.5f;
    public const float FLY_DELAY_FALLOUT_RATE = 0.95f;


    public const float TILES_VERTICAL_OFFSET = 0.075f;

    public static TileStacksGameManager Instance;

    [SerializeField] RectTransform canvasRect;
    [SerializeField] private TileStacksLevelVisualizer levelVisualizer;
    [SerializeField] private TileStacksUIManager uiManager;

    public GameOverView gameOverView;

    private List<List<TileStacksTileView>> activeTiles;
    private TilesStacksLevelData activeLevel;
    private List<TileStacksStackView> stackViews;
    public GameObject splashScreen;
    public TMP_Text splashText;

    [SerializeField] Texture2D _handTexture;
    public bool recordingMode;

    public int levelIndex=0;
    bool gameActive = false;
    int numColorInLevel = 0;
    int levelStartTotalTiles = 0;//keeping this for progress bar - because when playing the model is removing the played tiles - so cant know how many were there.

    public TileStacksUIManager UiManager { get => uiManager; set => uiManager = value; }

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
        //PlayerPrefs.DeleteAll();

        Init();

        
    }

    private async void Init()
    {

#if UNITY_EDITOR
        if (recordingMode)
            Cursor.SetCursor(_handTexture, Vector2.zero, CursorMode.ForceSoftware);
#endif

        DOTween.SetTweensCapacity(500,50);
        Application.targetFrameRate = 60;

        splashScreen.SetActive(true);

        TileStacksModelManager.Instance.Init();

        float baselineAspect = 9f / 16f; // Target aspect
        float baselineOrthoSize = 5f;    // Desired ortho size at 9:16

        float targetAspect = Camera.main.aspect;

        // Adjust orthographic size based on aspect

        if(targetAspect<baselineAspect)
        {
            float adjustedOrthoSize = baselineOrthoSize * (baselineAspect / targetAspect);

            Camera.main.orthographicSize = adjustedOrthoSize;
            Debug.Log($"[Ortho Adjust] Aspect: {targetAspect:F3}, Adjusted OrthoSize: {adjustedOrthoSize:F2}");
        }


        TinySauce.SubscribeOnInitFinishedEvent((param1, param2) =>
        {
            if (levelIndex == -1)
            {
                levelIndex = TileStacksModelManager.Instance.GetLastPlayedLevel();

                levelIndex++;

            }

            if (levelIndex == 0)
            {
                splashScreen.SetActive(false);
                uiManager.ShowTutorialImage(true, levelIndex);
            }
            else
                splashText.text = "CLICK TO CONTINUE";
        });

    }

    public void SplashClicked()
    {
        splashScreen.SetActive(false);
        BuildLevel();
    }

    public void BuildLevel()
    {
        activeLevel = TileStacksModelManager.Instance.GetLevel(levelIndex);
        (activeTiles, stackViews,numColorInLevel) = levelVisualizer.BuildLevel(activeLevel, TILES_VERTICAL_OFFSET);

        levelStartTotalTiles = activeLevel.stacks.Count * activeLevel.stacks[0].tiles.Count;

        SetInitalLockingState();

        if(activeLevel.alternateLocking)
            ApplyInitialAlternateButtonLocks();

        TinySauce.OnGameStarted(levelIndex + 1);

        uiManager.InitLevel(activeLevel,levelIndex);
        gameActive = true;
    }


    

    public void OnColorButtonClicked(int colorID, int buttonIndex, TileStacksColorButtonView clickedButton)
    {
        if (!gameActive || activeLevel.numTurns<=0)
            return;

        if (activeLevel.alternateLocking)
            ToggleButtonLocks();

        // ✅ Check if there's any playable tile
        bool hasMatchingTopTile = false;
        for (int i = 0; i < activeLevel.stacks.Count; i++)
        {
            var stack = activeLevel.stacks[i];
            var tiles = stack.tiles;

            if (tiles.Count == 0 || stack.isLocked)
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
            clickedButton.SetButtonBackToUnclicked(500);
            SoundsManager.Instance.ButtonClick(false);
            activeLevel.numTurns--;
            uiManager.SetTurns(activeLevel.numTurns);
            CheckGameOver();
            return;
        }

        Debug.Log("Color button clicked: " + colorID);
        SoundsManager.Instance.ButtonClick(true);

        

        List<(TileStacksTileView view, float delay)> flights = CollectTileFlights(colorID, out bool removedAny, out Dictionary<int, int> removedThisTurn);

        if (removedAny)
        {
            UpdateStackLockStatesAfterTurn(colorID, removedThisTurn);
            activeLevel.numTurns--;
            uiManager.SetTurns(activeLevel.numTurns);
        }

        AnimateTileFlights(flights, colorID, buttonIndex, clickedButton);
    }


    private void UpdateStackLockStatesAfterTurn(int clickedColor, Dictionary<int, int> removedThisTurn)
    {
        for (int i = 0; i < activeLevel.stacks.Count; i++)
        {
            var stack = activeLevel.stacks[i];

            if (!stack.isLocked) continue;

            if (stack.lockType == LockType.Accum)
            {
                if (stack.lockColor == -1)
                {
                    int total = activeLevel.playedTiles.Sum();
                    if (total >= stack.lockCount)
                        stack.isLocked = false;
                    else
                    {
                        //an accum lock collected tiles but not enough to unlock - update its counter
                        stackViews[i].UpdateLockCounter(total);
                        stack.lockCount -= total;
                    }
                }
                else
                {
                    if (clickedColor == stack.lockColor && removedThisTurn[stack.lockColor] >= stack.lockCount)
                    {
                        stack.isLocked = false;
                        stackViews[i].UnlockStackCover();
                    }
                    else if(clickedColor == stack.lockColor)
                    {
                        //an accum lock collected tiles but not enough to unlock - update its counter
                        stackViews[i].UpdateLockCounter(removedThisTurn[stack.lockColor]);
                        stack.lockCount -= removedThisTurn[stack.lockColor];
                    }
                }

            }
            else if (stack.lockType == LockType.SngPl)
            {
                if (stack.lockColor == -1)
                {
                    int totalThisTurn = removedThisTurn.Sum(p => p.Value);
                    if (totalThisTurn >= stack.lockCount)
                        stack.isLocked = false;
                    else
                        stackViews[i].UpdateLockCounter(removedThisTurn[clickedColor]);
                }
                else if (clickedColor == stack.lockColor && removedThisTurn[stack.lockColor] >= stack.lockCount)
                {
                    stack.isLocked = false;

                    stackViews[i].UnlockStackCover();
                }
                else if (clickedColor == stack.lockColor)
                {
                    stackViews[i].UpdateLockCounter(removedThisTurn[stack.lockColor]);
                }
            }

            if (!stack.isLocked)
            {
                Debug.Log($"Stack {i} unlocked! Required {stack.lockCount} of {(stack.lockColor == -1 ? "ANY" : stack.lockColor.ToString())}");
                stackViews[i].UnlockStackCover();
            }
        }
    }

    private void SetInitalLockingState()
    {
        // ✅ First: update lock states before checking
        for (int i = 0; i < activeLevel.stacks.Count; i++)
        {
            var stack = activeLevel.stacks[i];

            if (stack.lockCount > 0)
            {
                if (stack.lockType == LockType.Accum)
                {
                    if (stack.lockColor == -1)
                    {
                        int total = activeLevel.playedTiles.Sum();
                        stack.isLocked = total < stack.lockCount;
                    }
                    else
                    {
                        stack.isLocked = activeLevel.playedTiles[stack.lockColor] < stack.lockCount;
                    }
                }
                else if (stack.lockType == LockType.SngPl)
                {
                    stack.isLocked = true; // will resolve at end of this turn
                }
            }
            else
            {
                stack.isLocked = false;
            }
        }
    }

    private void ApplyInitialAlternateButtonLocks()
    {
        for (int i = 0; i < levelVisualizer.TileStacksColorButtonViews.Count; i++)
        {
            bool shouldLock = (i % 2 == 0); // even index = locked, odd index = unlocked
            levelVisualizer.TileStacksColorButtonViews[i].SetLock(shouldLock); // assuming this method exists
        }
    }

    private void ToggleButtonLocks()
    {
        foreach (var button in levelVisualizer.TileStacksColorButtonViews)
        {
            button.SetLock(!button.IsLocked); // assuming `IsLocked` is a property you have
        }
    }

    public void HideTutorialImage()
    {
        uiManager.ShowTutorialImage(false, 0);
        BuildLevel();

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

        bool levelCannotComplete = IsLevelUnwinnableDueToLocks();

        if (allCleared || activeLevel.numTurns<=0 || levelCannotComplete)
        {
            gameActive = false;

            bool win = allCleared;

            if ((activeLevel.numTurns <= 0 || levelCannotComplete)&& (win == false))
                win = false;

            TinySauce.OnGameFinished(win,activeLevel.numTurns);

            gameOverView.InitEndScreen(win, levelCannotComplete, levelIndex, () =>
            {
                StartCoroutine(ResetScene(() =>
                {
                    if (win)
                    {
                        TileStacksModelManager.Instance.SetLastPlayedLevel(levelIndex);
                        // Advance to next level (or loop)
                        levelIndex++;
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

    private bool IsLevelUnwinnableDueToLocks()
    {
        int[] remainingColorCounts = new int[9];

        // Count all remaining visible tiles
        foreach (var stack in activeLevel.stacks)
        {
            foreach (var tile in stack.tiles)
            {
                remainingColorCounts[tile.colorIndex]++;
            }
        }

        foreach (var stack in activeLevel.stacks)
        {
            if (stack.tiles.Count == 0 || stack.lockCount == 0)
                continue;

            // Skip already unlocked stacks
            if (!stack.isLocked)
                continue;

            if (stack.lockColor == -1)
            {
                int totalRemaining = remainingColorCounts.Sum();
                if (totalRemaining < stack.lockCount)
                {
                    Debug.Log($"[Unwinnable] Stack requires {stack.lockCount} of ANY color, but only {totalRemaining} tiles remain.");
                    return true;
                }
            }
            else
            {
                int required = stack.lockCount;
                int available = remainingColorCounts[stack.lockColor];
                if (available < required)
                {
                    Debug.Log($"[Unwinnable] Stack requires {required} of color {stack.lockColor}, but only {available} remain.");
                    return true;
                }
            }
        }

        return false;
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
                levelIndex++;

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

        int numItems3 = uiManager.DynamicUIElementsHolder.childCount - 1;

        for (int i = numItems3; i >= 0; i--)
            Destroy(uiManager.DynamicUIElementsHolder.GetChild(i).gameObject);

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

    private List<(TileStacksTileView view, float delay)> CollectTileFlights(int colorID, out bool removedAny, out Dictionary<int, int> removedThisTurn)
    {
        List<(TileStacksTileView view, float delay)> flights = new();
        removedAny = false;
        removedThisTurn = new Dictionary<int, int>();
        int tilesRemovedThisClick = 0;

        for (int c = 0; c < 9; c++)
            removedThisTurn[c] = 0;

        for (int i = 0; i < activeLevel.stacks.Count; i++)
        {
            var stack = activeLevel.stacks[i];
            var tiles = stack.tiles;

            // Skip locked stacks (accumulate mode)
            if (stack.isLocked)
                continue;

            float startDelay = 0;

            while (tiles.Count > 0 && tiles[tiles.Count - 1].colorIndex == colorID)
            {

                if (tiles.Count > 0 && tiles[tiles.Count - 1].startHidden)
                {
                    activeTiles[i][activeTiles[i].Count - 1].RevealTileColor();
                }


                var tileData = tiles[tiles.Count - 1];
                tiles.RemoveAt(tiles.Count - 1);

                var view = activeTiles[i][activeTiles[i].Count - 1];
                activeTiles[i].RemoveAt(activeTiles[i].Count - 1);

                activeLevel.playedTiles[colorID]++;

                flights.Add((view, startDelay));
                view.stackIndex = i;

                startDelay += TILES_FLY_DELAY;
                tilesRemovedThisClick++;
                removedAny = true;
                removedThisTurn[colorID]++;
            }

            // ✅ AFTER removing matching tiles, reveal all consecutive hidden tiles now exposed
            for (int j = tiles.Count - 1; j >= 0; j--)
            {
                if (tiles[j].startHidden)
                {
                    activeTiles[i][j].RevealTileColor();
                    tiles[j].startHidden = false;
                }
                else
                {
                    break; // Stop once we hit a non-hidden tile
                }
            }

        }

        // Filter out disqualified stacks for singlePlay locks
        for (int i = 0; i < activeLevel.stacks.Count; i++)
        {
            var stack = activeLevel.stacks[i];

            if (stack.lockType == LockType.SngPl && stack.lockCount > 0 && stack.isLocked)
            {
                if (stack.lockColor == -1)
                {
                    int totalThisTurn = removedThisTurn.Values.Sum();
                    if (totalThisTurn < stack.lockCount)
                    {
                        Debug.Log($"Stack {i} requires {stack.lockCount} (any color) in one turn — only got {totalThisTurn}.");
                        flights.RemoveAll(f => f.view.stackIndex == i);
                    }
                }
                else if (stack.lockColor == colorID && removedThisTurn[colorID] < stack.lockCount)
                {
                    Debug.Log($"Stack {i} requires {stack.lockCount} of color {stack.lockColor} in one turn — only got {removedThisTurn[colorID]}.");
                    flights.RemoveAll(f => f.view.stackIndex == i);
                }
            }
        }

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
        bool firstComplete = false;

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
                //activeLevel.playedTiles[colorID]++;

                if(firstComplete==false)
                {
                    firstComplete = true;
                    //generate counter effect
                    uiManager.GenerateCounterEffect(flights.Count, cumulativeDelay, colorID, clickedButton);
                }

                //uiManager.UpdateProgressBar(GetPlayedPercentage());

                pendingCallbacks--;
                if (pendingCallbacks == 0)
                {
                    Debug.Log("All tile animations complete.");
                    clickedButton.SetButtonBackToUnclicked(0);

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
        float baselineZ = -8.25f;

        float targetAspect = Camera.main.aspect;

        // For each step of aspect ratio increase (from 9:16 to 9:20), the Z offset goes -2 units.
        // So we compute how much wider we are relative to baseline, and scale that change.
        float zPerAspectUnit = (-9.25f - baselineZ) / ((9f / 20f) - baselineAspect); // -2 over delta
        float aspectDelta = targetAspect - baselineAspect;

        float adjustedZ = baselineZ + (zPerAspectUnit * aspectDelta);


        return Mathf.Min(adjustedZ,baselineZ);
    }

}
