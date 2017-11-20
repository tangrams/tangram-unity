using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using Mapzen.VectorData;
using Mapzen.Unity;
using Mapzen.VectorData.Filters;
using Mapzen;
using System.Threading;

[ExecuteInEditMode]
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

    private List<GameObject> tiles = new List<GameObject>();

    private IO tileIO = new IO();

    [SerializeField]
        private List<FeatureStyle> featureStyling = new List<FeatureStyle>();

    [HideInInspector]
    [SerializeField]
    private SceneGroupType groupOptions;

    [HideInInspector]
    [SerializeField]
    private RegionScaleUnits.Units regionScaleUnit = RegionScaleUnits.Units.Meters;

    private List<TileTask> readyTasks = new List<TileTask>();

    private int nTasksForArea = 0;

    private int generation = 0;

    private AsyncWorker worker = new AsyncWorker(2);

    public void DownloadTilesAsync()
    {
        TileBounds bounds = new TileBounds(Area);

        // Abort currently running tasks and increase generation
        worker.ClearTasks();
        readyTasks.Clear();
        nTasksForArea = 0;
        generation++;

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

                float scaleRatio = (float)tileAddress.GetSizeMercatorMeters() * regionScaleRatio * unitConverter;
                Matrix4x4 scale = Matrix4x4.Scale(new Vector3(scaleRatio, scaleRatio, scaleRatio));
                Matrix4x4 translate = Matrix4x4.Translate(new Vector3(offsetX * scaleRatio, 0.0f, offsetY * scaleRatio));
                Matrix4x4 transform = translate * scale;

                TileTask task = new TileTask(featureStyling, tileAddress, transform, response.data, generation);

                worker.RunAsync(() =>
                {
                    // Skip any task that has been generated for a different generation
                    if (generation == task.Generation)
                    {
                        task.Start();
                        readyTasks.Add(task);
                    }
                });
            };

            // Starts the HTTP request
            StartCoroutine(tileIO.FetchNetworkData(uri, onTileFetched));
        }
    }

    public bool HasPendingTasks()
    {
        return nTasksForArea > 0;
    }

    public bool FinishedRunningTasks()
    {
        // Number of tasks ready for the current generation
        int nTasksReady = 0;

        foreach (var task in readyTasks)
        {
            if (task.Generation == generation)
            {
                nTasksReady++;
            }
        }

        return nTasksReady == nTasksForArea;
    }

    public void GenerateSceneGraph()
    {
        // Merge all feature meshes
        List<FeatureMesh> features = new List<FeatureMesh>();
        foreach (var task in readyTasks)
        {
            features.AddRange(task.Data);
        }

        var mapRegion = new GameObject(RegionName);
        SceneGraph.Generate(features, mapRegion, groupOptions, gameObjectOptions);

        readyTasks.Clear();
        nTasksForArea = 0;
    }

    public int PendingTasks
    {
        get { return nTasksForArea; }
    }

    public List<GameObject> Tiles
    {
        get { return tiles; }
    }

    public List<FeatureStyle> FeatureStyling
    {
        get { return featureStyling; }
    }

    public float RegionScaleRatio {
        get { return regionScaleRatio; }
        set { regionScaleRatio = value; }
    }

    public SceneGroupType GroupOptions
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
