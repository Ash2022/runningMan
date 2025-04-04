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

    float xSpacing = 1f;
    float zSpacing = 2.5f;

    public Transform LevelRoot { get => levelRoot; set => levelRoot = value; }
    public Transform UiRoot { get => uiRoot; set => uiRoot = value; }

    public (List<List<TileStacksTileView>>, int) BuildLevel(TilesStacksLevelData data, float verticalStackOffset = 0.0875f)
    {
        Debug.Log("rectPos: " + TileStacksGameManager.Instance.RectToWorld(uiRoot.gameObject.GetComponent<RectTransform>().localPosition));

        List<List<TileStacksTileView>> allViews = new List<List<TileStacksTileView>>();
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

        // Get min/max ranges for normalization
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (var stack in data.stacks)
        {
            minX = Mathf.Min(minX, stack.position.x);
            maxX = Mathf.Max(maxX, stack.position.x);
            minY = Mathf.Min(minY, stack.position.y);
            maxY = Mathf.Max(maxY, stack.position.y);
        }

        foreach (var stack in data.stacks)
        {
            List<TileStacksTileView> stackViews = new List<TileStacksTileView>();

            float worldX = -2f + stack.position.x * xSpacing;
            float worldZ = -7f + stack.position.y * zSpacing;

            for (int j = 0; j < stack.tiles.Count; j++)
            {
                Vector3 pos = new Vector3(
                    worldX + offset.x,
                    j * verticalStackOffset + offset.y,
                    worldZ + offset.z
                );

                GameObject go = Instantiate(tilePrefab, levelRoot);
                go.transform.position = pos;
                TileStacksTileView view = go.GetComponent<TileStacksTileView>();
                view.SetColor(stack.tiles[j]); // Pass TileData object
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

        return (allViews, uniqueColors.Count);
    }

}
