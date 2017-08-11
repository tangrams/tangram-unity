using UnityEngine;
using UnityEditor;
using Mapzen;
using System;

public class PolylineBuilderEditor
{
    private bool show = false;
    private PolylineBuilder.Options defaultOptions;

    public PolylineBuilder.Options DefaultOptions
    {
        get
        {
            return defaultOptions;
        }
    }

    public PolylineBuilderEditor()
    {
        defaultOptions = new PolylineBuilder.Options();

        defaultOptions.Extrude = true;
        defaultOptions.MaxHeight = 3.0f;
        defaultOptions.MiterLimit = 3.0f;
        defaultOptions.Width = 15.0f;
    }

    private void LoadPreferences()
    {
        show = EditorPrefs.GetBool("PolylineBuilderEditor.show");
    }

    private void SavePreferences()
    {
        EditorPrefs.SetBool("PolylineBuilderEditor.show", show);
    }

    public PolylineBuilder.Options OnInspectorGUI(PolylineBuilder.Options options)
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
