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

    [SerializeField] List<Sprite> tutorialImages = new List<Sprite>();
    [SerializeField] TutorialImageView tutorialImageView;

    [SerializeField]List<Sprite> completionWords = new List<Sprite>();

    [SerializeField] Transform dynamicUIElementsHolder;
    [SerializeField] GameObject hiddenTilesPrefab;
    [SerializeField] GameObject counterEffectPrefab;
    [SerializeField] GameObject wordEffectPrefab;

    //[SerializeField] Material bgMaterial;

    public bool spriteCombo;

    public Transform DynamicUIElementsHolder { get => dynamicUIElementsHolder; set => dynamicUIElementsHolder = value; }

    public void InitLevel(TilesStacksLevelData levelData,int levelIndex)
    {
        levelText.text = "LEVEL "+(levelIndex+1).ToString();
        SetTurns(levelData.numTurns);

        //bgMaterial.mainTexture = TileStacksModelManager.Instance.GetBGSprite((levelIndex / 4) % 4);
    }

    public void SetTurns(int turns)
    {
        turnsText.text = "MOVES:<color=orange> " + turns.ToString();
    }

    public void ShowTutorialImage(bool show, int imageIndex)
    {
        if (show)
        {
            levelText.text = "";
            turnsText.text = ""; 

            if (imageIndex == 0)
                tutorialImageView.ShowTutorial(tutorialImages[0]);
            else
            {
                Sprite auxImage = null;

                if (imageIndex == 1)
                    auxImage=TileStacksModelManager.Instance.GetUnlockedColorSprite(0);
                else if(imageIndex == 4)
                    auxImage = TileStacksModelManager.Instance.GetUnlockedColorSprite(1);
                else if(imageIndex == 6)
                    auxImage = TileStacksModelManager.Instance.GetUnlockedColorSprite(2);
                else if(imageIndex == 8)
                    auxImage = TileStacksModelManager.Instance.GetUnlockedColorSprite(3);

                tutorialImageView.ShowTutorial(tutorialImages[imageIndex],auxImage);
            }
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


    public GameObject GenerateHiddenTilesIndication(Vector3 worldPos)
    {
        GameObject hiddenTilesIndication = Instantiate(hiddenTilesPrefab,dynamicUIElementsHolder);

        hiddenTilesIndication.GetComponent<RectTransform>().localPosition = TileStacksGameManager.Instance.WorldToRect(worldPos) - new Vector2(0,60);

        return hiddenTilesIndication;
    }

    internal void GenerateCounterEffect(int count, float delay, int colorIndex, TileStacksColorButtonView clickedButton)
    {
        GameObject counterEffectGO = Instantiate(counterEffectPrefab, dynamicUIElementsHolder);

        counterEffectGO.GetComponent<RectTransform>().localPosition = TileStacksGameManager.Instance.WorldToRect(clickedButton.gameObject.transform.position);

        RectTransform counterRect = counterEffectGO.GetComponent<RectTransform>();
        TMP_Text counterText = counterEffectGO.GetComponent<TMP_Text>();
        counterText.text = "0";
        
        counterText.color = TileStacksUtils.GetLessSaturatedColor(TileStacksModelManager.Instance.GetTileColor(colorIndex), 0.35f);

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
                    effectRect.localScale = Vector3.one * 0.35f;

                    float startY = effectRect.localPosition.y;

                    effectRect.DOLocalMoveY(startY + 380, 1.15f).SetEase(Ease.OutBack);
                    effectRect.DOLocalMoveX(0, 1.15f).SetEase(Ease.OutExpo);



                    effectCanvas.DOFade(1, 0.25f).OnComplete(() =>
                    {
                        effectCanvas.DOFade(0, 0.15f).SetDelay(0.75f).OnComplete(() =>
                        {
                            Destroy(wordEffectGO);
                        });
                    });
                    effectRect.DOScale(1.15f, 0.95f).SetEase(Ease.OutExpo);
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
        if (currentCombo > 73) return 14; // Sublime
        if (currentCombo > 69) return 13; // Celestial
        if (currentCombo > 65) return 12; // Glorious
        if (currentCombo > 61) return 11; // Cosmic
        if (currentCombo > 57) return 10; // Universal
        if (currentCombo > 53) return 9; // Infinite
        if (currentCombo > 48) return 8;  // Godlike
        if (currentCombo > 43) return 7;  // Legendary
        if (currentCombo > 38) return 6;  // Epic
        if (currentCombo > 33) return 5;  // Enormous
        if (currentCombo > 28) return 4;  // Colossal
        if (currentCombo > 23) return 3;  // Gigantic
        if (currentCombo > 18) return 2;  // Massive
        if (currentCombo > 13) return 1;   // Huge
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

        // Define thresholds and return index based on the range
        if (currentCombo > 73) return "Sublime";
        if (currentCombo > 69) return "Celestial";
        if (currentCombo > 65) return "Glorious";
        if (currentCombo > 61) return "Cosmic";
        if (currentCombo > 57) return "Universal";
        if (currentCombo > 53) return "Infinite";
        if (currentCombo > 48) return "Godlike";
        if (currentCombo > 43) return "Legendary";
        if (currentCombo > 38) return "Epic";
        if (currentCombo > 33) return "Enormous";
        if (currentCombo > 28) return "Colossal";
        if (currentCombo > 23) return "Gigantic";
        if (currentCombo > 18) return "Massive";
        if (currentCombo > 13) return "Huge";
        if (currentCombo > MIN_COMBO_FOR_WORD) return "Big";   // Big

        return ""; // Default case, should never be reached
    }

}
