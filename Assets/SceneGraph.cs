using System;
using System.Linq;
using Mapzen;
using UnityEngine;

public class SceneGraph
{
    /// <summary>
    /// Visits the scene group root recursively and generate a scene graph hierarchy in the Unity scene
    /// </summary>
    /// <param name="group">The scene group to visit.</param>
    /// <param name="groupOptions">The group options used to generate the hierarchy.</param>
    /// <param name="parent">The parent transform of the generated game object for the current scene group.</param>
    public static void Generate(SceneGroup group, SceneGroup.Type groupOptions, Transform parent)
    {
        if (group.meshData.Meshes.Count == 0 && group.childs.Count == 0)
        {
            return;
        }

        if (group.childs.Count > 0)
        {
            var gameObject = new GameObject(group.ToString());

            if (parent != null)
            {
                gameObject.transform.parent = parent;
            }

            foreach (var child in group.childs)
            {
                Generate(child.Value, groupOptions, gameObject.transform);
            }
        }
        else
        {
            group.meshData.FlipIndices();

            // Create one game object per mesh object 'bucket', each bucket is ensured to
            // have less that 65535 vertices (valid under Unity mesh max vertex count).
            for (int i = 0; i < group.meshData.Meshes.Count; ++i)
            {
                var meshData = group.meshData.Meshes[i];
                var gameObject = new GameObject(group.ToString());

                if (group.meshData.Meshes.Count > 1)
                {
                    gameObject.name += "_Part" + i;
                }

                gameObject.transform.parent = parent;

                var mesh = new Mesh();

                mesh.SetVertices(meshData.Vertices);
                mesh.subMeshCount = meshData.Submeshes.Count;
                for (int s = 0; s < meshData.Submeshes.Count; s++)
                {
                    mesh.SetTriangles(meshData.Submeshes[s].Indices, s);
                }
                mesh.RecalculateNormals();

                // Associate the mesh filter and mesh renderer components with this game object
                var materials = meshData.Submeshes.Select(s => s.Material).ToArray();
                var meshFilterComponent = gameObject.AddComponent<MeshFilter>();
                var meshRendererComponent = gameObject.AddComponent<MeshRenderer>();

                meshFilterComponent.mesh = mesh;
                meshRendererComponent.materials = materials;
            }
        }
    }
}

