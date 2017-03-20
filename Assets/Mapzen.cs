using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.InteropServices;

public class Mapzen : MonoBehaviour {

	[DllImport("UnityNativePlugin", EntryPoint = "UnityNativePlugin")]
	public static extern int UnityNativePlugin();

	delegate void HTTPRequestCallback(string error, string response);
	private UnityWebRequest request;
	private HTTPRequestCallback callback;

	public int tilex = 19294;
	public int tiley = 24642;
	public int tilez = 16;

	void Start () {
		int returnValue = UnityNativePlugin ();

		// Construct the HTTP request
		{
			string url = "https://tile.mapzen.com/mapzen/vector/v1/all/"
				+ this.tilex.ToString () + "/"
				+ this.tiley.ToString () + "/"
				+ this.tilez.ToString () + "/"
				+ ".json?api_key=vector-tiles-tyHL4AY";

			this.callback = delegate (string error, string response) {
				Debug.Log ("Error: " + error);
				Debug.Log ("Response: " + response);

				// Adding a tile object to the scene
				GameObject tilePrefab = Resources.Load ("Tile") as GameObject;

				TileData data = tilePrefab.GetComponent<TileData>();
				data.geoJSONData = response;

				// Instantiate a prefab running the script TileData.Start()
				Instantiate (tilePrefab);
			};
			this.request = UnityWebRequest.Get (url);
		}

		// Starts the HTTP request
		StartCoroutine (this.DoHTTPRequest ());
	}

	// Runs an HTTP request
	IEnumerator DoHTTPRequest() {
		yield return this.request.Send ();

		string data = System.Text.Encoding.Default.GetString (this.request.downloadHandler.data);
		this.callback (this.request.error, data);
	}

	// Update is called once per frame
	void Update () {

	}
}
