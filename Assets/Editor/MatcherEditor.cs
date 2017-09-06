using System;
using System.Linq;
using System.Collections.Generic;
using Mapzen;
using Mapzen.VectorData.Filters;
using UnityEditor;
using UnityEngine;

[Serializable]
public class MatcherEditor : EditorBase
{
    [SerializeField]
    private FeatureStyle.Matcher.Type selectedMatcherType;

    [SerializeField]
    private FeatureStyle.Matcher matcher;

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
    private List<MatcherEditor> matcherEditors;

    public FeatureStyle.Matcher Matcher
    {
        get { return matcher; }
    }

    public MatcherEditor(FeatureStyle.Matcher matcher)
        : base()
    {
        this.matcher = matcher;
        this.matcherEditors = new List<MatcherEditor>();

        // TODO: restore editor state from matcher
    }

    private MatcherEditor AddMatcherLayout()
    {
        MatcherEditor editor = null;

        EditorGUILayout.BeginHorizontal();
        {
            var matcherTypeList = Enum.GetValues(typeof(FeatureStyle.Matcher.Type)).Cast<FeatureStyle.Matcher.Type>();
            var matcherTypeStringList = matcherTypeList.Select(type => type.ToString());

            selectedMatcherType = (FeatureStyle.Matcher.Type)EditorGUILayout.Popup("Matcher:",
                (int)selectedMatcherType, matcherTypeStringList.ToArray());

            EditorConfig.SetColor(EditorConfig.AddButtonColor);
            if (GUILayout.Button(EditorConfig.AddButtonContent, EditorConfig.SmallButtonWidth)
                && selectedMatcherType != FeatureStyle.Matcher.Type.None)
            {
                var matcherType = (FeatureStyle.Matcher.Type)selectedMatcherType;
                var compoundMatcher = new FeatureStyle.Matcher(matcherType);

                editor = new MatcherEditor(compoundMatcher);
            }
            EditorConfig.ResetColor();
        }
        EditorGUILayout.EndHorizontal();

        return editor;
    }

    public IFeatureMatcher GetFeatureMatcher()
    {
        if (matcher.IsCompound())
        {
            var predicates = new IFeatureMatcher[matcherEditors.Count];

            for (int i = 0; i < matcherEditors.Count; ++i)
            {
                predicates[i] = matcherEditors[i].GetFeatureMatcher();
            }

            switch (matcher.MatcherType)
            {
                case FeatureStyle.Matcher.Type.AllOf:
                    return FeatureMatcher.AllOf(predicates);
                case FeatureStyle.Matcher.Type.NoneOf:
                    return FeatureMatcher.NoneOf(predicates);
                case FeatureStyle.Matcher.Type.AnyOf:
                    return FeatureMatcher.AnyOf(predicates);
            }
        }
        else
        {
            switch (matcher.MatcherType)
            {
                case FeatureStyle.Matcher.Type.PropertyRange:
                    double? min = minRangeEnabled ? (double)minRange : (double?)null;
                    double? max = maxRangeEnabled ? (double)maxRange : (double?)null;

                    return FeatureMatcher.HasPropertyInRange(propertyRange, min, max);
                case FeatureStyle.Matcher.Type.Property:
                    return FeatureMatcher.HasProperty(hasProperty);
                case FeatureStyle.Matcher.Type.PropertyValue:
                    return FeatureMatcher.HasPropertyWithValue(hasProperty, propertyValue);
                case FeatureStyle.Matcher.Type.PropertyRegex:
                    // TODO
                    return null;
            }
        }

        return null;
    }

    public void OnInspectorGUI()
    {
        EditorGUI.indentLevel++;

        if (matcher.IsCompound())
        {
            var editor = AddMatcherLayout();
            if (editor != null)
            {
                matcherEditors.Add(editor);
            }
        }
        else
        {
            switch (matcher.MatcherType)
            {
                case FeatureStyle.Matcher.Type.Property:
                    hasProperty = EditorGUILayout.TextField("Has property:", hasProperty);
                    break;

                case FeatureStyle.Matcher.Type.PropertyRange:
                    EditorGUILayout.BeginHorizontal();
                    minRange = EditorGUILayout.FloatField("min:", minRange);
                    minRangeEnabled = EditorGUILayout.Toggle(minRangeEnabled);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    maxRange = EditorGUILayout.FloatField("max:", maxRange);
                    maxRangeEnabled = EditorGUILayout.Toggle(maxRangeEnabled);
                    EditorGUILayout.EndHorizontal();
                    break;

                case FeatureStyle.Matcher.Type.PropertyValue:
                    hasProperty = EditorGUILayout.TextField("Property:", hasProperty);
                    propertyValue = EditorGUILayout.TextField("Property value:", propertyValue);
                    break;

                case FeatureStyle.Matcher.Type.PropertyRegex:
                    // TODO
                    break;
            }
        }

        for (int i = matcherEditors.Count - 1; i >= 0; i--)
        {
            var editor = matcherEditors[i];

            var state = FoldoutEditor.OnInspectorGUI(editor.GUID.ToString(), editor.Matcher.MatcherType.ToString());

            if (state.show)
            {
                editor.OnInspectorGUI();
            }

            if (state.markedForDeletion)
            {
                matcherEditors.RemoveAt(i);
            }
        }

        EditorGUI.indentLevel--;
    }
}


