using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Mapzen;

public class MapzenMap : MonoBehaviour
{
    delegate void HTTPRequestCallback(string error, byte[] response,TileAddress address);

    public string ApiKey = "vector-tiles-tyHL4AY";

    public TileArea Area = new TileArea(new LngLat(-74.014892578125, 40.70562793820589), new LngLat(-74.00390625, 40.713955826286046), 16);

    #if UNITY_WEBGL
    private const int nWorkers = 0;
    #else
    private const int nWorkers = 2;
    #endif

    private List<TileTask> pendingTasks = new List<TileTask>();
    private AsyncWorker worker = new AsyncWorker(nWorkers);

    void Start()
    {
        TileBounds bounds = new TileBounds(Area);

        foreach (var tileAddress in bounds.TileAddressRange)
        {
            var wrappedTileAddress = tileAddress.Wrapped();
            var url = string.Format("https://tile.mapzen.com/mapzen/vector/v1/all/{0}/{1}/{2}.json?api_key={3}",
                          wrappedTileAddress.z, wrappedTileAddress.x, wrappedTileAddress.y, ApiKey);

            HTTPRequestCallback callback = (string error, byte[] response, TileAddress address) =>
            {
                if (error != null)
                {
                    Debug.Log("Error: " + error);
                    return;
                }

                if (response.Length == 0)
                {
                    Debug.Log("Empty response");
                    return;
                }

                // Debug.Log("Response: " + response);

                // Adding a tile object to the scene
                GameObject tilePrefab = Resources.Load("Tile") as GameObject;

                // Instantiate a prefab running the script TileData.Start()
                var go = Instantiate(tilePrefab);

                MapTile tile = go.GetComponent<MapTile>();

                TileTask task = new TileTask(address, response, tile);

                task.offsetX = (address.x - bounds.min.x);
                task.offsetY = (-address.y + bounds.min.y);

                pendingTasks.Add(task);

                worker.RunAsync(() =>
                    {
                        task.Start();
                    });
            };
            UnityWebRequest request = UnityWebRequest.Get(url);

            // Starts the HTTP request
            StartCoroutine(DoHTTPRequest(request, callback, tileAddress));
        }
    }

    // Runs an HTTP request
    IEnumerator DoHTTPRequest(UnityWebRequest request, HTTPRequestCallback callback, TileAddress address)
    {
        yield return request.Send();

        byte[] data = request.downloadHandler.data;

        callback(request.error, data, address);
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
