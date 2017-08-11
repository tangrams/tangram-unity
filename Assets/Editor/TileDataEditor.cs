using UnityEngine;
using UnityEditor;
using Mapzen;
using System.IO;
using System;

public class TileDataEditor
{
    private bool show = false;

    private static GUILayoutOption buttonWidth = GUILayout.Width(100.0f);

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

        {
            EditorGUILayout.BeginHorizontal();
            mapzenMap.RegionName = EditorGUILayout.TextField("Region name:", mapzenMap.RegionName);
            var defaultColor = GUI.color;
            GUI.color = Color.green;
            if (GUILayout.Button("Download", buttonWidth))
            {
                mapzenMap.DownloadTiles();
            }
            GUI.color = defaultColor;
            EditorGUILayout.EndHorizontal();
        }

        {
            EditorGUILayout.BeginHorizontal();
            mapzenMap.ExportPath = EditorGUILayout.TextField("Export path:", mapzenMap.ExportPath);
            if (GUILayout.Button("Export", buttonWidth))
            {
                ExportGameObjects(mapzenMap);
            }
            EditorGUILayout.EndHorizontal();
        }

        SavePreferences();
    }

    private void ExportGameObjects(MapzenMap mapzenMap)
    {
        if (!CreateDirectoryAtPath(mapzenMap.ExportPath))
        {
            EditorUtility.DisplayDialog("Please provide a valid export path",
                "Unable to create or locate directory at path" + mapzenMap.ExportPath,
                "Ok");

            return;
        }

        for (int i = 0; i < mapzenMap.Tiles.Count; ++i)
        {
            var tile = mapzenMap.Tiles[i];

            float progress = (float)(i + 1) / mapzenMap.Tiles.Count;
            EditorUtility.DisplayProgressBar("Exporting tile assets",
                "Exporting tile asset " + tile.name,
                progress);

            string tileAssetPath = mapzenMap.ExportPath + "/" + tile.name;

            if (CreateDirectoryAtPath(tileAssetPath))
            {
                SaveGameObjectToDisk(tile, tileAssetPath);
            }
            else
            {
                Debug.LogError("Unable to save tile at path " + tileAssetPath);
            }
        }

        EditorUtility.ClearProgressBar();
    }

    private void SaveGameObjectToDisk(GameObject go, string rootPath)
    {
        var prefab = PrefabUtility.CreateEmptyPrefab(rootPath + "/" + go.name + ".prefab");
        var serializedPrefab = PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);

        var meshFilter = go.GetComponent<MeshFilter>().sharedMesh;
        var materials = go.GetComponent<MeshRenderer>().materials;

        serializedPrefab.GetComponent<MeshFilter>().mesh = meshFilter;
        serializedPrefab.GetComponent<MeshRenderer>().materials = materials;

        for (int i = 0; i < materials.Length; ++i)
        {
            // TODO: give better name to materials
            AssetDatabase.CreateAsset(materials[i], rootPath + "/" + materials[i].name + i + ".mat");
        }

        AssetDatabase.CreateAsset(meshFilter, rootPath + "/" + go.name + ".asset");
        AssetDatabase.SaveAssets();
    }

    private static bool CreateDirectoryAtPath(string path)
    {
        if (path == "")
        {
            return false;
        }

        if (Directory.Exists(path))
        {
            return true;
        }

        try
        {
            Directory.CreateDirectory(path);
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        return false;
    }

}
