using UnityEngine;
using UnityEditor;
using Mapzen;
using System;

public class PolygonBuilderEditor
{
    private class PolygonBuilderEditorPrefs
    {
        public bool show = false;
    }

    public static PolygonBuilder.Options DefaultOptions()
    {
        var defaultOptions = new PolygonBuilder.Options();

        defaultOptions.Extrude = true;
        defaultOptions.Enabled = true;
        defaultOptions.MaxHeight = 0.0f;

        return defaultOptions;
    }

    private static PolygonBuilderEditorPrefs LoadPreferences(string name)
    {
        var prefs = new PolygonBuilderEditorPrefs();
        prefs.show = EditorPrefs.GetBool(name + '.' + typeof(PolygonBuilderEditor).Name + ".show");
        return prefs;
    }

    private static void SavePreferences(PolygonBuilderEditorPrefs prefs, string name)
    {
        EditorPrefs.SetBool(name + '.' + typeof(PolygonBuilderEditor).Name + ".show", prefs.show);
    }

    public static PolygonBuilder.Options OnInspectorGUI(PolygonBuilder.Options options, string panelName)
    {
        var prefs = PolygonBuilderEditor.LoadPreferences(panelName);
        prefs.show = EditorGUILayout.Foldout(prefs.show, "Polygon builder options");
        if (!prefs.show)
        {
            PolygonBuilderEditor.SavePreferences(prefs, panelName);
            return options;
        }

        options.MaxHeight = EditorGUILayout.FloatField("Max Height: ", options.MaxHeight);
        options.Extrude = EditorGUILayout.Toggle("Extrude: ", options.Extrude);
        options.Enabled = EditorGUILayout.Toggle("Enabled: ", options.Enabled);

        PolygonBuilderEditor.SavePreferences(prefs, panelName);

        return options;
    }
}
