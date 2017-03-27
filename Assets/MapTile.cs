using System;
using System.Collections.Generic;
using UnityEngine;
using Mapzen.VectorData;

[RequireComponent(typeof(MeshFilter))]
public class MapTile : MonoBehaviour
{
	public List<FeatureCollection> Layers;

	public void BuildMesh()
	{
		var mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;

		// Let's just build a square for now.
		Vector3[] vertices =
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(1f, 0f, 1f),
			new Vector3(0f, 0f, 1f),
		};
		int[] indices = { 0, 3, 2, 2, 1, 0 };

		mesh.vertices = vertices;
		mesh.triangles = indices;
	}
}
