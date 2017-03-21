using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using MapzenHelpers;

public class TileData : MonoBehaviour {

	public string geoJSONData;

	// Use this for initialization
	void Start () {
		if (geoJSONData.Length == 0) {
			Debug.Log ("Empty geoJSON data");
		}

		var root = JSON.Parse (this.geoJSONData);
		if (root == null) {
			Debug.Log ("Error parsing the Geo JSON data");
			Debug.Log (geoJSONData);
			return;
		}

		GeoJSON geoJSON = new GeoJSON (root);

		List<string> layerNames = new List<string>() {"buildings", "earth"};
		List<Layer> layers = geoJSON.ExtractLayers (layerNames);

		// TODO: build mesh with the layer geometry
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
