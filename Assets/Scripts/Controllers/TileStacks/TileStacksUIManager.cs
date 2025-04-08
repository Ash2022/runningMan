using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileStacksUIManager : MonoBehaviour
{
    const int MIN_COMBO_FOR_WORD = 8;

    [SerializeField] private TMP_Text turnsText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TMP_Text resultText;
    [SerializeField] TMP_Text buttonText;
    [SerializeField] Image progressBar;

    [SerializeField] List<Sprite> tutorialImages = new List<Sprite>();
    [SerializeField] TutorialImageView tutorialImageView;

    [SerializeField]List<Sprite> completionWords = new List<Sprite>();

    [SerializeField] Transform dynamicUIElementsHolder;
    [SerializeField] GameObject hiddenTilesPrefab;
    [SerializeField] GameObject counterEffectPrefab;
    [SerializeField] GameObject wordEffectPrefab;

    public bool spriteCombo;

    public Transform DynamicUIElementsHolder { get => dynamicUIElementsHolder; set => dynamicUIElementsHolder = value; }

    public void InitLevel(TilesStacksLevelData levelData,int levelIndex)
    {
        levelText.text = "LEVEL "+(levelIndex+1).ToString();
        progressBar.fillAmount = 0;
        SetTurns(levelData.numTurns);
    }

    public void SetTurns(int turns)
    {
        turnsText.text = "TURNS:<color=orange> " + turns.ToString();
    }

    public void ShowTutorialImage(bool show, int imageIndex)
    {
        if (show)
        {
            if (imageIndex == 0)
                tutorialImageView.ShowTutorial(tutorialImages[0]);
            else
                tutorialImageView.ShowTutorial(tutorialImages[imageIndex]);
        }
        else
        {
            //hide
            tutorialImageView.HideTutorial();
        }
    }

    public bool IsTutorialImageShowing()
    {
        return tutorialImageView.gameObject.activeInHierarchy;
    }

    public void UpdateProgressBar(float progressValue)
    {
        progressBar.fillAmount = progressValue;
    }


    public GameObject GenerateHiddenTilesIndication(Vector3 worldPos)
    {
        GameObject hiddenTilesIndication = Instantiate(hiddenTilesPrefab,dynamicUIElementsHolder);

        hiddenTilesIndication.GetComponent<RectTransform>().localPosition = TileStacksGameManager.Instance.WorldToRect(worldPos) - new Vector2(0,70);

        return hiddenTilesIndication;
    }

    internal void GenerateCounterEffect(int count, float delay, int colorIndex, TileStacksColorButtonView clickedButton)
    {
        GameObject counterEffectGO = Instantiate(counterEffectPrefab, dynamicUIElementsHolder);

        counterEffectGO.GetComponent<RectTransform>().localPosition = TileStacksGameManager.Instance.WorldToRect(clickedButton.gameObject.transform.position);

        RectTransform counterRect = counterEffectGO.GetComponent<RectTransform>();
        TMP_Text counterText = counterEffectGO.GetComponent<TMP_Text>();
        counterText.text = "0";
        //counterText.color = TileStacksUtils.GetDarkerColor(TileStacksModelManager.Instance.GetTileColor(colorIndex));
        
        float startY = counterRect.localPosition.y;

        counterRect.DOAnchorPosY(startY + (count * 10.5f),delay).OnComplete(()=>
        {           

            if(count> MIN_COMBO_FOR_WORD)
            {
                if(spriteCombo)
                {
                    GameObject wordEffectGO = Instantiate(wordEffectPrefab, dynamicUIElementsHolder);
                    Image effectImage = wordEffectGO.GetComponent<Image>();
                    effectImage.sprite = completionWords[GetWordIndex(count)];
                    effectImage.SetNativeSize();
                    CanvasGroup effectCanvas = wordEffectGO.GetComponent<CanvasGroup>();
                    RectTransform effectRect = wordEffectGO.GetComponent<RectTransform>();
                    effectCanvas.alpha = 0;

                    //put the effect on the number and show the word

                    effectRect.localPosition = counterRect.localPosition;
                    effectRect.localScale = Vector3.one * 0.25f;

                    float startY = effectRect.localPosition.y;

                    effectRect.DOLocalMove(new Vector2(0, startY + 200), 1);
                    effectCanvas.DOFade(1, 0.25f).OnComplete(() =>
                    {
                        effectCanvas.DOFade(0, 0.15f).SetDelay(0.6f).OnComplete(() =>
                        {
                            Destroy(wordEffectGO);
                        });
                    });
                    effectRect.DOScale(1f, 0.95f);
                }
                else
                {
                    GameObject wordEffectGO = Instantiate(wordEffectPrefab, dynamicUIElementsHolder);
                    //Image effectImage = wordEffectGO.GetComponent<Image>();
                    //effectImage.sprite = completionWords[GetWordIndex(count)];
                    //effectImage.SetNativeSize();
                    TMP_Text effectText = wordEffectGO.GetComponent<TMP_Text>();

                    effectText.color = TileStacksUtils.GetLessSaturatedColor(TileStacksModelManager.Instance.GetTileColor(colorIndex),0.35f);
                    effectText.text = GetWordString(count);

                    CanvasGroup effectCanvas = wordEffectGO.GetComponent<CanvasGroup>();
                    RectTransform effectRect = wordEffectGO.GetComponent<RectTransform>();
                    effectCanvas.alpha = 0;

                    //put the effect on the number and show the word

                    effectRect.localPosition = counterRect.localPosition;
                    effectRect.localScale = Vector3.one * 0.25f;

                    float startY = effectRect.localPosition.y;

                    effectRect.DOLocalMove(new Vector2(0, startY + 200), 1).SetEase(Ease.InOutSine);
                    effectCanvas.DOFade(1, 0.25f).OnComplete(() =>
                    {
                        effectCanvas.DOFade(0, 0.15f).SetDelay(0.6f).OnComplete(() =>
                        {
                            Destroy(wordEffectGO);
                        });
                    });
                    effectRect.DOScale(1.1f, 0.95f).SetEase(Ease.InCubic);
                }


            }

            //destory the Game object and generate an effect for big/huge....
            Destroy(counterEffectGO);

        });
        DOVirtual.Int(0, count, delay-0.05f, (countValue) =>
        {
            counterText.text = countValue.ToString();
            counterText.fontSize = 50 + countValue;
        });

    }


    public static int GetWordIndex(int currentCombo)
    {

        if (currentCombo <= MIN_COMBO_FOR_WORD)
        {
            return -1; // Below 3, return -1
        }
        //Sublime
        //Celestial
        //Glorious

        if (currentCombo > 50)
        {
            return 14; // Block Party
        }

        // Define thresholds and return index based on the range
        if (currentCombo > 48) return 14; // Sublime
        if (currentCombo > 46) return 13; // Celestial
        if (currentCombo > 44) return 12; // Glorious
        if (currentCombo > 42) return 11; // Cosmic
        if (currentCombo > 40) return 10; // Universal
        if (currentCombo > 38) return 9; // Infinite
        if (currentCombo > 35) return 8;  // Godlike
        if (currentCombo > 33) return 7;  // Legendary
        if (currentCombo > 30) return 6;  // Epic
        if (currentCombo > 27) return 5;  // Enormous
        if (currentCombo > 24) return 4;  // Colossal
        if (currentCombo > 20) return 3;  // Gigantic
        if (currentCombo > 16) return 2;  // Massive
        if (currentCombo > 12) return 1;   // Huge
        if (currentCombo > MIN_COMBO_FOR_WORD) return 0;   // Big

        return 0; // Default case, should never be reached
    }

    public static string GetWordString(int currentCombo)
    {

        if (currentCombo <= MIN_COMBO_FOR_WORD)
        {
            return ""; // Below 3, return -1
        }
        //Sublime
        //Celestial
        //Glorious

        if (currentCombo > 50)
        {
            return "Sublime"; // Block Party
        }

        // Define thresholds and return index based on the range
        if (currentCombo > 48) return "Sublime";
        if (currentCombo > 46) return "Celestial";
        if (currentCombo > 44) return "Glorious";
        if (currentCombo > 42) return "Cosmic";
        if (currentCombo > 40) return "Universal";
        if (currentCombo > 38) return "Infinite";
        if (currentCombo > 35) return "Godlike";
        if (currentCombo > 33) return "Legendary";
        if (currentCombo > 30) return "Epic";
        if (currentCombo > 27) return "Enormous";
        if (currentCombo > 24) return "Colossal";
        if (currentCombo > 20) return "Gigantic";
        if (currentCombo > 16) return "Massive";
        if (currentCombo > 12) return "Huge";
        if (currentCombo > MIN_COMBO_FOR_WORD) return "";   // Big

        return ""; // Default case, should never be reached
    }

}
