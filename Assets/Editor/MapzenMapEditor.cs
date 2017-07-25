using UnityEngine;
using UnityEditor;
using Mapzen;
using Mapzen.VectorData.Filters;
using System;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(MapzenMap))]
public class MapzenMapEditor : Editor
{
    private MapzenMap mapzenMap;
    private Material featureMaterial;

    private PolylineBuilderEditor polylineBuilderEditor;
    private PolygonBuilderEditor polygonBuilderEditor;
    private FeatureFilterEditor featureFilterEditor;

    private bool showFeatureStyleGUI = true;
    private bool showExportGUI = true;

    void OnEnable()
    {
        mapzenMap = (MapzenMap)target;

        polygonBuilderEditor = new PolygonBuilderEditor();
        polylineBuilderEditor = new PolylineBuilderEditor();
        featureFilterEditor = new FeatureFilterEditor();
    }

    public override void OnInspectorGUI()
    {
        FeatureStyleGUI();

        ExportGUI();

        base.OnInspectorGUI();
    }

    private void ExportGUI()
    {
        showExportGUI = EditorGUILayout.Foldout(showExportGUI, "Data");
        if (!showExportGUI)
        {
            return;
        }

        if (GUILayout.Button("Download"))
        {
            ClearTiles();
            mapzenMap.DownloadTiles();
        }

        GUILayout.Label("Export path:");
        mapzenMap.ExportPath = GUILayout.TextField(mapzenMap.ExportPath);
        if (GUILayout.Button("Export"))
        {
            ExportGameObjects();
        }

        if (GUILayout.Button("Clear"))
        {
            ClearTiles();
        }
    }

    private void FeatureStyleGUI()
    {
        showFeatureStyleGUI = EditorGUILayout.Foldout(showFeatureStyleGUI, "Feature collection filtering");
        if (!showFeatureStyleGUI)
        {
            return;
        }

        // Material associated with the filter
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Filter material:");
            featureMaterial = EditorGUILayout.ObjectField(featureMaterial, typeof(Material)) as Material;
        }
        EditorGUILayout.EndHorizontal();

        var featureFilter = featureFilterEditor.OnInspectorGUI();
        var polygonOptions = polygonBuilderEditor.OnInspectorGUI();
        var polylineOptions = polylineBuilderEditor.OnInspectorGUI();

        if (GUILayout.Button("Create Filter")
            && featureMaterial != null
            && featureFilter != null)
        {
            var featureStyle = new FeatureStyle(featureFilter, featureMaterial,
                    polygonOptions, polylineOptions);
            mapzenMap.FeatureStyling.Add(featureStyle);
        }

        GUILayout.Space(10);

        // Show available filters
        if (mapzenMap.FeatureStyling.Count > 0)
        {
            GUILayout.Label("Filters:");

            foreach (var featureStyling in mapzenMap.FeatureStyling)
            {
                FeatureFilter filter = featureStyling.Filter as FeatureFilter;
                EditorGUILayout.BeginHorizontal();
                foreach (var layer in filter.CollectionNameSet)
                {
                    GUILayout.TextField(layer);
                }
                GUILayout.TextField(featureStyling.Material.name);
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Remove filters"))
            {
                mapzenMap.FeatureStyling.Clear();
            }
        }
    }

    public void ClearTiles()
    {
        for (int i = 0; i < mapzenMap.Tiles.Count; ++i)
        {
            DestroyImmediate(mapzenMap.Tiles[i]);
        }
        mapzenMap.Tiles.Clear();
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
