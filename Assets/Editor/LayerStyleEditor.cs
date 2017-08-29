using System;
using Mapzen;
using UnityEditor;
using UnityEngine;

public class LayerStyleEditor
{
    public LayerStyleEditor()
    {
    }

    public static void OnInspectorGUI(FeatureStyle.LayerStyle layerStyle, string panelName)
    {
        panelName += '.' + layerStyle.LayerName;

        EditorGUI.indentLevel++;

        layerStyle.PolygonBuilderOptions = PolygonBuilderEditor.OnInspectorGUI(layerStyle.PolygonBuilderOptions, panelName);
        layerStyle.PolylineBuilderOptions = PolylineBuilderEditor.OnInspectorGUI(layerStyle.PolylineBuilderOptions, panelName);

        EditorGUI.indentLevel--;

        layerStyle.Material = EditorGUILayout.ObjectField("Material:", layerStyle.Material, typeof(Material)) as Material;
    }
}
