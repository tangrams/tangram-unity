using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using Mapzen.VectorData;
using Mapzen.VectorData.Filters;
using Mapzen;

public class MapzenMap : MonoBehaviour
{
    public string ApiKey = "vector-tiles-tyHL4AY";

    public TileArea Area = new TileArea(
                               new LngLat(-74.014892578125, 40.70562793820589),
                               new LngLat(-74.00390625, 40.713955826286046),
                               16);

    private List<GameObject> tiles = new List<GameObject>();

    private UnityIO tileIO = new UnityIO();

    [SerializeField]
    private string exportPath = "Assets/Generated";

    [SerializeField]
    private List<FeatureStyle> featureStyling = new List<FeatureStyle>();

    private List<TileTask> tasks = new List<TileTask>();

    private int nTasksForArea = 0;

    public void DownloadTiles()
    {
        TileBounds bounds = new TileBounds(Area);

        tasks.Clear();
        nTasksForArea = 0;

        foreach (var tileAddress in bounds.TileAddressRange)
        {
            nTasksForArea++;
        }

        foreach (var tileAddress in bounds.TileAddressRange)
        {
            var wrappedTileAddress = tileAddress.Wrapped();
            var uri = new Uri(string.Format("https://tile.mapzen.com/mapzen/vector/v1/all/{0}/{1}/{2}.mvt?api_key={3}",
                              wrappedTileAddress.z, wrappedTileAddress.x, wrappedTileAddress.y, ApiKey));

            Debug.Log("URL request " + uri.AbsoluteUri);

            UnityIO.IORequestCallback onTileFetched = (response) =>
            {
                if (response.hasError())
                {
                    Debug.Log("TileIO Error: " + response.error);
                    return;
                }

                if (response.data.Length == 0)
                {
                    Debug.Log("Empty Response");
                    return;
                }

                float offsetX = (tileAddress.x - bounds.min.x);
                float offsetY = (-tileAddress.y + bounds.min.y);

                TileTask task = new TileTask(tileAddress, response.data, offsetX, offsetY);

                task.Start(featureStyling);

                OnTaskReady(task);
            };

            // Starts the HTTP request
            StartCoroutine(tileIO.FetchNetworkData(uri, onTileFetched));
        }
    }

    void OnTaskReady(TileTask readyTask)
    {
        tasks.Add(readyTask);

        if (tasks.Count == nTasksForArea)
        {
            List<SceneGroup> groups = new List<SceneGroup>();

            foreach (var task in tasks)
            {
                groups.AddRange(task.SceneGroups);
            }

            tasks.Clear();

            CreateSceneGraph(groups);
        }
    }

    private void CreateSceneGraph(List<SceneGroup> sceneGroups)
    {
        foreach (var sceneGroup in sceneGroups)
        {
            Visit(sceneGroup, this.transform);
        }
    }

    private void Visit(SceneGroup group, Transform parent)
    {
        var gameObject = new GameObject();

        gameObject.name = group.name;
        gameObject.transform.parent = parent;

        foreach (var child in group.childs)
        {
            Visit(child, gameObject.transform);
        }

        if (group.meshData != null)
        {
            FeatureBehavior featureBehavior = gameObject.AddComponent<FeatureBehavior>();
            featureBehavior.CreateUnityMesh(group.meshData, 0, 0);
        }
    }

    public List<GameObject> Tiles
    {
        get
        {
            return tiles;
        }
    }


    public List<FeatureStyle> FeatureStyling
    {
        get { return featureStyling; }
    }

    public string ExportPath
    {
        get
        {
            return exportPath;
        }
        set
        {
            exportPath = value;
        }
    }
}
