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
        defaultOptions.Enabled = true;
        defaultOptions.MaxHeight = 3.0f;
        defaultOptions.MiterLimit = 3.0f;
        defaultOptions.Width = 15.0f;

        return defaultOptions;
    }

    private static PolylineBuilderEditorPrefs LoadPreferences(string name)
    {
        var prefs = new PolylineBuilderEditorPrefs();
        prefs.show = EditorPrefs.GetBool(name + '.' + typeof(PolylineBuilderEditor).Name + ".show");
        return prefs;
    }

    private static void SavePreferences(PolylineBuilderEditorPrefs prefs, string name)
    {
        EditorPrefs.SetBool(name + '.' + typeof(PolylineBuilderEditor).Name + ".show", prefs.show);
    }

    public static PolylineBuilder.Options OnInspectorGUI(PolylineBuilder.Options options, string panelName)
    {
        var prefs = PolylineBuilderEditor.LoadPreferences(panelName);
        prefs.show = EditorGUILayout.Foldout(prefs.show, "Polyline builder options");
        if (!prefs.show)
        {
            PolylineBuilderEditor.SavePreferences(prefs, panelName);
            return options;
        }

        options.Width = EditorGUILayout.FloatField("Width: ", options.Width);
        options.MaxHeight = EditorGUILayout.FloatField("Max Height: ", options.MaxHeight);
        options.Extrude = EditorGUILayout.Toggle("Extrude: ", options.Extrude);
        options.Enabled = EditorGUILayout.Toggle("Enabled: ", options.Enabled);

        PolylineBuilderEditor.SavePreferences(prefs, panelName);

        return options;
    }
}
