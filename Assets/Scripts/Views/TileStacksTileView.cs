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
    public int stackIndex;

    [SerializeField]GameObject hiddenIndication;

    internal void SetColor(TileData _tileData)
    {
        gameObject.SetActive(false);
        tileData = _tileData;

        if (tileData.startHidden)
            renderer.material = TileStacksModelManager.Instance.GetHiddenMaterial();
        else
            renderer.material = TileStacksModelManager.Instance.GetTileMaterial(tileData.colorIndex);
    }

    public void RevealTileColor()
    {
        SoundsManager.Instance.HiddenTileUnlocked();

        hiddenIndication.SetActive(false);

        DoTileParticles();

        renderer.material = TileStacksModelManager.Instance.GetTileMaterial(tileData.colorIndex);
    }


    public void FlyTo(Vector3 targetPosition, float startDelay, float duration, Action done)
    {
        

        //SoundsManager.Instance.TileStartFlying();

        //Debug.Log("StartDelay: " + startDelay);

        Vector3 midPoint = (transform.position + targetPosition) / 2f;
        midPoint.y += 3f; // create upward arc by pulling Z closer (visually rising)


        transform.DORotate(new Vector3(-90, 0, 0), duration, RotateMode.Fast).SetDelay(startDelay);
        transform.DOMove(midPoint, duration / 2f).SetEase(Ease.OutSine).SetDelay(startDelay);
        transform.DOMove(targetPosition, duration / 2f).SetEase(Ease.InSine).SetDelay(startDelay + (duration / 2f)).OnComplete(() =>
        {
            SoundsManager.Instance.TileHitButton();
            SoundsManager.Instance.PlayHaptics(SoundsManager.TapticsStrenght.Light);
            done?.Invoke();
        });

    }


    public void PlayDestroyParticles(float startDelay,Action done)
    {
        Debug.Log("DestoryDelay:" + startDelay);
        StartCoroutine(DestoryTileRoutine(startDelay, done));
    }

    IEnumerator DestoryTileRoutine(float startDelay,Action done)
    {
        yield return new WaitForSeconds(startDelay);

        renderer.enabled = false;

        DoTileParticles();

        yield return new WaitForSeconds(1);

        done?.Invoke();

        Destroy(gameObject);
    }

    private void DoTileParticles()
    {
        particleSystem.gameObject.SetActive(false);

        Color tileColor = TileStacksModelManager.Instance.GetTileColor(tileData.colorIndex);

        var main = particleSystem.main;
        main.startColor = new ParticleSystem.MinMaxGradient(TileStacksUtils.GetDarkerColor(tileColor), tileColor);

        particleSystem.gameObject.SetActive(true);
    }

    public void SetHiddenBatchSize(int size)
    {
        // Store the size and optionally trigger visual logic
        //hiddenBatchSize = size;
        //ShowHiddenBatchVisual(size); // Your own method to show visuals

        hiddenIndication.SetActive(true);

        hiddenIndication.transform.localPosition = new Vector3(0,-0.6f, TileStacksGameManager.TILES_VERTICAL_OFFSET * size / 5f); //= TileStacksGameManager.Instance.UiManager.GenerateHiddenTilesIndication(transform.position - new Vector3(0,TileStacksGameManager.TILES_VERTICAL_OFFSET*size/2f,0));
    }

    public int GetTileColorIndex()
    {
        return tileData.colorIndex;
    }

    internal void DelayShowMe(float moveTime,float targetY,float startDelay)
    {
        
        transform.DOLocalMoveY(targetY, moveTime).SetDelay(startDelay).OnStart(()=>
        {
            gameObject.SetActive(true);
        });

        //Invoke("ShowMe",delay);
    }

    private void ShowMe()
    {
        gameObject.SetActive(true);
    }
}
