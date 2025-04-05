using System;

using UnityEngine;

public class ButtonClickForwarder : MonoBehaviour
{
    [SerializeField] Collider buttonCollider;
    Action clickAction=null;

    public void InitClick(Action _clickAction)
    {
        clickAction = _clickAction;
    }

    void OnMouseDown()
    {
        clickAction?.Invoke();
    }

    public void EndableDisableButton(bool enable)
    {
        buttonCollider.enabled = enable;
    }
}
