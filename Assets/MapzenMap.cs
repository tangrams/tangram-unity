using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Mapzen;

public class MapzenMap : MonoBehaviour {

	delegate void HTTPRequestCallback(string error, string response);
	private UnityWebRequest request;
	private HTTPRequestCallback callback;

	public int TileX = 19294;
	public int TileY = 24642;
	public int TileZ = 16;

	public string ApiKey = "vector-tiles-tyHL4AY";

	void Start() {

		// Construct the HTTP request
		{
			var url = string.Format("https://tile.mapzen.com/mapzen/vector/v1/all/{0}/{1}/{2}.json?api_key={3}",
				TileZ, TileX, TileY, ApiKey);

			Debug.Log("URL request " + url);

			callback = delegate(string error, string response) {
				if (error != null) {
					Debug.Log("Error: " + error);
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

				tile.Layers = geoJson.GetLayersByName(new List<string> { "water", "roads", "earth", "buildings" });

				tile.BuildMesh(tileAddress.GetSizeMercatorMeters());
			};
			request = UnityWebRequest.Get(url);
		}

		// Starts the HTTP request
		StartCoroutine(DoHTTPRequest());
	}

	// Runs an HTTP request
	IEnumerator DoHTTPRequest() {
		yield return request.Send();

		string data = System.Text.Encoding.Default.GetString(request.downloadHandler.data);
		callback(request.error, data);
	}

	// Update is called once per frame
	void Update() {

	}
}
