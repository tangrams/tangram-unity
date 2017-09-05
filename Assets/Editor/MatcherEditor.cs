using System;
using System.Collections.Generic;
using Mapzen;
using Mapzen.VectorData.Filters;
using UnityEditor;
using UnityEngine;

[Serializable]
public class MatcherEditor : EditorBase
{
    [SerializeField]
    private FeatureStyle.CompoundMatcher matcher;

    [SerializeField]
    private string hasProperty = "";

    [SerializeField]
    private string propertyValue = "";

    [SerializeField]
    private string propertyRange = "";

    [SerializeField]
    private float minRange;

    [SerializeField]
    private float maxRange;

    [SerializeField]
    private bool minRangeEnabled = true;

    [SerializeField]
    private bool maxRangeEnabled = true;

    [SerializeField]
    private string name;

    public FeatureStyle.CompoundMatcher Matcher
    {
        get { return matcher; }
    }

    public MatcherEditor(FeatureStyle.CompoundMatcher matcher)
        : base()
    {
        this.matcher = matcher;
    }

    public void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        {
            hasProperty = EditorGUILayout.TextField("Has property:", hasProperty);
            EditorConfig.SetColor(EditorConfig.AddButtonColor);
            if (GUILayout.Button(EditorConfig.AddButtonContent, EditorConfig.SmallButtonWidth))
            {
                matcher.Matchers.Add(FeatureMatcher.HasProperty(hasProperty));
            }
            EditorConfig.ResetColor();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            propertyValue = EditorGUILayout.TextField("Property value:", propertyValue);
            EditorConfig.SetColor(EditorConfig.AddButtonColor);
            if (GUILayout.Button(EditorConfig.AddButtonContent, EditorConfig.SmallButtonWidth))
            {
                
            }
            EditorConfig.ResetColor();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Property with range:");

        // TODO: add sliders
        EditorGUILayout.BeginHorizontal();
        minRange = EditorGUILayout.FloatField("min:", minRange);
        minRangeEnabled = EditorGUILayout.Toggle(minRangeEnabled);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        maxRange = EditorGUILayout.FloatField("max:", maxRange);
        maxRangeEnabled = EditorGUILayout.Toggle(maxRangeEnabled);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            propertyRange = EditorGUILayout.TextField("Property:", propertyRange);

            EditorConfig.SetColor(EditorConfig.AddButtonColor);
            if (GUILayout.Button(EditorConfig.AddButtonContent, EditorConfig.SmallButtonWidth))
            {
                double? min = minRangeEnabled ? (double)minRange : (double?)null;
                double? max = maxRangeEnabled ? (double)maxRange : (double?)null;

                matcher.Matchers.Add(FeatureMatcher.HasPropertyInRange(propertyRange, min, max));
            }
            EditorConfig.ResetColor();
        }
        EditorGUILayout.EndHorizontal();
    }
}


