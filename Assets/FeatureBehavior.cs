﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mapzen;
using Mapzen.VectorData;
using Mapzen.VectorData.Filters;
using SimpleJSON;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FeatureBehavior : MonoBehaviour
{
    public void CreateUnityMesh(List<Vector3> vertices, List<MeshData.Submesh> submeshes, float offsetX, float offsetY)
    {
        var mesh = new Mesh();

        mesh.SetVertices(vertices);

        mesh.subMeshCount = submeshes.Count;
        for (int i = 0; i < submeshes.Count; i++)
        {
            mesh.SetTriangles(submeshes[i].Indices, i);
        }

        mesh.RecalculateNormals();

        transform.Translate(new Vector3(offsetX, 0.0f, offsetY));

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().materials = submeshes.Select(s => s.Material).ToArray();
    }
}