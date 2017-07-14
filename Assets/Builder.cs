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

    private static Vector2 Miter(Vector3 d0, Vector3 d1, Vector3 n0, Vector3 n1)
    {
        var n0n1 = n0 + n1;
        n0n1.Normalize();

        Vector2 miter = new Vector2();

        double theta = Math.Atan2(d1.y, d1.x) - Math.Atan2(d0.y, d0.x);
        if (theta < 0.0)
        {
            theta += 2.0 * Math.PI;
        }

        double sinHalfTheta = Math.Sin(theta * 0.5);
        miter = n0n1 * (float)(1.0f / Math.Max(sinHalfTheta, 1e-5f));

        return miter;
    }

    private static bool Intersection(Vector2 p0, Vector2 p1, Vector2 v0, Vector2 v1, out Vector2 intersection)
    {
        intersection = new Vector2();

        var p0p1 = p1 - p0;

        var p0p1crossv0 = p0p1.x * v0.y - p0p1.y * v0.x;
        var p0p1crossv1 = p0p1.x * v1.y - p0p1.y * v1.x;

        var v1crossv0 = (v1.x * v0.y - v1.y * v0.x);

        float u = p0p1crossv1 / v1crossv0;
        float t = p0p1crossv0 / v1crossv0;

        // Collinear or parallel
        if (Math.Abs(v1crossv0) <= float.Epsilon)
        {
            return false;
        }

        if (u >= 0.0f && u <= 1.0f && t >= 0.0f && t <= 1.0f)
        {
            intersection = p0 + t * v1;
            return true;
        }

        return false;
    }

    private static void AddPolylinePolygonPoint(bool isFirstPoint, List<Vector2> points, Vector2 lastPoint, Vector2 currPoint, Vector2 nextPoint, float extrude, float miterLimit)
    {
        Vector2 perp = currPoint == lastPoint ? Perp(nextPoint - currPoint) : Perp(currPoint - lastPoint);

        var n0 = Perp(currPoint - lastPoint);
        var n1 = Perp(nextPoint - currPoint);

        if (isFirstPoint)
        {
            points.Add(lastPoint + perp * extrude);
            points.Add(lastPoint - perp * extrude);
        }

        if (currPoint == lastPoint || nextPoint == currPoint)
        {
            return;
        }

        var crossn0n1 = Vector3.Cross(new Vector3(n1.x, n1.y, 0.0f), new Vector3(n0.x, n0.y, 0.0f));
        bool isLeftTurn = crossn0n1.z < 0.0f;

        if (isLeftTurn)
        {
            var d0 = lastPoint - currPoint;
            var d1 = nextPoint - currPoint;

            var miter = Miter(d0, d1, n0, n1);

            // Cap the miter vector
            if (miter.magnitude > miterLimit)
            {
                miter *= miterLimit / miter.magnitude;
            }

            points.Add(currPoint - miter * extrude);
        }
        else
        {
            var p0 = lastPoint - n0 * extrude;
            var p1 = nextPoint - n1 * extrude;
            var v0 = (currPoint - nextPoint);
            var v1 = (currPoint - lastPoint);

            Vector2 intersection;
            if (Intersection(p0, p1, v0, v1, out intersection))
            {
                points.Add(intersection);
            }
            else
            {
                points.Add(currPoint - n0 * extrude);
            }
        }
    }

    public static Geometry PolylineToPolygon(Geometry geometry, float extrude, float miterLimit)
    {
        var pointsAsVec2Enum = geometry.points.Select(v => new Vector2(v.x, v.y));
        List<Vector2> geometryPoints = new List<Vector2>(pointsAsVec2Enum);
        List<Vector2> points = new List<Vector2>();
        Geometry outGeometry = new Geometry();

        // Only ever treat the first ring
        var polygonRing = geometry.rings[0];

        int pointIndex = 0;
        foreach (var ringSize in polygonRing)
        {
            // Trivial case, the string is only one line
            if (ringSize == 2)
            {
                var currPoint = geometryPoints[pointIndex];
                var nextPoint = geometryPoints[pointIndex + 1];

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
            else
            {
                int pointsCount = points.Count;

                // Backward iteration over the linestring
                int start = pointIndex + ringSize - 2;
                int end = pointIndex;
                for (int i = start; i > end; --i)
                {
                    var currPoint = geometryPoints[i];
                    var nextPoint = geometryPoints[i - 1];
                    var lastPoint = geometryPoints[i + 1];

                    AddPolylinePolygonPoint(i == start, points, lastPoint,
                        currPoint, nextPoint, extrude, miterLimit);
                }

                // Forward iteration over the linestring
                start = pointIndex + 1;
                end = pointIndex + ringSize - 1;
                for (int i = start; i < end; ++i)
                {
                    var currPoint = geometryPoints[i];
                    var nextPoint = geometryPoints[i + 1];
                    var lastPoint = geometryPoints[i - 1];

                    AddPolylinePolygonPoint(i == start, points, lastPoint,
                        currPoint, nextPoint, extrude, miterLimit);
                }

                outGeometry.rings.Add(new List<int>() { points.Count - pointsCount });
            }

            pointIndex += ringSize;
        }

        outGeometry.points = new List<Point>(points.Select(p => new Point(p.x, p.y)));

        return outGeometry;
    }
}
