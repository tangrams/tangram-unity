using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Mapzen;

public class MapzenMap : MonoBehaviour {

	delegate void HTTPRequestCallback(string error, string response);
	private HTTPRequestCallback callback;

	public int TileX = 19290;
	public int TileY = 24632;
	public int TileZ = 16;

        public int TileRangeX = 5;
        public int TileRangeY = 5;

	public string ApiKey = "vector-tiles-tyHL4AY";

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

                                callback = delegate(string error, string response)
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

                                        var tileAddress = new TileAddress(TileX, TileY, TileZ);
                                        var projection = GeoJSON.LocalCoordinateProjectionForTile(tileAddress);
                                        var geoJson = new GeoJSON(response, projection);

                                        tile.Layers = geoJson.GetLayersByName(new List<string> {
                                                "water",
                                                "roads",
                                                "earth",
                                                "buildings"
                                        });

                                        tile.BuildMesh(tileAddress.GetSizeMercatorMeters());
                                };
                                UnityWebRequest request = UnityWebRequest.Get(url);

                                // Starts the HTTP request
                                StartCoroutine(DoHTTPRequest(request));
                        }
		}
	}

	// Runs an HTTP request
        IEnumerator DoHTTPRequest(UnityWebRequest request)
        {
                yield return request.Send();

		string data = System.Text.Encoding.Default.GetString(request.downloadHandler.data);
		callback(request.error, data);
	}

	// Update is called once per frame
	void Update()
        {

	}
}
