using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TileStacksUtils 
{
    
    public static Color GetColorFromID(int id)
    {
        Color[] palette = new Color[]
        {
            Color.red, Color.green, Color.blue,
            Color.yellow, Color.magenta, Color.cyan,
            new Color(1f, 0.5f, 0f), // orange
            Color.gray, Color.white
        };

        return palette[id % palette.Length];
    }


    public static float GetButtonWorldX(int buttonCount, int buttonIndex)
    {
        if (buttonCount <= 0) return 0f;

        float totalWidth = 6f;
        float unitWidth = totalWidth / buttonCount;

        return -3f + unitWidth * (buttonIndex + 0.5f);
    }

    public static Color GetDarkerColor(Color original)
    {
        Color.RGBToHSV(original, out float h, out float s, out float v);
        v = Mathf.Clamp01(v - 0.25f);
        return Color.HSVToRGB(h, s, v);
    }
}
