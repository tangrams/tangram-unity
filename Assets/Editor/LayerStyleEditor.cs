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

    [SerializeField]
    private FeatureStyle.LayerStyle layerStyle;

    public FeatureStyle.LayerStyle LayerStyle
    {
        get { return layerStyle; }
    }

    public LayerStyleEditor(FeatureStyle.LayerStyle layerStyle)
        : base()
    {
        this.polygonBuilderEditor = new PolygonBuilderEditor();
        this.polylineBuilderEditor = new PolylineBuilderEditor();
        this.layerStyle = layerStyle;
    }

    public void OnInspectorGUI()
    {
        EditorGUI.indentLevel++;

        layerStyle.PolygonBuilderOptions = polygonBuilderEditor.OnInspectorGUI(layerStyle.PolygonBuilderOptions);
        layerStyle.PolylineBuilderOptions = polylineBuilderEditor.OnInspectorGUI(layerStyle.PolylineBuilderOptions);

        EditorGUI.indentLevel--;

        layerStyle.Material = EditorGUILayout.ObjectField("Material:", layerStyle.Material, typeof(Material)) as Material;
    }
}
