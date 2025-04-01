using UnityEngine;

public class UnitView : MonoBehaviour
{
    [SerializeField] private Renderer modelRenderer;

    public void SetColor(int colorId)
    {
        modelRenderer.material.color = Utils.GetColorFromId(colorId);

        gameObject.name = $"Unit_Color_{colorId}_{Utils.GetColorNameFromId(colorId)}";

    }

    
}