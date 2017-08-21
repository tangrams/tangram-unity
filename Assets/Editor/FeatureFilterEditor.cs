using UnityEngine;
using UnityEditor;
using Mapzen;
using Mapzen.VectorData.Filters;
using System.Collections.Generic;
using System;

public class FeatureFilterEditor
{
    private bool show = false;
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

    private void LoadPreferences(string name)
    {
        show = EditorPrefs.GetBool("FeatureFilterEditor.show" + name);
    }

    private void SavePreferences(string name)
    {
        EditorPrefs.SetBool("FeatureFilterEditor.show" + name, show);
    }

    public FeatureFilter OnInspectorGUI(FeatureFilter filter, string name)
    {
        LoadPreferences(name);

        show = EditorGUILayout.Foldout(show, "Filter");
        if (!show)
        {
            SavePreferences(name);
            return filter;
        }

        // Default layers
        EditorGUILayout.BeginHorizontal();
        {
            selectedLayer = EditorGUILayout.Popup("Default layer:",
                selectedLayer, defaultLayers.ToArray());

            EditorStyle.SetColor(EditorStyle.AddButtonColor);
            if (GUILayout.Button(EditorStyle.AddButtonContent, EditorStyle.SmallButtonWidth))
            {
                filter.CollectionNameSet.Add(defaultLayers[selectedLayer]);
            }
            EditorStyle.ResetColor();
        }
        EditorGUILayout.EndHorizontal();

        // Custom layer entry
        EditorGUILayout.BeginHorizontal();
        {
            customFeatureCollection = EditorGUILayout.TextField("Custom layer:", customFeatureCollection);

            EditorStyle.SetColor(EditorStyle.AddButtonColor);
            if (GUILayout.Button(EditorStyle.AddButtonContent, EditorStyle.SmallButtonWidth))
            {
                if (customFeatureCollection.Length == 0)
                {
                    Debug.LogError("Custom layer name can't be empty");
                }
                else
                {
                    filter.CollectionNameSet.Add(customFeatureCollection);
                }
            }
            EditorStyle.ResetColor();
        }
        EditorGUILayout.EndHorizontal();

        // Show currently create filters
        if (filter.CollectionNameSet.Count > 0)
        {
            for (int i = filter.CollectionNameSet.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(filter.CollectionNameSet[i]);
                EditorStyle.SetColor(EditorStyle.RemoveButtonColor);
                if (GUILayout.Button(EditorStyle.RemoveButtonContent, EditorStyle.SmallButtonWidth))
                {
                    filter.CollectionNameSet.RemoveAt(i);
                }
                EditorStyle.ResetColor();
                EditorGUILayout.EndHorizontal();
            }
        }

        SavePreferences(name);

        return filter;
    }
}
