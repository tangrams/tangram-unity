using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Mapzen;
using UnityEditor;

[CustomEditor(typeof(MapzenMap))]
public class MapzenMapEditor : Editor
{
    private MapzenMap mapzenMap;
    private string styleName = "";
    private bool showStyleFoldout = false;
    private bool showTileDataFoldout = false;

    void OnEnable()
    {
        this.mapzenMap = (MapzenMap)target;

        foreach (var style in this.mapzenMap.FeatureStyling)
        {
            if (!mapzenMap.StyleEditors.ContainsKey(style.Name))
            {
                mapzenMap.StyleEditors.Add(style.Name, new StyleEditor(style));
            }
        }
    }

    public override void OnInspectorGUI()
    {
        LoadPreferences();

        StyleFoldout();

        TileDataFoldout();

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

        SavePreferences();
    }

    private void StyleFoldout()
    {
        showStyleFoldout = EditorGUILayout.Foldout(showStyleFoldout, "Filtering and styling");
        if (!showStyleFoldout)
        {
            return;
        }

        EditorGUILayout.BeginHorizontal();
        {
            styleName = EditorGUILayout.TextField("Style name: ", styleName);

            EditorStyle.SetColor(EditorStyle.AddButtonColor);
            if (GUILayout.Button(EditorStyle.AddButtonContent, EditorStyle.SmallButtonWidth))
            {
                var queryStyleName = mapzenMap.FeatureStyling.Where(style => style.Name == styleName);

                if (styleName.Length == 0)
                {
                    Debug.LogError("The style name can't be empty");
                }
                else if (queryStyleName.Count() > 0)
                {
                    Debug.LogError("Style with name " + styleName + " already exists");
                }
                else
                {
                    var style = new FeatureStyle(styleName);
                    mapzenMap.FeatureStyling.Add(style);
                    mapzenMap.StyleEditors.Add(style.Name, new StyleEditor(style));
                }
            }
            EditorStyle.ResetColor();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel++;
        for (int i = mapzenMap.FeatureStyling.Count - 1; i >= 0; i--)
        {
            var styling = mapzenMap.FeatureStyling[i];
            var editor = mapzenMap.StyleEditors[styling.Name] as StyleEditor;

            var state = FoldoutEditor.OnInspectorGUI(styling.Name, styling.Name);

            if (state.show)
            {
                editor.OnInspectorGUI(styling);
            }

            if (state.markedForDeletion)
            {
                mapzenMap.FeatureStyling.RemoveAt(i);
                mapzenMap.StyleEditors.Remove(styling.Name);
            }
        }
        EditorGUI.indentLevel--;
    }

    private void TileDataFoldout()
    {
        showTileDataFoldout = EditorGUILayout.Foldout(showTileDataFoldout, "Tile data");
        if (!showTileDataFoldout)
        {
            return;
        }

        // Group options
        {
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

        mapzenMap.RegionName = EditorGUILayout.TextField("Region name:", mapzenMap.RegionName);
    }

    private void LoadPreferences()
    {
        string key = typeof(MapzenMapEditor).Name;
        showTileDataFoldout = EditorPrefs.GetBool(key + "showTileDataFoldout");
        showStyleFoldout = EditorPrefs.GetBool(key + "showStyleFoldout");
        styleName = EditorPrefs.GetString(key + "styleName");
    }

    private void SavePreferences()
    {
        string key = typeof(MapzenMapEditor).Name;
        EditorPrefs.SetBool(key + "showTileDataFoldout", showTileDataFoldout);
        EditorPrefs.SetBool(key + "showStyleFoldout", showStyleFoldout);
        EditorPrefs.SetString(key + "styleName", styleName);
    }

    private void SceneGroupToggle(MapzenMap mapzenMap, SceneGroup.Type type)
    {
        bool isSet = SceneGroup.Test(type, mapzenMap.GroupOptions);
        isSet = EditorGUILayout.Toggle(type.ToString(), isSet);

        mapzenMap.GroupOptions = isSet ?
            mapzenMap.GroupOptions | type :
            mapzenMap.GroupOptions & ~type;
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
                Debug.LogWarning("The style " + style.Name + " has no filter");
            }

            foreach (var filterStyle in style.FilterStyles)
            {
                if (filterStyle.Filter.CollectionNameSet.Count == 0)
                {
                    Debug.LogWarning("The style " + style.Name + " has a filter selecting no layer");
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
