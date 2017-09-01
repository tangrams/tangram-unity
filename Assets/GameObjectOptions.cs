using System;
using UnityEngine;

[Serializable]
public struct GameObjectOptions
{
    public bool IsStatic;
    public bool GeneratePhysicMeshCollider;
    public PhysicMaterial PhysicMaterial;
}

