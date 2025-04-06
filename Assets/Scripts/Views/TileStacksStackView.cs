using System;
using TMPro;
using UnityEngine;

public class TileStacksStackView : MonoBehaviour
{
    [SerializeField] private GameObject lockIndicator;
    [SerializeField] private SpriteRenderer lockColor;
    [SerializeField] private TMP_Text lockCounter;

    public void Setup(StackData data)
    {
        if (data.lockCount > 0)
        {
            lockIndicator.SetActive(true);

            var (pos, scaleY) = GetLockCoverTransform(data.tiles.Count);
            lockIndicator.transform.localPosition = pos;

            var sr = lockIndicator.GetComponent<SpriteRenderer>();
            sr.size = new Vector2(sr.size.x, scaleY);

            lockColor.color = TileStacksModelManager.Instance.GetTileColor(data.lockColor);
            lockCounter.text = data.lockCount.ToString();

            
        }
        else
        {
            lockIndicator.SetActive(false);
        }
    }


    public (Vector3 position, float yScale) GetLockCoverTransform(int numTiles)
    {
        const float baseYPos = 0.55f;
        const float baseYScale = 1.05f;
        const float z = -0.6f;

        const float posSlope = 0.0375f;     // per tile
        const float scaleSlope = 0.077777f; // per tile

        float yPos = baseYPos + (numTiles - 1) * posSlope;
        float yScale = baseYScale + (numTiles - 1) * scaleSlope;

        return (new Vector3(0f, yPos, z), yScale);
    }

    internal void UnlockStackCover()
    {
        lockIndicator.SetActive(false);
    }
}
