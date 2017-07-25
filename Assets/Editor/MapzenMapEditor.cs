using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapzenMap))]
public class MapzenMapEditor : Editor
{
    private MapzenMap mapzenMap;

    private FeatureStyleEditor featureStyleEditor;
    private TileDataEditor tileDataEditor;

    void OnEnable()
    {
        mapzenMap = (MapzenMap)target;
        featureStyleEditor = new FeatureStyleEditor();
        tileDataEditor = new TileDataEditor();
    }

    public override void OnInspectorGUI()
    {
        featureStyleEditor.OnInspectorGUI(mapzenMap);
        tileDataEditor.OnInspectorGUI(mapzenMap);
        base.OnInspectorGUI();
    }
}
