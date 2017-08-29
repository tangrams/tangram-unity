using UnityEngine;
using UnityEditor;
using Mapzen;
using System.IO;
using System;

public class TileDataEditor
{
    private bool show = false;

    private void LoadPreferences()
    {
        show = EditorPrefs.GetBool("TileDataEditor.show");
    }

    private void SavePreferences()
    {
        EditorPrefs.SetBool("TileDataEditor.show", show);
    }

    private void SceneGroupToggle(MapzenMap mapzenMap, SceneGroup.Type type)
    {
        bool isSet = SceneGroup.Test(type, mapzenMap.GroupOptions);
        isSet = EditorGUILayout.Toggle(type.ToString(), isSet);

        mapzenMap.GroupOptions = isSet ?
            mapzenMap.GroupOptions | type :
            mapzenMap.GroupOptions & ~type;
    }

    public void OnInspectorGUI(MapzenMap mapzenMap)
    {
        LoadPreferences();

        show = EditorGUILayout.Foldout(show, "Tile data");
        if (!show)
        {
            SavePreferences();
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

        mapzenMap.RegionName = EditorGUILayout.TextField("Region name:",
            mapzenMap.RegionName);

        SavePreferences();
    }
}
