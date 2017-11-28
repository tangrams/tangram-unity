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

        private List<GameObject> tiles = new List<GameObject>();

        private IO tileIO = new IO();

        private List<TileTask> tasks = new List<TileTask>();

        private int nTasksForArea = 0;

        private SceneGroup regionMap;

        public List<GameObject> Tiles
        {
            get { return tiles; }
        }

        public void DownloadTiles()
        {
            TileBounds bounds = new TileBounds(Area);

            tasks.Clear();
            nTasksForArea = 0;

            regionMap = new SceneGroup(0, RegionName);

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

                        TileTask task = new TileTask(tileAddress, GroupOptions, response.data, offsetX, offsetY, UnitsPerMeter);

                        task.Start(Styles, regionMap);

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

                SceneGraph.Generate(regionMap, null, GameObjectOptions);
            }
        }
    }
}
