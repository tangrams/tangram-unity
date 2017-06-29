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
        int indexOffset = 0;
        foreach (var polygonRing in geometry.rings)
        {
            foreach (var ringSize in polygonRing)
            {
                for (int i = pointOffset; i < pointOffset + ringSize; i++)
                {
                    int curr = i;
                    int next = (i + 1 == pointOffset + ringSize) ? pointOffset : i + 1;

                    var p0 = geometry.points[curr];
                    var p1 = geometry.points[next];

                    vertices.Add(new Vector3(p0.x, height, p0.y));
                    vertices.Add(new Vector3(p1.x, height, p1.y));
                    vertices.Add(new Vector3(p0.x, minHeight, p0.y));
                    vertices.Add(new Vector3(p1.x, minHeight, p1.y));

                    indices.Add(indexOffset + 1);
                    indices.Add(indexOffset + 3);
                    indices.Add(indexOffset + 2);
                    indices.Add(indexOffset + 1);
                    indices.Add(indexOffset + 2);
                    indices.Add(indexOffset + 0);

                    indexOffset += 4;
                }
                pointOffset += ringSize;
            }
        }

        outputMeshData.AddElements(vertices, indices, material);
    }

    public static Vector2 Perp(Vector2 d)
    {
        var p = new Vector2(-d.y, d.x);
        p.Normalize();
        return p;
    }

    public static Geometry PolylineToPolygon(Geometry geometry, float extrude)
    {
        List<Vector2> geometryPoints = new List<Vector2>(geometry.points.Select(v => new Vector2(v.x, v.y)));
        List<Vector2> points = new List<Vector2>();
        Geometry outGeometry = new Geometry();

        // Only ever treat the first ring
        var polygonRing = geometry.rings[0];

        int pointIndex = 0;
        foreach (var ringSize in polygonRing)
        {
            for (int i = pointIndex; i < pointIndex + ringSize - 1; ++i)
            {
                var currPoint = geometryPoints[i];
                var nextPoint = geometryPoints[i + 1];

                if (currPoint != nextPoint)
                {
                    var perp = Perp(nextPoint - currPoint) * extrude;

                    points.Add(nextPoint - perp);
                    points.Add(nextPoint + perp);
                    points.Add(currPoint + perp);
                    points.Add(currPoint - perp);

                    outGeometry.rings.Add(new List<int>() { 4 });
                }
            }

            pointIndex += ringSize;
        }

        outGeometry.points = new List<Point>(points.Select(p => new Point(p.x, p.y)));

        return outGeometry;
    }
}
