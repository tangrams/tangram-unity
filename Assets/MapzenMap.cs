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
    public GameObjectOptions gameObjectOptions;

    public float regionScaleRatio = 1.0f;

    public string ApiKey = "vector-tiles-tyHL4AY";

    public TileArea Area = new TileArea(
                               new LngLat(-74.014892578125, 40.70562793820589),
                               new LngLat(-74.00390625, 40.713955826286046),
                               16);

    public string RegionName = "";

    private List<GameObject> tiles = new List<GameObject>();

    private UnityIO tileIO = new UnityIO();

    [SerializeField]
    private List<FeatureStyle> featureStyling = new List<FeatureStyle>();

    [HideInInspector]
    [SerializeField]
    private SceneGroup.Type groupOptions;

    private List<TileTask> tasks = new List<TileTask>();

    private int nTasksForArea = 0;

    private SceneGroup regionMap;

    public void DownloadTiles()
    {
        TileBounds bounds = new TileBounds(Area);

        tasks.Clear();
        nTasksForArea = 0;

        regionMap = new SceneGroup(SceneGroup.Type.None, RegionName);

        foreach (var tileAddress in bounds.TileAddressRange)
        {
            nTasksForArea++;
        }

        foreach (var tileAddress in bounds.TileAddressRange)
        {
            var wrappedTileAddress = tileAddress.Wrapped();
            var uri = new Uri(string.Format("https://tile.mapzen.com/mapzen/vector/v1/all/{0}/{1}/{2}.mvt?api_key={3}",
                              wrappedTileAddress.z, wrappedTileAddress.x, wrappedTileAddress.y, ApiKey));

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

                TileTask task = new TileTask(tileAddress, groupOptions, response.data, offsetX, offsetY, regionScaleRatio);

                task.Start(featureStyling, regionMap);

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
            tasks.Clear();

            SceneGraph.Generate(regionMap, null, gameObjectOptions);
        }
    }

    public List<GameObject> Tiles
    {
        get { return tiles; }
    }

    public List<FeatureStyle> FeatureStyling
    {
        get { return featureStyling; }
    }

    public SceneGroup.Type GroupOptions
    {
        get { return groupOptions; }
        set { groupOptions = value; }
    }
}
