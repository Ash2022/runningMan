using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileStacksColorButtonView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] RectTransform rect;
    [SerializeField] TMP_Text counter;

    private int colorID;
    private int buttonIndex;

    public void Setup(int id, int _buttonIndex)
    {
        counter.text = "";
        colorID = id;
        buttonIndex = _buttonIndex;
        background.color = TileStacksUtils.GetColorFromID(colorID);
        button.onClick.AddListener(() => ButtonClicked());
    }

    private void ButtonClicked()
    {
        background.enabled = false;
        TileStacksGameManager.Instance.OnColorButtonClicked(colorID, buttonIndex,this);
    }

    public void ShowButton()
    {
        background.enabled = true;
    }

    internal void UpdateCounter(int v)
    {
        counter.text = v.ToString();
    }
}
