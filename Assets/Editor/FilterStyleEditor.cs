using System;
using System.Linq;
using System.Collections.Generic;
using Mapzen;
using Mapzen.Unity;
using Mapzen.VectorData.Filters;
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
            "water",
        });

    private static Dictionary<string, CompoundFeatureMatcher.Operator> matcherOperatorsTable
    = new Dictionary<string, CompoundFeatureMatcher.Operator>
    {
        { "all of", CompoundFeatureMatcher.Operator.All },
        { "none of", CompoundFeatureMatcher.Operator.None },
        { "any of", CompoundFeatureMatcher.Operator.Any },
    };

    [SerializeField]
    private string customFeatureCollection = "";

    [SerializeField]
    private int selectedLayer;

    [SerializeField]
    private int selectedMatcherOperator;

    [SerializeField]
    private List<LayerStyleEditor> layerStyleEditors;

    [SerializeField]
    private List<MatcherEditor> matcherEditors;

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
        this.matcherEditors = new List<MatcherEditor>();

        foreach (var layerStyle in filterStyle.LayerStyles)
        {
            layerStyleEditors.Add(new LayerStyleEditor(layerStyle));
        }

        foreach (var compoundMatcher in filterStyle.Matchers)
        {
            matcherEditors.Add(new MatcherEditor(compoundMatcher));
        }
    }

    private void AddLayerStyleLayout(FeatureStyle.FilterStyle filterStyle, string name)
    {
        EditorConfig.SetColor(EditorConfig.AddButtonColor);
        if (GUILayout.Button(EditorConfig.AddButtonContent, EditorConfig.SmallButtonWidth))
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
        EditorConfig.ResetColor();
    }

    private void AddMatcherLayout()
    {
        EditorConfig.SetColor(EditorConfig.AddButtonColor);
        if (GUILayout.Button(EditorConfig.AddButtonContent, EditorConfig.SmallButtonWidth))
        {
            var matcher = new FeatureStyle.CompoundMatcher();
            string key = matcherOperatorsTable.Keys.ElementAt(selectedMatcherOperator);
            matcher.Operator = matcherOperatorsTable[key];

            matcherEditors.Add(new MatcherEditor(matcher));
        }
        EditorConfig.ResetColor();
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

        EditorGUILayout.BeginHorizontal();
        {
            selectedMatcherOperator = EditorGUILayout.Popup("Property matcher:",
                selectedMatcherOperator, matcherOperatorsTable.Keys.ToArray());
            AddMatcherLayout();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel++;

        for (int i = matcherEditors.Count - 1; i >= 0; i--)
        {
            var editor = matcherEditors[i];

            var state = FoldoutEditor.OnInspectorGUI(editor.GUID.ToString(), editor.Matcher.Operator.ToString());

            if (state.show)
            {
                editor.OnInspectorGUI();
            }

            if (state.markedForDeletion)
            {
                // TODO: delete
            }
        }

        EditorGUI.indentLevel--;

        // filterStyle.Filter.Matcher = matcher;
    }
}
