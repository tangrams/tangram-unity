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

    public List<Vector3> Vertices { get; }

    public List<Submesh> Submeshes { get; }

    public MeshData()
    {
        Vertices = new List<Vector3>();
        Submeshes = new List<Submesh>();
    }

    public void AddElements(IEnumerable<Vector3> vertices, IEnumerable<int> indices, Material material)
    {
        int offset = Vertices.Count;
        Vertices.AddRange(vertices);

        // Find a submesh with this material, or create a new one.
        Submesh submesh = null;
        foreach (var s in Submeshes)
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
            Submeshes.Add(submesh);
        }

        foreach (var index in indices)
        {
            submesh.Indices.Add(index + offset);
        }
    }

    public void FlipIndices()
    {
        foreach (var submesh in Submeshes)
        {
            submesh.Indices.Reverse();
        }
    }
}
