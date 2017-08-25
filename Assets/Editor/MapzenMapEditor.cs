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

        bool valid = IsValid();

        EditorStyle.SetColor(valid ?
            EditorStyle.DownloadButtonEnabledColor :
            EditorStyle.DownloadButtonDisabledColor);

        if (GUILayout.Button("Download"))
        {
            if (valid)
            {
                LogWarnings();

                mapzenMap.DownloadTiles();
            }
            else
            {
                LogErrors();
            }
        }
        EditorStyle.ResetColor();
    }

    private bool IsValid()
    {
        return mapzenMap.RegionName.Length > 0 && mapzenMap.FeatureStyling.Count > 0;
    }

    private void LogWarnings()
    {
        foreach (var style in mapzenMap.FeatureStyling)
        {
            if (style.Filter.CollectionNameSet.Count == 0)
            {
                Debug.LogWarning("The style " + style.Name + " has a filter selecting no layer");
            }
        }
    }

    private void LogErrors()
    {
        if (mapzenMap.RegionName.Length == 0)
        {
            Debug.LogError("Make sure to give a region name");
        }

        if (mapzenMap.FeatureStyling.Count == 0)
        {
            Debug.LogError("Make sure to create at least one style");
        }
    }
}
