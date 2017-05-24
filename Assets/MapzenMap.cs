using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Mapzen;

public class MapzenMap : MonoBehaviour
{
    delegate void HTTPRequestCallback(string error,string response,TileAddress address);

    public int TileX = 19290;
    public int TileY = 24632;
    public int TileZ = 16;

    public int TileRangeX = 5;
    public int TileRangeY = 5;

    public string ApiKey = "vector-tiles-tyHL4AY";

    private List<TileTask> pendingTasks = new List<TileTask>();
    private AsyncWorker worker = new AsyncWorker(4);

    void Start()
    {
        // Construct the HTTP request
        for (int x = 0; x < TileRangeX; ++x)
        {
            for (int y = 0; y < TileRangeY; ++y)
            {
                int tileX = TileX + x;
                int tileY = TileY + y;

                var url = string.Format("https://tile.mapzen.com/mapzen/vector/v1/all/{0}/{1}/{2}.json?api_key={3}",
                              TileZ, tileX, tileY, ApiKey);

                Debug.Log("URL request " + url);

                HTTPRequestCallback callback = (string error, string response, TileAddress address) =>
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

                    Debug.Log("Response: " + response);

                    // Adding a tile object to the scene
                    GameObject tilePrefab = Resources.Load("Tile") as GameObject;

                    // Instantiate a prefab running the script TileData.Start()
                    var go = Instantiate(tilePrefab);

                    MapTile tile = go.GetComponent<MapTile>();

                    var layers = new List<string>
                    {
                        "water",
                        "roads",
                        "earth",
                        "buildings"
                    };

                    TileTask task = new TileTask(address, layers, response, tile);

                    task.offsetX = (address.x - TileX);
                    task.offsetY = (address.y - TileY);

                    pendingTasks.Add(task);

                    worker.RunAsync(() =>
                        {
                            task.Start();
                        });
                };
                UnityWebRequest request = UnityWebRequest.Get(url);

                // Starts the HTTP request
                StartCoroutine(DoHTTPRequest(request, callback, new TileAddress(tileX, tileY, TileZ)));
            }
        }
    }

    // Runs an HTTP request
    IEnumerator DoHTTPRequest(UnityWebRequest request, HTTPRequestCallback callback, TileAddress address)
    {
        yield return request.Send();

        string data = System.Text.Encoding.Default.GetString(request.downloadHandler.data);

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
