using UnityEngine;
using UnityEditor;
using Mapzen;
using Mapzen.VectorData.Filters;
using System.Collections.Generic;
using System.Linq;
using System;

public class FeatureStyleEditor
{
    private class FeatureStyleEditorPrefs
    {
        public bool show = true;
        public Dictionary<string, bool> showStyle;
        public Dictionary<string, string> filterStyleName;

        public FeatureStyleEditorPrefs()
        {
            this.showStyle = new Dictionary<string, bool>();
            this.filterStyleName = new Dictionary<string, string>();
        }
    }

    private static string featureStyleName = "";

    private static FeatureStyleEditorPrefs LoadPreferences(MapzenMap mapzenMap)
    {
        FeatureStyleEditorPrefs preferences = new FeatureStyleEditorPrefs();

        preferences.show = EditorPrefs.GetBool(typeof(FeatureStyleEditor).Name + ".show");

        foreach (var featureStyling in mapzenMap.FeatureStyling)
        {
            string prefKey = typeof(FeatureStyleEditor).Name + ".showStyle." + featureStyling.Name;
            preferences.showStyle[featureStyling.Name] = EditorPrefs.GetBool(prefKey);
            prefKey = typeof(FeatureStyleEditor).Name + ".filterStyleName." + featureStyling.Name;
            preferences.filterStyleName.Add(featureStyling.Name, EditorPrefs.GetString(prefKey));
        }

        return preferences;
    }

    private static void SavePreferences(FeatureStyleEditorPrefs preferences)
    {
        EditorPrefs.SetBool(typeof(FeatureStyleEditor).Name + ".show", preferences.show);

        foreach (var featureStyleName in preferences.showStyle.Keys)
        {
            string prefKey = typeof(FeatureStyleEditor).Name + ".showStyle." + featureStyleName;
            EditorPrefs.SetBool(prefKey, preferences.showStyle[featureStyleName]);
            prefKey = typeof(FeatureStyleEditor).Name + ".filterStyleName." + featureStyleName;
            EditorPrefs.SetString(prefKey, preferences.filterStyleName[featureStyleName]);
        }
    }

    private static void AddStyle(FeatureStyleEditorPrefs prefs, MapzenMap mapzenMap)
    {
        var queryStyleName = mapzenMap.FeatureStyling.Where(style => style.Name == featureStyleName);

        if (featureStyleName.Length == 0)
        {
            Debug.LogError("The style name can't be empty");
        }
        else if (queryStyleName.Count() > 0)
        {
            Debug.LogError("Style with name " + featureStyleName + " already exists");
        }
        else
        {
            var featureStyle = new FeatureStyle(featureStyleName);
            mapzenMap.FeatureStyling.Add(featureStyle);

            prefs.showStyle[featureStyle.Name] = false;
            prefs.filterStyleName[featureStyle.Name] = ""; 
        }
    }

    private static void AddFilter(FeatureStyleEditorPrefs prefs, FeatureStyle featureStyling, string filterStyleName)
    {
        var queryFilterStyleName = featureStyling.FilterStyles.Where(filterStyle => filterStyle.Name == filterStyleName);

        if (filterStyleName.Length == 0)
        {
            Debug.LogError("The filter name can't be empty");
        }
        else if (queryFilterStyleName.Count() > 0)
        {
            Debug.LogError("Filter with name " + filterStyleName + " already exists");
        }
        else
        {
            var filterStyle = new FeatureStyle.FilterStyle(filterStyleName);
            featureStyling.AddFilterStyle(filterStyle);
        }
    }

    public static void OnInspectorGUI(MapzenMap mapzenMap, string panelName)
    {
        var prefs = FeatureStyleEditor.LoadPreferences(mapzenMap);

        prefs.show = EditorGUILayout.Foldout(prefs.show, panelName);
        if (!prefs.show)
        {
            FeatureStyleEditor.SavePreferences(prefs);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        {
            featureStyleName = EditorGUILayout.TextField("Style name: ", featureStyleName);

            EditorStyle.SetColor(EditorStyle.AddButtonColor);
            if (GUILayout.Button(EditorStyle.AddButtonContent, EditorStyle.SmallButtonWidth))
            {
                FeatureStyleEditor.AddStyle(prefs, mapzenMap);
            }
            EditorStyle.ResetColor();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel++;

        for (int i = mapzenMap.FeatureStyling.Count - 1; i >= 0; i--)
        {
            var featureStyling = mapzenMap.FeatureStyling[i];
            bool showFeatureStyle = false;

            EditorGUILayout.BeginHorizontal();
            {
                showFeatureStyle = EditorGUILayout.Foldout(prefs.showStyle[featureStyling.Name], featureStyling.Name);
                prefs.showStyle[featureStyling.Name] = showFeatureStyle;

                EditorStyle.SetColor(EditorStyle.RemoveButtonColor);
                if (GUILayout.Button(EditorStyle.RemoveButtonContent, EditorStyle.SmallButtonWidth))
                {
                    mapzenMap.FeatureStyling.RemoveAt(i);
                }
                EditorStyle.ResetColor();
            }
            EditorGUILayout.EndHorizontal();

            if (!showFeatureStyle)
            {
                continue;
            }

            EditorGUILayout.BeginHorizontal();
            {
                prefs.filterStyleName[featureStyling.Name] = 
                    EditorGUILayout.TextField("Filter name: ", prefs.filterStyleName[featureStyling.Name]);

                EditorStyle.SetColor(EditorStyle.AddButtonColor);
                if (GUILayout.Button(EditorStyle.AddButtonContent, EditorStyle.SmallButtonWidth))
                {
                    FeatureStyleEditor.AddFilter(prefs, featureStyling, prefs.filterStyleName[featureStyling.Name]);
                }
                EditorStyle.ResetColor();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;

            foreach (var filterStyle in featureStyling.FilterStyles)
            {
                FilterStyleEditor.OnInspectorGUI(filterStyle);
            }

            EditorGUI.indentLevel--;

            // Separator
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        EditorGUI.indentLevel--;

        FeatureStyleEditor.SavePreferences(prefs);
    }
}
