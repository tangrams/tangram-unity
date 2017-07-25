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
    private Material featureMaterial;

    private bool show = true;

    public FeatureStyleEditor()
    {
        polygonBuilderEditor = new PolygonBuilderEditor();
        polylineBuilderEditor = new PolylineBuilderEditor();
        featureFilterEditor = new FeatureFilterEditor();
    }

    public FeatureStyle OnInspectorGUI(MapzenMap mapzenMap)
    {
        FeatureStyle featureStyle = null;

        show = EditorGUILayout.Foldout(show, "Feature collection filtering");
        if (!show)
        {
            return featureStyle;
        }

        // Material associated with the filter
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Filter material:");
            featureMaterial = EditorGUILayout.ObjectField(featureMaterial, typeof(Material)) as Material;
        }
        EditorGUILayout.EndHorizontal();

        var featureFilter = featureFilterEditor.OnInspectorGUI();
        var polygonOptions = polygonBuilderEditor.OnInspectorGUI();
        var polylineOptions = polylineBuilderEditor.OnInspectorGUI();

        if (GUILayout.Button("Create Filter")
            && featureMaterial != null
            && featureFilter != null)
        {
            featureStyle = new FeatureStyle(featureFilter, featureMaterial,
                polygonOptions, polylineOptions);

            mapzenMap.FeatureStyling.Add(featureStyle);
        }

        GUILayout.Space(10);

        // Show available filters
        if (mapzenMap.FeatureStyling.Count > 0)
        {
            GUILayout.Label("Filters:");

            foreach (var featureStyling in mapzenMap.FeatureStyling)
            {
                FeatureFilter filter = featureStyling.Filter as FeatureFilter;
                EditorGUILayout.BeginHorizontal();
                foreach (var layer in filter.CollectionNameSet)
                {
                    GUILayout.TextField(layer);
                }
                GUILayout.TextField(featureStyling.Material.name);
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Remove filters"))
            {
                mapzenMap.FeatureStyling.Clear();
            }
        }

        return featureStyle;
    }
}
