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

        public Material TerrainMaterial;

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

                // Create a GameObject for this tile.
                var terrainTileObject = new GameObject(wrappedTileAddress.ToString());
                terrainTileObject.transform.SetParent(transform);

                // Set the local position of the GameObject.
                var offset = wrappedTileAddress.GetOriginMercatorMeters() - Area.min.ToMercatorMeters();
                terrainTileObject.transform.localPosition = new Vector3((float)offset.x, 0.0f, (float)offset.y) * UnitsPerMeter;

                // Create a TerrainTileTask to generate this tile.
                var terrainTileTask = new TerrainTileTask {
                    Address = wrappedTileAddress,
                    ApiKey = ApiKey,
                    Resolution = 128,
                    UnitsPerMeter = UnitsPerMeter,
                    Material = TerrainMaterial,
                    TargetObject = terrainTileObject,
                };

                // Run the task as a Coroutine.
                StartCoroutine(terrainTileTask.Run());
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
