using System;
using System.Collections.Generic;
using Mapzen;
using UnityEditor;
using UnityEngine;

public class FilterStyleEditor
{
    private class FitlerStyleEditorPrefs
    {
        public bool show = false;
    }

    private static FitlerStyleEditorPrefs LoadPreferences(string panelName)
    {
        FitlerStyleEditorPrefs preferences = new FitlerStyleEditorPrefs();
        preferences.show = EditorPrefs.GetBool(typeof(FilterStyleEditor).Name + ".show." + panelName);
        return preferences;
    }

    private static void SavePreferences(FitlerStyleEditorPrefs prefs, string panelName)
    {
        EditorPrefs.SetBool(typeof(FilterStyleEditor).Name + ".show." + panelName, prefs.show);
    }

    private static string customFeatureCollection = "";
    private static int selectedLayer;
    private static List<string> defaultLayers = new List<string>(new string[]
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

    public static void OnInspectorGUI(FeatureStyle.FilterStyle filterStyle)
    {
        var prefs = LoadPreferences(filterStyle.Name);

        prefs.show = EditorGUILayout.Foldout(prefs.show, filterStyle.Name);

        if (!prefs.show)
        {
            SavePreferences(prefs, filterStyle.Name);
            return;
        }

        // Default layers
        EditorGUILayout.BeginHorizontal();
        {
            selectedLayer = EditorGUILayout.Popup("Default layer:",
                selectedLayer, defaultLayers.ToArray());

            EditorStyle.SetColor(EditorStyle.AddButtonColor);
            if (GUILayout.Button(EditorStyle.AddButtonContent, EditorStyle.SmallButtonWidth))
            {
                filterStyle.Filter.CollectionNameSet.Add(defaultLayers[selectedLayer]);
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
                    filterStyle.Filter.CollectionNameSet.Add(customFeatureCollection);
                }
            }
            EditorStyle.ResetColor();
        }
        EditorGUILayout.EndHorizontal();

        // Show currently create filters
        if (filterStyle.Filter.CollectionNameSet.Count > 0)
        {
            for (int i = filterStyle.Filter.CollectionNameSet.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(filterStyle.Filter.CollectionNameSet[i]);
                EditorStyle.SetColor(EditorStyle.RemoveButtonColor);
                if (GUILayout.Button(EditorStyle.RemoveButtonContent, EditorStyle.SmallButtonWidth))
                {
                    filterStyle.Filter.CollectionNameSet.RemoveAt(i);
                }
                EditorStyle.ResetColor();
                EditorGUILayout.EndHorizontal();
            }
        }

        SavePreferences(prefs, filterStyle.Name);
    }
}