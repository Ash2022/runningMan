using UnityEngine;

public static class Utils
{
    private static readonly Color[] palette = {
        Color.red, Color.green, Color.blue, Color.yellow, Color.cyan,
        new Color(1f, 0.5f, 0f), Color.magenta, Color.gray
    };

    public static Color GetColorFromId(int id)
    {
        return palette[id % palette.Length];
    }

    public static string GetColorNameFromId(int id)
    {
        string[] names = {
            "red", "green", "blue", "yellow", "cyan",
            "orange", "magenta", "gray"
        };
        return names[id % names.Length];
    }



}
