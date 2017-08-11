using UnityEngine;
using UnityEditor;
using Mapzen;
using Mapzen.VectorData.Filters;
using System.Collections.Generic;
using System.Linq;
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

        showStyle.Clear();

        foreach (var featureStyling in mapzenMap.FeatureStyling)
        {
            showStyle[featureStyling.Name] = EditorPrefs.GetBool("FeatureStyleEditor.showStyle" + featureStyling.Name);
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

        show = EditorGUILayout.Foldout(show, "Filtering and styling");
        if (!show)
        {
            SavePreferences(mapzenMap);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        {
            featureStyleName = EditorGUILayout.TextField("Style name: ", featureStyleName);

            EditorStyle.SetColor(EditorStyle.AddButtonColor);
            if (GUILayout.Button(EditorStyle.AddButtonContent, EditorStyle.SmallButtonWidth))
            {
                var queryStyleName = mapzenMap.FeatureStyling.Where(style => style.Name == featureStyleName);

                if (featureStyleName.Length == 0)
                {
                    Debug.LogError("The style name can't be empty");
                }
                else if (queryStyleName.Count() > 0)
                {
                    Debug.LogError("Style with name " + featureStyleName + " already exists");
                }
                else
                {
                    var defaultMaterial = new Material(Shader.Find("Diffuse"));
                    var defaultPolygonBuilderOptions = polygonBuilderEditor.DefaultOptions;
                    var defaultPolylineBuilderOptions = polylineBuilderEditor.DefaultOptions;
                    var defaultFilter = new FeatureFilter();
                    var defaultPhysicMaterial = new PhysicMaterial();
                    // Default to static game object and having collider component set
                    var isStatic = true;
                    var hasCollider = true;

                    var featureStyle = new FeatureStyle(defaultFilter, defaultMaterial, defaultPhysicMaterial, isStatic,
                                           hasCollider, featureStyleName, defaultPolygonBuilderOptions, 
                                           defaultPolylineBuilderOptions);

                    mapzenMap.FeatureStyling.Add(featureStyle);

                    showStyle[featureStyle.Name] = false;
                }
            }
            EditorStyle.ResetColor();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel++;

        for (int i = mapzenMap.FeatureStyling.Count - 1; i >= 0; i--)
        {
            var featureStyling = mapzenMap.FeatureStyling[i];

            EditorGUILayout.BeginHorizontal();
            {
                bool foldout = false;
                if (showStyle.ContainsKey(featureStyling.Name))
                {
                    foldout = showStyle[featureStyling.Name];
                }

                showStyle[featureStyling.Name] = EditorGUILayout.Foldout(foldout, featureStyling.Name);

                EditorStyle.SetColor(EditorStyle.RemoveButtonColor);
                if (GUILayout.Button(EditorStyle.RemoveButtonContent, EditorStyle.SmallButtonWidth))
                {
                    mapzenMap.FeatureStyling.RemoveAt(i);
                }
                EditorStyle.ResetColor();
            }
            EditorGUILayout.EndHorizontal();

            if (!showStyle[featureStyling.Name])
            {
                continue;
            }

            EditorGUI.indentLevel++;

            var polygonBuilderOptions = polygonBuilderEditor.OnInspectorGUI(featureStyling.PolygonBuilderOptions, featureStyling.Name);
            var polylineBuilderOptions = polylineBuilderEditor.OnInspectorGUI(featureStyling.PolylineBuilderOptions, featureStyling.Name);
            var filter = featureFilterEditor.OnInspectorGUI(featureStyling.Filter, featureStyling.Name);
            var material = EditorGUILayout.ObjectField("Material:", featureStyling.Material, typeof(Material)) as Material;
            var physicMaterial = EditorGUILayout.ObjectField("Physic Material:", featureStyling.PhysicMaterial, typeof(PhysicMaterial)) as PhysicMaterial;
            var isStatic = true;
            isStatic = EditorGUILayout.Toggle("Static Game Object", isStatic);
            var hasCollider = true;
            hasCollider = EditorGUILayout.Toggle("Has Collider Component", hasCollider);


            featureStyling.Filter = filter;
            featureStyling.Material = material;
            featureStyling.PhysicMaterial = physicMaterial;
            featureStyling.PolygonBuilderOptions = polygonBuilderOptions;
            featureStyling.PolylineBuilderOptions = polylineBuilderOptions;
            featureStyling.IsStatic = isStatic;
            featureStyling.HasCollider = hasCollider;

            // TODO: add interface for filter matcher


            EditorGUI.indentLevel--;

            // Separator
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        EditorGUI.indentLevel--;

        SavePreferences(mapzenMap);
    }
}
