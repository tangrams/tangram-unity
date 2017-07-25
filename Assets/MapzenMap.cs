using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using Mapzen;

public class MapzenMap : MonoBehaviour
{
    public int TileX = 19290;
    public int TileY = 24632;
    public int TileZ = 16;

    public int TileRangeX = 5;
    public int TileRangeY = 5;

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
        // Construct the HTTP request
        for (int x = 0; x < TileRangeX; ++x)
        {
            for (int y = 0; y < TileRangeY; ++y)
            {
                int tileX = TileX + x;
                int tileY = TileY + y;

				var uri = new Uri(string.Format("https://tile.mapzen.com/mapzen/vector/v1/all/{0}/{1}/{2}.mvt?api_key={3}",
					TileZ, tileX, tileY, ApiKey));

				Debug.Log("URL request " + uri.AbsoluteUri);

                TileAddress tileAddress = new TileAddress(tileX, tileY, TileZ);

                UnityIO.IORequestCallback onTileFetched = (response) => { 
                    if (response.hasError()) {
                        Debug.Log("TileIO Error: " + response.error);
                        return;
                    }
                    if (response.data.Length == 0) {
                        Debug.Log("Empty Response");
                        return;
                    }

                    GameObject tilePrefab = Resources.Load("Tile") as GameObject;

                    var go = Instantiate(tilePrefab);
                    go.name = tileAddress.ToString();
                    go.transform.parent = this.transform;

                    MapTile tile = go.GetComponent<MapTile>();

                    TileTask task = new TileTask(tileAddress, response.data, tile);
                    task.offsetX = (tileAddress.x - TileX);
                    task.offsetY = (tileAddress.y - TileY);

                    pendingTasks.Add(task);

                    worker.RunAsync(() =>
                    {
                        task.Start();
                    });
                };

                StartCoroutine(tileIO.FetchNetworkData(uri, onTileFetched));
            }
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
