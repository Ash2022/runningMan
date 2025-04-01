using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
using System;

public class GameManager : MonoBehaviour
{
    const float BETWEEN_PPL_DELAY = 0.2f;
    const float OBST_MOVE_TIME = 0.75f;

    public static GameManager Instance;
    [SerializeField] RectTransform canvasRect;
    [SerializeField] private ModelManager modelManager;
    [SerializeField] private LevelVisualizer levelVisualizer;

    private int currentLevelIndex = 3;
    private int startNumPersonInTurn = 0;
    private List<List<PersonView>> personViews;
    private List<ObstacleView> obstacleViews;

    LevelData currentLevel;
    int matchSize;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        modelManager.Init();
        BuildLevel();
    }

    private void BuildLevel()
    {

        currentLevel = modelManager.GetLevel(currentLevelIndex);

        matchSize = currentLevel.obstacles.Count > 0 ? currentLevel.obstacles[0].units.Count : 0;
      
        // Initialize resolved flags for model
        foreach (var obstacle in currentLevel.obstacles)
        {
            obstacle.InitializeResolvedFlags();
        }

        int currentDistance = currentLevel.horizon;
        for (int i = 0; i < currentLevel.obstacles.Count; i++)
        {
            currentLevel.obstacles[i].turnsToHitQueue = currentDistance;
            if (i < currentLevel.obstacles.Count - 1)
                currentDistance += currentLevel.obstacles[i].gapToNext;
        }

        startNumPersonInTurn = currentLevel.peopleQueues[0].Count;

        (personViews, obstacleViews) = levelVisualizer.BuildLevel(currentLevel);
    }
   
    public void OnQueueClicked(int queueIndex)
    {
        Debug.Log($"Queue {queueIndex} clicked.");

        StartCoroutine(ResolveQueue(queueIndex));
    }

    IEnumerator ResolveQueue(int queueIndex)
    {
        var queue = currentLevel.peopleQueues[queueIndex];
        
        List<(int personColor, PersonView personView)> peopleThisTurn = new();

        for (int i = 0; i < matchSize && queue.Count > 0 && personViews[queueIndex].Count > 0; i++)
        {
            int lastIndex = queue.Count - 1;
            int color = queue[lastIndex];
            queue.RemoveAt(lastIndex);

            PersonView view = personViews[queueIndex][0];
            personViews[queueIndex].RemoveAt(0);

            peopleThisTurn.Add((color, view));
        }

        for (int p = 0; p < peopleThisTurn.Count; p++)
        {
            var (personColor, personView) = peopleThisTurn[p];
            bool matched = false;

            for (int o = 0; o < currentLevel.obstacles.Count; o++)
            {
                ObstacleData obstacle = currentLevel.obstacles[o];
                float zPos = obstacleViews[o].transform.localPosition.z;
                if (zPos > 20f || zPos < 15f) continue;

                if (o > 0 && !currentLevel.obstacles[o - 1].unitsResolved.Contains(true))
                    continue;

                for (int u = 0; u < obstacle.units.Count; u++)
                {
                    if (!obstacle.unitsResolved[u] && obstacle.units[u] == personColor)
                    {
                        obstacle.unitsResolved[u] = true;
                        matched = true;

                        UnitView unitView = obstacleViews[o].unitViews[u];

                        personView.transform.DOMove(unitView.transform.position, 1).SetDelay(p * BETWEEN_PPL_DELAY).OnComplete(() =>
                        {
                            Destroy(personView.gameObject);
                            Destroy(unitView.gameObject);
                        });

                        Debug.Log($"MATCH: person color {personColor} matched unit {u} in obstacle {o}");
                        break;
                    }
                }

                if (matched) break;
            }

            if (!matched)
            {
                personView.transform.DOLocalMoveY(personView.transform.localPosition.y + 10f, 1).SetDelay(p * 0.15f).OnComplete(() =>
                {
                    Destroy(personView.gameObject);
                });

                Debug.Log($"NO MATCH: person color {personColor} did not match any unit.");
            }
        }

        float startDelay = BETWEEN_PPL_DELAY * matchSize + 1;

        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < personViews[queueIndex].Count; i++)
        {
            Vector3 pos = personViews[queueIndex][i].transform.localPosition;
            pos.z = 15f - (i * levelVisualizer.QueueZSpacing);
            personViews[queueIndex][i].transform.DOLocalMove(pos, OBST_MOVE_TIME);
        }

        RemoveCompletedObstacles();
        AdvanceObstacles();

        yield return new WaitForSeconds(OBST_MOVE_TIME);

        CheckForGameOver();
    }

    private void AdvanceObstacles()
    {
        for (int i = 0; i < obstacleViews.Count; i++)
        {
            // Move view
            var obstacleView = obstacleViews[i];
            Vector3 currentPos = obstacleView.transform.localPosition;
            currentPos.z -= 1f;

            obstacleView.transform.DOLocalMove(currentPos, OBST_MOVE_TIME);

            // Update model data
            currentLevel.obstacles[i].turnsToHitQueue--;

        }
    }

    private void RemoveCompletedObstacles()
    {   
        for (int i = currentLevel.obstacles.Count - 1; i >= 0; i--)
        {
            if (currentLevel.obstacles[i].unitsResolved.TrueForAll(res => res))
            {
                Debug.Log($"✅ Obstacle {i} completed and removed");

                // Remove data and visual
                currentLevel.obstacles.RemoveAt(i);
                //Destroy(obstacleViews[i].gameObject);
                obstacleViews.RemoveAt(i);
            }
        }
    }

    private void CheckForGameOver()
    {
        
        if (currentLevel.obstacles.Count == 0)
        {
            Debug.Log("🎉 Game Over: Win!");
            UIManager.Instance.ShowGameOver(true); // win = true
            return;
        }

        foreach (var obstacle in currentLevel.obstacles)
        {
            if (obstacle.turnsToHitQueue < 0)
            {
                Debug.Log("💀 Game Over: Obstacle reached the queue. You lose!");
                UIManager.Instance.ShowGameOver(false); // win = false
                return;
            }
        }
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

        personViews.Clear();
        obstacleViews.Clear();

        yield return new WaitForEndOfFrame();

        done?.Invoke();
    }


    public void EndGameScreenClicked(bool lastLevelWon)
    {
        StartCoroutine(ResetScene(()=>
        {
            if (lastLevelWon)
                // Advance to next level (or loop)
                currentLevelIndex = (currentLevelIndex + 1) % modelManager.GetNumLevels();

            BuildLevel();
        }));

        
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
            EndGameScreenClicked(false);

    }

    public Vector2 WorldToRect(Vector3 world)
    {
        Vector2 myCurrentHeightWorld = Camera.main.WorldToScreenPoint(world);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, myCurrentHeightWorld, Camera.main, out var localPos);
        return localPos;
    }
}
