using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Mapzen;
using UnityEditor;

[CustomEditor(typeof(MapzenMap))]
public class MapzenMapEditor : Editor
{
    private MapzenMap mapzenMap;
    private bool showTileDataFoldout = false;

    void OnEnable()
    {
        this.mapzenMap = (MapzenMap)target;
    }

    public override void OnInspectorGUI()
    {
        LoadPreferences();

        TileDataFoldout();

        base.OnInspectorGUI();

        bool valid = IsValid();

        EditorConfig.SetColor(valid ?
            EditorConfig.DownloadButtonEnabledColor :
            EditorConfig.DownloadButtonDisabledColor);

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

        EditorConfig.ResetColor();

        SavePreferences();
    }

    private void SceneGroupToggle(MapzenMap mapzenMap, SceneGroup.Type type)
    {
        bool isSet = SceneGroup.Test(type, mapzenMap.GroupOptions);
        isSet = EditorGUILayout.Toggle(type.ToString(), isSet);

        mapzenMap.GroupOptions = isSet ?
            mapzenMap.GroupOptions | type :
            mapzenMap.GroupOptions & ~type;
    }

    private void TileDataFoldout()
    {
        showTileDataFoldout = EditorGUILayout.Foldout(showTileDataFoldout, "Tile data");
        if (!showTileDataFoldout)
        {
            return;
        }

        GUILayout.Label("Group by:");

        EditorGUI.indentLevel++;

        EditorGUILayout.BeginHorizontal();
        SceneGroupToggle(mapzenMap, SceneGroup.Type.Feature);
        SceneGroupToggle(mapzenMap, SceneGroup.Type.Filter);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        SceneGroupToggle(mapzenMap, SceneGroup.Type.Layer);
        SceneGroupToggle(mapzenMap, SceneGroup.Type.Tile);
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel--;
    }

    private void LoadPreferences()
    {
        string key = typeof(MapzenMapEditor).Name;
        showTileDataFoldout = EditorPrefs.GetBool(key + ".showTileDataFoldout");
    }

    private void SavePreferences()
    {
        string key = typeof(MapzenMapEditor).Name;
        EditorPrefs.SetBool(key + ".showTileDataFoldout", showTileDataFoldout);
    }

    private bool IsValid()
    {
        return mapzenMap.RegionName.Length > 0 && mapzenMap.FeatureStyling.Count > 0;
    }

    private void LogWarnings()
    {
        foreach (var style in mapzenMap.FeatureStyling)
        {
            if (style.FilterStyles.Count == 0)
            {
                Debug.LogWarning("The style " + style.name + " has no filter");
            }

            foreach (var filterStyle in style.FilterStyles)
            {
                if (filterStyle.Filter.CollectionNameSet.Count == 0)
                {
                    Debug.LogWarning("The style " + style.name + " has a filter selecting no layer");
                }
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
