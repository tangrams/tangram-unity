using System;
using Mapzen.VectorData;
using LibTessDotNet;
using UnityEngine;
using System.Collections.Generic;

public class Builder
{
    public static MeshData TesselatePolygon(Geometry geometry, Color color, float height)
    {
        var meshData = new MeshData();
        Earcut earcut = new Earcut();
        List<float> points = new List<float>();
        List<int> rings = new List<int>();

        int pointIndex = 0;
        foreach (var ringSize in geometry.rings)
        {
            for (int i = pointIndex, contourIndex = 0; i < pointIndex + ringSize; ++i, ++contourIndex)
            {
                var point = geometry.points[i];
                points.Add(point.x);
                points.Add(point.y);
            }
            rings.Add(ringSize);
            pointIndex += ringSize;
        }
        earcut.Tesselate(points.ToArray(), rings.ToArray());

        for (int i = 0; i < earcut.indices.Length; ++i)
        {
            meshData.indices.Add((int)earcut.indices[i]);
        }

        for (int i = 0; i < earcut.vertices.Length; i += 2)
        {
            meshData.vertices.Add(new Vector3(earcut.vertices[i], height, earcut.vertices[i + 1]));
            meshData.colors.Add(color);
        }

        earcut.Release();

        return meshData;
    }

    public static MeshData TesselatePolygonExtrusion(Geometry geometry, Color color, float minHeight, float height)
    {
        var meshData = new MeshData();
        int pointIndex = 0;
        int indexOffset = 0;

        foreach (var ringSize in geometry.rings)
        {
            for (int i = pointIndex; i < pointIndex + ringSize - 1; i++)
            {
                var p0 = geometry.points[i];
                var p1 = geometry.points[i + 1];

                meshData.vertices.Add(new Vector3(p0.x, height, p0.y));
                meshData.vertices.Add(new Vector3(p1.x, height, p1.y));
                meshData.vertices.Add(new Vector3(p0.x, minHeight, p0.y));
                meshData.vertices.Add(new Vector3(p1.x, minHeight, p1.y));

                for (int colorIndex = 0; colorIndex < 4; ++colorIndex)
                {
                    meshData.colors.Add(color);
                }

                meshData.indices.Add(indexOffset + 2);
                meshData.indices.Add(indexOffset + 3);
                meshData.indices.Add(indexOffset + 1);
                meshData.indices.Add(indexOffset + 2);
                meshData.indices.Add(indexOffset + 1);
                meshData.indices.Add(indexOffset + 0);

                indexOffset += 4;
            }

            pointIndex += ringSize;
        }

        return meshData;
    }
}
