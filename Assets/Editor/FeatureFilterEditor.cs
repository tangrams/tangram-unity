using UnityEngine;
using UnityEditor;
using Mapzen;
using Mapzen.VectorData.Filters;
using System.Collections.Generic;
using System;

public class FeatureFilterEditor
{
    private bool show = false;
    private IFeatureFilter filter;

    private static GUILayoutOption buttonWidth = GUILayout.Width(50.0f);
    private static GUIContent addLayerButtonContent =
        new GUIContent("+", "Add layer collection");
    private static GUIContent removeLayerButtonContent =
        new GUIContent("-", "Remove layer collection");

    private string customFeatureCollection = "";
    private int selectedLayer;
    private List<string> layers = new List<string>();
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


    public FeatureFilterEditor()
    {
    }

    public IFeatureFilter OnInspectorGUI()
    {
        // Default layers
        EditorGUILayout.BeginHorizontal();
        {
            selectedLayer = EditorGUILayout.Popup("Default layer:",
                    selectedLayer, defaultLayers.ToArray());

            if (GUILayout.Button(addLayerButtonContent, buttonWidth))
            {
                layers.Add(defaultLayers[selectedLayer]);
            }
        }
        EditorGUILayout.EndHorizontal();

        // Custom layer entry
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Custom layer:");
            customFeatureCollection = GUILayout.TextField(customFeatureCollection);

            if (GUILayout.Button(addLayerButtonContent, buttonWidth)
                && customFeatureCollection.Length > 0)
            {
                layers.Add(customFeatureCollection);
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Show currently create filters
        if (layers.Count > 0)
        {
            GUILayout.Label("Filter layers:");

            for (int i = layers.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                string layer = layers[i];
                GUILayout.TextField(layer);
                if (GUILayout.Button(removeLayerButtonContent, buttonWidth))
                {
                    layers.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        return new FeatureFilter().TakeAllFromCollections(layers.ToArray());
    }
}
