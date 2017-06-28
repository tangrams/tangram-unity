using System;
using System.Collections.Generic;
using System.Linq;
using Mapzen.VectorData;
using UnityEngine;

public class PolygonBuilder : IGeometryHandler
{
    public struct Options
    {
        public Material Material;
        public bool Extrude;
        public float MinHeight;
        public float MaxHeight;
    }

    public PolygonBuilder(MeshData outputMeshData, Options options)
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
    Options options;

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

            extrusionVertices.Add(new Vector3(p0.X, options.MaxHeight, p0.Y));
            extrusionVertices.Add(new Vector3(p1.X, options.MaxHeight, p1.Y));
            extrusionVertices.Add(new Vector3(p0.X, options.MinHeight, p0.Y));
            extrusionVertices.Add(new Vector3(p1.X, options.MinHeight, p1.Y));

            extrusionIndices.Add(indexOffset + 1);
            extrusionIndices.Add(indexOffset + 3);
            extrusionIndices.Add(indexOffset + 2);
            extrusionIndices.Add(indexOffset + 1);
            extrusionIndices.Add(indexOffset + 2);
            extrusionIndices.Add(indexOffset + 0);
        }
        lastPoint = point;

        // Add the current point to the buffer of coordinates for the tesselator.
        coordinates.Add(point.X);
        coordinates.Add(point.Y);
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
        coordinates.Clear();
        rings.Clear();
        extrusionVertices.Clear();
        extrusionIndices.Clear();
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
