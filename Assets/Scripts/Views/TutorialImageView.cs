using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialImageView : MonoBehaviour
{
    [SerializeField] Image tutImage;

    public void ShowTutorial(Sprite sprite)
    {
        tutImage.sprite = sprite;
        tutImage.SetNativeSize();
        gameObject.SetActive(true);
    }

    public void HideTutorial()
    {
        gameObject.SetActive(false);
    }
}
