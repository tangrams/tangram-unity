using System;
using System.Collections.Generic;
using System.Linq;
using Mapzen.VectorData;
using UnityEngine;

public class Builder
{
    public struct PolygonOptions
    {
        public Material Material;
        public bool Extrude;
        public float MinHeight;
        public float MaxHeight;
    }

    protected struct PolygonBuilder : IGeometryHandler
    {
        public PolygonBuilder(MeshData outputMeshData, PolygonOptions options)
        {
            this.outputMeshData = outputMeshData;
            this.options = options;
            this.coordinates = new List<float>();
            this.rings = new List<int>();
            this.lastPoint = new Point();
            this.pointsInRing = 0;
            this.extrusionVertices = new List<Vector3>();
            this.extrusionIndices = new List<int>();
        }

        MeshData outputMeshData;
        PolygonOptions options;

        // Values for the tesselator.
        List<float> coordinates;
        List<int> rings;
        int pointsInRing;

        // Values for extrusions.
        Point lastPoint;
        List<Vector3> extrusionVertices;
        List<int> extrusionIndices;

        public bool OnPoint(Point point)
        {
            // For all but the first point in each ring, create a quad extending from the
            // previous point to the current point and from MinHeight to MaxHeight.
            if (options.Extrude && pointsInRing > 0)
            {
                var p0 = lastPoint;
                var p1 = point;

                var indexOffset = extrusionVertices.Count;

                extrusionVertices.Add(new Vector3(p0.x, options.MaxHeight, p0.y));
                extrusionVertices.Add(new Vector3(p1.x, options.MaxHeight, p1.y));
                extrusionVertices.Add(new Vector3(p0.x, options.MinHeight, p0.y));
                extrusionVertices.Add(new Vector3(p1.x, options.MinHeight, p1.y));

                extrusionIndices.Add(indexOffset + 2);
                extrusionIndices.Add(indexOffset + 3);
                extrusionIndices.Add(indexOffset + 1);
                extrusionIndices.Add(indexOffset + 2);
                extrusionIndices.Add(indexOffset + 1);
                extrusionIndices.Add(indexOffset + 0);
            }
            lastPoint = point;

            // Add the current point to the buffer of coordinates for the tesselator.
            coordinates.Add(point.x);
            coordinates.Add(point.y);
            pointsInRing++;

            return true;
        }

        public bool OnBeginLineString()
        {
            return false;
        }

        public bool OnEndLineString()
        {
            return false;
        }

        public bool OnBeginLinearRing()
        {
            pointsInRing = 0;
            return true;
        }

        public bool OnEndLinearRing()
        {
            rings.Add(pointsInRing);
            return true;
        }

        public bool OnBeginPolygon()
        {
            return true;
        }

        public bool OnEndPolygon()
        {
            // First add vertices and indices for extrusions.
            outputMeshData.AddElements(extrusionVertices, extrusionIndices, options.Material);

            // Then tesselate polygon interior and add vertices and indices.
            var earcut = new Earcut();

            earcut.Tesselate(coordinates.ToArray(), rings.ToArray());

            var vertices = new List<Vector3>(earcut.vertices.Length / 2);

            for (int i = 0; i < earcut.vertices.Length; i += 2)
            {
                vertices.Add(new Vector3(earcut.vertices[i], options.MaxHeight, earcut.vertices[i + 1]));
            }

            outputMeshData.AddElements(vertices, earcut.indices, options.Material);

            earcut.Release();

            return true;
        }
    }

    public static void TesselatePolygon(MeshData outputMeshData, Feature feature, Material material, float height)
    {
        var options = new PolygonOptions();
        options.Material = material;
        options.MaxHeight = height;

        var builder = new PolygonBuilder(outputMeshData, options);

        feature.HandleGeometry(builder);
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
