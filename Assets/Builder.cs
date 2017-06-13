using System;
using Mapzen.VectorData;
using UnityEngine;
using System.Collections.Generic;

public class Builder
{
    public static MeshData TesselatePolygon(Geometry geometry, Color color, float height)
    {
        var meshData = new MeshData();

        int pointOffset = 0;
        foreach (var polygonRing in geometry.rings)
        {
            var polygonMeshData = new MeshData();
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

            polygonMeshData.indices = new List<int>(earcut.indices);

            for (int i = 0; i < earcut.vertices.Length; i += 2)
            {
                polygonMeshData.vertices.Add(new Vector3(earcut.vertices[i], height, earcut.vertices[i + 1]));
                polygonMeshData.colors.Add(color);
            }

            earcut.Release();

            meshData.Add(polygonMeshData);
        }

        return meshData;
    }

    public static MeshData TesselatePolygonExtrusion(Geometry geometry, Color color, float minHeight, float height)
    {
        var meshData = new MeshData();

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

            pointOffset += pointIndex;
        }

        return meshData;
    }
}
