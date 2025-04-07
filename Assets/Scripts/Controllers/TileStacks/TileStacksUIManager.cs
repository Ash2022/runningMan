using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileStacksUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text turnsText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TMP_Text resultText;
    [SerializeField] TMP_Text buttonText;
    [SerializeField] Image progressBar;

    [SerializeField] List<Sprite> tutorialImages = new List<Sprite>();
    [SerializeField] TutorialImageView tutorialImageView;


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
}
