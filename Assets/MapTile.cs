using System;
using System.Collections.Generic;
using UnityEngine;
using Mapzen.VectorData;
using SimpleJSON;
using LibTessDotNet;

[RequireComponent(typeof(MeshFilter))]
public class MapTile : MonoBehaviour
{
	public List<FeatureCollection> Layers;

        private Dictionary<String, Color> layerColors = new Dictionary<String, Color> {
                { "water", Color.blue },
                { "earth", Color.green },
                { "roads", Color.gray },
                { "buildings", Color.black },
        };

	public void BuildMesh(double tileScale)
	{
		float inverseTileScale = 1.0f / (float)tileScale;
		var meshData = new MeshData();

                foreach (var layer in Layers)
                {
                        Color color = layerColors[layer.name];

                        foreach (var feature in layer.features)
                        {
                                // TODO: use extrusion scale and minHeight as options
                                float height = 0.0f;
                                float minHeight = 0.0f;

                                JSONNode heightNode;
                                if (feature.TryGetProperty("height", out heightNode))
                                {
                                    height = heightNode * inverseTileScale;
                                }

                                if (feature.geometry.type == GeometryType.Polygon)
                                {
                                        var polygonMeshData = Builder.TesselatePolygon(feature.geometry, color, height);
                                        meshData.Add(polygonMeshData);

                                        if (height > 0.0f)
                                        {
                                                var extrusionMeshData = Builder.TesselatePolygonExtrusion(feature.geometry, color, minHeight, height);
                                                meshData.Add(extrusionMeshData);
                                        }
                                }
                        }
                }

                meshData.FlipIndices();

                // Initialize Unity mesh
                {
                        var mesh = new Mesh ();
                        GetComponent<MeshFilter> ().mesh = mesh;

                        mesh.vertices = meshData.vertices.ToArray ();
                        mesh.triangles = meshData.indices.ToArray ();
                        mesh.colors = meshData.colors.ToArray ();
                        mesh.RecalculateNormals ();
                }
        }

	public void Update()
	{
		transform.Rotate(Vector3.up, Time.deltaTime * 10.0f);
	}
}
