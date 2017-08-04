using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    public class Submesh
    {
        public List<int> Indices;
        public Material Material;
    }

    public class MeshBucket
    {
        public List<Submesh> Submeshes;
        public List<Vector3> Vertices;

        public MeshBucket()
        {
            Vertices = new List<Vector3>();
            Submeshes = new List<Submesh>();
        }
    }

    public List<MeshBucket> Meshes;

    private static readonly int MaxVertexCount = 65535;

    public MeshData()
    {
        Meshes = new List<MeshBucket>();
    }

    public void AddElements(IEnumerable<Vector3> vertices, IEnumerable<int> indices, Material material)
    {
        var vertexList = new List<Vector3>(vertices);
        int vertexCount = vertexList.Count;

        MeshBucket bucket = null;

        if (Meshes.Count > 0)
        {
            var last = Meshes[Meshes.Count - 1];
            if (last.Vertices.Count + vertexCount < MaxVertexCount)
            {
                bucket = last;
            }
        }

        if (bucket == null)
        {
            bucket = new MeshBucket();
            Meshes.Add(bucket);
        }

        int offset = bucket.Vertices.Count;
        bucket.Vertices.AddRange(vertexList);

        // Find a submesh with this material, or create a new one.
        Submesh submesh = null;
        foreach (var s in bucket.Submeshes)
        {
            if (s.Material == material)
            {
                submesh = s;
                break;
            }
        }

        if (submesh == null)
        {
            submesh = new Submesh { Indices = new List<int>(), Material = material };
            bucket.Submeshes.Add(submesh);
        }

        foreach (var index in indices)
        {
            submesh.Indices.Add(index + offset);
        }
    }

    public void FlipIndices()
    {
        foreach (var bucket in Meshes)
        {
            foreach (var submesh in bucket.Submeshes)
            {
                submesh.Indices.Reverse();
            }
        }
    }
}
