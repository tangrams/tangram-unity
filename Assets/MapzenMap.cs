using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using Mapzen;

public class MapzenMap : MonoBehaviour
{
    public string ApiKey = "vector-tiles-tyHL4AY";

    public TileArea Area = new TileArea(new LngLat(-74.014892578125, 40.70562793820589), new LngLat(-74.00390625, 40.713955826286046), 16);

    #if UNITY_WEBGL
    private const int nWorkers = 0;
    #else
    private const int nWorkers = 2;
    #endif

    private List<TileTask> pendingTasks = new List<TileTask>();
    private AsyncWorker worker = new AsyncWorker(nWorkers);

    private UnityIO tileIO = new UnityIO();

    void Start()
    {
        TileBounds bounds = new TileBounds(Area);

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

                // Adding a tile object to the scene
                GameObject tilePrefab = Resources.Load("Tile") as GameObject;

                // Instantiate a prefab running the script TileData.Start()
                var go = Instantiate(tilePrefab);

                go.name = tileAddress.ToString();
                go.transform.parent = this.transform;

                MapTile tile = go.GetComponent<MapTile>();

                TileTask task = new TileTask(tileAddress, response.data, tile);

                task.offsetX = (tileAddress.x - bounds.min.x);
                task.offsetY = (-tileAddress.y + bounds.min.y);

                pendingTasks.Add(task);

                worker.RunAsync(() =>
                    {
                        task.Start();
                    });
            };

            // Starts the HTTP request
            StartCoroutine(tileIO.FetchNetworkData(uri, onTileFetched));
        }
    }

    // Update is called once per frame
    void Update()
    {
        List<TileTask> readyTasks = new List<TileTask>();

        foreach (TileTask task in pendingTasks)
        {
            if (task.IsReady())
            {
                task.GetMapTile().CreateUnityMesh(task.offsetX, task.offsetY);
                readyTasks.Add(task);
            }
        }

        foreach (TileTask readyTask in readyTasks)
        {
            pendingTasks.Remove(readyTask);
        }
    }
}
