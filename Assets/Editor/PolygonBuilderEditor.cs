using UnityEngine;
using UnityEditor;
using Mapzen;
using System;

public class PolygonBuilderEditor
{
    private bool show = false;
    private PolygonBuilder.Options options;

    public PolygonBuilderEditor()
    {
        options = new PolygonBuilder.Options();

        options.Extrude = true;
        options.MaxHeight = 0.0f;
    }

    private void LoadPreferences()
    {
        show = EditorPrefs.GetBool("PolygonBuilderEditor.show");
    }

    private void SavePreferences()
    {
        EditorPrefs.SetBool("PolygonBuilderEditor.show", show);
    }

    public PolygonBuilder.Options OnInspectorGUI()
    {
        LoadPreferences();
        show = EditorGUILayout.Foldout(show, "Polygon builder options");
        if (!show)
        {
            SavePreferences();
            return options;
        }

        options.MaxHeight = EditorGUILayout.FloatField("Max Height: ", options.MaxHeight);

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Extrude: ");
            options.Extrude = GUILayout.Toggle(options.Extrude, "Extrude");
        }
        GUILayout.EndHorizontal();

        SavePreferences();

        return options;
    }
}
