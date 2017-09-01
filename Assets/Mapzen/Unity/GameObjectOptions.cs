using System;
using UnityEngine;

namespace Mapzen.Unity
{
    [Serializable]
    public struct GameObjectOptions
    {
        public bool IsStatic;
        public bool GeneratePhysicMeshCollider;
        public PhysicMaterial PhysicMaterial;
    }
}

