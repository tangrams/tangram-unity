using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nextzen.Unity
{
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
            public List<Vector2> UVs;

            public MeshBucket()
            {
                Vertices = new List<Vector3>();
                UVs = new List<Vector2>();
                Submeshes = new List<Submesh>();
            }
        }

        public List<MeshBucket> Meshes;

        private static readonly int MaxVertexCount = 65535;

        public MeshData()
        {
            Meshes = new List<MeshBucket>();
        }

        public void Merge(MeshData other)
        {
            foreach (var bucket in other.Meshes)
            {
                foreach (var submesh in bucket.Submeshes)
                {
                    if (submesh.Indices.Count == 0)
                    {
                        continue;
                    }

                    int minIndex = int.MaxValue;
                    int maxIndex = int.MinValue;

                    foreach (var index in submesh.Indices)
                    {
                        minIndex = Math.Min(minIndex, index);
                        maxIndex = Math.Max(maxIndex, index);
                    }

                    int nIndices = maxIndex - minIndex + 1;
                    var uvs = bucket.UVs.GetRange(minIndex, nIndices);
                    var vertices = bucket.Vertices.GetRange(minIndex, nIndices);
                    var indices = submesh.Indices as IEnumerable<int>;

                    if (minIndex > 0)
                    {
                        indices = indices.Select(i => i - minIndex);
                    }

                    AddElements(vertices, uvs, indices, submesh.Material);
                }
            }
        }

        public void AddElements(IEnumerable<Vector3> vertices, IEnumerable<Vector2> uvs, IEnumerable<int> indices, Material material)
        {
            var vertexList = new List<Vector3>(vertices);
            int vertexCount = vertexList.Count;

            MeshBucket bucket = null;

            // Check whether the last available bucket is valid for use given the maximum vertex count
            if (Meshes.Count > 0)
            {
                var last = Meshes[Meshes.Count - 1];
                if (last.Vertices.Count + vertexCount < MaxVertexCount)
                {
                    bucket = last;
                }
            }

            // No bucket were found, instantiate a new one
            if (bucket == null)
            {
                bucket = new MeshBucket();
                Meshes.Add(bucket);
            }

            int offset = bucket.Vertices.Count;
            bucket.Vertices.AddRange(vertexList);
            bucket.UVs.AddRange(uvs);

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
    }
}