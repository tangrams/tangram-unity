using UnityEngine;
using UnityEditor;
using Mapzen;
using Mapzen.VectorData.Filters;
using System;
using System.IO;

[CustomEditor(typeof(MapzenMap))]
public class MapzenMapEditor : Editor
{
    private MapzenMap mapzenMap;
    private string featureCollection = "";
    private Material featureMaterial;

    void OnEnable()
    {
        mapzenMap = (MapzenMap)target;
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("Mapzen map Editor configuration");

        GUILayout.Space(10);

        GUILayout.Label("Export path:");

        mapzenMap.ExportPath = GUILayout.TextField(mapzenMap.ExportPath);

        if (GUILayout.Button("Export"))
        {
            ExportGameObjects();
        }
        if (GUILayout.Button("Download"))
        {
            ClearTiles();
            mapzenMap.DownloadTiles();
        }
        if (GUILayout.Button("Clear"))
        {
            ClearTiles();
            mapzenMap.FeatureStyling.Clear();
        }

        featureCollection = GUILayout.TextField(featureCollection);
        EditorGUILayout.BeginHorizontal();
        featureMaterial = EditorGUILayout.ObjectField(featureMaterial, typeof(Material)) as Material;
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("AddFilter")
            && featureMaterial != null
            && featureCollection.Length > 0)
        {
            var featureFilter = new FeatureFilter()
                .TakeAllFromCollections(featureCollection);

            mapzenMap.FeatureStyling.Add(featureFilter, featureMaterial);
        }

        EditorUtility.ClearProgressBar();

        GUILayout.Space(10);

        base.OnInspectorGUI();
    }

    public void ClearTiles()
    {
        for (int i = 0; i < mapzenMap.Tiles.Count; ++i)
        {
            DestroyImmediate(mapzenMap.Tiles[i]);
        }
        mapzenMap.Tiles.Clear();
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

    private void ExportGameObjects()
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
    }

    private void SaveGameObjectToDisk(GameObject go, string rootPath)
    {
        var prefab = PrefabUtility.CreateEmptyPrefab(rootPath + "/" + go.name + ".prefab");
        var serializedPrefab = PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);

        var meshFilter = go.GetComponent<MeshFilter>().mesh;
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
}
