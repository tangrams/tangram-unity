using UnityEngine;
using UnityEditor;
using Mapzen;
using System;

public class PolylineBuilderEditor
{
    private bool show = false;
    private PolylineBuilder.Options options;

    public PolylineBuilderEditor()
    {
        options = new PolylineBuilder.Options();

        options.Extrude = true;
        options.MaxHeight = 3.0f;
        options.MiterLimit = 3.0f;
        options.Width = 15.0f;
    }

    public PolylineBuilder.Options OnInspectorGUI()
    {
        show = EditorGUILayout.Foldout(show, "Polyline builder options");
        if (!show)
        {
            return options;
        }

        options.Width = EditorGUILayout.FloatField("Width: ", options.Width);
        options.MaxHeight = EditorGUILayout.FloatField("Max Height: ", options.MaxHeight);

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Extrude: ");
            options.Extrude = GUILayout.Toggle(options.Extrude, "Extrude");
        }
        GUILayout.EndHorizontal();

        return options;
    }
}