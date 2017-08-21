using System;
using UnityEditor;
using UnityEngine;

public class EditorStyle
{
    public static Color AddButtonColor = new Color(0.8f, 0.8f, 0.8f);

    public static Color DownloadButtonEnabledColor = new Color(0.35f, 0.80f, 0.30f);

    public static Color DownloadButtonDisabledColor = new Color(0.45f, 0.45f, 0.45f);

    public static Color RemoveButtonColor = new Color(0.5f, 0.5f, 0.5f);

    public static GUILayoutOption SmallButtonWidth = GUILayout.Width(50.0f);

    public static GUIContent AddButtonContent = new GUIContent("+", "Add");

    public static GUIContent RemoveButtonContent = new GUIContent("-", "Remove");

    public static Color DefaultColor;

    public static void SetColor(Color color)
    {
        DefaultColor = GUI.color;
        GUI.color = color;
    }

    public static void ResetColor()
    {
        GUI.color = DefaultColor;
    }
}

