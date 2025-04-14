using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileStacksLevelVisualizer : MonoBehaviour
{
    public const float OFF_SCREEN_EXTRA_Y = 15f;

    [SerializeField] private Transform levelRoot;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float yOffsetPerTile = 1f;
    [SerializeField] private Transform uiRoot;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject stackViewPrefab;

    [SerializeField] private GameObject stackClearIndication;

    List<TileStacksColorButtonView> tileStacksColorButtonViews = new List<TileStacksColorButtonView>();

    float xSpacing = 1f;
    float zSpacing = 2.25f;

    public Transform LevelRoot { get => levelRoot; set => levelRoot = value; }
    public Transform UiRoot { get => uiRoot; set => uiRoot = value; }
    public List<TileStacksColorButtonView> TileStacksColorButtonViews { get => tileStacksColorButtonViews; set => tileStacksColorButtonViews = value; }

    public (List<List<TileStacksTileView>>, List<TileStacksStackView>, int) BuildLevel(TilesStacksLevelData data, float verticalStackOffset)
    {
        
        List<List<TileStacksTileView>> allViews = new List<List<TileStacksTileView>>();
        List<TileStacksStackView> allStackViews = new List<TileStacksStackView>();
        HashSet<int> uniqueColors = new HashSet<int>();

        tileStacksColorButtonViews.Clear();

        // Collect color indexes
        foreach (var stack in data.stacks)
        {
            foreach (var tile in stack.tiles)
            {
                uniqueColors.Add(tile.colorIndex);
            }
        }

        foreach (Transform child in levelRoot)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in uiRoot)
        {
            Destroy(child.gameObject);
        }

        int width = 5;
        int height = 4;
        float worldX;
        float worldZ;

        int validStacksCounter = 0;

        //add empty indications
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                worldX = -2f + i * xSpacing;
                worldZ = -6f + j * zSpacing;

                GameObject stackRoot = Instantiate(stackClearIndication, levelRoot);
                stackRoot.transform.position = new Vector3(worldX + offset.x, offset.y, worldZ + offset.z);

                //Vector3 localPos = new Vector3(0, j * verticalStackOffset, 0);
            }
        }

        foreach (var stack in data.stacks)
        {
            List<TileStacksTileView> stackViews = new List<TileStacksTileView>();

            worldX = -2f + stack.position.x * xSpacing;
            worldZ = -6f + stack.position.y * zSpacing;

            // Create a stack container
            GameObject stackRoot = Instantiate(stackViewPrefab, levelRoot);
            stackRoot.transform.position = new Vector3(worldX + offset.x, offset.y, worldZ + offset.z);

            TileStacksStackView stackView = stackRoot.GetComponent<TileStacksStackView>();
            stackView.Setup(stack);

            allStackViews.Add(stackView);

            float baseIncrement = TileStacksGameManager.TILES_FLY_DELAY / 3f ;
            float decayRate = 0.9f;
            float cumulativeDelay = 0f + (validStacksCounter * 0.035f);
            float increment = 0.01f;

            validStacksCounter++;

            for (int j = 0; j < stack.tiles.Count; j++)
            {

                Vector3 localPos = new Vector3(0, (j+1)   * verticalStackOffset+ increment * 20, 0);

                GameObject tileGO = Instantiate(tilePrefab, stackRoot.transform);
                tileGO.transform.localPosition = localPos;

                TileStacksTileView view = tileGO.GetComponent<TileStacksTileView>();
                view.SetColor(stack.tiles[j]);

                view.DelayShowMe(0.3f, j * verticalStackOffset,cumulativeDelay);

                stackViews.Add(view);

                increment = baseIncrement * Mathf.Pow(decayRate, j);
                cumulativeDelay += increment;
            }

            stackView.ShowCoverIfApplicapble(cumulativeDelay);

            // Detect and mark hidden tile batches
            for (int j = stack.tiles.Count - 1; j >= 0; j--)
            {
                if (stack.tiles[j].startHidden)
                {
                    int batchCount = 1;
                    int k = j - 1;
                    while (k >= 0 && stack.tiles[k].startHidden)
                    {
                        batchCount++;
                        k--;
                    }

                    // Notify the top tile in the hidden batch
                    stackViews[j].SetHiddenBatchSize(batchCount);
                    break; // Only the topmost batch matters
                }
            }



            allViews.Add(stackViews);
        }

        int numButtons = uniqueColors.Count;
        float totalWidth = 5.6f;
        float buttonBaseWidth = 1f;

        float scaleFactor = 1f;
        float buttonArea = numButtons * buttonBaseWidth;
        if (buttonArea > totalWidth)
        {
            scaleFactor = totalWidth / buttonArea;
        }
        float scaledButtonWidth = buttonBaseWidth * scaleFactor;

        float gap = (totalWidth - numButtons * scaledButtonWidth) / (numButtons + 1);

        // Start from left edge, then step by (button + gap)
        float startX = -totalWidth / 2f + gap + scaledButtonWidth / 2f;

        int buttonIndex = 0;
        foreach (int color in uniqueColors)
        {
            float xPos = startX + buttonIndex * (scaledButtonWidth + gap);

            GameObject button = Instantiate(buttonPrefab, uiRoot);
            button.transform.localPosition = new Vector3(xPos, 0, TileStacksGameManager.Instance.GetButtonRowZ());
            button.transform.localScale = Vector3.one * scaleFactor;

            TileStacksColorButtonView cb = button.GetComponent<TileStacksColorButtonView>();
            cb.Setup(color, buttonIndex, button.transform.localPosition, button.transform.localScale);

            tileStacksColorButtonViews.Add(cb);

            buttonIndex++;
        }

        return (allViews, allStackViews, uniqueColors.Count);
    }

    public float GetButtonPositionX(int buttonIndex)
    {
        return tileStacksColorButtonViews[buttonIndex].transform.position.x;
    }

}
