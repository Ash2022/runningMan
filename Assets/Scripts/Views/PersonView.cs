using UnityEngine;

public class PersonView : MonoBehaviour
{
    [SerializeField] private Renderer modelRenderer;

    public void SetColor(int colorId)
    {
        modelRenderer.material.color = Utils.GetColorFromId(colorId);
    }
}
