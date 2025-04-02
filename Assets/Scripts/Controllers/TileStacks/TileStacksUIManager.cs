using TMPro;
using UnityEngine;

public class TileStacksUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text turnsText;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TMP_Text resultText;
    [SerializeField] TMP_Text buttonText;
    bool lastLevelResult;

    public void SetTurns(int turns)
    {
        turnsText.text = "Turns: " + turns.ToString();
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
        TileStacksGameManager.Instance.EndGameScreenClicked(lastLevelResult);
    }
}
