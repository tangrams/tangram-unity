using System;
using UnityEngine;

namespace Nextzen.Unity
{
    [Serializable]
    public struct GameObjectOptions
    {
        public bool IsStatic;
        public bool GeneratePhysicMeshCollider;
        public PhysicMaterial PhysicMaterial;
    }
}