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

    public PolygonBuilder.Options OnInspectorGUI()
    {
        show = EditorGUILayout.Foldout(show, "Polygon builder options");
        if (!show)
        {
            return options;
        }

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
