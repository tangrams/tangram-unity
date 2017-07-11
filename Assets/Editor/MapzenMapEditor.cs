using UnityEngine;
using UnityEditor;
using Mapzen;

[CustomEditor(typeof(MapzenMap))]
public class MapzenMapEditor : Editor, MapzenMap.IMapzenMapListener
{
    private MapzenMap mapzenMap;
    private const string saveRoot = "Assets/Generated/";

    void OnEnable()
    {
        mapzenMap = (MapzenMap)target;
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("Mapzen map Editor configuration");

        GUILayout.Space(10);

        // Register as listener on download completion
        if (mapzenMap.SaveTilesOnDisk)
        {
            mapzenMap.Listener = this;
        }

        base.OnInspectorGUI();
    }

    public void OnGameObjectReady(GameObject go)
    {
        string localPath = saveRoot + go.name + ".prefab";

        // if (AssetDatabase.LoadAssetAtPath(localPath, typeof(GameObject)))
        //{
        //   if (EditorUtility.DisplayDialog("Are you sure?", "The tile prefab already exists. Do you want to overwrite it?", "Yes", "No"))
        //    {
        //        SaveGameObjectToDisk(go, localPath);
        //    }
        //}
        //else
        {
            SaveGameObjectToDisk(go, localPath);
        }
    }

    private void SaveGameObjectToDisk(GameObject go, string localPath)
    {
        var prefab = PrefabUtility.CreateEmptyPrefab(localPath);
        var serializedPrefab = PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);

        var meshFilter = go.GetComponent<MeshFilter>().mesh;
        var materials = go.GetComponent<MeshRenderer>().materials;

        serializedPrefab.GetComponent<MeshFilter>().mesh = meshFilter;
        serializedPrefab.GetComponent<MeshRenderer>().materials = materials;

        for (int i = 0; i < materials.Length; ++i)
        {
            AssetDatabase.CreateAsset(materials[i], saveRoot + i + ".mat");
        }

        AssetDatabase.CreateAsset(meshFilter, saveRoot + go.name + ".asset");
        AssetDatabase.SaveAssets();
    }
}