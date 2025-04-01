using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TMP_Text resultText;
    [SerializeField] TMP_Text buttonText;
    bool lastLevelResult;

    void Awake()
    {
        Instance = this;
    }


    public void ShowGameOver(bool win)
    {
        gameOverPanel.SetActive(true);
        resultText.text = win ? "You Win!" : "You Lose!";
        buttonText.text = win ? "Next" : "Try Again";
        lastLevelResult = win;
    }

    public void NextButtonClicked()
    {
        gameOverPanel.SetActive(false);
        GameManager.Instance.EndGameScreenClicked(lastLevelResult);
    }

}
