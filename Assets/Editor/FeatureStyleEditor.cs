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

        show = EditorGUILayout.Foldout(show, "Feature collection filtering");
        if (!show)
        {
            SavePreferences(mapzenMap);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Filter name:");
            featureStyleName = GUILayout.TextField(featureStyleName);
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Create Filter"))
        {
            var defaultMaterial = new Material(Shader.Find("Diffuse"));
            var defaultPolygonBuilderOptions = polygonBuilderEditor.Options;
            var defaultPolylineBuilderOptions = polylineBuilderEditor.Options;
            var defaultFilter = new FeatureFilter();

            var featureStyle = new FeatureStyle(defaultFilter, defaultMaterial, featureStyleName,
                                   defaultPolygonBuilderOptions, defaultPolylineBuilderOptions);

            mapzenMap.FeatureStyling.Add(featureStyle);

            showStyle[featureStyle.Name] = false;
        }

        for (int i = mapzenMap.FeatureStyling.Count - 1; i >= 0; i--)
        {
            var featureStyling = mapzenMap.FeatureStyling[i];

            showStyle[featureStyling.Name] = EditorGUILayout.Foldout(showStyle[featureStyling.Name],
                featureStyling.Name);

            if (!showStyle[featureStyling.Name])
            {
                continue;
            }

            var filter = featureFilterEditor.OnInspectorGUI(featureStyling.Filter);
            var material = EditorGUILayout.ObjectField(featureStyling.Material, typeof(Material)) as Material;

            featureStyling.Filter = filter;
            featureStyling.Material = material;
            featureStyling.PolygonBuilderOptions = polygonBuilderEditor.OnInspectorGUI();
            featureStyling.PolylineBuilderOptions = polylineBuilderEditor.OnInspectorGUI();

            // TODO: add interface for filter matcher

            if (GUILayout.Button("Remove Filter"))
            {
                mapzenMap.FeatureStyling.RemoveAt(i);
            }
        }

        SavePreferences(mapzenMap);
    }
}
