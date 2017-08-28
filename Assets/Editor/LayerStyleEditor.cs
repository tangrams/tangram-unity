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
        layerStyle.Material = EditorGUILayout.ObjectField("Material:", layerStyle.Material, typeof(Material)) as Material;
        layerStyle.PolygonBuilderOptions = PolygonBuilderEditor.OnInspectorGUI(layerStyle.PolygonBuilderOptions, "");
        layerStyle.PolylineBuilderOptions = PolylineBuilderEditor.OnInspectorGUI(layerStyle.PolylineBuilderOptions, "");
    }
}
