using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapzenMap))]
public class MapzenMapEditor : Editor
{
    private MapzenMap mapzenMap;

    private FeatureStyleEditor featureStyleEditor = new FeatureStyleEditor();
    private TileDataEditor tileDataEditor = new TileDataEditor();

    void OnEnable()
    {
        mapzenMap = (MapzenMap)target;
    }

    public override void OnInspectorGUI()
    {
        featureStyleEditor.OnInspectorGUI(mapzenMap);

        tileDataEditor.OnInspectorGUI(mapzenMap);

        base.OnInspectorGUI();

        var defaultColor = GUI.color;
        GUI.color = Color.green;
        if (GUILayout.Button("Download"))
        {
            mapzenMap.DownloadTiles();
        }
        GUI.color = defaultColor;
    }
}
