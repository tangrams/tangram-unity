using System;
using System.Linq;
using Mapzen;
using UnityEngine;
using UnityEditor;

public class SceneGraph
{
    /// <summary>
    /// Visits the scene group root recursively and generate a scene graph hierarchy in the Unity scene
    /// </summary>
    /// <param name="group">The scene group to visit.</param>
    /// <param name="parent">The parent transform of the generated game object for the current scene group.</param>
    public static void Generate(SceneGroup group, Transform parent, GameObjectOptions options)
    {
        if (group.meshData.Meshes.Count == 0 && group.childs.Count == 0)
        {
            return;
        }

        if (group.childs.Count > 0)
        {
            var gameObject = new GameObject(group.ToString());
            gameObject.isStatic = options.IsStatic;

            if (parent != null)
            {
                gameObject.transform.parent = parent;
            }

            foreach (var child in group.childs)
            {
                Generate(child.Value, gameObject.transform, options);
            }
        }
        else
        {
            group.meshData.FlipIndices();

            if (group.meshData.Meshes.Count > 1)
            {
                var gameObject = new GameObject(group.ToString());
                gameObject.transform.parent = parent;
                parent = gameObject.transform;
                gameObject.isStatic = options.IsStatic;
            }

            // Create one game object per mesh object 'bucket', each bucket is ensured to
            // have less that 65535 vertices (valid under Unity mesh max vertex count).
            for (int i = 0; i < group.meshData.Meshes.Count; ++i)
            {
                var meshBucket = group.meshData.Meshes[i];
                var gameObject = new GameObject(group.ToString());
                gameObject.isStatic = options.IsStatic;

                if (group.meshData.Meshes.Count > 1)
                {
                    gameObject.name += "_Part" + i;
                }

                gameObject.transform.parent = parent;

                var mesh = new Mesh();

                mesh.SetVertices(meshBucket.Vertices);
                mesh.subMeshCount = meshBucket.Submeshes.Count;
                for (int s = 0; s < meshBucket.Submeshes.Count; s++)
                {
                    mesh.SetTriangles(meshBucket.Submeshes[s].Indices, s);
                }
                mesh.RecalculateNormals();

                if (options.IsStatic)
                {
                    // Generate default uvs for this mesh
                    Unwrapping.GenerateSecondaryUVSet(mesh);
                }

                // Associate the mesh filter and mesh renderer components with this game object
                var materials = meshBucket.Submeshes.Select(s => s.Material).ToArray();
                var meshFilterComponent = gameObject.AddComponent<MeshFilter>();
                var meshRendererComponent = gameObject.AddComponent<MeshRenderer>();
                meshRendererComponent.materials = materials;
                meshFilterComponent.mesh = mesh;

                if (options.GeneratePhysicMeshCollider)
                {
                    var meshColliderComponent = gameObject.AddComponent<MeshCollider>();
                    meshColliderComponent.material = options.PhysicMaterial;
                    meshColliderComponent.sharedMesh = mesh;
                }
            }
        }
    }
}

