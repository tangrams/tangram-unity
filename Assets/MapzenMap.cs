using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Mapzen.Unity;
using Mapzen;

public class MapzenMap : MonoBehaviour
{
    public GameObjectOptions gameObjectOptions;

    private float regionScaleRatio = 1.0f;

    public string ApiKey = "vector-tiles-tyHL4AY";

    public TileArea Area = new TileArea(
                               new LngLat(-74.014892578125, 40.70562793820589),
                               new LngLat(-74.00390625, 40.713955826286046),
                               16);

    public string RegionName = "";

    private List<MeshFilter> meshFilters = new List<MeshFilter>();

    private IO tileIO = new IO();

    [SerializeField]
    private List<FeatureStyle> featureStyling = new List<FeatureStyle>();

    [HideInInspector]
    [SerializeField]
    private SceneGroup.Type groupOptions;

    [HideInInspector]
    [SerializeField]
    private RegionScaleUnits.Units regionScaleUnit = RegionScaleUnits.Units.Meters;

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

            IO.IORequestCallback onTileFetched = (response) =>
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


                float unitConverter = 1.0f;
                switch (regionScaleUnit)
                {
                    case RegionScaleUnits.Units.Meters:
                        unitConverter = 1.0f;
                        break;
                    case RegionScaleUnits.Units.KiloMeters:
                        unitConverter = 0.001f;
                        break;
                    case RegionScaleUnits.Units.Miles:
                        unitConverter = 0.00063f;
                        break;
                    case RegionScaleUnits.Units.Feet:
                        unitConverter = 3.28f;
                        break;
                    default:
                        unitConverter = 0.0f;
                        break;
                 }

                TileTask task = new TileTask(tileAddress, groupOptions, response.data, offsetX, offsetY, regionScaleRatio * unitConverter);

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

            meshFilters = SceneGraph.Generate(regionMap, null, gameObjectOptions);
        }
    }

    public List<MeshFilter> MeshFilters
    {
        get { return meshFilters; }
    }

    public List<FeatureStyle> FeatureStyling
    {
        get { return featureStyling; }
    }

    public float RegionScaleRatio {
        get { return regionScaleRatio; }
        set { regionScaleRatio = value; }
    }

    public SceneGroup.Type GroupOptions
    {
        get { return groupOptions; }
        set { groupOptions = value; }
    }

    public RegionScaleUnits.Units RegionScaleUnit
    {
        get { return regionScaleUnit; }
        set { regionScaleUnit = value; }
    }
}
