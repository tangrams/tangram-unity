using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Styling configuration for the editor.
/// </summary>
public class EditorConfig
{
    public static Color AddButtonColor = new Color(0.8f, 0.8f, 0.8f);

    public static Color DownloadButtonEnabledColor = new Color(0.35f, 0.80f, 0.30f);

    public static Color DownloadButtonDisabledColor = new Color(0.45f, 0.45f, 0.45f);

    public static Color ExportButtonEnabledColor = new Color(0.30f, 0.35f, 0.85f);

    public static Color ExportButtonDisabledColor = new Color(0.45f, 0.45f, 0.45f);

    public static Color RemoveButtonColor = new Color(0.5f, 0.5f, 0.5f);

    public static GUILayoutOption SmallButtonWidth = GUILayout.Width(50.0f);

    public static GUIContent AddButtonContent = new GUIContent("+", "Add");

    public static GUIContent RemoveButtonContent = new GUIContent("-", "Remove");

    public static Color DefaultColor;

    /// <summary>
    /// Globally sets the color of the GUI, saves the current color.
    /// </summary>
    /// <param name="color">Color.</param>
    public static void SetColor(Color color)
    {
        DefaultColor = GUI.color;
        GUI.color = color;
    }

    /// <summary>
    /// Globally resets the color of the GUI to its previous state.
    /// </summary>
    public static void ResetColor()
    {
        GUI.color = DefaultColor;
    }
}

