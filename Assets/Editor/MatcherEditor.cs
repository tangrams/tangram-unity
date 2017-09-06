using System;
using System.Linq;
using System.Collections.Generic;
using Mapzen;
using Mapzen.Unity;
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

        foreach (var matcherChild in matcher.Matchers)
        {
            this.matcherEditors.Add(new MatcherEditor(matcherChild));
        }
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
                var newMatcher = new FeatureStyle.Matcher(matcherType);

                editor = new MatcherEditor(newMatcher);
                matcher.Matchers.Add(newMatcher);
            }
            EditorConfig.ResetColor();
        }
        EditorGUILayout.EndHorizontal();

        return editor;
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
                    matcher.HasProperty = EditorGUILayout.TextField("Has property:", matcher.HasProperty);
                    break;

                case FeatureStyle.Matcher.Type.PropertyRange:
                    matcher.HasProperty = EditorGUILayout.TextField("Property:", matcher.HasProperty);
                    EditorGUILayout.BeginHorizontal();
                    matcher.MinRange = EditorGUILayout.FloatField("min:", matcher.MinRange);
                    matcher.MinRangeEnabled = EditorGUILayout.Toggle(matcher.MinRangeEnabled);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    matcher.MaxRange = EditorGUILayout.FloatField("max:", matcher.MaxRange);
                    matcher.MaxRangeEnabled = EditorGUILayout.Toggle(matcher.MaxRangeEnabled);
                    EditorGUILayout.EndHorizontal();
                    break;

                case FeatureStyle.Matcher.Type.PropertyValue:
                    matcher.HasProperty = EditorGUILayout.TextField("Property:", matcher.HasProperty);
                    matcher.PropertyValue = EditorGUILayout.TextField("Property value:", matcher.PropertyValue);
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
                matcher.Matchers.Remove(editor.Matcher);
                matcherEditors.RemoveAt(i);
            }
        }

        EditorGUI.indentLevel--;
    }
}


