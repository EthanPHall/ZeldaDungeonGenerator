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
    White,
    Invalid
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

    public static List<KeyColors> GetAllColors()
    {
        return new List<KeyColors> { KeyColors.Black, KeyColors.Blue, KeyColors.Green, KeyColors.Red, KeyColors.Yellow, KeyColors.Cyan, KeyColors.Magenta, KeyColors.White };
    }

    public static int GetRelativePlacement(KeyColors keyColor)
    {
        switch (keyColor)
        {
            case KeyColors.White:
                return 0;
            case KeyColors.Magenta:
                return 1;
            case KeyColors.Cyan:
                return 2;
            case KeyColors.Yellow:
                return 3;
            case KeyColors.Red:
                return 4;
            case KeyColors.Green:
                return 5;
            case KeyColors.Blue:
                return 6;
            case KeyColors.Black:
                return 7;
            default:
                return 8;
        }
    }

    public static KeyColors GetPreviousColor(KeyColors keyColor)
    {
        switch (keyColor)
        {
            case KeyColors.Magenta:
                return KeyColors.White;
            case KeyColors.Cyan:
                return KeyColors.Magenta;
            case KeyColors.Yellow:
                return KeyColors.Cyan;
            case KeyColors.Red:
                return KeyColors.Yellow;
            case KeyColors.Green:
                return KeyColors.Red;
            case KeyColors.Blue:
                return KeyColors.Green;
            case KeyColors.Black:
                return KeyColors.Blue;
            default:
                return KeyColors.Invalid;
        }
    }

    public static KeyColors GetBossColor()
    {
        return KeyColors.Black;
    }
}