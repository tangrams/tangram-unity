using UnityEngine;
using UnityEditor;
using Mapzen;
using System;

public class PolylineBuilderEditor
{
    private bool show = false;
    private PolylineBuilder.Options options;
    private string maxHeight;
    private string width;

    private static GUILayoutOption layoutWidth = GUILayout.Width(200);

    public PolylineBuilderEditor()
    {
        options = new PolylineBuilder.Options();

        options.Extrude = true;
        options.MaxHeight = 3.0f;
        options.MiterLimit = 3.0f;
        options.Width = 15.0f;

        maxHeight = options.MaxHeight.ToString();
        width = options.Width.ToString();
    }

    public PolylineBuilder.Options OnInspectorGUI()
    {
        show = EditorGUILayout.Foldout(show, "Polyline builder options");
        if (!show)
        {
            return options;
        }

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Width ");
            EditorUtil.FloatField(ref width, ref options.Width, layoutWidth);
        }
        GUILayout.EndHorizontal();

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