using UnityEngine;
using UnityEditor;
using Mapzen;
using System;

public class PolygonBuilderEditor
{
    private bool show = false;
    private PolygonBuilder.Options options;
    private string maxHeight;

    private static GUILayoutOption layoutWidth = GUILayout.Width(200);

    public PolygonBuilderEditor()
    {
        options = new PolygonBuilder.Options();

        options.Extrude = true;
        options.MaxHeight = 0.0f;

        maxHeight = options.MaxHeight.ToString();
    }

    public PolygonBuilder.Options OnInspectorGUI()
    {
        show = EditorGUILayout.Foldout(show, "Polygon builder options");
        if (!show)
        {
            return options;
        }

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Max Height: ");
            EditorUtil.FloatField(ref maxHeight, ref options.MaxHeight, layoutWidth);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Extrude: ");
            options.Extrude = GUILayout.Toggle(options.Extrude, "Extrude", layoutWidth);
        }
        GUILayout.EndHorizontal();

        return options;
    }
}
