using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileStacksTileView : MonoBehaviour
{
    [SerializeField] Renderer renderer;

    internal void SetColor(TileData tileData)
    {
        renderer.material.color = TileStacksUtils.GetColorFromID(tileData.colorIndex);
    }


    public void FlyTo(Vector3 targetPosition, float startDelay, float duration, Action done)
    {
        Debug.Log("StartDelay: " + startDelay);

        Vector3 midPoint = (transform.position + targetPosition) / 2f;
        midPoint.y += 3f; // create upward arc by pulling Z closer (visually rising)


        transform.DORotate(new Vector3(-90, 0, 0), duration, RotateMode.Fast).SetDelay(startDelay);
        transform.DOMove(midPoint, duration / 2f).SetEase(Ease.OutSine).SetDelay(startDelay);
        transform.DOMove(targetPosition, duration / 2f).SetEase(Ease.InSine).SetDelay(startDelay + (duration / 2f)).OnComplete(() =>
        {
            done?.Invoke();
        });

    }

}
