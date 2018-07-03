using System;
using UnityEngine;

namespace Nextzen.Unity
{
    [Serializable]
    public struct PolylineOptions
    {
        public Material Material;
        public ExtrusionType Extrusion;
        public float MinHeight;
        public float MaxHeight;
        public float Width;
        public float MiterLimit;
        public bool Enabled;
    }
}