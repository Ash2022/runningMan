using System.Collections.Generic;
using UnityEngine;

public class ObstacleView : MonoBehaviour
{
    

    public List<UnitView> unitViews = new List<UnitView>();

    public void Build(List<int> units, GameObject unitPrefab)
    {
        float totalWidth = 6f;
        int unitCount = units.Count;
        float spacing = totalWidth / unitCount;
        float scale = Mathf.Min(spacing * 0.7f, 1f);
        float startX = -totalWidth / 2f + spacing / 2f;

        for (int i = 0; i < unitCount; i++)
        {
            float x = startX + i * spacing;
            Vector3 localPos = new Vector3(x, 0, 0);

            GameObject unitGO = Instantiate(unitPrefab, transform);
            unitGO.transform.localPosition = localPos;
            unitGO.transform.localScale = new Vector3(scale, 1f, scale);

            UnitView view = unitGO.GetComponent<UnitView>();
            view.SetColor(units[i]);
            unitViews.Add(view);
        }
    }
}