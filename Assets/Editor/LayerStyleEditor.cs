using System;
using Mapzen;
using UnityEditor;
using UnityEngine;

[SerializeField]
public class LayerStyleEditor : EditorBase
{
    [SerializeField]
    private PolygonBuilderEditor polygonBuilderEditor;

    [SerializeField]
    private PolylineBuilderEditor polylineBuilderEditor;

    public LayerStyleEditor()
        : base()
    {
        polygonBuilderEditor = new PolygonBuilderEditor();
        polylineBuilderEditor = new PolylineBuilderEditor();
    }

    public void OnInspectorGUI(FeatureStyle.LayerStyle layerStyle)
    {
        EditorGUI.indentLevel++;

        layerStyle.PolygonBuilderOptions = polygonBuilderEditor.OnInspectorGUI(layerStyle.PolygonBuilderOptions);
        layerStyle.PolylineBuilderOptions = polylineBuilderEditor.OnInspectorGUI(layerStyle.PolylineBuilderOptions);

        EditorGUI.indentLevel--;

        layerStyle.Material = EditorGUILayout.ObjectField("Material:", layerStyle.Material, typeof(Material)) as Material;
    }
}
