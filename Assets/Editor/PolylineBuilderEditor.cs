using UnityEngine;
using UnityEditor;
using Mapzen;
using System;

public class PolylineBuilderEditor
{
    private bool show = false;
    private PolylineBuilder.Options options;

    public PolylineBuilder.Options Options
    {
        get
        {
            return options;
        }
    }

    public PolylineBuilderEditor()
    {
        options = new PolylineBuilder.Options();

        options.Extrude = true;
        options.MaxHeight = 3.0f;
        options.MiterLimit = 3.0f;
        options.Width = 15.0f;
    }

    private void LoadPreferences()
    {
        show = EditorPrefs.GetBool("PolylineBuilderEditor.show");
    }

    private void SavePreferences()
    {
        EditorPrefs.SetBool("PolylineBuilderEditor.show", show);
    }

    public PolylineBuilder.Options OnInspectorGUI()
    {
        LoadPreferences();
        show = EditorGUILayout.Foldout(show, "Polyline builder options");
        if (!show)
        {
            SavePreferences();
            return options;
        }

        options.Width = EditorGUILayout.FloatField("Width: ", options.Width);
        options.MaxHeight = EditorGUILayout.FloatField("Max Height: ", options.MaxHeight);
        options.Extrude = EditorGUILayout.Toggle("Extrude: ", options.Extrude);

        SavePreferences();

        return options;
    }
}
