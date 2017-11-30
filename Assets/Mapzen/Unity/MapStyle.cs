using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mapzen.Unity
{
    [Serializable]
    [CreateAssetMenu(menuName = "Mapzen/MapStyle")]
    public class MapStyle : ScriptableObject
    {
        public List<FeatureLayer> Layers;

        public RegionMap Map;

        public MapStyle()
        {
            this.Layers = new List<FeatureLayer>();
        }
    }
}
