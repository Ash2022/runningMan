using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileStacksTileView : MonoBehaviour
{
    [SerializeField] Renderer renderer;
    [SerializeField]ParticleSystem particleSystem;
    TileData tileData;
    internal void SetColor(TileData _tileData)
    {
        tileData = _tileData;

        if (tileData.startHidden)
            renderer.material.color = Color.white;
        else
            renderer.material.color = TileStacksUtils.GetColorFromID(tileData.colorIndex);
    }

    public void RevealTileColor()
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


    public void PlayDestroyParticles(float startDelay,Action done)
    {
        StartCoroutine(DestoryTileRoutine(startDelay, done));
    }

    IEnumerator DestoryTileRoutine(float startDelay,Action done)
    {
        yield return new WaitForSeconds(startDelay);

        renderer.enabled = false;

        particleSystem.gameObject.SetActive(false);

        Color color = TileStacksUtils.GetColorFromID(tileData.colorIndex);

        var main = particleSystem.main;
        main.startColor = color;

        particleSystem.gameObject.SetActive(true);

        yield return new WaitForSeconds(1);

        done?.Invoke();
        Destroy(gameObject);
    }

}
