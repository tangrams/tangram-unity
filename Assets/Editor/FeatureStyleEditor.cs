using UnityEditor;
using UnityEngine;
using System;
using Mapzen;

[CustomEditor(typeof(FeatureStyle))]
public class FeatureStyleEditor : Editor
{
    private FeatureStyle featureStyle;
    private StyleEditor styleEditor;

    void OnEnable()
    {
        this.featureStyle = (FeatureStyle)target;
        this.styleEditor = new StyleEditor(featureStyle);
    }

    public override void OnInspectorGUI()
    {
        styleEditor.OnInspectorGUI();
    }
}