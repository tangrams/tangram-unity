using System;
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
    public void CreateUnityMesh(MeshData meshData, float offsetX, float offsetY)
    {
        var mesh = new Mesh();

        mesh.SetVertices(meshData.Vertices);

        mesh.subMeshCount = meshData.Submeshes.Count;
        for (int i = 0; i < meshData.Submeshes.Count; i++)
        {
            mesh.SetTriangles(meshData.Submeshes[i].Indices, i);
        }

        mesh.RecalculateNormals();

        transform.Translate(new Vector3(offsetX, 0.0f, offsetY));

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().materials = meshData.Submeshes.Select(s => s.Material).ToArray();
    }
}
