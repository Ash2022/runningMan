using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TileStacksColorButtonView : MonoBehaviour
{
    [SerializeField] private ButtonClickForwarder buttonClickForwarder;
    [SerializeField] private SpriteRenderer buttonImage;
    [SerializeField] TMP_Text counter;
    [SerializeField] Transform counterBox;
    private int colorID;
    private int buttonIndex;
    private int startCounterValue = 0;
    Vector3 counterBoxStartPosition;

    public void Setup(int _colorID, int _index, Vector3 worldPosition, Vector3 scale)
    {
        transform.position = worldPosition;
        transform.localScale = scale;
        counter.text = "";
        colorID = _colorID;
        buttonIndex = _index;
        buttonImage.color = TileStacksUtils.GetColorFromID(colorID);

        buttonClickForwarder.InitClick(ButtonClicked);

        counterBoxStartPosition = counterBox.localPosition;
    }

    private void ButtonClicked()
    {
        buttonClickForwarder.EndableDisableButton(false);

        Color orgColor = buttonImage.color;

        buttonImage.color = TileStacksUtils.GetLighterColor(orgColor);
        TileStacksGameManager.Instance.OnColorButtonClicked(colorID, buttonIndex,this);

        startCounterValue = TileStacksGameManager.Instance.GetButtonsCollected(colorID);
    }

    public void ShowButton()
    {
        buttonImage.color = TileStacksUtils.GetColorFromID(colorID);
        buttonClickForwarder.EndableDisableButton(true);
    }

    public void UpdateButtonToModel()
    {
        counter.text = TileStacksGameManager.Instance.GetButtonsCollected(colorID).ToString();
        counterBox.transform.localPosition = counterBoxStartPosition;
    }

    internal void UpdateStackingCounter(int counterValue)
    {
        int modifiedValue = counterValue - startCounterValue;
        counter.text = (modifiedValue).ToString();
        counterBox.transform.localPosition = counterBoxStartPosition - new Vector3(0, 0, TileStacksGameManager.TILES_VERTICAL_OFFSET * modifiedValue);
    }
}
