using System;
using System.Linq;
using Mapzen;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mapzen.Unity
{
    public class SceneGraph
    {
        private static GameObject AddGameObjectGroupToHierarchy(SceneGroupType groupType, GameObject parentGameObject,
                SceneGroupType groupOptions, FeatureMesh featureMesh)
        {
            GameObject gameObject = null;

            string name = featureMesh.GetName(groupType);

            if (name.Length == 0)
            {
                name = parentGameObject.name;
            }

            if (SceneGroup.Test(groupType, groupOptions))
            {
                if (groupOptions == SceneGroupType.None)
                {
                    gameObject = parentGameObject;
                }
                else
                {
                    if (parentGameObject != null) {
                        for (int i = 0; i < parentGameObject.transform.childCount; ++i)
                        {
                            var child = parentGameObject.transform.GetChild (i);

                            if (child.name == name)
                            {
                                gameObject = child.gameObject;
                                break;
                            }
                        }
                    }

                    if (!gameObject)
                    {
                        gameObject = new GameObject(name);
                        gameObject.transform.parent = parentGameObject.transform;
                    }
                }
            }

            return gameObject;
        }

        public static void MergeGameObjectGroupMeshData(Dictionary<GameObject, MeshData> gameObjects, SceneGroupType groupType,
                GameObject gameObject, SceneGroupType groupOptions, FeatureMesh featureMesh)
        {
            // Merge the mesh data from the feature for the game object
            if (gameObject != null && groupType == SceneGroup.GetLeaf(groupOptions))
            {
                if (gameObjects.ContainsKey(gameObject))
                {
                    gameObjects[gameObject].Merge(featureMesh.Mesh);
                }
                else
                {
                    MeshData data = new MeshData();
                    data.Merge(featureMesh.Mesh);
                    gameObjects.Add(gameObject, data);
                }
            }
        }

        public static void Generate(List<FeatureMesh> features, GameObject mapRegion, SceneGroupType groupOptions, GameObjectOptions gameObjectOptions)
        {
            Dictionary<GameObject, MeshData> gameObjectMeshData = new Dictionary<GameObject, MeshData>();

            GameObject none, tile, layer, filter, feature;

            // Generate all game object with the appropriate hiarchy
            foreach (var featureMesh in features)
            {
                GameObject parent = mapRegion;

                // Group 'none'
                none = AddGameObjectGroupToHierarchy(SceneGroupType.None, parent, groupOptions, featureMesh);
                MergeGameObjectGroupMeshData(gameObjectMeshData, SceneGroupType.None, none, groupOptions, featureMesh);

                // Group 'tile'
                tile = AddGameObjectGroupToHierarchy(SceneGroupType.Tile, parent, groupOptions, featureMesh);
                MergeGameObjectGroupMeshData(gameObjectMeshData, SceneGroupType.Tile, tile, groupOptions, featureMesh);

                // group 'filter'
                parent = tile == null ? parent : tile;
                filter = AddGameObjectGroupToHierarchy(SceneGroupType.Filter, parent, groupOptions, featureMesh);
                MergeGameObjectGroupMeshData(gameObjectMeshData, SceneGroupType.Filter, filter, groupOptions, featureMesh);

                // group 'layer'
                parent = filter == null ? parent : filter;
                layer = AddGameObjectGroupToHierarchy(SceneGroupType.Layer, parent, groupOptions, featureMesh);
                MergeGameObjectGroupMeshData(gameObjectMeshData, SceneGroupType.Layer, layer, groupOptions, featureMesh);

                // group 'feature'
                parent = layer == null ? parent : layer;
                feature = AddGameObjectGroupToHierarchy( SceneGroupType.Feature, parent, groupOptions, featureMesh);
                MergeGameObjectGroupMeshData(gameObjectMeshData, SceneGroupType.Feature, feature, groupOptions, featureMesh);
            }

            // Initialize game objects and associate their components (physics, rendering)
            foreach (var pair in gameObjectMeshData)
            {
                var meshData = pair.Value;
                var root = pair.Key;

                // Create one game object per mesh object 'bucket', each bucket is ensured to
                // have less that 65535 vertices (valid under Unity mesh max vertex count).
                for (int i = 0; i < meshData.Meshes.Count; ++i)
                {
                    var meshBucket = meshData.Meshes[i];
                    GameObject gameObject;

                    if (meshData.Meshes.Count > 1)
                    {
                        gameObject = new GameObject(root.name + "_Part" + i);
                        gameObject.transform.parent = root.transform;
                    }
                    else
                    {
                        gameObject = root.gameObject;
                    }

                    gameObject.isStatic = gameObjectOptions.IsStatic;

                    var mesh = new Mesh();

                    mesh.SetVertices(meshBucket.Vertices);
                    mesh.SetUVs(0, meshBucket.UVs);
                    mesh.subMeshCount = meshBucket.Submeshes.Count;
                    for (int s = 0; s < meshBucket.Submeshes.Count; s++)
                    {
                        mesh.SetTriangles(meshBucket.Submeshes[s].Indices, s);
                    }
                    mesh.RecalculateNormals();

                    // Associate the mesh filter and mesh renderer components with this game object
                    var materials = meshBucket.Submeshes.Select(s => s.Material).ToArray();
                    var meshFilterComponent = gameObject.AddComponent<MeshFilter>();
                    var meshRendererComponent = gameObject.AddComponent<MeshRenderer>();
                    meshRendererComponent.materials = materials;
                    meshFilterComponent.mesh = mesh;

                    if (gameObjectOptions.GeneratePhysicMeshCollider)
                    {
                        var meshColliderComponent = gameObject.AddComponent<MeshCollider>();
                        meshColliderComponent.material = gameObjectOptions.PhysicMaterial;
                        meshColliderComponent.sharedMesh = mesh;
                    }
                }
            }
        }
    }
}
