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
        public static string Key = "FeatureStyleEditor";
        public bool show = true;
        public Dictionary<string, bool> showStyle;

        public FeatureStyleEditorPrefs()
        {
            showStyle = new Dictionary<string, bool>();
        }
    }

    private static string featureStyleName = "";

    private static FeatureStyleEditorPrefs LoadPreferences(MapzenMap mapzenMap)
    {
        FeatureStyleEditorPrefs preferences = new FeatureStyleEditorPrefs();

        preferences.show = EditorPrefs.GetBool(FeatureStyleEditorPrefs.Key + ".show");

        foreach (var featureStyling in mapzenMap.FeatureStyling)
        {
            preferences.showStyle[featureStyling.Name] =
                EditorPrefs.GetBool(FeatureStyleEditorPrefs.Key + ".showStyle." + featureStyling.Name);
        }

        return preferences;
    }

    private static void SavePreferences(FeatureStyleEditorPrefs preferences)
    {
        EditorPrefs.SetBool(FeatureStyleEditorPrefs.Key + ".show", preferences.show);

        foreach (var featureStyleName in preferences.showStyle.Keys)
        {
            EditorPrefs.SetBool(FeatureStyleEditorPrefs.Key + ".showStyle." + featureStyleName,
                preferences.showStyle[featureStyleName]);
        }
    }

    public static void OnInspectorGUI(MapzenMap mapzenMap)
    {
        var prefs = LoadPreferences(mapzenMap);

        prefs.show = EditorGUILayout.Foldout(prefs.show, "Filtering and styling");
        if (!prefs.show)
        {
            SavePreferences(prefs);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        {
            featureStyleName = EditorGUILayout.TextField("Style name: ", featureStyleName);

            EditorStyle.SetColor(EditorStyle.AddButtonColor);
            if (GUILayout.Button(EditorStyle.AddButtonContent, EditorStyle.SmallButtonWidth))
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
                    /*
                    var defaultMaterial = new Material(Shader.Find("Diffuse"));
                    var defaultPolygonBuilderOptions = polygonBuilderEditor.DefaultOptions;
                    var defaultPolylineBuilderOptions = polylineBuilderEditor.DefaultOptions;
                    var defaultFilter = new FeatureFilter();

                    var featureStyle = new FeatureStyle(defaultFilter, defaultMaterial, featureStyleName,
                                           defaultPolygonBuilderOptions, defaultPolylineBuilderOptions);
                    */

                    var featureStyle = new FeatureStyle(featureStyleName);
                    mapzenMap.FeatureStyling.Add(featureStyle);

                    prefs.showStyle[featureStyle.Name] = false;
                }
            }
            EditorStyle.ResetColor();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel++;

        for (int i = mapzenMap.FeatureStyling.Count - 1; i >= 0; i--)
        {
            var featureStyling = mapzenMap.FeatureStyling[i];

            EditorGUILayout.BeginHorizontal();
            {
                prefs.showStyle[featureStyling.Name] =
                    EditorGUILayout.Foldout(prefs.showStyle[featureStyling.Name], featureStyling.Name);

                EditorStyle.SetColor(EditorStyle.RemoveButtonColor);
                if (GUILayout.Button(EditorStyle.RemoveButtonContent, EditorStyle.SmallButtonWidth))
                {
                    mapzenMap.FeatureStyling.RemoveAt(i);
                }
                EditorStyle.ResetColor();
            }
            EditorGUILayout.EndHorizontal();

            if (!prefs.showStyle[featureStyling.Name])
            {
                continue;
            }

            EditorGUI.indentLevel++;

            // featureStyling.PolygonBuilderOptions = polygonBuilderEditor.OnInspectorGUI(featureStyling.PolygonBuilderOptions, featureStyling.Name);
            // featureStyling.PolylineBuilderOptions = polylineBuilderEditor.OnInspectorGUI(featureStyling.PolylineBuilderOptions, featureStyling.Name);
            // featureFilterEditor.OnInspectorGUI(featureStyling.Filter, featureStyling.Name);
            // featureStyling.Material = EditorGUILayout.ObjectField("Material:", featureStyling.Material, typeof(Material)) as Material;

            // TODO: add interface for filter matcher

            EditorGUI.indentLevel--;

            // Separator
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        EditorGUI.indentLevel--;

        SavePreferences(prefs);
    }
}
