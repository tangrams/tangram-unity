using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mapzen.Unity
{
    [Serializable]
    [CreateAssetMenu(menuName = "Mapzen/Style")]
    public class MapStyle : ScriptableObject
    {
        public List<FeatureLayer> Layers;

        public MapStyle()
        {
            this.Layers = new List<FeatureLayer>();
        }
    }
}
