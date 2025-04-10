using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class TileStacksStackView : MonoBehaviour
{
    [SerializeField] private GameObject lockIndicator;
    [SerializeField] private SpriteRenderer lockIndicationBG;
    [SerializeField] private SpriteRenderer lockColor;
    [SerializeField] private SpriteRenderer lockDataBG;
    [SerializeField] private TMP_Text lockCounter;

    StackData data;

    int lockValue = 0;

    public void Setup(StackData data)
    {
        this.data = data;

        if (data.lockCount > 0)
        {
            lockIndicator.SetActive(true);

            var (pos, scaleY) = GetLockCoverTransform(data.tiles.Count);
            lockIndicator.transform.localPosition = pos;

            var sr = lockIndicator.GetComponent<SpriteRenderer>();
            sr.size = new Vector2(sr.size.x, scaleY*2);

            lockColor.sprite = TileStacksModelManager.Instance.GetLocksIndication(data.lockColor);
            lockCounter.text = data.lockCount.ToString();

            lockValue = data.lockCount;

            if (data.lockType == LockType.Accum)
                lockIndicationBG.sprite = TileStacksModelManager.Instance.GetStackCover(true);
            else
                lockIndicationBG.sprite = TileStacksModelManager.Instance.GetStackCover(false);



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

        UpdateLockCounter(data.lockCount,()=>
        {
            SoundsManager.Instance.StackUnlcoked();
            lockIndicationBG.DOFade(0, 1f);
            lockColor.DOFade(0, 1f);
            lockDataBG.DOFade(0, 1f);
            lockCounter.DOFade(0, 1f);

            lockIndicator.transform.DOLocalMoveY(lockIndicator.transform.localPosition.y + 3, 1).OnComplete(() =>
            {
                lockIndicator.SetActive(false);
            });
        });
   
    }

    internal void UpdateLockCounter(int total,Action done=null)
    {
        //startStackData.lockCount-=total;

        
        DOVirtual.Int(data.lockCount, data.lockCount - total, total * TileStacksGameManager.TILES_FLY_DELAY, (counter) =>
        {
            lockCounter.text = counter.ToString();
        }).SetDelay(TileStacksGameManager.TILES_FLY_TIME).OnComplete(()=>
        {
            done?.Invoke();

            //in this mode we count it back
            if (data.lockType == LockType.SngPl && done==null)
            {
                DOVirtual.Int(data.lockCount - total, data.lockCount, 0.5f, (counter) =>
                {
                    lockCounter.text = counter.ToString();
                }).SetDelay(0.25f);
            }
        });
       
        //lockCounter.text = (data.lockCount -total).ToString();

    }
}
