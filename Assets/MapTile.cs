using System;
using System.Collections.Generic;
using UnityEngine;
using Mapzen.VectorData;
using LibTessDotNet;

[RequireComponent(typeof(MeshFilter))]
public class MapTile : MonoBehaviour
{
	public List<FeatureCollection> Layers;

	public void BuildMesh()
	{
		var mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;

		// Let's just build a square for now.
		Vector3[] positions =
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(1f, 0f, 1f),
			new Vector3(0f, 0f, 1f),
		};

		var tess = new Tess();
		var contour = new ContourVertex[positions.Length];
		var indices = new List<int>();
		var vertices = new List<Vector3>();

		for (int i = 0; i < positions.Length; ++i)
		{
			contour[i].Position = new Vec3 { X = positions[i].x, Y = positions[i].y, Z = positions[i].z };
		}

		tess.AddContour(contour, ContourOrientation.Original);
		tess.Tessellate(WindingRule.NonZero, ElementType.Polygons, 3);

		for (int i = 0; i < tess.ElementCount * 3; ++i)
		{
			indices.Add(tess.Elements[i]);
		}

		// TODO: find how tess can return CCW polygons as output
		indices.Reverse();

		for (int i = 0; i < tess.VertexCount; ++i)
		{
			var position = tess.Vertices[i].Position;
			vertices.Add(new Vector3(position.X, position.Y, position.Z));
		}

		mesh.vertices = vertices.ToArray();
		mesh.triangles = indices.ToArray();
	}
}
