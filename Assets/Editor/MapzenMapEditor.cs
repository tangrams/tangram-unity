using UnityEngine;
using UnityEditor;
using Mapzen;

[CustomEditor(typeof(MapzenMap))]
public class MapzenMapEditor : Editor, MapzenMap.IMapzenMapListener
{
    private MapzenMap mapzenMap;

    void OnEnable()
    {
        mapzenMap = (MapzenMap)target;

        // Register as listener on download completion
        mapzenMap.Listener = this;
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("Mapzen map Editor configuration");

        GUILayout.Space(10);

        base.OnInspectorGUI();
    }

    public void OnGameObjectReady(GameObject go)
    {
        var prefab = PrefabUtility.CreateEmptyPrefab("Assets/" + go.name + ".prefab");
        var serializedPrefab = PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);

        var meshFilter = go.GetComponent<MeshFilter>().mesh;
        var materials = go.GetComponent<MeshRenderer>().materials;

        serializedPrefab.GetComponent<MeshFilter>().mesh = meshFilter;
        serializedPrefab.GetComponent<MeshRenderer>().materials = materials;

        AssetDatabase.CreateAsset(meshFilter, "Assets/" + go.name + "-mesh.asset");
    }
}