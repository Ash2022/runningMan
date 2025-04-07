using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndScreenUnlockView : MonoBehaviour
{
    [SerializeField]CanvasGroup canvasGroup;
    [SerializeField] Image fillBGImage;
    [SerializeField] Image fillImage;
    [SerializeField] RectTransform mainImageRect;
    [SerializeField] GameObject backParticles;


    [SerializeField] TMP_Text bottomLevelsLeftText;
    

    int totalSteps = 0;
    int currentStep = 0;

    public CanvasGroup CanvasGroup { get => canvasGroup; set => canvasGroup = value; }
    public GameObject BackParticles { get => backParticles; set => backParticles = value; }

    public void InitDisplay(Sprite spriteBG,Sprite spriteFill, int totalSteps, int currentStep)
    {
        canvasGroup.DOFade(1, 0.35f).OnComplete(()=>
        {
            backParticles.SetActive(true);
        });

        this.totalSteps = totalSteps;
        this.currentStep = currentStep;

        fillImage.sprite = spriteFill;

        fillImage.fillAmount = (float)currentStep /totalSteps;


        //mainFeatureImage.sprite = spriteBG;

        //mainFeatureImage.SetNativeSize();
        fillBGImage.sprite = spriteBG;
        mainImageRect.localScale = Vector3.one;

        fillImage.SetNativeSize();
        fillBGImage.SetNativeSize();

        //titleText.text = title;
        SetProgessText();
    }


    public void InitDisplay(Color fillColor, int totalSteps, int currentStep)
    {
        canvasGroup.DOFade(1, 0.35f).OnComplete(() =>
        {
            backParticles.SetActive(true);
        });

        this.totalSteps = totalSteps;
        this.currentStep = currentStep;

        fillImage.color = fillColor;

        fillImage.fillAmount = (float)currentStep / totalSteps;

        mainImageRect.localScale = Vector3.one;

        SetProgessText();
    }

    private void SetProgessText()
    {
        int delta = totalSteps - currentStep;

        if (delta == 0)
            bottomLevelsLeftText.text = "UNLOCKS <color=#FEF123>NOW</color>";
        else if(delta == 1)
            bottomLevelsLeftText.text = "UNLOCKS IN <color=#FEF123>1</color> LEVEL";
        else
            bottomLevelsLeftText.text = "UNLOCKS IN <color=#FEF123>" + delta + "</color> LEVELS";
    }

    public void UpdateProgress(Action done)
    {
        float startValue = fillImage.fillAmount;

        currentStep++;

        float endValue = (float)currentStep / totalSteps;
        
        DOVirtual.Float(startValue, endValue, 1f, v => fillImage.fillAmount = v).OnComplete(()=>
        {
            //if (currentStep == totalSteps)
            //    SoundsManager.Instance.PlayCakeFeatureUnlocked();

            SetProgessText();

            done?.Invoke();
        });

    }

    public void HideEndScreenUnlocks()
    {
        backParticles.SetActive(false);
        canvasGroup.DOFade(0, 0.35f);
    }

}
