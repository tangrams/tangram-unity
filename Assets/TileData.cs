using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileData : MonoBehaviour {

	public string geoJSONData;

	// Use this for initialization
	void Start () {
		if (geoJSONData.Length == 0) {
			Debug.Log ("Empty geoJSON data");
		}

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
