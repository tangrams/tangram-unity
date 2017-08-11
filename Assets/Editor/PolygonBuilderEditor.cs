using UnityEngine;
using UnityEditor;
using Mapzen;
using System;

public class PolygonBuilderEditor
{
    private bool show = false;
    private PolygonBuilder.Options defaultOptions;

    public PolygonBuilder.Options DefaultOptions
    {
        get
        {
            return defaultOptions;
        }
    }

    public PolygonBuilderEditor()
    {
        defaultOptions = new PolygonBuilder.Options();

        defaultOptions.Extrude = true;
        defaultOptions.MaxHeight = 0.0f;
    }

    private void LoadPreferences(string name)
    {
        show = EditorPrefs.GetBool("PolygonBuilderEditor.show" + name);
    }

    private void SavePreferences(string name)
    {
        EditorPrefs.SetBool("PolygonBuilderEditor.show" + name, show);
    }

    public PolygonBuilder.Options OnInspectorGUI(PolygonBuilder.Options options, string name)
    {
        LoadPreferences(name);
        show = EditorGUILayout.Foldout(show, "Polygon builder options");
        if (!show)
        {
            SavePreferences(name);
            return options;
        }

        options.MaxHeight = EditorGUILayout.FloatField("Max Height: ", options.MaxHeight);
        options.Extrude = EditorGUILayout.Toggle("Extrude: ", options.Extrude);

        SavePreferences(name);

        return options;
    }
}
