using UnityEngine;
using UnityEditor;
using Mapzen;
using System;

public class PolylineBuilderEditor
{
    private class PolylineBuilderEditorPrefs
    {
        public bool show = false;
    }

    public static PolylineBuilder.Options DefaultOptions()
    {
        var defaultOptions = new PolylineBuilder.Options();

        defaultOptions.Extrude = true;
        defaultOptions.MaxHeight = 3.0f;
        defaultOptions.MiterLimit = 3.0f;
        defaultOptions.Width = 15.0f;

        return defaultOptions;
    }

    private static PolylineBuilderEditorPrefs LoadPreferences(string name)
    {
        var prefs = new PolylineBuilderEditorPrefs();
        prefs.show = EditorPrefs.GetBool("PolylineBuilderEditor.show" + name);
        return prefs;
    }

    private static void SavePreferences(PolylineBuilderEditorPrefs prefs, string name)
    {
        EditorPrefs.SetBool("PolylineBuilderEditor.show" + name, prefs.show);
    }

    public static PolylineBuilder.Options OnInspectorGUI(PolylineBuilder.Options options, string name)
    {
        var prefs = PolylineBuilderEditor.LoadPreferences(name);
        prefs.show = EditorGUILayout.Foldout(prefs.show, "Polyline builder options");
        if (!prefs.show)
        {
            PolylineBuilderEditor.SavePreferences(prefs, name);
            return options;
        }

        options.Width = EditorGUILayout.FloatField("Width: ", options.Width);
        options.MaxHeight = EditorGUILayout.FloatField("Max Height: ", options.MaxHeight);
        options.Extrude = EditorGUILayout.Toggle("Extrude: ", options.Extrude);

        PolylineBuilderEditor.SavePreferences(prefs, name);

        return options;
    }
}
