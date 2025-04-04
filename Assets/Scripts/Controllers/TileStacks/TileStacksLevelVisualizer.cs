using System.Collections.Generic;
using UnityEngine;

public class TileStacksLevelVisualizer : MonoBehaviour
{
    [SerializeField] private Transform levelRoot;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float yOffsetPerTile = 1f;
    [SerializeField] private Transform uiRoot;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject stackViewPrefab;

    float xSpacing = 1f;
    float zSpacing = 2.5f;

    public Transform LevelRoot { get => levelRoot; set => levelRoot = value; }
    public Transform UiRoot { get => uiRoot; set => uiRoot = value; }

    public (List<List<TileStacksTileView>>, List<TileStacksStackView>, int) BuildLevel(TilesStacksLevelData data, float verticalStackOffset)
    {
        Debug.Log("rectPos: " + TileStacksGameManager.Instance.RectToWorld(uiRoot.gameObject.GetComponent<RectTransform>().localPosition));

        List<List<TileStacksTileView>> allViews = new List<List<TileStacksTileView>>();
        List<TileStacksStackView> allStackViews = new List<TileStacksStackView>();
        HashSet<int> uniqueColors = new HashSet<int>();

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

        foreach (var stack in data.stacks)
        {
            List<TileStacksTileView> stackViews = new List<TileStacksTileView>();

            float worldX = -2f + stack.position.x * xSpacing;
            float worldZ = -7f + stack.position.y * zSpacing;

            // Create a stack container
            GameObject stackRoot = Instantiate(stackViewPrefab, levelRoot);
            stackRoot.transform.position = new Vector3(worldX + offset.x, offset.y, worldZ + offset.z);

            TileStacksStackView stackView = stackRoot.GetComponent<TileStacksStackView>();
            stackView.Setup(stack);

            allStackViews.Add(stackView);

            for (int j = 0; j < stack.tiles.Count; j++)
            {
                Vector3 localPos = new Vector3(0, j * verticalStackOffset, 0);

                GameObject tileGO = Instantiate(tilePrefab, stackRoot.transform);
                tileGO.transform.localPosition = localPos;

                TileStacksTileView view = tileGO.GetComponent<TileStacksTileView>();
                view.SetColor(stack.tiles[j]);

                stackViews.Add(view);
            }

            allViews.Add(stackViews);
        }

        int buttonIndex = 0;
        foreach (int color in uniqueColors)
        {
            GameObject button = Instantiate(buttonPrefab, uiRoot);
            TileStacksColorButtonView cb = button.GetComponent<TileStacksColorButtonView>();
            cb.Setup(color, buttonIndex);
            buttonIndex++;
        }

        return (allViews, allStackViews, uniqueColors.Count);
    }


}
