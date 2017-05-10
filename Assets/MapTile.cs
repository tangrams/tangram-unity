using System;
using System.Collections.Generic;
using UnityEngine;
using Mapzen.VectorData;
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
        var mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        var indices = new List<int>();
        var vertices = new List<Vector3>();
        var colors = new List<Color>();

        foreach (var layer in Layers)
        {
            Color color = layerColors[layer.name];

            foreach (var feature in layer.features)
            {
                // TODO: use extrusion scale and minHeight as options
                float height = 0.0f;
                float minHeight = 0.0f;

                // Extract height
                SimpleJSON.JSONNode heightNode;
                if (feature.TryGetProperty("height", out heightNode))
                {
                    height = heightNode * (1.0f / (float)tileScale);
                }

                var tess = new Tess();

                var geometry = feature.geometry;

                if (geometry.type == GeometryType.Polygon)
                {
                    int pointIndex = 0;
                    foreach (var ringSize in geometry.rings)
                    {
                        var contour = new ContourVertex[ringSize];

                        for (int i = pointIndex, contourIndex = 0; i < pointIndex + ringSize; ++i, ++contourIndex)
                        {
                            var point = geometry.points[i];
                            contour[contourIndex].Position = new Vec3 { X = point.x, Y = height, Z = point.y };
                        }

                        pointIndex += ringSize;
                        tess.AddContour(contour, ContourOrientation.Original);
                    }
                }
                tess.Tessellate(WindingRule.NonZero, ElementType.Polygons, 3);

                // Build walls
                if (height > 0.0f)
                {
                    int pointIndex = 0;
                    int indexOffset = vertices.Count;

                    foreach (var ringSize in geometry.rings)
                    {
                        for (int i = pointIndex; i < pointIndex + ringSize - 1; i++)
                        {
                            var p0 = geometry.points[i];
                            var p1 = geometry.points[i+1];

                            vertices.Add(new Vector3(p0.x, height, p0.y));
                            vertices.Add(new Vector3(p1.x, height, p1.y));
                            vertices.Add(new Vector3(p0.x, minHeight, p0.y));
                            vertices.Add(new Vector3(p1.x, minHeight, p1.y));

                            for (int colorIndex = 0; colorIndex < 4; ++colorIndex) {
                                colors.Add(color);
                            }

                            indices.Add(indexOffset + 2);
                            indices.Add(indexOffset + 3);
                            indices.Add(indexOffset + 1);
                            indices.Add(indexOffset + 2);
                            indices.Add(indexOffset + 1);
                            indices.Add(indexOffset + 0);

                            indexOffset += 4;
                        }

                        pointIndex += ringSize;
                    }
                }

				int offset = vertices.Count;

                for (int i = 0; i < tess.ElementCount * 3; ++i)
                {
                    indices.Add(offset + tess.Elements[i]);
                }

                for (int i = 0; i < tess.VertexCount; ++i)
                {
                    var position = tess.Vertices[i].Position;
                    vertices.Add(new Vector3(position.X, position.Y, position.Z));
                    colors.Add(color);
                }
            }
		}

		indices.Reverse();

		mesh.vertices = vertices.ToArray();
		mesh.triangles = indices.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
	}

	public void Update()
	{
		transform.Rotate(Vector3.up, Time.deltaTime * 10.0f);
	}
}
