using System;
using System.Collections.Generic;
using System.Linq;
using Mapzen.VectorData;
using UnityEngine;

public class Builder
{
    public static void TesselatePolygon(MeshData outputMeshData, Geometry geometry, Material material, float height)
    {
        int pointOffset = 0;
        foreach (var polygonRing in geometry.rings)
        {
            Earcut earcut = new Earcut();
            List<float> points = new List<float>();

            int pointIndex = 0;
            foreach (var ringSize in polygonRing)
            {
                for (int i = pointIndex; i < pointIndex + ringSize; ++i)
                {
                    var point = geometry.points[i + pointOffset];
                    points.Add(point.x);
                    points.Add(point.y);
                }
                pointIndex += ringSize;
            }

            pointOffset += pointIndex;

            earcut.Tesselate(points.ToArray(), polygonRing.ToArray());

            var indices = new List<int>(earcut.indices);
            var vertices = new List<Vector3>(earcut.vertices.Length / 2);

            for (int i = 0; i < earcut.vertices.Length; i += 2)
            {
                vertices.Add(new Vector3(earcut.vertices[i], height, earcut.vertices[i + 1]));
            }

            earcut.Release();

            outputMeshData.AddElements(vertices, indices, material);
        }
    }

    public static void TesselatePolygonExtrusion(MeshData outputMeshData, Geometry geometry, Material material, float minHeight, float height)
    {

        var vertices = new List<Vector3>();
        var indices = new List<int>();

        int pointOffset = 0;
        foreach (var polygonRing in geometry.rings)
        {
            int pointIndex = 0;
            int indexOffset = 0;
            foreach (var ringSize in polygonRing)
            {
                for (int i = pointIndex; i < pointIndex + ringSize - 1; i++)
                {
                    var p0 = geometry.points[pointOffset + i];
                    var p1 = geometry.points[pointOffset + i + 1];

                    vertices.Add(new Vector3(p0.x, height, p0.y));
                    vertices.Add(new Vector3(p1.x, height, p1.y));
                    vertices.Add(new Vector3(p0.x, minHeight, p0.y));
                    vertices.Add(new Vector3(p1.x, minHeight, p1.y));

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

            pointOffset += pointIndex;
        }

        outputMeshData.AddElements(vertices, indices, material);
    }
}
