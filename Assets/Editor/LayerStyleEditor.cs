using System;

public class LayerStyleEditor
{
    private PolylineBuilderEditor polylineBuilderEditor;
    private PolygonBuilderEditor polygonBuilderEditor;

    public LayerStyleEditor()
    {
        polygonBuilderEditor = new PolygonBuilderEditor();
        polylineBuilderEditor = new PolylineBuilderEditor();
    }
}