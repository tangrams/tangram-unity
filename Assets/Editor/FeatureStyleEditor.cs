using UnityEngine;
using UnityEditor;
using Mapzen;
using Mapzen.VectorData.Filters;
using System.Collections.Generic;
using System;

public class FeatureStyleEditor
{
    private PolylineBuilderEditor polylineBuilderEditor;
    private PolygonBuilderEditor polygonBuilderEditor;
    private FeatureFilterEditor featureFilterEditor;
    private string featureStyleName = "";
    private bool show = true;
    private Dictionary<string, bool> showStyle;

    public FeatureStyleEditor()
    {
        polygonBuilderEditor = new PolygonBuilderEditor();
        polylineBuilderEditor = new PolylineBuilderEditor();
        featureFilterEditor = new FeatureFilterEditor();

        showStyle = new Dictionary<string, bool>();
    }

    private void LoadPreferences(MapzenMap mapzenMap)
    {
        show = EditorPrefs.GetBool("FeatureStyleEditor.show");

        foreach (var featureStyling in mapzenMap.FeatureStyling)
        {
            showStyle[featureStyling.Name] =
                EditorPrefs.GetBool("FeatureStyleEditor.showStyle" + featureStyling.Name);
        }
    }

    private void SavePreferences(MapzenMap mapzenMap)
    {
        EditorPrefs.SetBool("FeatureStyleEditor.show", show);

        foreach (var featureStyling in mapzenMap.FeatureStyling)
        {
            EditorPrefs.SetBool("FeatureStyleEditor.showStyle" + featureStyling.Name,
                showStyle[featureStyling.Name]);
        }
    }

    public void OnInspectorGUI(MapzenMap mapzenMap)
    {
        LoadPreferences(mapzenMap);

        show = EditorGUILayout.Foldout(show, "Filtering and styling");
        if (!show)
        {
            SavePreferences(mapzenMap);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        {
            featureStyleName = EditorGUILayout.TextField("Style name: ", featureStyleName);

            EditorStyle.SetColor(EditorStyle.AddButtonColor);
            if (GUILayout.Button(EditorStyle.AddButtonContent, EditorStyle.SmallButtonWidth))
            {
                var defaultMaterial = new Material(Shader.Find("Diffuse"));
                var defaultPolygonBuilderOptions = polygonBuilderEditor.DefaultOptions;
                var defaultPolylineBuilderOptions = polylineBuilderEditor.DefaultOptions;
                var defaultFilter = new FeatureFilter();

                var featureStyle = new FeatureStyle(defaultFilter, defaultMaterial, featureStyleName,
                                       defaultPolygonBuilderOptions, defaultPolylineBuilderOptions);

                mapzenMap.FeatureStyling.Add(featureStyle);

                showStyle[featureStyle.Name] = false;
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
                showStyle[featureStyling.Name] = EditorGUILayout.Foldout(showStyle[featureStyling.Name],
                    featureStyling.Name);
                EditorStyle.SetColor(EditorStyle.RemoveButtonColor);
                if (GUILayout.Button(EditorStyle.RemoveButtonContent, EditorStyle.SmallButtonWidth))
                {
                    mapzenMap.FeatureStyling.RemoveAt(i);
                }
                EditorStyle.ResetColor();
            }
            EditorGUILayout.EndHorizontal();

            if (!showStyle[featureStyling.Name])
            {
                continue;
            }

            EditorGUI.indentLevel++;

            var polygonBuilderOptions = polygonBuilderEditor.OnInspectorGUI(featureStyling.PolygonBuilderOptions);
            var polylineBuilderOptions = polylineBuilderEditor.OnInspectorGUI(featureStyling.PolylineBuilderOptions);
            var filter = featureFilterEditor.OnInspectorGUI(featureStyling.Filter);
            var material = EditorGUILayout.ObjectField("Material:", featureStyling.Material, typeof(Material)) as Material;

            featureStyling.Filter = filter;
            featureStyling.Material = material;
            featureStyling.PolygonBuilderOptions = polygonBuilderOptions;
            featureStyling.PolylineBuilderOptions = polylineBuilderOptions;

            // TODO: add interface for filter matcher


            EditorGUI.indentLevel--;

            // Separator
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        EditorGUI.indentLevel--;

        SavePreferences(mapzenMap);
    }
}
