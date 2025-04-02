using UnityEngine;
using UnityEngine.UI;

public class TileStacksColorButtonView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] RectTransform rect;

    private int colorID;
    private int buttonIndex;

    public void Setup(int id, int _buttonIndex)
    {
        colorID = id;
        buttonIndex = _buttonIndex;
        background.color = TileStacksUtils.GetColorFromID(colorID);
        button.onClick.AddListener(() => TileStacksGameManager.Instance.OnColorButtonClicked(colorID, buttonIndex));
    }

    
}
