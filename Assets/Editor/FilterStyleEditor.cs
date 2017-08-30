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
    private List<LayerStyleEditor> layerStyleEditors;

    [SerializeField]
    private FeatureStyle.FilterStyle filterStyle;

    public FeatureStyle.FilterStyle FilterStyle
    {
        get { return filterStyle; }
    }

    public FilterStyleEditor(FeatureStyle.FilterStyle filterStyle)
        : base()
    {
        this.filterStyle = filterStyle;
        this.layerStyleEditors = new List<LayerStyleEditor>();

        foreach (var layerStyle in filterStyle.LayerStyles)
        {
            layerStyleEditors.Add(new LayerStyleEditor(layerStyle));
        }
    }

    private void AddLayerStyleLayout(FeatureStyle.FilterStyle filterStyle, string name)
    {
        EditorStyle.SetColor(EditorStyle.AddButtonColor);
        if (GUILayout.Button(EditorStyle.AddButtonContent, EditorStyle.SmallButtonWidth))
        {
            // Layers within a filter are identifier by their layer name
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

                // Create the associated layer editor
                layerStyleEditors.Add(new LayerStyleEditor(layerStyle));
            }
        }
        EditorStyle.ResetColor();
    }

    public void OnInspectorGUI()
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

        EditorGUI.indentLevel++;

        for (int i = layerStyleEditors.Count - 1; i >= 0; i--)
        {
            var editor = layerStyleEditors[i];
            var layerStyling = editor.LayerStyle;

            var state = FoldoutEditor.OnInspectorGUI(editor.GUID.ToString(), layerStyling.LayerName);

            if (state.show)
            {
                editor.OnInspectorGUI();
            }

            if (state.markedForDeletion)
            {
                // Remove the layer from the filter styles
                filterStyle.RemoveLayerStyle(layerStyling);

                // Remove the associated layer editor
                layerStyleEditors.RemoveAt(i);
            }
        }

        EditorGUI.indentLevel--;

        // TODO: Matchers
    }
}
