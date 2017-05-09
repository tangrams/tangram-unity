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

	public void BuildMesh()
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
                var tess = new Tess();

                var geometry = feature.geometry;

                if (geometry.type == GeometryType.Polygon)
                {
                    int pointIndex = 0;
                    foreach (var ringSize in geometry.rings)
                    {
                        var contour = new ContourVertex[ringSize];
                        int contourIndex = 0;
                        for (int i = pointIndex; i < pointIndex + ringSize; ++i)
                        {
                            var point = geometry.points[i];
                            contour[contourIndex++].Position = new Vec3 { X = point.x, Y = point.y, Z = 0 };
                        }
                        pointIndex += ringSize;
                        tess.AddContour(contour, ContourOrientation.Original);
                    }
                }
                tess.Tessellate(WindingRule.NonZero, ElementType.Polygons, 3);

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

		// TODO: find how tess can return CCW polygons as output
		indices.Reverse();

		mesh.vertices = vertices.ToArray();
		mesh.triangles = indices.ToArray();
        mesh.colors = colors.ToArray();
	}
}
