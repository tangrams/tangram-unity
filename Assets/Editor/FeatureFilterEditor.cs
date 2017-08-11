using UnityEngine;
using UnityEditor;
using Mapzen;
using Mapzen.VectorData.Filters;
using System.Collections.Generic;
using System;

public class FeatureFilterEditor
{
    private bool show = false;

    private static GUILayoutOption buttonWidth = GUILayout.Width(50.0f);
    private static GUIContent addLayerButtonContent =
        new GUIContent("+", "Add layer collection");
    private static GUIContent removeLayerButtonContent =
        new GUIContent("-", "Remove layer collection");

    private string customFeatureCollection = "";
    private int selectedLayer;
    private List<string> defaultLayers = new List<string>(new string[]
        {
            "boundaries",
            "buildings",
            "earth",
            "landuse",
            "places",
            "pois",
            "roads",
            "transit",
            "water"
        });


    public FeatureFilter OnInspectorGUI(FeatureFilter filter)
    {
        // Default layers
        EditorGUILayout.BeginHorizontal();
        {
            selectedLayer = EditorGUILayout.Popup("Default layer:",
                selectedLayer, defaultLayers.ToArray());

            if (GUILayout.Button(addLayerButtonContent, buttonWidth))
            {
                filter.CollectionNameSet.Add(defaultLayers[selectedLayer]);
            }
        }
        EditorGUILayout.EndHorizontal();

        // Custom layer entry
        EditorGUILayout.BeginHorizontal();
        {
            customFeatureCollection = EditorGUILayout.TextField("Custom layer:", customFeatureCollection);

            if (GUILayout.Button(addLayerButtonContent, buttonWidth)
                && customFeatureCollection.Length > 0)
            {
                filter.CollectionNameSet.Add(customFeatureCollection);
            }
        }
        EditorGUILayout.EndHorizontal();

        // Show currently create filters
        if (filter.CollectionNameSet.Count > 0)
        {
            for (int i = filter.CollectionNameSet.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                string layer = filter.CollectionNameSet[i];
                EditorGUILayout.TextField(layer);

                if (GUILayout.Button(removeLayerButtonContent, buttonWidth))
                {
                    filter.CollectionNameSet.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        return filter;
    }
}
