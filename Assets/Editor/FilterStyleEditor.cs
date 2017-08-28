using System;
using System.Linq;
using System.Collections.Generic;
using Mapzen;
using UnityEditor;
using UnityEngine;

public class FilterStyleEditor
{
    private class FitlerStyleEditorPrefs
    {
        public bool show = false;
        public Dictionary<string, bool> showLayer;

        public FitlerStyleEditorPrefs()
        {
            this.showLayer = new Dictionary<string, bool>();
        }
    }

    private static FitlerStyleEditorPrefs LoadPreferences(FeatureStyle.FilterStyle filterStyle)
    {
        FitlerStyleEditorPrefs preferences = new FitlerStyleEditorPrefs();
        preferences.show = EditorPrefs.GetBool(typeof(FilterStyleEditor).Name + ".show." + filterStyle.Name);

        foreach (var layerStyling in filterStyle.LayerStyles)
        {
            string prefKey = typeof(FilterStyleEditor).Name + "." + filterStyle.Name + ".show." + layerStyling.LayerName;
            string dicKey = filterStyle.Name + layerStyling.LayerName;
            preferences.showLayer[dicKey] = EditorPrefs.GetBool(prefKey);
        }
        return preferences;
    }

    private static void SavePreferences(FitlerStyleEditorPrefs prefs, FeatureStyle.FilterStyle filterStyle)
    {
        EditorPrefs.SetBool(typeof(FilterStyleEditor).Name + ".show." + filterStyle.Name, prefs.show);

        foreach (var layerStyling in filterStyle.LayerStyles)
        {
            string prefKey = typeof(FilterStyleEditor).Name + "." + filterStyle.Name + ".show." + layerStyling.LayerName;
            string dicKey = filterStyle.Name + layerStyling.LayerName;
            EditorPrefs.SetBool(prefKey, prefs.showLayer[dicKey]);
        }
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

    private static void AddLayerStyleLayout(FitlerStyleEditorPrefs prefs, FeatureStyle.FilterStyle filterStyle, string name)
    {
        EditorStyle.SetColor(EditorStyle.AddButtonColor);
        if (GUILayout.Button(EditorStyle.AddButtonContent, EditorStyle.SmallButtonWidth))
        {   
            var queryLayer = filterStyle.LayerStyles.Where(layerStyle => name == layerStyle.LayerName);

            if (name.Length == 0)
            {
                Debug.LogError("Layer name can't be empty");
            }
            else if (queryLayer.Count() > 0)
            {
                Debug.LogError("A layer with name " + name + " already exists");
            }
            else
            {
                var layerStyle = new FeatureStyle.LayerStyle(name);

                // Default configuration for the layer
                layerStyle.PolygonBuilderOptions = PolygonBuilderEditor.DefaultOptions();
                layerStyle.PolylineBuilderOptions = PolylineBuilderEditor.DefaultOptions();
                layerStyle.Material = new Material(Shader.Find("Diffuse"));

                filterStyle.AddLayerStyle(layerStyle);
                filterStyle.Filter.CollectionNameSet.Add(name);

                prefs.showLayer[filterStyle.Name + name] = false;
            }
        }
        EditorStyle.ResetColor();
    }

    public static void OnInspectorGUI(FeatureStyle.FilterStyle filterStyle)
    {
        var prefs = FilterStyleEditor.LoadPreferences(filterStyle);

        prefs.show = EditorGUILayout.Foldout(prefs.show, filterStyle.Name);

        if (!prefs.show)
        {
            FilterStyleEditor.SavePreferences(prefs, filterStyle);
            return;
        }

        // Default layers
        EditorGUILayout.BeginHorizontal();
        {
            selectedLayer = EditorGUILayout.Popup("Default layer:", selectedLayer, defaultLayers.ToArray());
            FilterStyleEditor.AddLayerStyleLayout(prefs, filterStyle, defaultLayers[selectedLayer]);
        }
        EditorGUILayout.EndHorizontal();

        // Custom layer entry
        EditorGUILayout.BeginHorizontal();
        {
            customFeatureCollection = EditorGUILayout.TextField("Custom layer:", customFeatureCollection);
            FilterStyleEditor.AddLayerStyleLayout(prefs, filterStyle, customFeatureCollection);
        }
        EditorGUILayout.EndHorizontal();

        for (int i = filterStyle.LayerStyles.Count - 1; i >= 0; i--)
        {
            var layerStyle = filterStyle.LayerStyles[i];

            bool showLayer = false;

            EditorGUILayout.BeginHorizontal();
            {
                string prefShowKey = filterStyle.Name + layerStyle.LayerName;
                showLayer = EditorGUILayout.Foldout(prefs.showLayer[prefShowKey], layerStyle.LayerName);
                prefs.showLayer[prefShowKey] = showLayer;

                EditorStyle.SetColor(EditorStyle.RemoveButtonColor);
                if (GUILayout.Button(EditorStyle.RemoveButtonContent, EditorStyle.SmallButtonWidth))
                {
                    filterStyle.LayerStyles.RemoveAt(i);
                }
                EditorStyle.ResetColor();
            }
            EditorGUILayout.EndHorizontal();

            if (!showLayer)
            {
                continue;
            }

            LayerStyleEditor.OnInspectorGUI(layerStyle, "");

            // TODO: Matchers
        }

        FilterStyleEditor.SavePreferences(prefs, filterStyle);
    }
}