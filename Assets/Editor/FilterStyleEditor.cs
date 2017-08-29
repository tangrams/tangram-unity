using System;
using System.Linq;
using System.Collections.Generic;
using Mapzen;
using UnityEditor;
using UnityEngine;

[Serializable]
public class FilterStyleEditor : EditorBase
{
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

    [SerializeField]
    private string customFeatureCollection = "";

    [SerializeField]
    private int selectedLayer;

    [SerializeField]
    private Dictionary<string, LayerStyleEditor> layerStyleEditors;

    public FilterStyleEditor(FeatureStyle.FilterStyle filterStyle)
        : base()
    {
        this.layerStyleEditors = new Dictionary<string, LayerStyleEditor>();
        foreach (var layerStyle in filterStyle.LayerStyles)
        {
            layerStyleEditors.Add(layerStyle.LayerName, new LayerStyleEditor());
        }
    }

    private void AddLayerStyleLayout(FeatureStyle.FilterStyle filterStyle, string name)
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
                layerStyleEditors.Add(layerStyle.LayerName, new LayerStyleEditor());
            }
        }
        EditorStyle.ResetColor();
    }

    public void OnInspectorGUI(FeatureStyle.FilterStyle filterStyle)
    {
        // Default layers
        EditorGUILayout.BeginHorizontal();
        {
            selectedLayer = EditorGUILayout.Popup("Default layer:", selectedLayer, defaultLayers.ToArray());
            AddLayerStyleLayout(filterStyle, defaultLayers[selectedLayer]);
        }
        EditorGUILayout.EndHorizontal();

        // Custom layer entry
        EditorGUILayout.BeginHorizontal();
        {
            customFeatureCollection = EditorGUILayout.TextField("Custom layer:", customFeatureCollection);
            AddLayerStyleLayout(filterStyle, customFeatureCollection);
        }
        EditorGUILayout.EndHorizontal();

        for (int i = filterStyle.LayerStyles.Count - 1; i >= 0; i--)
        {
            var layerStyling = filterStyle.LayerStyles[i];
            var editor = layerStyleEditors[layerStyling.LayerName];

            var state = FoldoutEditor.OnInspectorGUI(editor.GUID.ToString(), layerStyling.LayerName);

            if (state.show)
            {
                editor.OnInspectorGUI(layerStyling);
            }

            if (state.markedForDeletion)
            {
                filterStyle.Filter.CollectionNameSet.Remove(layerStyling.LayerName);
                filterStyle.LayerStyles.RemoveAt(i);
                layerStyleEditors.Remove(layerStyling.LayerName);
            }
        }

        // TODO: Matchers
    }
}