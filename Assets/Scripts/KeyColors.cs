using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KeyColors
{
    Black,
    Blue,
    Green,
    Red,
    Yellow,
    Cyan,
    Magenta,
    White
}

public static class KeyColorsUtil
{
    public static Color GetColor(KeyColors keyColor)
    {
        switch (keyColor)
        {
            case KeyColors.Black:
                return Color.black;
            case KeyColors.Blue:
                return Color.blue;
            case KeyColors.Green:
                return Color.green;
            case KeyColors.Red:
                return Color.red;
            case KeyColors.Yellow:
                return Color.yellow;
            case KeyColors.Cyan:
                return Color.cyan;
            case KeyColors.Magenta:
                return Color.magenta;
            case KeyColors.White:
                return Color.white;
            default:
                return Color.grey;
        }
    }

    public static List<KeyColors> GetNonBossColors()
    {
        return new List<KeyColors> { KeyColors.Blue, KeyColors.Green, KeyColors.Red, KeyColors.Yellow, KeyColors.Cyan, KeyColors.Magenta, KeyColors.White };
    }

    public static KeyColors GetBossColor()
    {
        return KeyColors.Black;
    }
}