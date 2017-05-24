using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    public List<int> indices = new List<int>();
    public List<Vector3> vertices = new List<Vector3>();
    public List<Color> colors = new List<Color>();

    public void FlipIndices()
    {
        indices.Reverse();
    }

    public void Add(MeshData meshData)
    {
        int indexOffset = vertices.Count;

        for (int i = 0; i < meshData.indices.Count; ++i)
        {
            indices.Add(indexOffset + meshData.indices[i]);
        }

        vertices.AddRange(meshData.vertices);
        colors.AddRange(meshData.colors);
    }
}
