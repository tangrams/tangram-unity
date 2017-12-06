using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mapzen.VectorData;
using Mapzen.Unity;
using Mapzen.VectorData.Formats;

namespace Mapzen
{
    public class RegionMap : MonoBehaviour
    {
        // Public fields
        // These are serialized, so renaming them will break asset compatibility.

        public string ApiKey = "";

        public TileArea Area = new TileArea(
            new LngLat(-74.014892578125, 40.70562793820589),
            new LngLat(-74.00390625, 40.713955826286046),
            16);

        public float UnitsPerMeter = 1.0f;

        public string RegionName = "";

        public SceneGroupType GroupOptions;

        public GameObjectOptions GameObjectOptions;

        public List<MapStyle> Styles = new List<MapStyle>();

        // Private fields

        private IO tileIO = new IO();

        private List<TileTask> tasks = new List<TileTask>();

        private int nTasksForArea = 0;

        private int generation = 0;

        private AsyncWorker worker = new AsyncWorker(2);

        private GameObject regionMap;

        private TileCache tileCache = new TileCache(50);

        public void DownloadTilesAsync()
        {
            TileBounds bounds = new TileBounds(Area);

            // Abort currently running tasks and increase generation
            worker.ClearTasks();
            tasks.Clear();
            nTasksForArea = 0;
            generation++;

            foreach (var tileAddress in bounds.TileAddressRange)
            {
                nTasksForArea++;
            }

            foreach (var tileAddress in bounds.TileAddressRange)
            {
                float offsetX = (tileAddress.x - bounds.min.x);
                float offsetY = (-tileAddress.y + bounds.min.y);

                float scaleRatio = (float)tileAddress.GetSizeMercatorMeters() * UnitsPerMeter;
                Matrix4x4 scale = Matrix4x4.Scale(new Vector3(scaleRatio, scaleRatio, scaleRatio));
                Matrix4x4 translate = Matrix4x4.Translate(new Vector3(offsetX * scaleRatio, 0.0f, offsetY * scaleRatio));
                Matrix4x4 transform = translate * scale;

                IEnumerable<FeatureCollection> featureCollections = tileCache.Get(tileAddress);

                if (featureCollections != null)
                {
                    var task = new TileTask(Styles, tileAddress, transform, generation);

                    worker.RunAsync(() =>
                    {
                        if (generation == task.Generation)
                        {
                            task.Start(featureCollections);
                            tasks.Add(task);
                        }
                    });
                }
                else
                {
                    // Use a local generation variable to be used in IORequestCallback coroutine
                    int requestGeneration = generation;

                    var wrappedTileAddress = tileAddress.Wrapped();

                    var uri = new Uri(string.Format("https://tile.mapzen.com/mapzen/vector/v1/all/{0}/{1}/{2}.mvt?api_key={3}",
                        wrappedTileAddress.z,
                        wrappedTileAddress.x,
                        wrappedTileAddress.y,
                        ApiKey));

                    IO.IORequestCallback onTileFetched = (response) =>
                    {
                        if (requestGeneration != generation)
                        {
                            // Another request has been made before the coroutine was triggered
                            return;
                        }

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

                        var task = new TileTask(Styles, tileAddress, transform, generation);

                        worker.RunAsync(() =>
                        {
                            // Skip any tasks that have been generated for a different generation
                            if (generation == task.Generation)
                            {
                                // var tileData = new GeoJsonTile(address, response);
                                var mvtTile = new MvtTile(tileAddress, response.data);

                                // Save the tile feature collections in the cache for later use
                                tileCache.Add(tileAddress, mvtTile.FeatureCollections);

                                task.Start(mvtTile.FeatureCollections);

                                tasks.Add(task);
                            }
                        });
                    };

                    // Starts the HTTP request
                    StartCoroutine(tileIO.FetchNetworkData(uri, onTileFetched));
                }
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

            foreach (var task in tasks)
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
            if (regionMap != null)
            {
                DestroyImmediate(regionMap);
            }

            // Merge all feature meshes
            List<FeatureMesh> features = new List<FeatureMesh>();
            foreach (var task in tasks)
            {
                if (task.Generation == generation)
                {
                    features.AddRange(task.Data);
                }
            }

            tasks.Clear();
            nTasksForArea = 0;

            regionMap = new GameObject(RegionName);
            var sceneGraph = new SceneGraph(regionMap, GroupOptions, GameObjectOptions, features);

            sceneGraph.Generate();
        }

        public bool IsValid()
        {
            bool hasStyle = Styles.Any(style => style != null);
            bool hasApiKey = ApiKey.Length > 0;
            return RegionName.Length > 0 && hasStyle && hasApiKey;
        }

        public void LogWarnings()
        {
            if (ApiKey.Length == 0)
            {
                Debug.LogWarning("Make sure to set an API key in the Map Builder");
            }

            foreach (var style in Styles)
            {
                if (style == null)
                {
                    Debug.LogWarning("'Null' style provided in feature styling collection");
                    continue;
                }

                if (style.Layers.Count == 0)
                {
                    Debug.LogWarning("The style " + style.name + " has no filter");
                }

                foreach (var filterStyle in style.Layers)
                {
                    if (filterStyle.GetFilter().CollectionNameSet.Count == 0)
                    {
                        Debug.LogWarning("The style " + style.name + " has a filter selecting no layer");
                    }
                }
            }
        }

        public void LogErrors()
        {
            if (RegionName.Length == 0)
            {
                Debug.LogError("Make sure to give a region name");
            }

            if (!Styles.Any(style => style != null))
            {
                Debug.LogError("Make sure to create at least one style");
            }
        }
    }
}
